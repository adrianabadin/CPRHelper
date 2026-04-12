---
status: awaiting_human_verify
trigger: "After logout (local data deleted) → login again → navigating to Historial shows the login screen. Logging in from there loops back to the same login screen instead of showing Historial."
created: 2026-03-31T00:00:00Z
updated: 2026-04-01T00:10:00Z
---

## Current Focus

hypothesis: CONFIRMED — LoginPage pushed onto tab nav stack is never popped after login; GoToAsync("//MainPage") switches tabs but leaves stale LoginPage on Historial stack
test: Changed all post-login navigation from GoToAsync("//MainPage") to GoToAsync("..") to pop LoginPage
expecting: After login, user returns to the tab they were on; no stale LoginPage on any tab stack
next_action: Await human verification of the fix on device

## Symptoms

expected: After second login, navigating to Historial shows the session list (even if empty after download completes).
actual: Historial tab/navigation shows login screen. Logging in redirects back to the same login screen in a loop.
errors: No explicit error — navigation loop.
reproduction:
  1. Log in (first time) — Historial works normally, download runs
  2. Log out — local sessions deleted
  3. Log in again (second time)
  4. Navigate to Historial → shows login screen
  5. Log in from that screen → loops back to login screen
started: Phase 05.2 just implemented. The logout flow now calls DeleteLocalUserSessionsAsync before sign-out.

## Eliminated

(none — first hypothesis confirmed)

## Evidence

- timestamp: 2026-04-01T00:01:00Z
  checked: HistorialPage.xaml.cs OnAppearing
  found: No auth guard, no redirect to login. Just loads sessions if IsSavedView.
  implication: HistorialPage itself does NOT redirect to login. The login screen appearing must be a stale page on the navigation stack.

- timestamp: 2026-04-01T00:02:00Z
  checked: HistorialViewModel.cs
  found: No auth check, no navigation to LoginPage anywhere.
  implication: Confirms HistorialViewModel is not the source of the redirect.

- timestamp: 2026-04-01T00:03:00Z
  checked: AuthViewModel.SignInWithEmailAsync (line 172)
  found: After successful login, navigates with `await Shell.Current.GoToAsync("//MainPage")`. This is an absolute route that switches tabs.
  implication: This switches to MainPage tab but does NOT pop LoginPage from whatever tab stack it was pushed onto.

- timestamp: 2026-04-01T00:04:00Z
  checked: AuthViewModel.NavigateToLoginAsync (line 526)
  found: Uses `GoToAsync("LoginPage")` — a relative push. When user is on Historial tab, this pushes LoginPage onto //HistorialPage/LoginPage.
  implication: LoginPage gets pushed onto Historial tab's stack. After login navigates to //MainPage, the LoginPage remains on Historial's stack.

- timestamp: 2026-04-01T00:05:00Z
  checked: AppShell.xaml route structure
  found: HistorialPage is a TabBar ShellContent at route "HistorialPage". LoginPage is registered as a relative route via Routing.RegisterRoute.
  implication: Shell navigation architecture: LoginPage pushed as child of current tab. Tab switching doesn't clear it.

- timestamp: 2026-04-01T00:06:00Z
  checked: All sign-in methods (Email, Google, Apple) and SignUp
  found: ALL use `GoToAsync("//MainPage")` after success. Same problem applies to Google/Apple OAuth and registration.
  implication: Fix must address all sign-in paths, not just email.

- timestamp: 2026-04-01T00:07:00Z
  checked: SignUpAsync email-verification-pending path (line 244)
  found: Uses `GoToAsync("LoginPage")` which PUSHES another LoginPage on top of RegisterPage instead of popping back.
  implication: Additional stack pollution — fixed to use `GoToAsync("..")` to pop back to existing LoginPage.

## Resolution

root_cause: LoginPage is pushed as a relative route onto the current tab's navigation stack (e.g., //HistorialPage/LoginPage). After successful login, GoToAsync("//MainPage") switches the active tab but does NOT pop LoginPage from the originating tab's stack. When the user returns to the Historial tab, the stale LoginPage is still on top, creating a login loop since each successful login just switches to MainPage tab without ever cleaning the Historial stack.
fix: Changed all post-login navigation in AuthViewModel from GoToAsync("//MainPage") to GoToAsync("..") (pop back). This properly removes LoginPage from the navigation stack and returns the user to whichever tab they were on. Also fixed SignUpAsync paths: auto-confirm uses GoToAsync("../..") to pop both RegisterPage and LoginPage; email-verification-pending uses GoToAsync("..") to pop back to LoginPage instead of pushing a duplicate.
verification: (awaiting human verification on device)
files_changed:
  - AclsTracker/ViewModels/AuthViewModel.cs
