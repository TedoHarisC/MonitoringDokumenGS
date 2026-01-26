using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
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

        public InvoiceService(
            ApplicationDBContext context,
            IAuditLog auditLog,
            INotifications notificationService)
        {
            _context = context;
            _auditLog = auditLog;
            _notificationService = notificationService;
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

        // ========================= GET BY ID =========================
        public async Task<InvoiceDto?> GetByIdAsync(Guid id)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Where(x => x.InvoiceId == id && !x.IsDeleted)
                .Select(InvoiceMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<InvoiceDto> CreateAsync(InvoiceDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = new Invoice
            {
                InvoiceId = dto.InvoiceId == Guid.Empty ? Guid.NewGuid() : dto.InvoiceId,
                VendorId = dto.VendorId,
                CreatedByUserId = dto.CreatedByUserId,
                InvoiceNumber = dto.InvoiceNumber,
                ProgressStatusId = dto.ProgressStatusId,
                InvoiceAmount = dto.InvoiceAmount,
                TaxAmount = dto.TaxAmount,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
                UpdatedAt = null,
                UpdatedBy = null,
                IsDeleted = false
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

            // Create notification for invoice creator
            if (entity.CreatedByUserId != Guid.Empty)
            {
                try
                {
                    await _notificationService.CreateAsync(new Dtos.Infrastructure.NotificationDto
                    {
                        UserId = entity.CreatedByUserId,
                        Title = "Invoice Created",
                        Message = $"Invoice {entity.InvoiceNumber} has been successfully created with amount {entity.InvoiceAmount:N2}"
                    });
                }
                catch (Exception ex)
                {
                    // Log notification error but don't fail the invoice creation
                    Console.WriteLine($"Failed to create notification: {ex.Message}");
                }
            }

            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(InvoiceDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = await _context.Invoices
                .FirstOrDefaultAsync(x => x.InvoiceId == dto.InvoiceId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.VendorId = dto.VendorId;
            entity.CreatedByUserId = dto.CreatedByUserId;
            entity.InvoiceNumber = dto.InvoiceNumber;
            entity.ProgressStatusId = dto.ProgressStatusId;
            entity.InvoiceAmount = dto.InvoiceAmount;
            entity.TaxAmount = dto.TaxAmount;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = dto.UpdatedBy;
            entity.IsDeleted = dto.IsDeleted;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "Invoice",
                "Update",
                old,
                entity.ToDto(),
                entity.InvoiceId.ToString()
            );

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
