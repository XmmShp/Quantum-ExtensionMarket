using System.ComponentModel.DataAnnotations;

namespace ExtensionMarket.Models;

public enum UserRole
{
    User = 1,
    Developer,
    Reviewer,
    Admin
}


public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public List<UserRole> Roles { get; set; } = [UserRole.Developer];

    // Navigation property
    public List<Extension> CreatedExtensions { get; set; } = [];

    public DateTime CreatedDate { get; set; }
    public DateTime LastLogin { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}