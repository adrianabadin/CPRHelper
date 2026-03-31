using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AclsTracker.Models;

/// <summary>
/// Supabase Postgrest model mapping to the remote sessions table.
/// Used for upload/download operations in the sync service.
/// </summary>
[Table("sessions")]
public class SessionSupabase : BaseModel
{
    /// <summary>Unique session identifier (UUID).</summary>
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    /// <summary>ID of the authenticated user who owns this session.</summary>
    [Column("user_id")]
    public string? UserId { get; set; }

    /// <summary>Patient first name.</summary>
    [Column("patient_name")]
    public string PatientName { get; set; } = string.Empty;

    /// <summary>Patient last name.</summary>
    [Column("patient_last_name")]
    public string PatientLastName { get; set; } = string.Empty;

    /// <summary>Patient national ID.</summary>
    [Column("patient_dni")]
    public string PatientDNI { get; set; } = string.Empty;

    /// <summary>When the ACLS session started.</summary>
    [Column("session_start_time")]
    public DateTime SessionStartTime { get; set; }

    /// <summary>When the ACLS session ended.</summary>
    [Column("session_end_time")]
    public DateTime SessionEndTime { get; set; }

    /// <summary>When the record was created locally.</summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
