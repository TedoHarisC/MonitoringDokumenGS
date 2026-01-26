using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Transaction;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/attachments")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachment _attachmentService;
        private readonly IFile _fileService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AttachmentsController(
            IAttachment attachmentService,
            IFile fileService,
            IHttpContextAccessor httpContextAccessor)
        {
            _attachmentService = attachmentService;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/attachments/by-reference/{referenceId}
        [HttpGet("by-reference/{referenceId:guid}")]
        public async Task<IActionResult> GetByReference(Guid referenceId)
        {
            var attachments = await _attachmentService.GetByReferenceIdAsync(referenceId);
            return Ok(attachments);
        }

        // POST: api/attachments/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            [FromForm] IFormFile file,
            [FromForm] string module,           // "Invoices" or "Contracts"
            [FromForm] int attachmentTypeId,
            [FromForm] Guid referenceId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            // Validate file size (max 10MB)
            const long maxSize = 10 * 1024 * 1024;
            if (file.Length > maxSize)
                return BadRequest(new { message = "File size exceeds 10MB limit" });

            // Get current user ID
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                // Upload file to storage
                var uploadResult = await _fileService.SaveAsync(
                    file,
                    module,
                    attachmentTypeId.ToString(),
                    referenceId,
                    userId
                );

                // Save attachment metadata to database
                var attachmentDto = new AttachmentDto
                {
                    AttachmentTypeId = attachmentTypeId,
                    ReferenceId = referenceId,
                    FileName = uploadResult.OriginalName,
                    FilePath = uploadResult.RelativePath,
                    FileSize = (int)uploadResult.Size,
                    CreatedBy = userId
                };

                var created = await _attachmentService.CreateAsync(attachmentDto);

                return Ok(new
                {
                    success = true,
                    message = "File uploaded successfully",
                    data = created
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Upload failed: {ex.Message}" });
            }
        }

        // DELETE: api/attachments/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var attachment = await _attachmentService.GetByIdAsync(id);
            if (attachment == null)
                return NotFound(new { message = "Attachment not found" });

            // TODO: Also delete physical file from storage if needed
            // var physicalPath = Path.Combine(_rootPath, attachment.FilePath);
            // if (System.IO.File.Exists(physicalPath))
            //     System.IO.File.Delete(physicalPath);

            var deleted = await _attachmentService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Attachment not found" });

            return Ok(new { success = true, message = "Attachment deleted" });
        }
    }
}
