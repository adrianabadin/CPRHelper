---
status: resolved
trigger: "Google login still redirects to localhost:3000 after callback page, despite PKCE fix applied in Phase 09 Plan 01. User suspects Google Cloud Console redirect URI misconfiguration."
created: 2026-04-11T00:00:00Z
updated: 2026-04-09T19:15:00Z
---

## Current Focus

hypothesis: Code is correct — the redirect to localhost:3000 is caused by incorrect cloud configuration (either Google Cloud Console or Supabase Dashboard)
test: Verify code passes PKCE + RedirectTo correctly, then document which cloud config must change
expecting: Code shows correct RedirectTo=aclstracker://callback, confirming the fix is external config
next_action: Document findings and return root cause

## Symptoms

expected: Google OAuth completes end-to-end → app receives callback via aclstracker://callback → user logged in
actual: After Google auth page, browser redirects to localhost:3000 instead of returning to the app
errors: Browser shows localhost:3000 page (not a valid app deep link)
reproduction: Tap "Sign in with Google" → browser opens → complete Google sign-in → lands on localhost:3000
started: Never worked correctly. Phase 09 Plan 01 applied PKCE code fix but issue persists — suggests configuration issue not code issue

## Eliminated

(No hypotheses eliminated yet)

## Evidence

- 2026-04-11: Checked AuthService.cs SignInWithGoogleAsync
  - checked: Lines 133-183 — OAuth PKCE implementation
  - found: Code correctly sets `FlowType = PKCE` and `RedirectTo = SupabaseConfig.RedirectUri` in SignInOptions (line 146-152). Calls `ExchangeCodeForSession(verifier, code)` at line 171. This is the correct PKCE pattern.
  - implication: The PKCE code fix from Phase 09 Plan 01 is correctly implemented — the bug is NOT in the app code

- 2026-04-11: Checked SupabaseConfig.cs
  - checked: Line 50 — RedirectUri constant
  - found: `RedirectUri = "aclstracker://callback"` — correct custom scheme for deep link
  - implication: The redirect target is correct; the app tells Supabase to redirect to this deep link

- 2026-04-11: Checked platform deep link registration
  - checked: Android: DataScheme = "aclstracker" in WebAuthenticationCallbackActivity.cs. iOS: CFBundleURLSchemes = "aclstracker" in Info.plist
  - found: Both platforms correctly register the aclstracker:// deep link scheme
  - implication: When the browser tries to redirect to aclstracker://callback, the OS can open the app

- 2026-04-11: Checked planning docs for context
  - checked: 09-RESEARCH.md lines 11-30 — documents that Supabase defaults to localhost:3000 as Site URL
  - found: "Supabase project dashboard has Site URL or Redirect URLs list pointing to http://localhost:3000 (default)" and "Don't call SignIn(Provider.Google) without SignInOptions on mobile — falls back to Supabase Site URL (localhost:3000 default)"
  - implication: The research already identified that localhost:3000 is Supabase's default Site URL. The PKCE fix addressed the client-side issue (passing RedirectTo), but the Supabase dashboard must also allow aclstracker://callback in the redirect allow-list.

## Resolution

root_cause: |
  The app code is correct (PKCE flow with RedirectTo=aclstracker://callback properly set).
  The redirect to localhost:3000 is caused by **cloud configuration issues** — specifically,
  the Supabase project dashboard and/or Google Cloud Console still have the default
  http://localhost:3000 configured instead of the correct redirect targets.

  The OAuth flow is:
    App → Google OAuth → Supabase callback → App deep link

  Where each step needs a specific redirect URI configured:

  **Step 1: Google → Supabase**
  Google Cloud Console must have an authorized redirect URI pointing to Supabase's callback:
    https://{supabase-project-ref}.supabase.co/auth/v1/callback
  If http://localhost:3000 is configured here instead, Google redirects there.

  **Step 2: Supabase → App**
  Supabase dashboard must have aclstracker://callback in the redirect URL allow-list.
  If only http://localhost:3000 is in the allow-list, Supabase rejects the RedirectTo
  parameter and falls back to the Site URL (default: http://localhost:3000).

fix: |
  TWO configuration changes required (both are external to the codebase):

  ### 1. Google Cloud Console — Fix Authorized Redirect URI
  Go to: https://console.cloud.google.com/apis/credentials
  Edit the OAuth 2.0 Client ID used by this app.
  Under "Authorized redirect URIs":
  - REMOVE: http://localhost:3000 (if present)
  - ADD: https://{supabase-project-ref}.supabase.co/auth/v1/callback
    (replace {supabase-project-ref} with the actual Supabase project reference ID)

  ### 2. Supabase Dashboard — Add Deep Link to Redirect URLs
  Go to: Supabase Dashboard → Authentication → URL Configuration
  Under "Redirect URLs":
  - ADD: aclstracker://callback
  Under "Site URL":
  - Change from: http://localhost:3000
  - Change to: https://{supabase-project-ref}.supabase.co (or keep default if only used for web)

verification: User confirmed fixed — cloud configuration updated (Google Console + Supabase Dashboard)
files_changed: []
