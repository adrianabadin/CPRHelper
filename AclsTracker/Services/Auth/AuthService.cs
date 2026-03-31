using System.Diagnostics;
using AclsTracker.Constants;
using AclsTracker.Models;
using Microsoft.Maui;
using Supabase;
using Session = Supabase.Gotrue.Session;
using User = Supabase.Gotrue.User;
using ProviderAuthState = Supabase.Gotrue.ProviderAuthState;

namespace AclsTracker.Services.Auth;

/// <summary>
/// Supabase-based authentication service implementation.
/// Handles email/password, Google OAuth, Apple Sign-In, and profile management.
/// </summary>
public class AuthService : IAuthService
{
    private readonly Client _supabase;
    
    /// <summary>
    /// Event fired when authentication state changes (sign-in, sign-out, token refresh).
    /// </summary>
    public event EventHandler<bool>? AuthStateChanged;

    /// <summary>
    /// Creates a new AuthService instance.
    /// </summary>
    /// <param name="supabase">Supabase client instance (registered as singleton)</param>
    public AuthService(Client supabase)
    {
        _supabase = supabase;
        
        // Subscribe to auth state changes - the delegate signature is inferred
        _supabase.Auth.AddStateChangedListener((user, session) =>
        {
            var isLoggedIn = session != null;
            Debug.WriteLine($"[AuthService] Auth state changed, IsLoggedIn: {isLoggedIn}");
            AuthStateChanged?.Invoke(this, isLoggedIn);
        });
    }

    // ============ Email/Password Authentication ============

    /// <inheritdoc />
    public async Task<bool> SignInWithEmailAsync(string email, string password)
    {
        try
        {
            Debug.WriteLine($"[AuthService] SignInWithEmailAsync: {email}");
            var session = await _supabase.Auth.SignIn(email, password);
            var success = session != null;
            Debug.WriteLine($"[AuthService] SignInWithEmailAsync result: success={success}");
            return success;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthService] SignInWithEmailAsync failed: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SignUpWithEmailAsync(string email, string password, string nombre, string apellido, string telefono)
    {
        try
        {
            Debug.WriteLine($"[AuthService] SignUpWithEmailAsync: {email}");

            // Create auth user
            var session = await _supabase.Auth.SignUp(email, password);

            if (session?.User == null)
            {
                // Email verification may be required - profile data will be stored temporarily
                Debug.WriteLine("[AuthService] SignUpWithEmailAsync: session is null, email verification may be pending");

                // Store profile data in Preferences for later upsert after verification
                Preferences.Set("pending_profile_email", email);
                Preferences.Set("pending_profile_nombre", nombre);
                Preferences.Set("pending_profile_apellido", apellido);
                Preferences.Set("pending_profile_telefono", telefono);

                return true; // Sign up initiated, verification pending
            }

            // Profile base row is auto-created by DB trigger (handle_new_user).
            // Update with additional fields (nombre, apellido, telefono).
            Debug.WriteLine($"[AuthService] SignUpWithEmailAsync: user created {session.User.Id}, updating profile");
            await _supabase.From<UserProfile>()
                .Where(x => x.Id == session.User.Id)
                .Set(x => x.Nombre, nombre)
                .Set(x => x.Apellido, apellido)
                .Set(x => x.Telefono, telefono)
                .Update();
            Debug.WriteLine($"[AuthService] SignUpWithEmailAsync: profile updated for user {session.User.Id}");

            return true;
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            Debug.WriteLine($"[AuthService] SignUpWithEmailAsync GotrueException: code={ex.StatusCode} message={ex.Message}");
            // Rethrow Gotrue exceptions so the ViewModel can surface specific error messages
            // (e.g. 429 rate limit, 422 invalid email, 400 user already registered)
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthService] SignUpWithEmailAsync failed: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ResetPasswordAsync(string email)
    {
        try
        {
            Debug.WriteLine($"[AuthService] ResetPasswordAsync: {email}");
            await _supabase.Auth.ResetPasswordForEmail(email);
            Debug.WriteLine("[AuthService] ResetPasswordAsync: success");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthService] ResetPasswordAsync failed: {ex.Message}");
            return false;
        }
    }

    // ============ OAuth Authentication ============

    /// <inheritdoc />
    public async Task<bool> SignInWithGoogleAsync()
    {
        try
        {
            // WebAuthenticator is not supported on Windows
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                Debug.WriteLine("[AuthService] SignInWithGoogleAsync: not supported on Windows");
                throw new PlatformNotSupportedException("WebAuthenticator is not supported on Windows. Please use email/password authentication.");
            }

            Debug.WriteLine("[AuthService] SignInWithGoogleAsync: initiating OAuth PKCE flow");
            
            // Get the OAuth sign-in state containing the URL
            var authState = await _supabase.Auth.SignIn(Supabase.Gotrue.Constants.Provider.Google);
            
            // Extract the URL from the ProviderAuthState
            var signInUrl = authState.Uri.ToString();
            
            Debug.WriteLine($"[AuthService] SignInWithGoogleAsync: opening browser");
            
            // Open system browser for authentication
            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(signInUrl),
                new Uri(SupabaseConfig.RedirectUri));
            
            Debug.WriteLine("[AuthService] SignInWithGoogleAsync: OAuth callback received");
            
            // After callback, the Supabase client should have processed the session
            var hasSession = _supabase.Auth.CurrentSession != null;
            Debug.WriteLine($"[AuthService] SignInWithGoogleAsync: session established={hasSession}");
            
            return hasSession;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthService] SignInWithGoogleAsync failed: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SignInWithAppleAsync()
    {
        try
        {
            Debug.WriteLine("[AuthService] SignInWithAppleAsync: starting");
            
            // Check if iOS 13+ for native Apple Sign-In
            var isIOS = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Version.Major >= 13;
            
            if (isIOS)
            {
                Debug.WriteLine("[AuthService] SignInWithAppleAsync: using native iOS Apple Sign-In");
                
                // Use native Apple Sign-In authenticator on iOS
                var result = await AppleSignInAuthenticator.AuthenticateAsync();
                
                if (result != null && !string.IsNullOrEmpty(result.IdToken))
                {
                    Debug.WriteLine("[AuthService] SignInWithAppleAsync: native auth succeeded");
                    
                    // Note: Apple Sign-In with Supabase requires proper configuration
                    Debug.WriteLine("[AuthService] SignInWithAppleAsync: Apple ID token received, but Supabase token exchange not implemented");
                    return false;
                }
                
                Debug.WriteLine("[AuthService] SignInWithAppleAsync: native auth returned null result");
                return false;
            }
            else
            {
                // On Android or older iOS, use OAuth PKCE flow
                Debug.WriteLine("[AuthService] SignInWithAppleAsync: using OAuth PKCE flow");
                
                var authState = await _supabase.Auth.SignIn(Supabase.Gotrue.Constants.Provider.Apple);
                var signInUrl = authState.Uri.ToString();
                
                var result = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(signInUrl),
                    new Uri(SupabaseConfig.RedirectUri));
                
                return _supabase.Auth.CurrentSession != null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthService] SignInWithAppleAsync failed: {ex.Message}");
            return false;
        }
    }

    // ============ Session Management ============

    /// <inheritdoc />
    public async Task SignOutAsync()
    {
        try
        {
            Debug.WriteLine("[AuthService] SignOutAsync");
            await _supabase.Auth.SignOut();
            Debug.WriteLine("[AuthService] SignOutAsync: signed out successfully");
            // Explicitly fire the event — Supabase's state-change listener is not reliable on SignOut.
            AuthStateChanged?.Invoke(this, false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthService] SignOutAsync failed: {ex.Message}");
            AuthStateChanged?.Invoke(this, false);
        }
    }

    /// <inheritdoc />
    public bool IsLoggedIn => _supabase.Auth.CurrentSession != null;

    /// <inheritdoc />
    public string? CurrentUserEmail => _supabase.Auth.CurrentUser?.Email;

    /// <inheritdoc />
    public string? CurrentUserId => _supabase.Auth.CurrentUser?.Id;

    /// <inheritdoc />
    public string? CurrentUserAvatarUrl
    {
        get
        {
            // First try from user metadata (OAuth providers)
            if (_supabase.Auth.CurrentUser?.UserMetadata != null)
            {
                var metadata = _supabase.Auth.CurrentUser.UserMetadata;
                if (metadata.TryGetValue("avatar_url", out var avatar) && avatar != null)
                {
                    return avatar.ToString();
                }
            }
            
            // Fall back to cached avatar
            return _cachedAvatarUrl;
        }
    }
    
    private string? _cachedAvatarUrl;

    // ============ Profile Management ============

    /// <inheritdoc />
    public async Task<UserProfile?> GetProfileAsync()
    {
        try
        {
            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId))
            {
                Debug.WriteLine("[AuthService] GetProfileAsync: no current user");
                return null;
            }

            Debug.WriteLine($"[AuthService] GetProfileAsync: fetching profile for user {userId}");
            
            var result = await _supabase.From<UserProfile>()
                .Where(x => x.Id == userId)
                .Get();
            
            var profile = result.Models.FirstOrDefault();
            
            if (profile != null)
            {
                _cachedAvatarUrl = profile.AvatarUrl;
            }
            
            return profile;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthService] GetProfileAsync failed: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateProfileAsync(UserProfile profile)
    {
        try
        {
            Debug.WriteLine($"[AuthService] UpdateProfileAsync: updating profile for {profile.Id}");
            
            // Ensure the profile ID matches the current user
            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId) || profile.Id != userId)
            {
                profile.Id = userId ?? profile.Id;
            }
            
            await _supabase.From<UserProfile>().Upsert(profile);
            
            // Update cached avatar
            _cachedAvatarUrl = profile.AvatarUrl;
            
            Debug.WriteLine("[AuthService] UpdateProfileAsync: success");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthService] UpdateProfileAsync failed: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<string?> UploadAvatarAsync(string localFilePath)
    {
        try
        {
            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId))
            {
                Debug.WriteLine("[AuthService] UploadAvatarAsync: no current user");
                return null;
            }

            Debug.WriteLine($"[AuthService] UploadAvatarAsync: uploading avatar for user {userId}");

            // Read file bytes
            var fileBytes = await File.ReadAllBytesAsync(localFilePath);
            var fileName = $"{userId}/avatar.jpg";
            
            var storage = _supabase.Storage.From("avatars");

            // Upload with upsert=true so re-uploading overwrites the existing file instead of
            // throwing SupabaseStorageException "The resource already exists".
            await storage.Upload(fileBytes, fileName, new Supabase.Storage.FileOptions { Upsert = true });

            // Get public URL with a cache-busting timestamp query parameter so MAUI's Image
            // control does not display the previously-cached version after a re-upload.
            var baseUrl = storage.GetPublicUrl(fileName);
            var publicUrl = $"{baseUrl}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            
            Debug.WriteLine($"[AuthService] UploadAvatarAsync: uploaded to {publicUrl}");
            
            // Update profile with avatar URL
            var profile = await GetProfileAsync();
            if (profile != null)
            {
                profile.AvatarUrl = publicUrl;
                await UpdateProfileAsync(profile);
            }
            
            _cachedAvatarUrl = publicUrl;
            
            return publicUrl;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthService] UploadAvatarAsync failed: {ex.Message}");
            return null;
        }
    }
}