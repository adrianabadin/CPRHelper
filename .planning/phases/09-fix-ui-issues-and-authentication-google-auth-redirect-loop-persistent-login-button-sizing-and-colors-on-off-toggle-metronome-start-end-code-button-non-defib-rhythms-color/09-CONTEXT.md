# Phase 9: Fix UI Issues and Authentication - Context

**Gathered:** 2026-04-09
**Status:** Ready for planning

<domain>
## Phase Boundary

Fix 6 issues across authentication and main UI:

1. **Google OAuth redirect loop** — Login via Google redirects to `localhost:3000` and never completes
2. **Metronome toggle button** — Currently shows `True`/`False`, must show `ON`/`OFF` and be smaller
3. **Metronome +/− buttons** — Must be smaller so the metronome row fits the UI
4. **INICIAR / FINALIZAR CODIGO button** — Slightly less height, color = current AESP orange (#E65100)
5. **Non-defibrillable rhythm buttons (AESP, Asistolia)** — Change color to yellow
6. **Persistent login across app restarts** — Session currently lost when app closes; must persist

Out of scope: New auth providers, new UI screens, new functionality, redesign of other buttons.

</domain>

<decisions>
## Implementation Decisions

### 1. Google OAuth redirect bug (investigation needed)
- Symptom: After Google auth completes in browser, user is redirected to `localhost:3000` and the app never receives the callback
- Code uses `WebAuthenticator` with `SupabaseConfig.RedirectUri = "aclstracker://callback"`
- Android intent filter already registered (`WebAuthenticationCallbackActivity.cs`, `DataScheme = "aclstracker"`)
- iOS Info.plist has `aclstracker` URL scheme
- **Likely root cause:** Supabase project dashboard has `Site URL` or `Redirect URLs` list pointing to `http://localhost:3000` (default). Google OAuth provider returns to Supabase, then Supabase redirects to its configured `Site URL` instead of the mobile deep link.
- **Fix direction:** Verify Supabase dashboard → Auth → URL Configuration has `aclstracker://callback` in redirect allow-list and as Site URL (or use a proper hosted redirect). Confirm `SignInWithOAuth` passes `redirectTo` parameter explicitly if SDK supports it.
- Researcher must verify: (a) current Supabase URL config, (b) whether `supabase-csharp` SDK exposes `redirectTo` option on `SignIn(Provider.Google)`, (c) how PKCE code exchange happens after deep-link callback.

### 2. Persistent login
- **Expected behavior:** Sesión persistente hasta logout manual. User stays logged in indefinitely across app closes/reopens until they explicitly press Logout. Supabase refresh tokens handle renewal automatically.
- **Current behavior:** Session is lost when app closes. Phase 05.1 CONTEXT claimed this was done but it is NOT working — this is a bug.
- **Investigation needed:** Check `App.xaml.cs` / `MauiProgram.cs` for Supabase client initialization. `supabase-csharp` requires `SessionHandler` + `PersistSession = true` + file-backed `SessionPersistor` implementation. Likely missing or not calling `RetrieveSessionAsync` on app startup.
- **Acceptance:** Close app → reopen → user still logged in with avatar visible, no re-login required.

### 3. Metronome ON/OFF toggle button
- **Current:** `Text="{Binding IsPlaying, StringFormat='{0}'}"` → renders "True"/"False"
- **Fix:** Use `BoolToOnOffConverter` or a computed property (e.g., `IsPlayingLabel` returning `"ON"`/`"OFF"`). Prefer converter for reusability, but property is acceptable.
- **Size:** Smaller — Claude's discretion. Target ~56×36 from current 70×44.
- Color unchanged (blue `#1976D2` / dark `#1565C0`).

### 4. Metronome +/− buttons
- **Current:** 44×44 circular
- **Fix:** Smaller — Claude's discretion. Target ~36×36 circular (CornerRadius=18), reduce FontSize slightly to fit.
- Goal: the whole metronome `HorizontalStackLayout` (pulse + BPM label + "BPM" + − + ON/OFF + +) must fit comfortably in one row without wrap or overflow.

### 5. INICIAR / FINALIZAR CODIGO button
- **Current INICIAR:** `BackgroundColor="#D32F2F"` (red), `HeightRequest="48"`
- **Current FINALIZAR:** `BackgroundColor="#757575"` (gray), `HeightRequest="48"`
- **Fix:**
  - INICIAR CODIGO → background `#E65100` (the current AESP orange, captured before AESP changes to yellow)
  - FINALIZAR CODIGO → keep current gray `#757575` (visual differentiation between inactive/active session)
  - Height reduced — Claude's discretion. Target `HeightRequest="40"` (from 48).
- Text, font, corner radius unchanged.

### 6. Non-defibrillable rhythms (AESP + Asistolia)
- **Current color:** `#E65100` (orange)
- **New color:** `#FBC02D` (ámbar fuerte — strong amber/yellow)
- **Text color:** `#333333` (dark) for accessibility contrast on yellow. Override the current `TextColor="White"` for these two buttons only.
- **Selection indicator (DataTrigger):** keep `BorderColor="White"` + `BorderWidth="2"` — it still reads over yellow.
- Other rhythm buttons (RCE green, TV/FV red) unchanged.

### Claude's Discretion
- Exact pixel values for size reductions within the ranges noted above
- Whether to implement `BoolToOnOffConverter` as a new converter class or an `IsPlayingLabel` computed property on `MetronomeViewModel`
- How to structure the Supabase session persistence fix (which handler interface, where to persist — `SecureStorage` vs `Preferences` vs file)
- Whether to verify the OAuth fix requires only Supabase dashboard config change, or code change in `SignInWithGoogleAsync` as well

</decisions>

<specifics>
## Specific Ideas

- **Color capture note:** The user explicitly said "el naranja del botón AESP" for the código button. Since AESP is *changing* from orange to yellow in this same phase, we captured the CURRENT orange (`#E65100`) as the target — not the post-change yellow. Verify with user if any confusion during planning.
- **Persistence was promised in Phase 05.1** CONTEXT.md ("Persistencia de sesión: Sí — tokens persisten entre cierres de app") but isn't working. This is a correctness bug against the prior decision, not a new feature.
- **OAuth `localhost:3000`** is Supabase's default Site URL — strong hint the fix is in the dashboard, not the client code, though both should be verified.

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- **Converters already exist:** `BoolToColorConverter`, `IsNotNullConverter`, `InvertBoolConverter`, `BoolToOpacityConverter` in `AclsTracker/Converters/`. Add a `BoolToOnOffConverter` in the same location if going the converter route.
- **`AuthService`** (`AclsTracker/Services/Auth/AuthService.cs`): handles all auth logic. `SignInWithGoogleAsync` is where the OAuth flow lives (lines 133–172). Session persistence fix likely touches `MauiProgram.cs` Supabase client registration and `App.xaml.cs` startup.
- **`SupabaseConfig.RedirectUri`** (`AclsTracker/Constants/SupabaseConfig.cs:50`): `"aclstracker://callback"` — correct deep link, already wired on Android and iOS.

### Established Patterns
- **XAML inline styling** — Colors are hard-coded hex values inline, not via ResourceDictionary. Keep this convention for the 3 color changes.
- **MVVM with CommunityToolkit.Mvvm** — ViewModels use `[ObservableProperty]`. If adding `IsPlayingLabel`, it can be a simple computed getter that raises `PropertyChanged` when `IsPlaying` changes.
- **Bug-fix pattern from Phase 08** — Phase 08 did similar UI tweaks (overlay banner, compress timer cards). Keep commits per-issue for clean history.

### Integration Points
- `AclsTracker/Controls/MetronomePulse.xaml` lines 30–54 — metronome buttons (−, ON/OFF, +)
- `AclsTracker/Controls/RhythmSelector.xaml` lines 39–74 — AESP & ASISTOLIA buttons
- `AclsTracker/Views/MainPage.xaml` lines 27–46 — INICIAR/FINALIZAR CODIGO buttons
- `AclsTracker/Services/Auth/AuthService.cs` lines 133–172 — Google OAuth
- `AclsTracker/MauiProgram.cs` — Supabase client DI registration (session persistence fix lives here)
- `AclsTracker/App.xaml.cs` — app startup (session restore likely needed here)

</code_context>

<deferred>
## Deferred Ideas

None — all 6 items fit cleanly within the phase boundary. No scope creep surfaced during discussion.

</deferred>

---

*Phase: 09-fix-ui-issues-and-authentication*
*Context gathered: 2026-04-09*
