using ExtensionMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace ExtensionMarket.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Extension> Extensions { get; set; }
    public DbSet<ExtensionVersion> ExtensionVersions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
}