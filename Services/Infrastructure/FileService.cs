using Microsoft.Extensions.Options;
using MonitoringDokumenGS.Interfaces;

public class FileService : IFile
{
    private readonly string _rootPath;
    private readonly IAuditLog _audit;

    public FileService(
        IOptions<FileStorageOptions> options,
        IAuditLog audit)
    {
        _rootPath = options.Value.RootPath;
        _audit = audit;
    }

    public async Task<FileUploadResult> SaveAsync(
        IFormFile file,
        string module,
        string category,
        Guid referenceId,
        Guid userId)
    {
        var ext = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{ext}";

        var relativePath = Path.Combine(
            module,
            referenceId.ToString(),
            category
        );

        var fullPath = Path.Combine(_rootPath, relativePath);
        Directory.CreateDirectory(fullPath);

        var finalFile = Path.Combine(fullPath, storedName);

        using var stream = new FileStream(finalFile, FileMode.Create);
        await file.CopyToAsync(stream);

        // AUDIT LOG
        await _audit.LogAsync(
            userId.ToString(),
            "UPLOAD_FILE",
            $"{module} - {category}",
            referenceId.ToString(),
            storedName
        );

        return new FileUploadResult
        {
            OriginalName = file.FileName,
            StoredName = storedName,
            RelativePath = Path.Combine(relativePath, storedName),
            Size = file.Length
        };
    }
}
