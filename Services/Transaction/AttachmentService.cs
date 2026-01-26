using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Transaction;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Mappings.Transaction;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Services.Transaction
{
    public class AttachmentService : IAttachment
    {
        private readonly ApplicationDBContext _context;

        public AttachmentService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AttachmentDto>> GetAllAsync()
        {
            return await _context.Attachments
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(AttachmentMappings.ToDtoExpr)
                .ToListAsync();
        }

        public async Task<AttachmentDto?> GetByIdAsync(Guid id)
        {
            return await _context.Attachments
                .AsNoTracking()
                .Where(x => x.AttachmentId == id)
                .Select(AttachmentMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AttachmentDto>> GetByReferenceIdAsync(Guid referenceId)
        {
            return await _context.Attachments
                .AsNoTracking()
                .Where(x => x.ReferenceId == referenceId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(AttachmentMappings.ToDtoExpr)
                .ToListAsync();
        }

        public async Task<AttachmentDto> CreateAsync(AttachmentDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = new Attachment
            {
                AttachmentId = dto.AttachmentId == Guid.Empty ? Guid.NewGuid() : dto.AttachmentId,
                AttachmentTypeId = dto.AttachmentTypeId,
                ReferenceId = dto.ReferenceId,
                FileName = dto.FileName,
                FilePath = dto.FilePath,
                FileSize = dto.FileSize,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy
            };

            _context.Attachments.Add(entity);
            await _context.SaveChangesAsync();

            return entity.ToDto();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Attachments
                .FirstOrDefaultAsync(x => x.AttachmentId == id);

            if (entity == null)
                return false;

            _context.Attachments.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
