using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AclsTracker.Models;

/// <summary>
/// User profile model matching the Supabase profiles table schema.
/// Inherits from BaseModel to work with Supabase.Client.From<TModel>().
/// </summary>
[Table("profiles")]
public class UserProfile : BaseModel
{
    /// <summary>
    /// Unique identifier (UUID from Supabase auth.users)
    /// </summary>
    [PrimaryKey("id", false)] // false = not auto-generated (we use the auth.users UUID)
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User email address (set during registration)
    /// </summary>
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User first name
    /// </summary>
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// User last name
    /// </summary>
    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// User phone number
    /// </summary>
    [Column("telefono")]
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// URL to user's avatar image (from OAuth provider or uploaded)
    /// </summary>
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Last profile update timestamp
    /// </summary>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Profile creation timestamp
    /// </summary>
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Computed full name combining Nombre and Apellido
    /// </summary>
    [JsonIgnore]
    public string FullName => $"{Nombre} {Apellido}".Trim();
}