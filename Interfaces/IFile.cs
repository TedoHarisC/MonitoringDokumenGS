namespace MonitoringDokumenGS.Interfaces
{
    public interface IFile
    {
        Task<string> SaveAsync(
            IFormFile file,
            Guid vendorId,
            string module,
            Guid referenceId
        );

        FileStream Get(string filePath);
    }
}