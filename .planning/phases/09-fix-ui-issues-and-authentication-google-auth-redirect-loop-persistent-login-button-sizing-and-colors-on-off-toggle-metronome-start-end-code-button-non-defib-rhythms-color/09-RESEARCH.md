# Phase 9: Fix UI Issues and Authentication - Research

**Researched:** 2026-04-09
**Domain:** Supabase auth (OAuth PKCE + session persistence) + .NET MAUI XAML UI tweaks
**Confidence:** HIGH

## Summary

Phase 9 is a bug-fix/UI-polish phase covering six discrete issues. Two are authentication bugs with real root causes in `supabase-csharp` API misuse (OAuth redirect loop, broken session persistence); four are mechanical XAML tweaks (button sizing, colors, ON/OFF label).

The OAuth redirect loop is almost certainly a combination of two problems: (1) `AuthService.SignInWithGoogleAsync` calls `_supabase.Auth.SignIn(Provider.Google)` with **no `SignInOptions`**, meaning no `FlowType=PKCE` and no `RedirectTo="aclstracker://callback"` — so Supabase falls back to the dashboard Site URL (which defaults to `http://localhost:3000`); and (2) even if the browser returned to the deep link, the code never calls `ExchangeCodeForSession(PKCEVerifier, code)`, so no session is ever established. The Supabase dashboard redirect allow-list must also include `aclstracker://callback`.

The persistent-login bug is a startup ordering problem: `MauiProgram.cs` calls `SetPersistence(sessionHandler)` but never calls `LoadSession()` + `RetrieveSessionAsync()` to actually hydrate the in-memory session from SecureStorage. `InitializeAsync()` in gotrue-csharp does **not** automatically load from the persistence handler — those are separate, explicit calls.

**Primary recommendation:** Fix OAuth by passing `SignInOptions { FlowType = PKCE, RedirectTo = SupabaseConfig.RedirectUri }`, parsing the `code` query param from the WebAuthenticator callback, and calling `ExchangeCodeForSession`. Fix persistence by calling `LoadSession()` then `await RetrieveSessionAsync()` on app startup. Also verify the Supabase dashboard has `aclstracker://callback` in Authentication → URL Configuration → Redirect URLs. UI tweaks are surgical XAML edits to three files.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**1. Google OAuth redirect bug — investigation required**
- Symptom: After Google auth completes in browser, user is redirected to `localhost:3000` and the app never receives the callback.
- Code uses `WebAuthenticator` with `SupabaseConfig.RedirectUri = "aclstracker://callback"`.
- Android intent filter already registered (`WebAuthenticationCallbackActivity.cs`, `DataScheme = "aclstracker"`).
- iOS Info.plist has `aclstracker` URL scheme.
- **Likely root cause:** Supabase project dashboard has `Site URL` or `Redirect URLs` list pointing to `http://localhost:3000` (default). Google OAuth provider returns to Supabase, then Supabase redirects to its configured `Site URL` instead of the mobile deep link.
- **Fix direction:** Verify Supabase dashboard → Auth → URL Configuration has `aclstracker://callback` in redirect allow-list and as Site URL (or use a proper hosted redirect). Confirm `SignInWithOAuth` passes `redirectTo` parameter explicitly if SDK supports it.
- Researcher must verify: (a) current Supabase URL config, (b) whether `supabase-csharp` SDK exposes `redirectTo` option on `SignIn(Provider.Google)`, (c) how PKCE code exchange happens after deep-link callback.

**2. Persistent login**
- **Expected behavior:** Sesión persistente hasta logout manual. User stays logged in indefinitely across app closes/reopens until they explicitly press Logout. Supabase refresh tokens handle renewal automatically.
- **Current behavior:** Session is lost when app closes. Phase 05.1 CONTEXT claimed this was done but it is NOT working — this is a bug.
- **Investigation needed:** Check `App.xaml.cs` / `MauiProgram.cs` for Supabase client initialization. `supabase-csharp` requires `SessionHandler` + `PersistSession = true` + file-backed `SessionPersistor` implementation. Likely missing or not calling `RetrieveSessionAsync` on app startup.
- **Acceptance:** Close app → reopen → user still logged in with avatar visible, no re-login required.

**3. Metronome ON/OFF toggle button**
- **Current:** `Text="{Binding IsPlaying, StringFormat='{0}'}"` → renders "True"/"False".
- **Fix:** Use `BoolToOnOffConverter` or a computed property (e.g., `IsPlayingLabel` returning `"ON"`/`"OFF"`). Prefer converter for reusability, but property is acceptable.
- **Size:** Smaller — Claude's discretion. Target ~56×36 from current 70×44.
- Color unchanged (blue `#1976D2` / dark `#1565C0`).

**4. Metronome +/− buttons**
- **Current:** 44×44 circular.
- **Fix:** Smaller — Claude's discretion. Target ~36×36 circular (CornerRadius=18), reduce FontSize slightly to fit.
- Goal: the whole metronome `HorizontalStackLayout` (pulse + BPM label + "BPM" + − + ON/OFF + +) must fit comfortably in one row without wrap or overflow.

**5. INICIAR / FINALIZAR CODIGO button**
- **Current INICIAR:** `BackgroundColor="#D32F2F"` (red), `HeightRequest="48"`.
- **Current FINALIZAR:** `BackgroundColor="#757575"` (gray), `HeightRequest="48"`.
- **Fix:**
  - INICIAR CODIGO → background `#E65100` (the current AESP orange, captured before AESP changes to yellow).
  - FINALIZAR CODIGO → keep current gray `#757575` (visual differentiation between inactive/active session).
  - Height reduced — Claude's discretion. Target `HeightRequest="40"` (from 48).
- Text, font, corner radius unchanged.

**6. Non-defibrillable rhythms (AESP + Asistolia)**
- **Current color:** `#E65100` (orange).
- **New color:** `#FBC02D` (ámbar fuerte — strong amber/yellow).
- **Text color:** `#333333` (dark) for accessibility contrast on yellow. Override the current `TextColor="White"` for these two buttons only.
- **Selection indicator (DataTrigger):** keep `BorderColor="White"` + `BorderWidth="2"` — it still reads over yellow.
- Other rhythm buttons (RCE green, TV/FV red) unchanged.

### Claude's Discretion

- Exact pixel values for size reductions within the ranges noted above.
- Whether to implement `BoolToOnOffConverter` as a new converter class or an `IsPlayingLabel` computed property on `MetronomeViewModel`.
- How to structure the Supabase session persistence fix (which handler interface, where to persist — `SecureStorage` vs `Preferences` vs file).
- Whether to verify the OAuth fix requires only Supabase dashboard config change, or code change in `SignInWithGoogleAsync` as well.

### Deferred Ideas (OUT OF SCOPE)

None — all 6 items fit cleanly within the phase boundary. No scope creep surfaced during discussion.
</user_constraints>

<phase_requirements>
## Phase Requirements

No numeric requirement IDs in ROADMAP for Phase 9 (marked TBD). Derived from CONTEXT.md decisions:

| ID (derived) | Description | Research Support |
|----|-------------|-----------------|
| P09-AUTH-01 | Google OAuth completes end-to-end on Android/iOS without redirecting to `localhost:3000`; after browser returns to `aclstracker://callback`, a Supabase session is established and `IsLoggedIn == true`. | PKCE flow with `SignInOptions.RedirectTo` + `ExchangeCodeForSession`; dashboard redirect allow-list. See **Architecture Patterns → Pattern 1** and **Common Pitfalls → Pitfall 1**. |
| P09-AUTH-02 | Session persists across app restarts until explicit logout; user reopens app and is still logged in with avatar visible, no re-auth required. | `SetPersistence` + explicit `LoadSession()` + `await RetrieveSessionAsync()` during startup. See **Architecture Patterns → Pattern 2**. |
| P09-UI-01 | Metronome toggle button shows `ON`/`OFF` (not `True`/`False`) and fits the row in the metronome control. | `BoolToOnOffConverter` (preferred, reusable) or `IsPlayingLabel` computed property. See **Code Examples → Example 1**. |
| P09-UI-02 | Metronome `+` / `−` buttons reduced (~44→36px) so `HorizontalStackLayout` fits the row without wrap/overflow. | XAML edit to `MetronomePulse.xaml`; keep `CornerRadius = Width/2` for circular shape. |
| P09-UI-03 | `INICIAR CODIGO` button uses `#E65100` (captured orange), height reduced from 48 to ~40; `FINALIZAR CODIGO` stays gray `#757575` at same new height. | XAML edit to `MainPage.xaml` lines 28–45. |
| P09-UI-04 | AESP + ASISTOLIA rhythm buttons use `#FBC02D` background + `#333333` text, retain white border DataTrigger for selection. | XAML edit to `RhythmSelector.xaml` lines 39–74. |
</phase_requirements>

## Standard Stack

### Core (already installed, no additions needed)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `supabase-csharp` / `gotrue-csharp` | current (installed) | Auth, PKCE OAuth, session persistence | Official C# SDK used throughout the project |
| `Microsoft.Maui.Authentication.WebAuthenticator` | MAUI 8 built-in | OAuth callback via system browser + deep link | Standard cross-platform OAuth helper on MAUI |
| `Microsoft.Maui.Storage.SecureStorage` | MAUI 8 built-in | Encrypted token storage (Android Keystore / iOS Keychain) | Already used by `SupabaseSessionHandler`; HIPAA-appropriate |
| `CommunityToolkit.Mvvm` | installed | `[ObservableProperty]`, `RelayCommand` | Project MVVM convention |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| (none new) | — | — | Phase 9 adds no dependencies |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `BoolToOnOffConverter` | Computed `IsPlayingLabel` property on `MetronomeViewModel` | Converter is reusable and matches the 4 existing converters in `AclsTracker/Converters/`; computed property requires manual `OnPropertyChanged` coupling to `IsPlaying`. **Recommend converter.** |
| Passing `access_token`/`refresh_token` via deep link (implicit flow) | PKCE + `ExchangeCodeForSession` | Implicit flow exposes tokens in the callback URL — insecure. PKCE is Supabase's recommended mobile flow. |

**Installation:** none — no new packages.

## Architecture Patterns

### Recommended Project Structure (unchanged)
```
AclsTracker/
├── Services/Auth/
│   ├── AuthService.cs             # Fix SignInWithGoogleAsync here
│   ├── SupabaseSessionHandler.cs  # OK as-is
│   └── IAuthService.cs
├── Converters/
│   └── BoolToOnOffConverter.cs    # NEW (if going converter route)
├── Controls/
│   ├── MetronomePulse.xaml        # Button sizing + ON/OFF binding
│   └── RhythmSelector.xaml        # AESP + ASISTOLIA color
├── Views/MainPage.xaml            # INICIAR/FINALIZAR color + height
├── MauiProgram.cs                 # Restore session on startup
└── App.xaml.cs                    # Already calls InitializeAsync — add RetrieveSessionAsync
```

### Pattern 1: Supabase PKCE OAuth with MAUI WebAuthenticator

**What:** Initiate PKCE flow explicitly with `RedirectTo`, open system browser via `WebAuthenticator`, parse `?code=` from the callback URL, and exchange the code for a session.

**When to use:** Any OAuth provider (Google, Apple) on mobile platforms with `supabase-csharp`.

**Example (target shape of the fixed `SignInWithGoogleAsync`):**
```csharp
// Source: https://github.com/supabase-community/gotrue-csharp README,
//         https://supabase.com/docs/guides/auth/sessions/pkce-flow

public async Task<bool> SignInWithGoogleAsync()
{
    if (DeviceInfo.Platform == DevicePlatform.WinUI)
        throw new PlatformNotSupportedException("Use email/password on Windows.");

    // 1. Initiate PKCE flow — MUST pass SignInOptions with FlowType=PKCE and RedirectTo
    var authState = await _supabase.Auth.SignIn(
        Supabase.Gotrue.Constants.Provider.Google,
        new Supabase.Gotrue.SignInOptions
        {
            FlowType = Supabase.Gotrue.Constants.OAuthFlowType.PKCE,
            RedirectTo = SupabaseConfig.RedirectUri   // "aclstracker://callback"
        });

    // 2. Open system browser; WebAuthenticator waits for the deep-link callback
    var callback = await WebAuthenticator.Default.AuthenticateAsync(
        authState.Uri,
        new Uri(SupabaseConfig.RedirectUri));

    // 3. Extract ?code=... from the callback query parameters
    //    WebAuthenticatorResult.Properties contains the query string dictionary.
    if (!callback.Properties.TryGetValue("code", out var code) || string.IsNullOrEmpty(code))
    {
        Debug.WriteLine("[AuthService] OAuth callback missing 'code' parameter");
        return false;
    }

    // 4. Exchange the code + stored PKCEVerifier for a real session
    var session = await _supabase.Auth.ExchangeCodeForSession(authState.PKCEVerifier, code);
    return session != null;
}
```

Key points:
- **`authState.PKCEVerifier` is critical** — it's generated in step 1 and *must* be passed to `ExchangeCodeForSession`. Do not discard `authState` between the browser open and the exchange call.
- **`WebAuthenticator` requires the redirect URI to be a custom scheme registered on the platform.** Already done (`aclstracker://` on Android intent filter + iOS Info.plist).
- The current code calls `SignIn(Provider.Google)` with no options and never calls `ExchangeCodeForSession` — that is why the flow never completes.

### Pattern 2: Supabase Session Restore on Startup

**What:** After configuring the persistence handler, explicitly load the stored session into the client and then validate/refresh it.

**When to use:** At app startup, once per process.

**Example (target shape for `MauiProgram.cs` + `App.xaml.cs`):**
```csharp
// Source: https://github.com/supabase-community/gotrue-csharp (README persistence example)

// MauiProgram.cs — registration unchanged except add a startup hook:
var sessionHandler = new SupabaseSessionHandler();
var supabase = new Client(SupabaseConfig.Url, SupabaseConfig.AnonKey, new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = false
});
supabase.Auth.SetPersistence(sessionHandler);

// ---- Critical: load the persisted session into memory BEFORE InitializeAsync ----
// LoadSession() is synchronous and pulls from SupabaseSessionHandler.LoadSession().
supabase.Auth.LoadSession();

builder.Services.AddSingleton(supabase);

// App.xaml.cs — after InitializeAsync, call RetrieveSessionAsync to refresh tokens
Task.Run(async () =>
{
    await _supabase.InitializeAsync();
    // Validates the loaded session and auto-refreshes the access token if expired.
    await _supabase.Auth.RetrieveSessionAsync();
});
```

Key points:
- `SetPersistence` **only registers the handler** — it does not read from storage on its own.
- `InitializeAsync` does **not** call `LoadSession`. These are independent.
- `RetrieveSessionAsync` refreshes an expired access token using the persisted refresh token. This is what gives "logged in indefinitely" behavior.
- After both calls, `_supabase.Auth.CurrentSession` is populated and the `AuthStateChanged` listener will fire, propagating `IsLoggedIn = true` through `AuthViewModel`.

### Pattern 3: MAUI BoolToOnOffConverter

**What:** Stateless `IValueConverter` returning `"ON"`/`"OFF"`.
**When to use:** Any boolean that needs a two-string UI label.

```csharp
// Source: pattern matches existing AclsTracker/Converters/BoolToActiveColorConverter.cs
using System.Globalization;

namespace AclsTracker.Converters;

public class BoolToOnOffConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "ON" : "OFF";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

Register in `App.xaml` `ResourceDictionary` (global, matches existing `IsNotNullConverter` registration), and use from `MetronomePulse.xaml`:
```xml
<Button Text="{Binding IsPlaying, Converter={StaticResource BoolToOnOffConverter}}" ... />
```

### Anti-Patterns to Avoid

- **Don't** call `SignIn(Provider.Google)` without `SignInOptions` on mobile — falls back to Supabase Site URL (localhost:3000 default).
- **Don't** assume `WebAuthenticator` auto-processes Supabase auth cookies — it only returns the callback URL. You still must call `ExchangeCodeForSession`.
- **Don't** hard-code colors in a new `Styles.xaml` ResourceDictionary for a 3-color fix — the existing project convention is inline hex per CONTEXT.md `## Established Patterns`.
- **Don't** make `IsPlayingLabel` a computed property without raising `PropertyChanged` when `IsPlaying` changes — prefer the converter.
- **Don't** set `TextColor="White"` on the AESP/Asistolia buttons after color change — fails WCAG contrast on yellow (`#FBC02D` + white ≈ 1.7:1). Use `#333333` (~10:1).

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| OAuth PKCE code/verifier generation | Manual SHA256 + base64url encoding of code verifier | `supabase-csharp` returns `ProviderAuthState.PKCEVerifier` directly from `SignIn()` | Already implemented in the SDK; re-implementing risks RFC 7636 subtle bugs |
| Parsing the OAuth callback URL query string | Regex over `callback.CallbackUri.Query` | `callback.Properties["code"]` — `WebAuthenticator` already parses query params into a dictionary | Built-in and cross-platform |
| Session encryption/storage | Custom file-based encrypted blob | `SecureStorage.Default` (already used by `SupabaseSessionHandler`) | Uses Android Keystore / iOS Keychain automatically |
| Token refresh loop | Manual timer polling `/token?grant_type=refresh_token` | `SupabaseOptions.AutoRefreshToken = true` (already set) + `RetrieveSessionAsync()` on startup | Built into `supabase-csharp` |
| ON/OFF label source-generation | Custom `[ObservableProperty]` change subscription logic | `IValueConverter` (30 lines, matches project pattern) | Reusable, stateless, project convention |

**Key insight:** Both auth bugs are **API-misuse bugs**, not missing functionality. The SDK already handles PKCE, token refresh, and persistence — the code just isn't calling the right methods in the right order.

## Common Pitfalls

### Pitfall 1: OAuth redirect to `localhost:3000`
**What goes wrong:** After Google auth completes, browser redirects to `http://localhost:3000` instead of `aclstracker://callback`, and the app never receives the callback.
**Why it happens:** (1) `SignInOptions.RedirectTo` not passed → Supabase uses the project's `Site URL`, which defaults to `http://localhost:3000`. (2) Even if passed, the URL must be in the Supabase dashboard allow-list or it's rejected.
**How to avoid:**
  1. Pass `SignInOptions { FlowType = PKCE, RedirectTo = "aclstracker://callback" }`.
  2. In Supabase Dashboard → **Authentication → URL Configuration**, add `aclstracker://callback` to **Redirect URLs** (allow-list). Optionally also set it as the **Site URL** (though keeping Site URL = hosted web URL and adding the scheme to redirect allow-list is the common pattern).
  3. Verify the Google OAuth provider in Supabase has valid client ID/secret.
**Warning signs:** Browser address bar ends at `localhost:3000`; app logs never show "OAuth callback received"; `CurrentSession` stays null.

### Pitfall 2: "Session persisted" but lost on restart
**What goes wrong:** User logs in, app shows avatar, closes app, reopens — back to login screen.
**Why it happens:** `SetPersistence` only registers the save/load/destroy callbacks. It does **not** auto-load on initialization. `InitializeAsync` does not read from the persistence handler. Need explicit `LoadSession()` + `RetrieveSessionAsync()`.
**How to avoid:** Call `supabase.Auth.LoadSession()` in `MauiProgram.cs` right after `SetPersistence`, then `await _supabase.Auth.RetrieveSessionAsync()` in `App.xaml.cs` after `InitializeAsync()`.
**Warning signs:** `SecureStorage` contains `supabase_session` key after login, but `_supabase.Auth.CurrentSession == null` on next startup.

### Pitfall 3: `ExchangeCodeForSession` throws "both auth code and code verifier should be non-empty"
**What goes wrong:** Exchange fails after OAuth callback.
**Why it happens:** The `ProviderAuthState` from `SignIn` was discarded between steps, so `PKCEVerifier` is lost. Or the `code` query parameter was not extracted from the callback URL.
**How to avoid:** Keep `authState` in the same method scope from `SignIn` through `ExchangeCodeForSession`. Use `callback.Properties["code"]`, not manual URI parsing.
**Warning signs:** `Supabase.Gotrue.Exceptions.GotrueException` with message about missing verifier or code.

### Pitfall 4: MAUI `Button.Text` bound to bool renders "True"/"False"
**What goes wrong:** `{Binding IsPlaying, StringFormat='{0}'}` shows the raw `bool.ToString()`.
**Why it happens:** `StringFormat` uses `.NET` format — booleans have no special formatter, so `"{0}"` produces `"True"`/`"False"`.
**How to avoid:** Use an `IValueConverter` or computed string property. `StringFormat` alone cannot transform booleans.

### Pitfall 5: Yellow button with white text fails contrast
**What goes wrong:** `#FBC02D` + `White` text ≈ 1.7:1 contrast — unreadable.
**Why it happens:** Yellow is near-white in luminance.
**How to avoid:** Use dark text (`#333333`) on yellow backgrounds. WCAG AA requires 4.5:1 for normal text; `#333333` on `#FBC02D` ≈ 10:1.

### Pitfall 6: Circular MAUI `Button` needs `CornerRadius = Width/2`
**What goes wrong:** Changing `WidthRequest`/`HeightRequest` from 44 to 36 without adjusting `CornerRadius=22` leaves a super-ellipse instead of a circle.
**How to avoid:** When resizing circular buttons, update `CornerRadius` to half the new size (36 → 18).

## Code Examples

Verified patterns referenced from files already in this repository or the `gotrue-csharp` README.

### Example 1: Existing converter pattern to clone
```csharp
// Source: AclsTracker/Converters/BoolToActiveColorConverter.cs
public class BoolToActiveColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b) return Color.FromArgb("#1565C0");
        return Color.FromArgb("#E0E0E0");
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```
Clone this shape for `BoolToOnOffConverter`.

### Example 2: Current broken MetronomePulse toggle line
```xml
<!-- Current — renders "True"/"False" -->
<Button x:Name="ToggleBtn"
        Text="{Binding IsPlaying, StringFormat='{0}'}"
        WidthRequest="70" HeightRequest="44"
        CornerRadius="8" ... />

<!-- After fix — target -->
<Button x:Name="ToggleBtn"
        Text="{Binding IsPlaying, Converter={StaticResource BoolToOnOffConverter}}"
        WidthRequest="56" HeightRequest="36"
        CornerRadius="8" ... />
```

### Example 3: Current metronome +/- buttons (44×44 circular)
```xml
<!-- Source: AclsTracker/Controls/MetronomePulse.xaml lines 30-54 -->
<!-- After fix: WidthRequest=36 HeightRequest=36 CornerRadius=18 FontSize=18 -->
```

### Example 4: PKCE OAuth SignIn with RedirectTo
Shown above in **Architecture Patterns → Pattern 1**.

### Example 5: Session load on startup
Shown above in **Architecture Patterns → Pattern 2**.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Supabase implicit OAuth flow (tokens in URL fragment) | PKCE flow with `ExchangeCodeForSession` | gotrue-js v2 / gotrue-csharp 4.x | Mobile apps must use PKCE; implicit flow exposes tokens in deep-link URL |
| Manual `File.ReadAllText` for session storage | `IGotrueSessionPersistence<Session>` interface + `SetPersistence` | gotrue-csharp 4.x | Already adopted in this project — just needs `LoadSession` call |

**Deprecated/outdated:**
- `StringFormat='{0}'` for bool-to-text in XAML: never worked, always use `IValueConverter`.
- Trying to handle OAuth cookies manually in `WebAuthenticator` result: use PKCE code exchange.

## Open Questions

1. **Is `aclstracker://callback` in the Supabase dashboard redirect allow-list?**
   - What we know: The custom scheme is wired on Android and iOS. The constant exists in code.
   - What's unclear: Whether the dashboard setting matches. The user/developer must check **Supabase Dashboard → Authentication → URL Configuration → Redirect URLs**.
   - Recommendation: First task in the auth plan should be a **checkpoint:human-verify** confirming the dashboard allow-list contains `aclstracker://callback`. Without this, all code fixes will still fail.

2. **Does `gotrue-csharp`'s `LoadSession()` need to be called before or after `AddStateChangedListener`?**
   - What we know: The README example calls `LoadSession` before any listeners; the listener fires after.
   - What's unclear: Whether calling `LoadSession` before `AuthService` is constructed (which wires the listener) will cause a missed `AuthStateChanged` event.
   - Recommendation: Call `LoadSession()` **after** `AuthService` is registered and instantiated, or call `RetrieveSessionAsync()` which will fire the state-changed event on success. Prefer doing the restore in `App.xaml.cs` (where `AuthService` is already resolved via DI) rather than in `MauiProgram.cs`.

3. **Is the AESP button's current color `#E65100` still visible in the final screenshot after Phase 8?**
   - What we know: CONTEXT.md explicitly pinned this as the "current" color captured pre-yellow-change.
   - Recommendation: Plan tasks for issue #5 (INICIAR/FINALIZAR color) must execute before or independently of issue #6 (AESP color change) in the same phase, since the XAML files differ (MainPage vs RhythmSelector) — no ordering constraint, both can be done in parallel or single commit.

## Validation Architecture

> `workflow.nyquist_validation` is `true` in `.planning/config.json` — section included.

### Test Framework
| Property | Value |
|----------|-------|
| Framework | **None detected** — no `*.Tests.csproj`, no xUnit/NUnit/MSTest references. Project has never used automated tests. |
| Config file | none |
| Quick run command | `dotnet build AclsTracker/AclsTracker.csproj -c Debug -f net8.0-android` (build-only smoke) |
| Full suite command | `dotnet build AclsTracker/AclsTracker.csproj -c Debug` (all target frameworks) |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| P09-AUTH-01 | Google OAuth end-to-end completes and sets `CurrentSession` | **manual-only** (requires real Google account + browser + device) | none — human verify | N/A |
| P09-AUTH-02 | Session persists across app restart | **manual-only** (requires closing/reopening app on device) | none — human verify | N/A |
| P09-UI-01 | ON/OFF label renders instead of True/False | smoke (build + visual) | `dotnet build` then run on emulator | N/A |
| P09-UI-01 | `BoolToOnOffConverter` logic | unit (if test project added) | N/A | no test project |
| P09-UI-02 | Metronome row fits without wrap | visual only | run on emulator, inspect | N/A |
| P09-UI-03 | INICIAR color `#E65100` + height 40 | visual only | run on emulator, inspect | N/A |
| P09-UI-04 | AESP/Asistolia yellow + dark text | visual only | run on emulator, inspect | N/A |

**Rationale for manual-only tests:**
- OAuth requires a real browser round-trip to Google and a real Supabase backend — not feasible in a unit test.
- Session persistence requires an OS-level app-restart cycle (`SecureStorage` is real Keystore/Keychain).
- UI XAML tweaks are visually verifiable; the project has no UI test harness (Appium/MAUI UI test).

### Sampling Rate
- **Per task commit:** `dotnet build AclsTracker/AclsTracker.csproj -c Debug -f net8.0-android` (validates XAML + C# compile)
- **Per wave merge:** `dotnet build AclsTracker/AclsTracker.csproj -c Debug` (all TFMs)
- **Phase gate:** Manual human-verify on a real Android device for OAuth + persistence, plus visual inspection for all four UI tweaks. Record outcome in `checkpoint:human-verify` task.

### Wave 0 Gaps
- [ ] No test infrastructure gaps introduced by Phase 9 — matches project baseline (zero tests). **Do NOT** add a test project for this phase; scope is bug-fix only.
- [ ] If `BoolToOnOffConverter` is added, a one-line unit test would be trivial but requires setting up a new `AclsTracker.Tests` project — **defer** unless the user asks for it.

## Sources

### Primary (HIGH confidence)
- `AclsTracker/Services/Auth/AuthService.cs` lines 133–172 — current broken `SignInWithGoogleAsync` (no `SignInOptions`, no `ExchangeCodeForSession`)
- `AclsTracker/Services/Auth/SupabaseSessionHandler.cs` — already correctly implements `IGotrueSessionPersistence<Session>`
- `AclsTracker/MauiProgram.cs` lines 57–68 — `SetPersistence` called, but `LoadSession` missing
- `AclsTracker/App.xaml.cs` lines 19–30 — calls `InitializeAsync` but not `RetrieveSessionAsync`
- `AclsTracker/Controls/MetronomePulse.xaml` lines 30–54 — metronome buttons
- `AclsTracker/Controls/RhythmSelector.xaml` lines 39–74 — AESP + ASISTOLIA buttons
- `AclsTracker/Views/MainPage.xaml` lines 27–46 — INICIAR/FINALIZAR buttons
- `AclsTracker/Converters/BoolToActiveColorConverter.cs` — pattern template for new converter
- [Supabase — Native Mobile Deep Linking](https://supabase.com/docs/guides/auth/native-mobile-deep-linking) — PKCE + `RedirectTo` + allow-list pattern
- [Supabase — PKCE flow](https://supabase.com/docs/guides/auth/sessions/pkce-flow) — `exchangeCodeForSession` behavior
- [Supabase — Redirect URLs](https://supabase.com/docs/guides/auth/redirect-urls) — dashboard allow-list semantics
- [gotrue-csharp GitHub README](https://github.com/supabase-community/gotrue-csharp) — `SetPersistence`, `LoadSession`, `RetrieveSessionAsync` pattern; `SignInOptions.FlowType = PKCE`; `ExchangeCodeForSession(verifier, code)` signature

### Secondary (MEDIUM confidence)
- [Supabase — Login with Google](https://supabase.com/docs/guides/auth/social-login/auth-google) — Google provider config
- [Supabase C# API Reference — signInWithOAuth](https://supabase.com/docs/reference/csharp/auth-signinwithoauth)

### Tertiary (LOW confidence)
- [Supabase Auth JS Issue #1026](https://github.com/supabase/auth-js/issues/1026) — "both auth code and code verifier should be non-empty" error symptom; JS-specific but same semantics apply to C# port

## Metadata

**Confidence breakdown:**
- OAuth PKCE fix: **HIGH** — supported by official Supabase docs + gotrue-csharp README + direct code inspection showing the missing `SignInOptions` and `ExchangeCodeForSession` call
- Session persistence fix: **HIGH** — supported by gotrue-csharp README showing `LoadSession`/`RetrieveSessionAsync` as separate from `SetPersistence`, and direct code inspection confirming they are not called
- UI tweaks (XAML): **HIGH** — direct code inspection, mechanical edits with measured targets
- `BoolToOnOffConverter` vs computed property: **HIGH** — project already has 4 analogous converters
- WCAG contrast for `#FBC02D` + `#333333`: **HIGH** — standard contrast math
- Supabase dashboard allow-list current state: **UNKNOWN** — cannot verify from code alone; flagged as Open Question #1 requiring human verify

**Research date:** 2026-04-09
**Valid until:** 2026-05-09 (30 days — supabase-csharp is semi-stable; dashboard UI occasionally changes)
