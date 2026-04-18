using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IConfiguration _configuration;
        private readonly string _rootPath;

        public AttachmentsController(
            IAttachment attachmentService,
            IFile fileService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AttachmentsController> logger,
            IOptions<FileStorageOptions> fileStorageOptions,
            IConfiguration configuration)
        {
            _attachmentService = attachmentService;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _rootPath = fileStorageOptions.Value.RootPath;
            _configuration = configuration;
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

        // GET: api/attachments/file/{id}?token=xxx  (for direct browser download over HTTP)
        [AllowAnonymous]
        [HttpGet("file/{id:guid}")]
        public async Task<IActionResult> DownloadWithToken(Guid id, [FromQuery] string token)
        {
            // Validate JWT token manually
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Token is required" });
            }

            try
            {
                var jwtKey = _configuration["Jwt:Key"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtKey!);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                }, out _);
            }
            catch (Exception)
            {
                return Unauthorized(new { message = "Invalid or expired token" });
            }

            try
            {
                var attachment = await _attachmentService.GetByIdAsync(id);
                if (attachment == null)
                {
                    return NotFound(new { message = "Attachment not found" });
                }

                var filePath = Path.Combine(_rootPath, attachment.FilePath);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "File not found on server" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(attachment.FileName);

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
        /// <summary>
        /// Upload file attachment (Invoices/Contracts)
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/attachments/upload
        ///     Content-Type: multipart/form-data
        /// </remarks>
        /// <param name="file">File to upload</param>
        /// <param name="module">Module name (Invoices/Contracts)</param>
        /// <param name="attachmentTypeId">Attachment type ID</param>
        /// <param name="referenceId">Reference ID (Guid)</param>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        // public async Task<IActionResult> Upload(
        //     [FromForm] IFormFile file,
        //     [FromForm] string module,
        //     [FromForm] int attachmentTypeId,
        //     [FromForm] Guid referenceId)
        public async Task<IActionResult> Upload([FromForm] UploadAttachmentRequest request)
        {
            var file = request.File;
            var module = request.Module;
            var attachmentTypeId = request.AttachmentTypeId;
            var referenceId = request.ReferenceId;

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
