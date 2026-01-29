using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly ILogger<AttachmentsController> _logger;
        private readonly string _rootPath;

        public AttachmentsController(
            IAttachment attachmentService,
            IFile fileService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AttachmentsController> logger,
            IOptions<FileStorageOptions> fileStorageOptions)
        {
            _attachmentService = attachmentService;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _rootPath = fileStorageOptions.Value.RootPath;
        }

        // GET: api/attachments/by-reference/{referenceId}
        [HttpGet("by-reference/{referenceId:guid}")]
        public async Task<IActionResult> GetByReference(Guid referenceId)
        {
            var attachments = await _attachmentService.GetByReferenceIdAsync(referenceId);
            return Ok(attachments);
        }

        // GET: api/attachments/download/{id}
        [HttpGet("download/{id:guid}")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var attachment = await _attachmentService.GetByIdAsync(id);
                if (attachment == null)
                {
                    _logger.LogWarning("Attachment not found: {AttachmentId}", id);
                    return NotFound(new { message = "Attachment not found" });
                }

                var filePath = Path.Combine(_rootPath, attachment.FilePath);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Physical file not found: {FilePath}", filePath);
                    return NotFound(new { message = "File not found on server" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(attachment.FileName);

                _logger.LogInformation("Attachment downloaded: {AttachmentId}, {FileName}", id, attachment.FileName);

                return File(fileBytes, contentType, attachment.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment {AttachmentId}", id);
                return StatusCode(500, new { message = "Error downloading file" });
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }

        // POST: api/attachments/upload
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(
            IFormFile file,
            string module,           // "Invoices" or "Contracts"
            int attachmentTypeId,
            Guid referenceId)
        {
            _logger.LogInformation("Upload request received - Module: {Module}, ReferenceId: {ReferenceId}, File: {FileName}",
                module, referenceId, file?.FileName);

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Upload failed - No file provided");
                return BadRequest(new { message = "No file uploaded" });
            }

            // Validate file size (max 10MB)
            const long maxSize = 10 * 1024 * 1024;
            if (file.Length > maxSize)
            {
                _logger.LogWarning("Upload failed - File too large: {FileSize} bytes", file.Length);
                return BadRequest(new { message = "File size exceeds 10MB limit" });
            }

            // Get current user ID
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Upload failed - User not authenticated");
                return Unauthorized(new { message = "User not authenticated" });
            }

            try
            {
                _logger.LogInformation("Saving file to storage...");
                // Upload file to storage
                var uploadResult = await _fileService.SaveAsync(
                    file,
                    module,
                    attachmentTypeId.ToString(),
                    referenceId,
                    userId
                );

                _logger.LogInformation("File saved to storage. Path: {FilePath}", uploadResult.RelativePath);

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

                _logger.LogInformation("Saving attachment metadata to database - ReferenceId: {ReferenceId}, FileName: {FileName}",
                    referenceId, uploadResult.OriginalName);

                var created = await _attachmentService.CreateAsync(attachmentDto);

                _logger.LogInformation("Attachment saved successfully - AttachmentId: {AttachmentId}", created.AttachmentId);

                return Ok(new
                {
                    success = true,
                    message = "File uploaded successfully",
                    data = created
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload failed - Module: {Module}, ReferenceId: {ReferenceId}, Error: {Message}",
                    module, referenceId, ex.Message);
                return StatusCode(500, new
                {
                    message = $"Upload failed: {ex.Message}",
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
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
