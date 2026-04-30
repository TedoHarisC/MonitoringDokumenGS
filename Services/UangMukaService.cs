using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models.Transaction;
using Microsoft.EntityFrameworkCore;

namespace MonitoringDokumenGS.Services
{
    public class UangMukaService : IUangMuka
    {
        private readonly ApplicationDBContext _db;
        private readonly IAuditLog _auditLog;
        public UangMukaService(ApplicationDBContext db, IAuditLog auditLog)
        {
            _db = db;
            _auditLog = auditLog;
        }

        public async Task<IEnumerable<UangMuka>> GetAllAsync()
        {
            var uangMukas = await _db.UangMukas
                .AsNoTracking()
                .Include(x => x.UangMukaBudgetCodes).ThenInclude(bc => bc.BudgetCode)
                .Include(x => x.UangMukaCoaTexts).ThenInclude(ct => ct.CoaText)
                .Include(x => x.RelatedUangMuka)
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
                .Include(x => x.UangMukaBudgetCodes).ThenInclude(bc => bc.BudgetCode)
                .Include(x => x.UangMukaCoaTexts).ThenInclude(ct => ct.CoaText)
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

        // public async Task<UangMuka> CreateAsync(UangMuka model)
        // {
        //     model.UangMukaId = Guid.NewGuid().ToString();
        //     model.CreatedAt = DateTime.UtcNow;
        //     _db.UangMukas.Add(model);
        //     await _db.SaveChangesAsync();
        //     // Insert ke junction table jika ada
        //     if (model.UangMukaBudgetCodes != null && model.UangMukaBudgetCodes.Any())
        //     {
        //         foreach (var bc in model.UangMukaBudgetCodes)
        //         {
        //             bc.UangMukaId = model.UangMukaId;
        //             _db.UangMukaBudgetCode.Add(bc);
        //         }
        //     }
        //     if (model.UangMukaCoaTexts != null && model.UangMukaCoaTexts.Any())
        //     {
        //         foreach (var ct in model.UangMukaCoaTexts)
        //         {
        //             ct.UangMukaId = model.UangMukaId;
        //             _db.UangMukaCoaText.Add(ct);
        //         }
        //     }
        //     await _db.SaveChangesAsync();

        //     // Audit log
        //     await _auditLog.LogAsync(
        //         "UangMuka",
        //         "Create",
        //         null,
        //         model,
        //         model.UangMukaId
        //     );

        //     return model;
        // }

        public async Task<UangMuka> CreateAsync(UangMuka model)
        {
            model.UangMukaId = Guid.NewGuid().ToString();
            model.CreatedAt = DateTime.UtcNow;

            // set FK ke child (tanpa Add manual)
            if (model.UangMukaBudgetCodes != null)
            {
                foreach (var bc in model.UangMukaBudgetCodes)
                {
                    bc.UangMukaId = model.UangMukaId;
                }
            }

            if (model.UangMukaCoaTexts != null)
            {
                foreach (var ct in model.UangMukaCoaTexts)
                {
                    ct.UangMukaId = model.UangMukaId;
                }
            }

            _db.UangMukas.Add(model);
            await _db.SaveChangesAsync();

            await _auditLog.LogAsync(
                "UangMuka",
                "Create",
                null,
                model,
                model.UangMukaId
            );

            return model;
        }

        public async Task<UangMuka> UpdateAsync(string id, UangMuka model)
        {
            var existing = await _db.UangMukas
                .Include(x => x.UangMukaBudgetCodes).ThenInclude(bc => bc.BudgetCode)
                .Include(x => x.UangMukaCoaTexts).ThenInclude(ct => ct.CoaText)
                .FirstOrDefaultAsync(x => x.UangMukaId == id && !x.IsDeleted);
            if (existing == null) throw new KeyNotFoundException("Uang Muka not found");

            var old = new UangMuka
            {
                UangMukaId = existing.UangMukaId,
                Jenis = existing.Jenis,
                UangMukaRelatedId = existing.UangMukaRelatedId,
                NoSAP = existing.NoSAP,
                Amount = existing.Amount,
                AtasNama = existing.AtasNama,
                Deskripsi = existing.Deskripsi,
                StartDate = existing.StartDate,
                EndDate = existing.EndDate,
                StatusId = existing.StatusId,
                UpdatedAt = existing.UpdatedAt,
                UpdatedBy = existing.UpdatedBy,
                IsDeleted = existing.IsDeleted,
                UangMukaBudgetCodes = existing.UangMukaBudgetCodes?.ToList() ?? new List<UangMukaBudgetCode>(),
                UangMukaCoaTexts = existing.UangMukaCoaTexts?.ToList() ?? new List<UangMukaCoaText>()
            };

            existing.Jenis = model.Jenis;
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
            // Update junction table
            // Remove old
            _db.UangMukaBudgetCode.RemoveRange(existing.UangMukaBudgetCodes ?? new List<UangMukaBudgetCode>());
            _db.UangMukaCoaText.RemoveRange(existing.UangMukaCoaTexts ?? new List<UangMukaCoaText>());
            // Add new
            if (model.UangMukaBudgetCodes != null && model.UangMukaBudgetCodes.Any())
            {
                foreach (var bc in model.UangMukaBudgetCodes)
                {
                    bc.UangMukaId = existing.UangMukaId;
                    _db.UangMukaBudgetCode.Add(bc);
                }
            }
            if (model.UangMukaCoaTexts != null && model.UangMukaCoaTexts.Any())
            {
                foreach (var ct in model.UangMukaCoaTexts)
                {
                    ct.UangMukaId = existing.UangMukaId;
                    _db.UangMukaCoaText.Add(ct);
                }
            }
            await _db.SaveChangesAsync();

            // Audit log
            await _auditLog.LogAsync(
                "UangMuka",
                "Update",
                old,
                existing,
                existing.UangMukaId
            );

            return existing;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var existing = await _db.UangMukas
                .Include(x => x.UangMukaBudgetCodes).ThenInclude(bc => bc.BudgetCode)
                .Include(x => x.UangMukaCoaTexts).ThenInclude(ct => ct.CoaText)
                .FirstOrDefaultAsync(x => x.UangMukaId == id && !x.IsDeleted);
            if (existing == null) return false;

            var old = new UangMuka
            {
                UangMukaId = existing.UangMukaId,
                Jenis = existing.Jenis,
                UangMukaRelatedId = existing.UangMukaRelatedId,
                NoSAP = existing.NoSAP,
                Amount = existing.Amount,
                AtasNama = existing.AtasNama,
                Deskripsi = existing.Deskripsi,
                StartDate = existing.StartDate,
                EndDate = existing.EndDate,
                StatusId = existing.StatusId,
                UpdatedAt = existing.UpdatedAt,
                UpdatedBy = existing.UpdatedBy,
                IsDeleted = existing.IsDeleted,
                UangMukaBudgetCodes = existing.UangMukaBudgetCodes?.ToList() ?? new List<UangMukaBudgetCode>(),
                UangMukaCoaTexts = existing.UangMukaCoaTexts?.ToList() ?? new List<UangMukaCoaText>()
            };

            // Remove related junctions
            _db.UangMukaBudgetCode.RemoveRange(existing.UangMukaBudgetCodes ?? new List<UangMukaBudgetCode>());
            _db.UangMukaCoaText.RemoveRange(existing.UangMukaCoaTexts ?? new List<UangMukaCoaText>());
            existing.IsDeleted = true;
            await _db.SaveChangesAsync();

            // Audit log
            await _auditLog.LogAsync(
                "UangMuka",
                "Delete",
                old,
                null,
                existing.UangMukaId
            );

            return true;
        }
    }
}
