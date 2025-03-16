namespace ExtensionMarket.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveExtensionFileAsync(Guid extensionId, string versionNumber, IFormFile file);
    Task<(byte[] FileContent, string ContentType, string FileName)> GetExtensionFileAsync(Guid extensionId, string versionNumber);
    Task<bool> DeleteExtensionFileAsync(Guid extensionId, string versionNumber);
    Task<long> GetFileSizeAsync(Guid extensionId, string versionNumber);
}