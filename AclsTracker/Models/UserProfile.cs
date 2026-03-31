using Supabase.Postgrest.Models;

namespace AclsTracker.Models;

/// <summary>
/// User profile model matching the Supabase profiles table schema.
/// Inherits from BaseModel to work with Supabase.Client.From&lt;TModel&gt;().
/// </summary>
public class UserProfile : BaseModel
{
    /// <summary>
    /// Unique identifier (UUID from Supabase auth.users)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User email address (set during registration)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User first name
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// User last name
    /// </summary>
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// User phone number
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// URL to user's avatar image (from OAuth provider or uploaded)
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Last profile update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Profile creation timestamp
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Computed full name combining Nombre and Apellido
    /// </summary>
    public string FullName => $"{Nombre} {Apellido}".Trim();
}