using ExtensionMarket.Data;
using ExtensionMarket.Interfaces;
using ExtensionMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace ExtensionMarket.Services;

public class ExtensionService(ApplicationDbContext context, IFileStorageService fileStorageService, IAuditLogService auditLogService) : IExtensionService
{
    public async Task<IEnumerable<Extension>> GetAllExtensionsAsync()
    {
        return await context.Extensions
            .Include(e => e.Author)
            .Include(e => e.Versions.OrderByDescending(v => v.UploadedDate).Take(1))
            .ToListAsync();
    }

    public async Task<Extension?> GetExtensionByIdAsync(Guid id)
    {
        var extension = await context.Extensions
            .Include(e => e.Author)
            .Include(e => e.Versions.OrderByDescending(v => v.UploadedDate))
            .FirstOrDefaultAsync(e => e.Id == id);

        return extension; // Explicitly return the nullable extension
    }

    public async Task<IEnumerable<Extension>> GetExtensionsByAuthorIdAsync(Guid authorId)
    {
        return await context.Extensions
            .Include(e => e.Author)
            .Include(e => e.Versions.OrderByDescending(v => v.UploadedDate).Take(1))
            .Where(e => e.AuthorId == authorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Extension>> SearchExtensionsAsync(string searchTerm, List<string>? tags = null)
    {
        var query = context.Extensions
            .Include(e => e.Author)
            .Include(e => e.Versions.OrderByDescending(v => v.UploadedDate).Take(1))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(e =>
                e.Name.Contains(searchTerm) ||
                e.Description.Contains(searchTerm)
            );
        }

        if (tags != null && tags.Count != 0)
        {
            query = query.Where(e => e.Tags.Any(tags.Contains));
        }

        return await query.ToListAsync();
    }

    public async Task<Extension> CreateExtensionAsync(
        string name,
        string description,
        Guid authorId,
        List<string> tags)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Create extension entity
            var extension = new Extension
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                AuthorId = authorId,
                Tags = tags,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            // Add extension to database
            await context.Extensions.AddAsync(extension);
            await context.SaveChangesAsync();

            // Log extension creation
            await auditLogService.CreateAuditLogAsync(
                "ExtensionCreated",
                extension.AuthorId,
                $"Extension '{extension.Name}' created.",
                extension.Id
            );

            await transaction.CommitAsync();

            // Return extension with version included
            var createdExtension = await GetExtensionByIdAsync(extension.Id);
            return createdExtension!; // We know this exists since we just created it
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Extension> UpdateExtensionAsync(Guid id, string name, string description, List<string> tags)
    {
        var existingExtension = await context.Extensions.FindAsync(id);
        if (existingExtension == null)
        {
            throw new KeyNotFoundException($"Extension with ID {id} not found.");
        }

        // Update extension properties
        existingExtension.Name = name;
        existingExtension.Description = description;
        existingExtension.Tags = tags;
        existingExtension.LastUpdated = DateTime.UtcNow;

        // Save changes
        context.Extensions.Update(existingExtension);
        await context.SaveChangesAsync();

        // Log extension update
        await auditLogService.CreateAuditLogAsync(
            "ExtensionUpdated",
            existingExtension.AuthorId,
            $"Extension '{existingExtension.Name}' updated.",
            existingExtension.Id
        );

        var updatedExtension = await GetExtensionByIdAsync(existingExtension.Id);
        return updatedExtension!; // We know this exists since we just updated it
    }

    public async Task<bool> DeleteExtensionAsync(Guid id)
    {
        var extension = await context.Extensions
            .Include(e => e.Versions)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (extension == null)
        {
            return false;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Delete all version files
            foreach (var version in extension.Versions)
            {
                await fileStorageService.DeleteExtensionFileAsync(extension.Id, version.VersionNumber);
            }

            // Log extension deletion before actually deleting
            var extensionName = extension.Name;
            var authorId = extension.AuthorId;

            // Delete extension and its versions (cascade delete)
            context.Extensions.Remove(extension);
            await context.SaveChangesAsync();

            // Log extension deletion
            await auditLogService.CreateAuditLogAsync(
                "ExtensionDeleted",
                authorId,
                $"Extension '{extensionName}' deleted.",
                id
            );

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ExtensionVersion> AddExtensionVersionAsync(
        Guid extensionId,
        string versionNumber,
        string quantumVersionSupport,
        string releaseNotes,
        IFormFile extensionFile)
    {
        var extension = await context.Extensions.FindAsync(extensionId);
        if (extension == null)
        {
            throw new KeyNotFoundException($"Extension with ID {extensionId} not found.");
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Create version entity
            var version = new ExtensionVersion
            {
                Id = Guid.NewGuid(),
                ExtensionId = extensionId,
                VersionNumber = versionNumber,
                QuantumVersionSupport = quantumVersionSupport,
                ReleaseNotes = releaseNotes,
                Status = ExtensionVersionStatus.Pending, // Initial status is pending
                UploadedDate = DateTime.UtcNow
            };

            // Save extension file
            _ = await fileStorageService.SaveExtensionFileAsync(extensionId, versionNumber, extensionFile);
            version.DownloadUrl = $"/extensions/{extensionId}/versions/{versionNumber}/download";

            // Add version to database
            await context.ExtensionVersions.AddAsync(version);
            await context.SaveChangesAsync();

            // Update extension's last updated date
            extension.LastUpdated = DateTime.UtcNow;
            context.Extensions.Update(extension);
            await context.SaveChangesAsync();

            // Log version creation
            await auditLogService.CreateAuditLogAsync(
                "ExtensionVersionCreated",
                extension.AuthorId,
                $"Version {versionNumber} created for extension '{extension.Name}'.",
                extensionId,
                version.Id
            );

            await transaction.CommitAsync();
            return version;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<ExtensionVersion>> GetExtensionVersionsAsync(Guid extensionId)
    {
        return await context.ExtensionVersions
            .Where(v => v.ExtensionId == extensionId)
            .OrderByDescending(v => v.UploadedDate)
            .ToListAsync();
    }

    public async Task<bool> IsCompatibleWithQuantumVersionAsync(Guid extensionId, string quantumVersion)
    {
        // Get the latest published version of the extension
        var latestVersion = await context.ExtensionVersions
            .Where(v => v.ExtensionId == extensionId && v.Status == ExtensionVersionStatus.Published)
            .OrderByDescending(v => v.UploadedDate)
            .FirstOrDefaultAsync();

        return latestVersion != null &&
               // Parse version range and check compatibility
               // This is a simplified version check - in a real application, you would use a proper version range parser
               IsVersionInRange(quantumVersion, latestVersion.QuantumVersionSupport);
    }

    public async Task<bool> IncrementDownloadCountAsync(Guid versionId)
    {
        var version = await context.ExtensionVersions
            .Include(v => v.Extension)
            .FirstOrDefaultAsync(v => v.Id == versionId);

        if (version == null)
        {
            return false;
        }

        // Increment download count
        version.DownloadCount++;

        // Save changes
        await context.SaveChangesAsync();

        // Log download
        await auditLogService.CreateAuditLogAsync(
            "ExtensionDownloaded",
            Guid.Empty, // Anonymous download
            $"Version {version.VersionNumber} of extension '{version.Extension?.Name}' downloaded.",
            version.ExtensionId,
            version.Id
        );

        return true;
    }

    public async Task<ExtensionVersion?> GetExtensionVersionByIdAsync(Guid versionId)
    {
        return await context.ExtensionVersions
            .Include(v => v.Extension)
            .FirstOrDefaultAsync(v => v.Id == versionId);
    }

    public async Task<ExtensionVersion?> UpdateExtensionVersionStatusAsync(Guid versionId, ExtensionVersionStatus status)
    {
        var version = await context.ExtensionVersions
            .Include(v => v.Extension)
            .FirstOrDefaultAsync(v => v.Id == versionId);

        if (version == null)
        {
            return null;
        }

        // Update the status
        version.Status = status;

        // Log the status change
        await auditLogService.CreateAuditLogAsync(
            "UpdateExtensionVersionStatus",
            Guid.Parse(version.Extension?.AuthorId.ToString() ?? Guid.Empty.ToString()),
            $"Updated extension version status to {status}",
            version.ExtensionId,
            version.Id);

        await context.SaveChangesAsync();
        return version;
    }

    // Helper method to check if a version is in range
    private static bool IsVersionInRange(string version, string versionRange)
    {
        // This is a simplified version check - in a real application, you would use a proper version range parser
        // For example: "1.0.0-2.0.0" means compatible with versions between 1.0.0 and 2.0.0

        // For this example, we'll just check if the version is contained in the range string
        // A real implementation would parse the range and do proper semantic version comparison
        return versionRange.Contains(version);
    }
}