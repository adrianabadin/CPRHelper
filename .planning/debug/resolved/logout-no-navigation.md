---
status: resolved
trigger: "logout-no-navigation: Al cerrar sesión, la app no navega de vuelta a MainPage/Shell — falla silenciosa."
created: 2026-03-31T00:00:00Z
updated: 2026-03-31T00:01:00Z
---

## Current Focus

hypothesis: CONFIRMED — SignOutAsync in AuthViewModel calls _authService.SignOutAsync() and clears local state, but never calls Shell.Current.GoToAsync to navigate anywhere
test: read SignOutAsync command in AuthViewModel (lines 374-396)
expecting: navigation call after sign-out — it is absent
next_action: add Shell.Current.GoToAsync("//MainPage") after sign-out succeeds in AuthViewModel

## Symptoms

expected: Al hacer logout, la app debería navegar al Shell principal / MainPage
actual: El logout se ejecuta pero la pantalla no cambia; queda donde está
errors: Falla silenciosa, sin errores visibles
reproduction: Iniciar sesión → navegar a cualquier pantalla → hacer logout → pantalla no cambia
started: unknown — never verified to work

## Eliminated

- hypothesis: Navigation call exists but targets wrong route
  evidence: No navigation call exists at all in SignOutAsync — ruling out wrong route
  timestamp: 2026-03-31T00:01:00Z

- hypothesis: AppShell does not have MainPage registered
  evidence: AppShell.xaml has TabBar with ShellContent Route="MainPage" — route is valid
  timestamp: 2026-03-31T00:01:00Z

- hypothesis: AuthStateChanged event triggers navigation elsewhere
  evidence: OnAuthStateChanged in AuthViewModel only updates IsLoggedIn, UserDisplayName, UserAvatarUrl — no navigation
  timestamp: 2026-03-31T00:01:00Z

## Evidence

- timestamp: 2026-03-31T00:01:00Z
  checked: AuthViewModel.SignOutAsync (lines 374-396)
  found: calls _authService.SignOutAsync(), clears CurrentProfile/UserDisplayName/UserAvatarUrl, shows toast — no GoToAsync call
  implication: This is the direct cause. Sign-in commands (SignInWithEmailAsync, SignInWithGoogleAsync, SignInWithAppleAsync) all do call Shell.Current.GoToAsync("//MainPage") on success. SignOutAsync does not call any navigation.

- timestamp: 2026-03-31T00:01:00Z
  checked: AuthViewModel.OnAuthStateChanged (lines 121-142)
  found: handles isLoggedIn branch (loads profile/avatar) and else branch (clears display state) — no navigation in either branch
  implication: The event handler is not a fallback navigation path

- timestamp: 2026-03-31T00:01:00Z
  checked: AppShell.xaml routes
  found: LoginPage, RegisterPage, ProfilePage are registered as modal routes; MainPage is a TabBar ShellContent with Route="MainPage" — reachable via "//MainPage"
  implication: The route "//MainPage" is valid and is already used correctly by sign-in commands

- timestamp: 2026-03-31T00:01:00Z
  checked: ProfilePage.xaml logout button (line 99)
  found: bound to SignOutCommand — correct binding, uses the same AuthViewModel.SignOutAsync command
  implication: The button is wired correctly; the missing navigation is purely in the ViewModel command

## Resolution

root_cause: AuthViewModel.SignOutAsync executes the sign-out and clears local state but never calls Shell.Current.GoToAsync("//MainPage"). Every sign-in path navigates to //MainPage on success, but the sign-out path has no corresponding navigation call.
fix: Added await Shell.Current.GoToAsync("//MainPage") in both the try branch (after successful sign-out) and the catch branch (so navigation happens even if an exception occurs, since AuthService already fires AuthStateChanged(false) in both paths) of AuthViewModel.SignOutAsync.
verification: confirmed by user — app navigates back to MainPage after logout
files_changed:
  - AclsTracker/ViewModels/AuthViewModel.cs
