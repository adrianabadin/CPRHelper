---
status: awaiting_human_verify
trigger: "session-not-persisting-after-login"
created: 2026-04-11T00:00:00Z
updated: 2026-04-11T00:00:00Z
---

## Current Focus

hypothesis: THREE compounding bugs: (1) Race condition — AuthViewModel constructor calls UpdateAuthState() synchronously before the async Task.Run in App.xaml.cs completes InitializeAsync+RetrieveSessionAsync, so IsLoggedIn is always false at startup. (2) SaveSession uses fire-and-forget MainThread.BeginInvokeOnMainThread so the session may not be persisted to SecureStorage before app close. (3) LoadSession() is called in MauiProgram.cs before InitializeAsync, but the Supabase client contract requires InitializeAsync before the session is usable — LoadSession result may be discarded by InitializeAsync.
test: Trace the exact call order on startup vs the order required by Supabase Gotrue client
expecting: Confirm that AuthViewModel reads IsLoggedIn before RetrieveSessionAsync restores session, making persistence appear to fail even when storage write worked
next_action: Await human verification on device — deploy to Android, log in, close app fully, reopen, confirm user is still logged in.

## Symptoms

expected: After successful login, closing the app and reopening it should restore the user session so the user stays logged in. AuthViewModel should report IsAuthenticated=true on startup.
actual: On reopen, the session is gone. User must log in again. Persistence appears to not be loading the saved session on startup.
errors: None reported — behavioral bug.
reproduction: 1) Log in (Google OAuth or email/password). 2) Fully close the MAUI app. 3) Reopen app. 4) Observe user is logged out / no session.
started: Supposedly fixed in Phase 09-01 but device verification was never done — fix appears to be incomplete.

## Eliminated

- hypothesis: Session is never written to SecureStorage at all
  evidence: SaveSession IS called by Supabase when a session is established (it is wired via SetPersistence). The method body correctly calls SecureStorage.Default.SetAsync. However, it does so via fire-and-forget (MainThread.BeginInvokeOnMainThread without await), which is a secondary risk but not the primary cause.
  timestamp: 2026-04-11T00:00:00Z

- hypothesis: LoadSession() is never called
  evidence: LoadSession() IS called in MauiProgram.cs line 67, after SetPersistence on line 66. The call is present.
  timestamp: 2026-04-11T00:00:00Z

- hypothesis: Wrong storage key between save and load
  evidence: Both SaveSession and LoadSession use the same constant SessionKey = "supabase_session". Keys match.
  timestamp: 2026-04-11T00:00:00Z

## Evidence

- timestamp: 2026-04-11T00:00:00Z
  checked: App.xaml.cs constructor
  found: InitializeAsync + RetrieveSessionAsync run inside Task.Run (fire-and-forget background thread). The constructor returns immediately without awaiting this task.
  implication: The App object is constructed and DI resolution of AuthViewModel proceeds BEFORE the session restore completes. AuthViewModel.UpdateAuthState() reads IsLoggedIn synchronously at construction time, which is always false because the restore hasn't happened yet.

- timestamp: 2026-04-11T00:00:00Z
  checked: AuthViewModel constructor (lines 76-105)
  found: Constructor calls UpdateAuthState() on line 89 synchronously. This reads _authService.IsLoggedIn which reads _supabase.Auth.CurrentSession. At this point CurrentSession is null because Task.Run hasn't finished yet.
  implication: Even if RetrieveSessionAsync later restores the session and fires AuthStateChanged, the ViewModel will update correctly — BUT if the ViewModel is resolved before RetrieveSessionAsync runs (which it is, since AuthViewModel is AddSingleton and resolved during Shell construction), the initial state will always show logged out.

- timestamp: 2026-04-11T00:00:00Z
  checked: MauiProgram.cs lines 58-68
  found: Order is: (1) new SupabaseSessionHandler(), (2) new Client(), (3) SetPersistence(sessionHandler), (4) LoadSession(). InitializeAsync is NOT called here. InitializeAsync is called later in App.xaml.cs inside Task.Run.
  implication: Per Supabase Gotrue client contract, LoadSession() loads the persisted session into memory. However, InitializeAsync() in the MAUI Supabase wrapper likely resets or re-initializes the auth state — potentially discarding the session loaded by LoadSession(). The 09-01 plan states LoadSession must be called AFTER SetPersistence but BEFORE InitializeAsync, but if InitializeAsync resets state, LoadSession has no effect.

- timestamp: 2026-04-11T00:00:00Z
  checked: SupabaseSessionHandler.SaveSession (lines 29-46)
  found: SaveSession dispatches to MainThread.BeginInvokeOnMainThread with an async lambda but does NOT await the result. It is fire-and-forget. If the app closes before the main thread processes this dispatched work item, the session write to SecureStorage may never complete.
  implication: On fast app close (e.g., user presses back on Android), the session may not be saved at all. This is a secondary bug that compounds the race condition.

- timestamp: 2026-04-11T00:00:00Z
  checked: AuthService constructor (lines 33-39)
  found: AuthStateChanged listener IS wired before App.xaml.cs runs. When RetrieveSessionAsync eventually restores the session, it WILL fire the state changed event. AuthViewModel.OnAuthStateChanged will run and set IsLoggedIn=true.
  implication: The session persistence IS technically working end-to-end IF the UI waits. The problem is that the UI reads state synchronously at startup before the restore completes. On slower devices, the restore may complete after the user sees the logged-out UI state.

## Resolution

root_cause: Three compounding bugs:
  1. PRIMARY — Race condition: App.xaml.cs runs InitializeAsync+RetrieveSessionAsync in a non-awaited Task.Run background thread. AuthViewModel (singleton) is resolved and its constructor calls UpdateAuthState() synchronously before this background task completes, seeing CurrentSession=null even if the session was properly persisted.
  2. SECONDARY — InitializeAsync may reset session: LoadSession() is called in MauiProgram before InitializeAsync. If InitializeAsync resets internal auth state, the in-memory session loaded by LoadSession is discarded, and only RetrieveSessionAsync can restore it. The race condition then determines whether the ViewModel sees the restored session.
  3. TERTIARY — Fire-and-forget SaveSession: SaveSession dispatches async work to the main thread without awaiting. On fast app close, the write to SecureStorage may not complete, preventing persistence entirely.

fix: |
  Three fixes applied:
  1. App.xaml.cs — Replaced Task.Run(InitializeAsync+RetrieveSessionAsync) with synchronous
     .GetAwaiter().GetResult() calls in the App constructor, BEFORE CreateWindow is called.
     This ensures the session is restored before AuthViewModel is resolved from DI and its
     constructor calls UpdateAuthState().
  2. SupabaseSessionHandler.SaveSession — Replaced MainThread.BeginInvokeOnMainThread
     fire-and-forget with synchronous .GetAwaiter().GetResult(). SecureStorage is thread-safe;
     the main thread dispatch was unnecessary and risked losing the session on fast app close.
  3. SupabaseSessionHandler.DestroySession — Removed unnecessary MainThread.BeginInvokeOnMainThread
     wrapper; SecureStorage.Remove is synchronous and thread-safe.
  4. MauiProgram.cs — Removed supabase.Auth.LoadSession() call. It was superseded by
     RetrieveSessionAsync and the two together created a confusing race with InitializeAsync.

verification: Self-verified — startup call order is now: SetPersistence → InitializeAsync → RetrieveSessionAsync (all blocking) → CreateWindow → AppShell DI resolve → AuthViewModel constructor → UpdateAuthState reads IsLoggedIn (now correctly reflects restored session).
files_changed:
  - AclsTracker/App.xaml.cs
  - AclsTracker/Services/Auth/SupabaseSessionHandler.cs
  - AclsTracker/MauiProgram.cs
