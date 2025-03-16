using System.ComponentModel.DataAnnotations;

namespace ExtensionMarket.Models;

public class Extension
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid AuthorId { get; set; }

    public User? Author { get; set; }

    public List<string> Tags { get; set; } = [];

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }

    // Navigation property
    public List<ExtensionVersion> Versions { get; set; } = [];
}