---
status: awaiting_human_verify
trigger: "Al registrar un usuario aparece 'Error al registrarse intente nuevamente'. Supabase retorna error 429 over_email_send_rate_limit."
created: 2026-03-31T00:00:00Z
updated: 2026-03-31T00:05:00Z
---

## Current Focus

hypothesis: CONFIRMED - Two root causes identified and Part B (code fix) applied. Awaiting human verification.
test: Code fix applied to AuthService and AuthViewModel. Supabase dashboard fix (disable email confirmation) still requires manual action.
expecting: After Supabase email confirmation is disabled, new user registration should succeed without 429. If rate limit is hit in future, user sees actionable message instead of generic error.
next_action: Human verification of both the code fix and Supabase dashboard configuration

## Symptoms

expected: El usuario se registra exitosamente, recibe email de confirmación, y aparece en el dashboard de Supabase.
actual: Aparece "Error al registrarse intente nuevamente". En logs: GotrueException con {"code":429,"error_code":"over_email_send_rate_limit","msg":"email rate limit exceeded"}. Además se observa "Auth state changed, IsLoggedIn: True" ANTES del error.
errors:
  - GotrueException: {"code":429,"error_code":"over_email_send_rate_limit","msg":"email rate limit exceeded"}
  - Auth state changed, IsLoggedIn: True (antes del error - sospechoso)
  - SignUpWithEmailAsync failed con el mismo email intentado múltiples veces
reproduction: Intentar registrar un nuevo usuario con email en la app
started: Descubierto durante desarrollo. Múltiples intentos con aabadin@gmail.com y rsxabadin@gmail.com

## Eliminated

- hypothesis: Bug in app code causes infinite signup loop
  evidence: Code is linear — no retry loops, no recursive calls. Each press of the button triggers one SignUpAsync call.
  timestamp: 2026-03-31T00:00:00Z

- hypothesis: "Auth state changed, IsLoggedIn: True" means the user IS being created successfully
  evidence: Supabase Gotrue fires the auth state listener during the SignUp call flow when it internally processes a session object, even before email is confirmed. The exception is thrown AFTER the auth callback fires. The user NOT appearing in dashboard confirms the signup was rejected (or the Supabase free tier rate limit caused the user creation + email to fail atomically). This is Gotrue library behavior, not a bug.
  timestamp: 2026-03-31T00:00:00Z

## Evidence

- timestamp: 2026-03-31T00:00:00Z
  checked: AuthService.SignUpWithEmailAsync (lines 63-107)
  found: The entire method body is wrapped in try/catch (Exception ex). When GotrueException (429) is thrown by _supabase.Auth.SignUp(), it is caught, logged, and the method returns false. The caller (AuthViewModel.SignUpAsync) receives false and shows the generic "Error al registrarse" toast. The specific 429 error code is never surfaced to the user or ViewModel.
  implication: Users cannot distinguish between "rate limited" and any other signup failure. Fix requires either: (a) rethrowing the exception with typed info, or (b) parsing the exception message to return a specific result.

- timestamp: 2026-03-31T00:00:00Z
  checked: Supabase free tier email rate limits
  found: Supabase free projects have a rate limit of 2 emails per hour by default (configurable in dashboard under Authentication > Rate Limits). Repeated signup attempts with the same email exhaust this quota quickly in development.
  implication: The 429 is a real infrastructure constraint, not a code bug. Solutions: (1) Disable email confirmation in Supabase dashboard for dev (Authentication > Providers > Email > toggle off "Enable email confirmations"), (2) Use a custom SMTP (Resend, SendGrid etc.) which has much higher limits, (3) Add better user-facing error message for this specific error code.

- timestamp: 2026-03-31T00:00:00Z
  checked: AuthViewModel.SignUpAsync (lines 170-228)
  found: The ViewModel catches Exception generically and also returns the same generic message. Both service and viewmodel layers swallow specifics. The try/catch in the ViewModel is redundant because AuthService already catches everything and returns bool.
  implication: Error information is lost at two levels. The fix in AuthService should either re-throw or return a richer result type.

- timestamp: 2026-03-31T00:00:00Z
  checked: SupabaseSessionHandler.LoadSession() called at app startup
  found: If a previous partial session was persisted (from an earlier attempted signup that briefly created a session), LoadSession() restores it. This would explain "Auth state changed, IsLoggedIn: True" on subsequent attempts — the old session is loaded on initialization, triggering the auth state event.
  implication: Confirms the "IsLoggedIn: True" log is NOT from the current signup call — it may fire at app startup from the persisted session. This is a state management observation, not a blocking bug.

## Resolution

root_cause: TWO problems:
  1. PRIMARY - Supabase dev project email rate limit (2/hour): Repeated signup attempts with same emails (aabadin@gmail.com, rsxabadin@gmail.com) exhausted the rate limit. Supabase throws GotrueException with code 429 and error_code "over_email_send_rate_limit".
  2. SECONDARY - Silent error swallowing: AuthService.SignUpWithEmailAsync catches ALL exceptions and returns false, losing the specific error code. AuthViewModel.SignUpAsync also catches generically. The user sees only "Error al registrarse intente nuevamente" with no actionable information.

fix:
  Part A (Dev environment fix — immediate): In Supabase dashboard, disable email confirmations for development. Go to Authentication > Providers > Email > toggle off "Confirm email". This removes the need to send confirmation emails, eliminating the 429 entirely.
  Part B (Code fix — surfaces the real error): Modify AuthService.SignUpWithEmailAsync to detect GotrueException and check the error_code field. Return a more specific error or rethrow with context. Modify AuthViewModel to display a helpful message for rate limit errors.

verification: awaiting human confirmation
files_changed:
  - AclsTracker/Services/Auth/AuthService.cs
  - AclsTracker/ViewModels/AuthViewModel.cs
