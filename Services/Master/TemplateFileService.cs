using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Services.Master
{
    public class TemplateFileService : ITemplateFile
    {
        private readonly ApplicationDBContext _db;
        private readonly IFile _fileService;
        public TemplateFileService(ApplicationDBContext db, IFile fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<IEnumerable<TemplateFile>> GetAllAsync()
        {
            return await _db.TemplateFile.AsNoTracking().ToListAsync();
        }

        public async Task<TemplateFile?> GetByIdAsync(int id)
        {
            return await _db.TemplateFile.AsNoTracking().FirstOrDefaultAsync(t => t.id == id);
        }

        public async Task<IEnumerable<TemplateFile>> GetByPermissionAsync(string permission)
        {
            return await _db.TemplateFile.AsNoTracking().Where(t => t.Permission == permission).ToListAsync();
        }

        public async Task<TemplateFile> CreateAsync(TemplateFile templateFile)
        {
            templateFile.CreatedAt = DateTime.Now;
            _db.TemplateFile.Add(templateFile);
            await _db.SaveChangesAsync();
            return templateFile;
        }

        public async Task<TemplateFile> UpdateAsync(int id, TemplateFile templateFile)
        {
            var existing = await _db.TemplateFile.FirstOrDefaultAsync(t => t.id == id);
            if (existing == null) throw new KeyNotFoundException("TemplateFile not found");
            existing.Title = templateFile.Title;
            existing.Permission = templateFile.Permission;
            existing.FileName = templateFile.FileName;
            existing.FilePath = templateFile.FilePath;
            existing.FileSize = templateFile.FileSize;
            // CreatedAt & CreatedBy tidak diubah
            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _db.TemplateFile.FirstOrDefaultAsync(t => t.id == id);
            if (existing == null) return false;
            _db.TemplateFile.Remove(existing);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<TemplateFile> UploadAsync(IFormFile file, string title, string permission, string module, Guid createdBy, int? id = null)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("File tidak valid");
            var uploadResult = await _fileService.SaveAsync(file, module, "_", Guid.Empty, createdBy);
            if (id.HasValue)
            {
                // Update file lama
                var existing = await _db.TemplateFile.FindAsync(id.Value);
                if (existing == null) throw new ArgumentException("Data tidak ditemukan");
                existing.FileName = uploadResult.OriginalName;
                existing.FilePath = uploadResult.RelativePath;
                existing.FileSize = (int)uploadResult.Size;
                existing.Title = title;
                existing.Permission = permission;
                await _db.SaveChangesAsync();
                return existing;
            }
            else
            {
                // Tambah baru
                var templateFile = new TemplateFile
                {
                    Title = title,
                    Permission = permission,
                    FileName = uploadResult.OriginalName,
                    FilePath = uploadResult.RelativePath,
                    FileSize = (int)uploadResult.Size,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy
                };
                _db.TemplateFile.Add(templateFile);
                await _db.SaveChangesAsync();
                return templateFile;
            }
        }
    }
}
