using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Transaction;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Mappings.Transaction;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Services.Transaction
{
    public class ContractService : IContract
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;

        public ContractService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<ContractDto>> GetAllAsync()
        {
            return await _context.Contracts
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(ContractMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= GET BY ID =========================
        public async Task<ContractDto?> GetByIdAsync(Guid id)
        {
            return await _context.Contracts
                .AsNoTracking()
                .Where(x => x.ContractId == id && !x.IsDeleted)
                .Select(ContractMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<ContractDto> CreateAsync(ContractDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = new Contract
            {
                ContractId = dto.ContractId == Guid.Empty ? Guid.NewGuid() : dto.ContractId,
                VendorId = dto.VendorId,
                CreatedByUserId = dto.CreatedByUserId,
                ContractNumber = dto.ContractNumber,
                ContractDescription = dto.ContractDescription,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ApprovalStatusId = dto.ApprovalStatusId,
                ContractStatusId = dto.ContractStatusId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
                UpdatedAt = null,
                UpdatedBy = null,
                IsDeleted = false
            };

            _context.Contracts.Add(entity);
            await _context.SaveChangesAsync();

            var result = entity.ToDto();

            await _auditLog.LogAsync(
                "Contract",
                "Create",
                null,
                result,
                entity.ContractId.ToString()
            );

            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(ContractDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = await _context.Contracts
                .FirstOrDefaultAsync(x => x.ContractId == dto.ContractId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.VendorId = dto.VendorId;
            entity.CreatedByUserId = dto.CreatedByUserId;
            entity.ContractNumber = dto.ContractNumber;
            entity.ContractDescription = dto.ContractDescription;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.ApprovalStatusId = dto.ApprovalStatusId;
            entity.ContractStatusId = dto.ContractStatusId;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = dto.UpdatedBy;
            entity.IsDeleted = dto.IsDeleted;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "Contract",
                "Update",
                old,
                entity.ToDto(),
                entity.ContractId.ToString()
            );

            return true;
        }

        // ========================= DELETE (SOFT) =========================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Contracts
                .FirstOrDefaultAsync(x => x.ContractId == id);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "Contract",
                "Delete",
                old,
                null,
                entity.ContractId.ToString()
            );

            return true;
        }
    }
}
