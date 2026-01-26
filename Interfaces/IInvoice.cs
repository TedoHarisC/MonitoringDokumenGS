using MonitoringDokumenGS.Dtos.Transaction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IInvoice
    {
        Task<IEnumerable<InvoiceDto>> GetAllAsync();
        Task<IEnumerable<InvoiceDto>> GetAllByVendorAsync(Guid vendorId);
        Task<InvoiceDto?> GetByIdAsync(Guid id);
        Task<InvoiceDto> CreateAsync(InvoiceDto dto);
        Task<bool> UpdateAsync(InvoiceDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
