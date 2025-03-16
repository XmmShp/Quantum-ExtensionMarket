using System.ComponentModel.DataAnnotations;

namespace ExtensionMarket.Models;

public class AuditLog
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Action { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    public string Details { get; set; } = string.Empty;

    // Additional fields for tracking
    public Guid? ExtensionId { get; set; }
    public Guid? ExtensionVersionId { get; set; }
}