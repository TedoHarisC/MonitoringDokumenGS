using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MonitoringDokumenGS.Dtos.Master;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models;
using Microsoft.Extensions.Options;
using MonitoringDokumenGS.Models.Infrastructure;
using System.IO;

namespace MonitoringDokumenGS.Controllers.API
{
    [ApiController]
    [Route("api/template-files")]

    public class TemplateFilesController : ControllerBase
    {
        private readonly ITemplateFile _service;
        private readonly FileStorageOptions _fileStorageOptions;
        public TemplateFilesController(ITemplateFile service, IOptions<FileStorageOptions> fileStorageOptions)
        {
            _service = service;
            _fileStorageOptions = fileStorageOptions.Value;
        }
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _service.GetByIdAsync(id);
            if (file == null || string.IsNullOrEmpty(file.FilePath) || string.IsNullOrEmpty(file.FileName))
                return NotFound();
            var filePath = Path.Combine(_fileStorageOptions.RootPath, file.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound();
            var contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType, file.FileName);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemplateFile>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateFile>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("permission/{permission}")]
        public async Task<ActionResult<IEnumerable<TemplateFile>>> GetByPermission(string permission)
        {
            var result = await _service.GetByPermissionAsync(permission);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<TemplateFile>> Create([FromBody] TemplateFile templateFile)
        {
            var created = await _service.CreateAsync(templateFile);
            return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TemplateFile>> Update(int id, [FromBody] TemplateFile templateFile)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, templateFile);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadTemplateFileRequest request)
        {
            var file = request.File;
            var permission = request.Permission;
            var title = request.Title;
            var module = request.Module ?? "TemplateFiles";
            var id = request.Id;
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "User not authenticated" });
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });
            // Max 10MB
            const long maxSize = 10 * 1024 * 1024;
            if (file.Length > maxSize)
                return BadRequest(new { message = "File size exceeds 10MB limit" });
            try
            {
                var created = await _service.UploadAsync(file, title, permission, module, userId, id);
                return Ok(new { success = true, message = id.HasValue ? "File updated successfully" : "File uploaded successfully", data = created });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Upload failed: {ex.Message}" });
            }
        }
    }
}
