using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Interfaces
{
    public interface ITemplateFile
    {
        Task<IEnumerable<TemplateFile>> GetAllAsync();
        Task<TemplateFile?> GetByIdAsync(int id);
        Task<IEnumerable<TemplateFile>> GetByPermissionAsync(string permission);
        Task<TemplateFile> CreateAsync(TemplateFile templateFile);
        Task<TemplateFile> UpdateAsync(int id, TemplateFile templateFile);
        Task<bool> DeleteAsync(int id);
        Task<TemplateFile> UploadAsync(IFormFile file, string title, string permission, string module, Guid createdBy, int? id = null);
    }
}
