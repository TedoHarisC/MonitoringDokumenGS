using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IInvoiceProgressStatuses
    {
        Task<IEnumerable<InvoiceProgressStatusDto>> GetAllAsync();
        Task<PagedResponse<InvoiceProgressStatusDto>> GetPagedAsync(int page, int pageSize);
        Task<InvoiceProgressStatusDto?> GetByIdAsync(int id);
        Task<InvoiceProgressStatusDto> CreateAsync(InvoiceProgressStatusDto dto);
        Task<bool> UpdateAsync(InvoiceProgressStatusDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
