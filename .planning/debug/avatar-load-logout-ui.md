---
status: verifying
trigger: "Fix two bugs in AuthAvatarControl and AuthViewModel"
created: 2026-03-31T00:00:00Z
updated: 2026-03-31T00:00:00Z
---

## Current Focus

hypothesis: Root causes confirmed per pre-filled symptoms. Applying targeted fixes.
test: N/A - root causes confirmed externally
expecting: N/A
next_action: Apply fix to AuthViewModel.OnAuthStateChanged + UpdateAuthState, then fix AuthAvatarControl.xaml DataTrigger

## Symptoms

expected: Avatar image loads after login; logout clears avatar and restores "Iniciar Sesión" label
actual: 1) Avatar is null after login because GetProfileAsync() was never called. 2) After logout, avatar stays visible and label "Iniciar Sesión" does not reappear (DataTrigger only handles True case).
errors: none (silent UI failures)
reproduction: 1) Login with email/password -> avatar null. 2) Login then logout -> label "Iniciar Sesión" hidden, avatar still showing.
started: always

## Eliminated

- hypothesis: AuthService.CurrentUserAvatarUrl has a bug
  evidence: It correctly reads user metadata for OAuth then falls back to _cachedAvatarUrl. Cache is never populated on login path because GetProfileAsync() is never called.
  timestamp: 2026-03-31T00:00:00Z

- hypothesis: DataTrigger reversal is automatic in MAUI
  evidence: MAUI DataTriggers do not always revert setters reliably when the binding value returns to its non-trigger state; an explicit trigger for False is required for deterministic behavior.
  timestamp: 2026-03-31T00:00:00Z

## Evidence

- timestamp: 2026-03-31T00:00:00Z
  checked: AuthViewModel.OnAuthStateChanged (line 111-127)
  found: When isLoggedIn=true, sets UserAvatarUrl = _authService.CurrentUserAvatarUrl. CurrentUserAvatarUrl returns _cachedAvatarUrl which is null unless GetProfileAsync() has been called.
  implication: Avatar URL is always null after first login unless profile page was visited.

- timestamp: 2026-03-31T00:00:00Z
  checked: AuthViewModel.UpdateAuthState (line 101-109)
  found: Same issue: reads CurrentUserAvatarUrl without ever calling GetProfileAsync(). Also called in constructor, so initial state on app resume is also broken for non-OAuth users.
  implication: Same fix applies to UpdateAuthState.

- timestamp: 2026-03-31T00:00:00Z
  checked: AuthViewModel.OnAuthStateChanged else branch (line 122-125)
  found: Sets UserDisplayName and UserAvatarUrl to empty/null, but does NOT set CurrentProfile = null.
  implication: CurrentProfile retains stale data after logout.

- timestamp: 2026-03-31T00:00:00Z
  checked: AuthAvatarControl.xaml Label DataTrigger (line 13-17)
  found: Only one DataTrigger exists for Value="True" (IsVisible=False). No explicit trigger for Value="False". MAUI default value restore is unreliable in this control tree context.
  implication: After logout IsLoggedIn becomes False, but MAUI may not restore IsVisible=True on the Label.

## Resolution

root_cause: |
  Bug 1: OnAuthStateChanged and UpdateAuthState read CurrentUserAvatarUrl before GetProfileAsync() has
  populated _cachedAvatarUrl, so UserAvatarUrl is always null for email/password users.
  Bug 2: AuthAvatarControl.xaml Label DataTrigger only declares the True->False setter; the False->True
  revert is not guaranteed by MAUI, causing the label to stay hidden after logout.
  Bug 3 (related): OnAuthStateChanged else branch does not clear CurrentProfile.
  Bug 4 (root of logout regression): AuthService.SignOutAsync() only fired AuthStateChanged on the
  error path. On success it relied solely on Supabase's AddStateChangedListener, which is not reliably
  invoked on explicit SignOut in the Supabase .NET client. As a result IsLoggedIn was never set to false,
  so the avatar container remained visible (showing empty avatar + no name) and "Iniciar Sesión" stayed hidden.
fix: |
  1. AuthViewModel.OnAuthStateChanged: when isLoggedIn=true, fire-and-forget GetProfileAsync() and set
     UserAvatarUrl from the result. Also clear CurrentProfile in the else branch.
  2. AuthViewModel.UpdateAuthState: same profile load pattern as async context allows.
  3. AuthAvatarControl.xaml: add explicit DataTrigger for Value="False" with IsVisible=True.
  4. AuthService.SignOutAsync(): explicitly fire AuthStateChanged?.Invoke(this, false) on the success
     path as well as the error path, so IsLoggedIn is always set to false after sign-out.
verification: self-verified (code review); awaiting human confirmation in device
files_changed:
  - AclsTracker/ViewModels/AuthViewModel.cs
  - AclsTracker/Controls/AuthAvatarControl.xaml
  - AclsTracker/Services/Auth/AuthService.cs
