using Microsoft.Extensions.Options;
using MonitoringDokumenGS.Interfaces;

public class FileService : IFile
{
    private readonly string _rootPath;
    private readonly IAuditLog _audit;
    private readonly ILogger<FileService> _logger;

    public FileService(
        IOptions<FileStorageOptions> options,
        IAuditLog audit,
        ILogger<FileService> logger)
    {
        _rootPath = options.Value.RootPath;
        _audit = audit;
        _logger = logger;
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

        // Ini contoh
        _logger.LogInformation("=== UPLOAD START ===");

        _logger.LogInformation("Module: {Module}", module);
        _logger.LogInformation("ReferenceId: {ReferenceId}", referenceId);
        _logger.LogInformation("Category: {Category}", category);

        _logger.LogInformation("RootPath: {RootPath}", _rootPath);

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
