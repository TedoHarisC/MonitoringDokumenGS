namespace MonitoringDokumenGS.Interfaces
{
    public interface IFile
    {
        Task<FileUploadResult> SaveAsync(
            IFormFile file,
            string module,        // "Invoices", "Contracts"
            string category,      // "BAST", "FAKTUR", dll
            Guid referenceId,     // InvoiceId / ContractId
            Guid userId
        );
    }
}