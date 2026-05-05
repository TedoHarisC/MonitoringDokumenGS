using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Infrastructure;
using MonitoringDokumenGS.Dtos.Transaction;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Mappings.Transaction;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Services.Transaction
{
    public class InvoiceService : IInvoice
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;
        private readonly INotifications _notificationService;
        private readonly IBudgetNotificationJob _budgetNotificationJob;

        public InvoiceService(
            ApplicationDBContext context,
            IAuditLog auditLog,
            INotifications notificationService,
            IBudgetNotificationJob budgetNotificationJob)
        {
            _context = context;
            _auditLog = auditLog;
            _notificationService = notificationService;
            _budgetNotificationJob = budgetNotificationJob;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<InvoiceDto>> GetAllAsync()
        {
            return await _context.Invoices
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(InvoiceMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= GET ALL BY VENDOR =========================
        public async Task<IEnumerable<InvoiceDto>> GetAllByVendorAsync(Guid vendorId)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.VendorId == vendorId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(InvoiceMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= GET BY ID =========================
        public async Task<InvoiceDto?> GetByIdAsync(Guid id)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(x => x.Vendor)
                .Where(x => x.InvoiceId == id && !x.IsDeleted)
                .Select(InvoiceMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<InvoiceDto> CreateAsync(InvoiceDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var createdAt = DateTime.UtcNow;

            // Calculate IsOnTime: submission should be <= 7th day of InvoiceYear/InvoiceMonth
            var deadline = new DateTime(dto.InvoiceYear, dto.InvoiceMonth, 7);
            var isOnTime = createdAt.Date <= deadline.Date;

            var entity = new Invoice
            {
                InvoiceId = dto.InvoiceId == Guid.Empty ? Guid.NewGuid() : dto.InvoiceId,
                VendorId = dto.VendorId,
                CreatedByUserId = dto.CreatedByUserId,
                InvoiceNumber = dto.InvoiceNumber,
                InvoiceDescription = dto.InvoiceDescription,
                NoSAP = dto.NoSAP,
                ProgressStatusId = dto.ProgressStatusId,
                InvoiceAmount = dto.InvoiceAmount,
                TaxAmount = dto.TaxAmount,
                GrandTotal = dto.GrandTotal,
                CreatedAt = createdAt,
                CreatedBy = dto.CreatedBy,
                UpdatedAt = null,
                UpdatedBy = null,
                IsDeleted = false,
                InvoiceYear = dto.InvoiceYear,
                InvoiceMonth = dto.InvoiceMonth,
                IsOnTime = isOnTime,
                BudgetCodeId = dto.BudgetCodeId == Guid.Empty ? null : dto.BudgetCodeId,
                CoaTextId = dto.CoaTextId == 0 ? null : dto.CoaTextId,
            };

            _context.Invoices.Add(entity);
            await _context.SaveChangesAsync();

            var result = entity.ToDto();

            await _auditLog.LogAsync(
                "Invoice",
                "Create",
                null,
                result,
                entity.InvoiceId.ToString()
            );

            // Send notification to all admins when user creates invoice
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
                        Title = "New Invoice Submitted",
                        Message = $"User {creatorUsername} has submitted invoice {entity.InvoiceNumber} with amount {entity.InvoiceAmount:N2} for review."
                    });
                }
            }
            catch (Exception ex)
            {
                // Log notification error but don't fail the invoice creation
                Console.WriteLine($"Failed to create admin notifications: {ex.Message}");
            }

            // Real-time budget check after invoice created
            try
            {
                await _budgetNotificationJob.CheckBudgetOnInvoiceChangeAsync(entity.InvoiceYear, entity.InvoiceMonth);
            }
            catch (Exception ex)
            {
                // Log error but don't fail invoice creation
                Console.WriteLine($"Failed to run budget check: {ex.Message}");
            }

            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(InvoiceDto dto)
        {
            Console.WriteLine($"[INVOICE UPDATE START] InvoiceId: {dto.InvoiceId}, UpdatedBy from DTO: {dto.UpdatedBy}");

            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = await _context.Invoices
                .FirstOrDefaultAsync(x => x.InvoiceId == dto.InvoiceId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            // Check if progress status is changed
            bool progressStatusChanged = entity.ProgressStatusId != dto.ProgressStatusId;
            int oldProgressStatusId = entity.ProgressStatusId;

            // Recalculate IsOnTime based on CreatedAt and InvoiceYear/Month
            var deadline = new DateTime(dto.InvoiceYear, dto.InvoiceMonth, 7);
            var isOnTime = entity.CreatedAt.Date <= deadline.Date;

            entity.VendorId = dto.VendorId;
            entity.CreatedByUserId = dto.CreatedByUserId;
            entity.InvoiceNumber = dto.InvoiceNumber;
            entity.InvoiceDescription = dto.InvoiceDescription;
            entity.NoSAP = dto.NoSAP;
            entity.ProgressStatusId = dto.ProgressStatusId;
            entity.InvoiceAmount = dto.InvoiceAmount;
            entity.TaxAmount = dto.TaxAmount;
            entity.GrandTotal = dto.GrandTotal;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = dto.UpdatedBy;
            entity.IsDeleted = dto.IsDeleted;
            entity.InvoiceYear = dto.InvoiceYear;
            entity.InvoiceMonth = dto.InvoiceMonth;
            entity.IsOnTime = isOnTime;
            entity.BudgetCodeId = dto.BudgetCodeId == Guid.Empty ? null : dto.BudgetCodeId;
            entity.CoaTextId = dto.CoaTextId == 0 ? null : dto.CoaTextId;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "Invoice",
                "Update",
                old,
                entity.ToDto(),
                entity.InvoiceId.ToString()
            );

            if (progressStatusChanged && entity.CreatedByUserId != Guid.Empty)
            {
                try
                {
                    // Get status names
                    var oldStatus = await _context.InvoiceProgressStatuses
                        .Where(s => s.ProgressStatusId == oldProgressStatusId)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync() ?? "Unknown";

                    var newStatus = await _context.InvoiceProgressStatuses
                        .Where(s => s.ProgressStatusId == entity.ProgressStatusId)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync() ?? "Unknown";

                    var notificationDto = new NotificationDto
                    {
                        UserId = entity.CreatedBy,
                        Title = "Invoice Status Updated",
                        Message = $"Your invoice {entity.InvoiceNumber} status has been updated from '{oldStatus}' to '{newStatus}' by administrator."
                    };

                    await _notificationService.CreateAsync(notificationDto);

                }
                catch (Exception ex)
                {
                    // Log notification error but don't fail the update
                    Console.WriteLine($"[INVOICE UPDATE] ✗ Failed to create notification: {ex.Message}");
                    Console.WriteLine($"[INVOICE UPDATE] Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[INVOICE UPDATE] Inner exception: {ex.InnerException.Message}");
                    }
                }

            }

            // Real-time budget check after invoice updated
            // Check if amount or year/month changed - these affect budget
            if (old.InvoiceAmount != entity.InvoiceAmount ||
                old.InvoiceYear != entity.InvoiceYear ||
                old.InvoiceMonth != entity.InvoiceMonth)
            {
                try
                {
                    // Check budget for new month/year
                    await _budgetNotificationJob.CheckBudgetOnInvoiceChangeAsync(entity.InvoiceYear, entity.InvoiceMonth);

                    // Also check old month/year if changed
                    if (old.InvoiceYear != entity.InvoiceYear || old.InvoiceMonth != entity.InvoiceMonth)
                    {
                        await _budgetNotificationJob.CheckBudgetOnInvoiceChangeAsync(old.InvoiceYear, old.InvoiceMonth);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail invoice update
                    Console.WriteLine($"Failed to run budget check: {ex.Message}");
                }
            }

            return true;
        }

        // ========================= DELETE (SOFT DELETE) =========================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Invoices
                .FirstOrDefaultAsync(x => x.InvoiceId == id);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "Invoice",
                "Delete",
                old,
                null,
                entity.InvoiceId.ToString()
            );

            return true;
        }
    }
}
