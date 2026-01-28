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
        private readonly INotifications _notificationService;

        public ContractService(
            ApplicationDBContext context,
            IAuditLog auditLog,
            INotifications notificationService)
        {
            _context = context;
            _auditLog = auditLog;
            _notificationService = notificationService;
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

        // ========================= GET ALL BY VENDOR =========================
        public async Task<IEnumerable<ContractDto>> GetAllByVendorAsync(Guid vendorId)
        {
            return await _context.Contracts
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.VendorId == vendorId)
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

            // Send notification to all admins when user creates contract
            try
            {
                // Get all admin and super admin user IDs
                var adminRoleIds = await _context.Roles
                    .Where(r => r.Code == "ADMIN" || r.Code == "SUPER_ADMIN")
                    .Select(r => r.RoleId)
                    .ToListAsync();

                var adminUserIds = await _context.UserRoles
                    .Where(ur => adminRoleIds.Contains(ur.RoleId) && !ur.IsDeleted)
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToListAsync();

                // Get creator username for better notification message
                var creatorUsername = await _context.Users
                    .Where(u => u.UserId == entity.CreatedByUserId)
                    .Select(u => u.Username)
                    .FirstOrDefaultAsync() ?? "Unknown User";

                // Send notification to each admin
                foreach (var adminUserId in adminUserIds)
                {
                    await _notificationService.CreateAsync(new Dtos.Infrastructure.NotificationDto
                    {
                        UserId = adminUserId,
                        Title = "New Contract Submitted",
                        Message = $"User {creatorUsername} has submitted contract {entity.ContractNumber} (Period: {entity.StartDate:yyyy-MM-dd} to {entity.EndDate:yyyy-MM-dd}) for review."
                    });
                }
            }
            catch (Exception ex)
            {
                // Log notification error but don't fail the contract creation
                Console.WriteLine($"Failed to create admin notifications: {ex.Message}");
            }

            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(ContractDto dto)
        {
            Console.WriteLine($"[CONTRACT UPDATE START] ContractId: {dto.ContractId}, UpdatedBy from DTO: {dto.UpdatedBy}");

            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = await _context.Contracts
                .FirstOrDefaultAsync(x => x.ContractId == dto.ContractId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            // Check if approval status is changed
            int oldApprovalStatusId = entity.ApprovalStatusId;

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

            bool approvalStatusChanged = oldApprovalStatusId != dto.ApprovalStatusId;

            if (approvalStatusChanged && entity.CreatedByUserId != Guid.Empty)
            {
                try
                {
                    Console.WriteLine($"[CONTRACT UPDATE] Preparing to send notification...");

                    // Get status names
                    var oldStatus = await _context.ApprovalStatuses
                        .Where(s => s.ApprovalStatusId == oldApprovalStatusId)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync() ?? "Unknown";

                    var newStatus = await _context.ApprovalStatuses
                        .Where(s => s.ApprovalStatusId == entity.ApprovalStatusId)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync() ?? "Unknown";

                    Console.WriteLine($"[CONTRACT UPDATE] Status names - Old: {oldStatus}, New: {newStatus}");
                    Console.WriteLine($"[CONTRACT UPDATE] Sending notification to user {entity.CreatedByUserId}");

                    var notificationDto = new Dtos.Infrastructure.NotificationDto
                    {
                        UserId = entity.CreatedBy,
                        Title = "Contract Approval Status Updated",
                        Message = $"Your contract {entity.ContractNumber} approval status has been updated from '{oldStatus}' to '{newStatus}' by administrator."
                    };

                    Console.WriteLine($"[CONTRACT UPDATE] Notification DTO: UserId={notificationDto.UserId}, Title={notificationDto.Title}");
                    await _notificationService.CreateAsync(notificationDto);

                    Console.WriteLine($"[CONTRACT UPDATE] ✓ Notification sent successfully to user {entity.CreatedByUserId}");
                }
                catch (Exception ex)
                {
                    // Log notification error but don't fail the update
                    Console.WriteLine($"[CONTRACT UPDATE] ✗ Failed to create notification: {ex.Message}");
                    Console.WriteLine($"[CONTRACT UPDATE] Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[CONTRACT UPDATE] Inner exception: {ex.InnerException.Message}");
                    }
                }
            }

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
