using MonitoringDokumenGS.Dtos.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IInvoiceProgressStatuses
    {
        Task<IEnumerable<InvoiceProgressStatusDto>> GetAllAsync();
        Task<InvoiceProgressStatusDto?> GetByIdAsync(int id);
    }
}
