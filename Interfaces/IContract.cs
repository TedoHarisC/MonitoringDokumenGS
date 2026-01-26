using MonitoringDokumenGS.Dtos.Transaction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IContract
    {
        Task<IEnumerable<ContractDto>> GetAllAsync();
        Task<IEnumerable<ContractDto>> GetAllByVendorAsync(Guid vendorId);
        Task<ContractDto?> GetByIdAsync(Guid id);
        Task<ContractDto> CreateAsync(ContractDto dto);
        Task<bool> UpdateAsync(ContractDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
