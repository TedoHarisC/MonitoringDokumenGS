using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models.Transaction;
using Microsoft.EntityFrameworkCore;

namespace MonitoringDokumenGS.Services
{
    public class UangMukaService : IUangMuka
    {
        private readonly ApplicationDBContext _db;
        public UangMukaService(ApplicationDBContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<UangMuka>> GetAllAsync()
        {
            var uangMukas = await _db.UangMukas
                                .AsNoTracking()
                                .Include(x => x.BudgetCode)
                                .Include(x => x.RelatedUangMuka)
                                .Include(x => x.CoaText)
                                .Where(x => !x.IsDeleted)
                                .OrderByDescending(x => x.CreatedAt)
                                .ToListAsync();

            var advancedStatusIds = uangMukas
                .Where(x => x.Jenis == "Advanced")
                .Select(x => x.StatusId)
                .Distinct()
                .ToList();

            var biayaStatusIds = uangMukas
                .Where(x => x.Jenis != "Advanced")
                .Select(x => x.StatusId)
                .Distinct()
                .ToList();

            var advancedStatuses = await _db.AdvancedStatuses
                .AsNoTracking()
                .Where(x => advancedStatusIds.Contains(x.AdvancedStatusesId))
                .ToDictionaryAsync(x => x.AdvancedStatusesId, x => x.Name);

            var biayaStatuses = await _db.BiayaRealisasiStatuses
                .AsNoTracking()
                .Where(x => biayaStatusIds.Contains(x.BiayaRealisasiStatusesId))
                .ToDictionaryAsync(x => x.BiayaRealisasiStatusesId, x => x.Name);

            foreach (var item in uangMukas)
            {
                if (item.Jenis == "Advanced")
                {
                    item.Status = advancedStatuses.TryGetValue(item.StatusId, out var status) ? status : null;
                }
                else
                {
                    item.Status = biayaStatuses.TryGetValue(item.StatusId, out var status) ? status : null;
                }
            }

            return uangMukas;
        }

        public async Task<UangMuka?> GetByIdAsync(string id)
        {
            var item = await _db.UangMukas
                .AsNoTracking()
                .Include(x => x.BudgetCode)
                .Include(x => x.CoaText)
                .Include(x => x.RelatedUangMuka)
                .FirstOrDefaultAsync(x => x.UangMukaId == id && !x.IsDeleted);
            if (item == null) return null;

            // Ambil nama status
            if (item.Jenis == "Advanced")
            {
                var advStatus = await _db.AdvancedStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.AdvancedStatusesId == item.StatusId);
                item.Status = advStatus?.Name;
            }
            else
            {
                var biayaStatus = await _db.BiayaRealisasiStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.BiayaRealisasiStatusesId == item.StatusId);
                item.Status = biayaStatus?.Name;
            }
            return item;
        }

        public async Task<UangMuka> CreateAsync(UangMuka model)
        {
            model.UangMukaId = Guid.NewGuid().ToString();
            model.CreatedAt = DateTime.UtcNow;
            // Pastikan null diterima
            if (model.BudgetCodeId == Guid.Empty) model.BudgetCodeId = null;
            if (model.CoaTextId == 0) model.CoaTextId = null;
            _db.UangMukas.Add(model);
            await _db.SaveChangesAsync();
            return model;
        }

        public async Task<UangMuka> UpdateAsync(string id, UangMuka model)
        {
            var existing = await _db.UangMukas.FirstOrDefaultAsync(x => x.UangMukaId == id && !x.IsDeleted);
            if (existing == null) throw new KeyNotFoundException("Uang Muka not found");
            existing.Jenis = model.Jenis;
            existing.BudgetCodeId = (model.BudgetCodeId == Guid.Empty) ? null : model.BudgetCodeId;
            existing.CoaTextId = (model.CoaTextId == 0) ? null : model.CoaTextId;
            existing.UangMukaRelatedId = model.UangMukaRelatedId;
            existing.NoSAP = model.NoSAP;
            existing.Amount = model.Amount;
            existing.AtasNama = model.AtasNama;
            existing.Deskripsi = model.Deskripsi;
            existing.StartDate = model.StartDate;
            existing.EndDate = model.EndDate;
            existing.StatusId = model.StatusId;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = model.UpdatedBy;
            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var existing = await _db.UangMukas.FirstOrDefaultAsync(x => x.UangMukaId == id && !x.IsDeleted);
            if (existing == null) return false;
            existing.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
