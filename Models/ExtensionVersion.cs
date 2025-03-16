using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExtensionMarket.Models;

public enum ExtensionVersionStatus
{
    Pending = 1,
    Published,
    Rejected
}

public class ExtensionVersion
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ExtensionId { get; set; }

    [ForeignKey("ExtensionId")]
    public Extension? Extension { get; set; }

    [Required]
    public string VersionNumber { get; set; } = string.Empty;

    [Required]
    public string QuantumVersionSupport { get; set; } = string.Empty;

    public string ReleaseNotes { get; set; } = string.Empty;

    [Required]
    public string DownloadUrl { get; set; } = string.Empty;

    [Required]
    public ExtensionVersionStatus Status { get; set; } = ExtensionVersionStatus.Pending;

    public DateTime UploadedDate { get; set; }

    // Additional properties for tracking
    public int DownloadCount { get; set; } = 0;
}