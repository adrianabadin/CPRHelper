using AclsTracker.Models;

namespace AclsTracker.Services.Auth;

/// <summary>
/// Authentication service contract defining all auth operations supported by the app.
/// Implementations should handle session persistence and OAuth callbacks.
/// </summary>
public interface IAuthService
{
    // ============ Email/Password Authentication ============

    /// <summary>
    /// Sign in with email and password credentials.
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="password">User password</param>
    /// <returns>True if sign-in successful, false otherwise</returns>
    Task<bool> SignInWithEmailAsync(string email, string password);

    /// <summary>
    /// Register a new user with email, password, and profile data.
    /// </summary>
    /// <param name="email">User email address (used as username)</param>
    /// <param name="password">User password</param>
    /// <param name="nombre">User first name</param>
    /// <param name="apellido">User last name</param>
    /// <param name="telefono">User phone number</param>
    /// <returns>True if registration successful, false otherwise</returns>
    Task<bool> SignUpWithEmailAsync(string email, string password, string nombre, string apellido, string telefono);

    /// <summary>
    /// Send password reset email to user.
    /// </summary>
    /// <param name="email">User email address</param>
    /// <returns>True if reset email sent successfully</returns>
    Task<bool> ResetPasswordAsync(string email);

    // ============ OAuth Authentication ============

    /// <summary>
    /// Initiate Google OAuth sign-in flow.
    /// Opens system browser for authentication.
    /// </summary>
    /// <returns>True if OAuth flow completed successfully</returns>
    Task<bool> SignInWithGoogleAsync();

    /// <summary>
    /// Initiate Apple Sign-In flow.
    /// Uses native ASAuthorizationController on iOS/macOS.
    /// </summary>
    /// <returns>True if Apple Sign-In completed successfully</returns>
    Task<bool> SignInWithAppleAsync();

    // ============ Session Management ============

    /// <summary>
    /// Sign out current user and clear local session.
    /// </summary>
    Task SignOutAsync();

    /// <summary>
    /// Check if a user is currently authenticated.
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// Get current authenticated user's email, if available.
    /// </summary>
    string? CurrentUserEmail { get; }

    /// <summary>
    /// Get current authenticated user's avatar URL, if available.
    /// </summary>
    string? CurrentUserAvatarUrl { get; }

    // ============ Profile Management ============

    /// <summary>
    /// Retrieve current user's profile from the backend.
    /// </summary>
    /// <returns>UserProfile if authenticated, null otherwise</returns>
    Task<UserProfile?> GetProfileAsync();

    /// <summary>
    /// Update current user's profile data.
    /// </summary>
    /// <param name="profile">Profile data to update</param>
    /// <returns>True if update successful</returns>
    Task<bool> UpdateProfileAsync(UserProfile profile);

    /// <summary>
    /// Upload a local avatar image and associate with user profile.
    /// </summary>
    /// <param name="localFilePath">Local path to avatar image file</param>
    /// <returns>URL of uploaded avatar, or null if upload failed</returns>
    Task<string?> UploadAvatarAsync(string localFilePath);

    // ============ Events ============

    /// <summary>
    /// Event fired when authentication state changes (sign-in, sign-out, token refresh).
    /// </summary>
    event EventHandler<bool>? AuthStateChanged;
}
