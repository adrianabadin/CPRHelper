using SQLite;

namespace AclsTracker.Models;

[Table("Sessions")]
public class Session
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public string PatientLastName { get; set; } = string.Empty;
    public string PatientDNI { get; set; } = string.Empty;

    public DateTime SessionStartTime { get; set; }
    public DateTime SessionEndTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
