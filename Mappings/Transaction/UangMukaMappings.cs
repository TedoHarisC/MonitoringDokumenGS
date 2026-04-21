using System;
using System.Collections.Generic;
using System.Linq;
using MonitoringDokumenGS.Dtos.Transaction;
using MonitoringDokumenGS.Models.Transaction;

namespace MonitoringDokumenGS.Mappings.Transaction
{
    public static class UangMukaMappings
    {
        public static UangMukaDto ToDto(this UangMuka entity)
        {
            return new UangMukaDto
            {
                UangMukaId = entity.UangMukaId,
                Jenis = entity.Jenis,
                BudgetCodeIds = entity.UangMukaBudgetCodes?.Select(x => x.BudgetCodeId).ToList(),
                CoaTextIds = entity.UangMukaCoaTexts?.Select(x => x.CoaTextId).ToList(),
                UangMukaRelatedId = entity.UangMukaRelatedId,
                NoSAP = entity.NoSAP,
                Amount = entity.Amount,
                AtasNama = entity.AtasNama,
                Deskripsi = entity.Deskripsi,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                StatusId = entity.StatusId,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
                IsDeleted = entity.IsDeleted,
                BudgetCodes = entity.UangMukaBudgetCodes == null
                    ? new List<Dtos.Transaction.SimpleBudgetCodeDto>()
                    : entity.UangMukaBudgetCodes
                        .Where(x => x.BudgetCode != null)
                        .Select(x => new Dtos.Transaction.SimpleBudgetCodeDto
                        {
                            BudgetCodeId = x.BudgetCode!.BudgetCodeId,
                            Code = x.BudgetCode!.Code,
                            Description = x.BudgetCode!.Description
                        }).ToList(),
                CoaTexts = entity.UangMukaCoaTexts == null
                    ? new List<Dtos.Transaction.SimpleCoaTextDto>()
                    : entity.UangMukaCoaTexts
                        .Where(x => x.CoaText != null)
                        .Select(x => new Dtos.Transaction.SimpleCoaTextDto
                        {
                            CoaTextId = x.CoaText!.VendorCategoryId,
                            Name = x.CoaText!.Name
                        }).ToList()
            };
        }

        public static void UpdateEntity(this UangMuka entity, UangMukaDto dto)
        {
            entity.Jenis = dto.Jenis ?? entity.Jenis;
            entity.UangMukaRelatedId = dto.UangMukaRelatedId;
            entity.NoSAP = dto.NoSAP;
            entity.Amount = dto.Amount;
            entity.AtasNama = dto.AtasNama ?? entity.AtasNama;
            entity.Deskripsi = dto.Deskripsi ?? entity.Deskripsi;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.StatusId = dto.StatusId ?? entity.StatusId;
            entity.UpdatedAt = DateTime.UtcNow;
            // BudgetCodeIds dan CoaTextIds dihandle di service
        }
    }
}
