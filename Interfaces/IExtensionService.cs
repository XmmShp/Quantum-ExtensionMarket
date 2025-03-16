using ExtensionMarket.Models;

namespace ExtensionMarket.Interfaces;

public interface IExtensionService
{
    Task<IEnumerable<Extension>> GetAllExtensionsAsync();
    Task<Extension?> GetExtensionByIdAsync(Guid id);
    Task<IEnumerable<Extension>> GetExtensionsByAuthorIdAsync(Guid authorId);
    Task<IEnumerable<Extension>> SearchExtensionsAsync(string searchTerm, List<string>? tags = null);
    Task<Extension> CreateExtensionAsync(string name, string description, Guid authorId, List<string> tags);
    Task<Extension> UpdateExtensionAsync(Guid id, string name, string description, List<string> tags);
    Task<bool> DeleteExtensionAsync(Guid id);
    Task<ExtensionVersion> AddExtensionVersionAsync(Guid extensionId, string versionNumber, string quantumVersionSupport, string releaseNotes, IFormFile extensionFile);
    Task<IEnumerable<ExtensionVersion>> GetExtensionVersionsAsync(Guid extensionId);
    Task<bool> IsCompatibleWithQuantumVersionAsync(Guid extensionId, string quantumVersion);
    Task<bool> IncrementDownloadCountAsync(Guid versionId);
    Task<ExtensionVersion?> UpdateExtensionVersionStatusAsync(Guid versionId, ExtensionVersionStatus status);
    Task<ExtensionVersion?> GetExtensionVersionByIdAsync(Guid versionId);
}