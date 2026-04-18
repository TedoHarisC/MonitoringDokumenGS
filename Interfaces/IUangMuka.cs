using MonitoringDokumenGS.Models.Transaction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IUangMuka
    {
        Task<IEnumerable<UangMuka>> GetAllAsync();
        Task<UangMuka?> GetByIdAsync(string id);
        Task<UangMuka> CreateAsync(UangMuka model);
        Task<UangMuka> UpdateAsync(string id, UangMuka model);
        Task<bool> DeleteAsync(string id);
    }
}
