using ExtensionMarket.Interfaces;

namespace ExtensionMarket.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _baseStoragePath;

    public FileStorageService()
    {
        // Get base storage path from Configurations
        _baseStoragePath = Configurations.FileStorageBasePath;

        // Ensure the base directory exists
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
        }
    }

    public async Task<string> SaveExtensionFileAsync(Guid extensionId, string versionNumber, IFormFile file)
    {
        // Create directory for extension if it doesn't exist
        var extensionPath = Path.Combine(_baseStoragePath, extensionId.ToString());
        if (!Directory.Exists(extensionPath))
        {
            Directory.CreateDirectory(extensionPath);
        }

        // Save file with version number as filename
        var filePath = Path.Combine(extensionPath, $"{versionNumber}.zip");

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return filePath;
    }

    public async Task<(byte[] FileContent, string ContentType, string FileName)> GetExtensionFileAsync(Guid extensionId, string versionNumber)
    {
        var filePath = Path.Combine(_baseStoragePath, extensionId.ToString(), $"{versionNumber}.zip");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Extension file for version {versionNumber} not found.");
        }

        var fileContent = await File.ReadAllBytesAsync(filePath);
        return (fileContent, "application/zip", $"{extensionId}_{versionNumber}.zip");
    }

    public Task<bool> DeleteExtensionFileAsync(Guid extensionId, string versionNumber)
    {
        var filePath = Path.Combine(_baseStoragePath, extensionId.ToString(), $"{versionNumber}.zip");

        if (!File.Exists(filePath))
        {
            return Task.FromResult(false);
        }

        File.Delete(filePath);

        // Check if directory is empty and delete it if it is
        var extensionPath = Path.Combine(_baseStoragePath, extensionId.ToString());
        if (Directory.Exists(extensionPath) && !Directory.EnumerateFileSystemEntries(extensionPath).Any())
        {
            Directory.Delete(extensionPath);
        }

        return Task.FromResult(true);
    }

    public Task<long> GetFileSizeAsync(Guid extensionId, string versionNumber)
    {
        var filePath = Path.Combine(_baseStoragePath, extensionId.ToString(), $"{versionNumber}.zip");

        if (!File.Exists(filePath))
        {
            return Task.FromResult(0L);
        }

        var fileInfo = new FileInfo(filePath);
        return Task.FromResult(fileInfo.Length);
    }
}