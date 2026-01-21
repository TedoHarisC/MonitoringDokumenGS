using Microsoft.Extensions.Options;
using MonitoringDokumenGS.Interfaces;

public class FileService : IFile
{
    private readonly string _rootPath;

    public FileService(IOptions<FileStorageOptions> options)
    {
        _rootPath = options.Value.RootPath;
    }

    public async Task<string> SaveAsync(
        IFormFile file,
        Guid vendorId,
        string module,
        Guid referenceId)
    {
        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";

        var directory = Path.Combine(
            _rootPath,
            $"vendor-{vendorId}",
            module,
            DateTime.UtcNow.Year.ToString(),
            referenceId.ToString()
        );

        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, storedFileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fullPath;
    }

    public FileStream Get(string filePath)
    {
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }
}
