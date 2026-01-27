using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Transaction;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize] // Changed to allow all authenticated users
    [ApiController]
    [Route("api/invoices")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoice _service;
        private readonly IUser _userService;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(IInvoice service, IUser userService, ILogger<InvoicesController> logger)
        {
            _service = service;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Get current user's role and vendor
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var isSuperAdminOrAdmin = userRoles.Contains("SUPER_ADMIN") || userRoles.Contains("ADMIN");

                IEnumerable<InvoiceDto> result;

                if (isSuperAdminOrAdmin)
                {
                    // Super Admin or Admin: Get all invoices
                    result = await _service.GetAllAsync();
                    _logger.LogInformation("User {UserId} with role {Roles} accessed all invoices", userId, string.Join(", ", userRoles));
                }
                else
                {
                    // Regular user: Get only their vendor's invoices
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null || user.VendorId == Guid.Empty)
                    {
                        _logger.LogWarning("User {UserId} has no vendor assigned", userId);
                        return Ok(new List<InvoiceDto>()); // Return empty list if no vendor assigned
                    }

                    result = await _service.GetAllByVendorAsync(user.VendorId);
                    _logger.LogInformation("User {UserId} with vendor {VendorId} accessed their invoices", userId, user.VendorId);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices");
                return StatusCode(500, new { message = "An error occurred while retrieving invoices" });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var item = await _service.GetByIdAsync(id);
                if (item == null) return NotFound(new { message = "Invoice not found" });

                // Check if user has access to this invoice
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var isSuperAdminOrAdmin = userRoles.Contains("SUPER_ADMIN") || userRoles.Contains("ADMIN");

                if (!isSuperAdminOrAdmin)
                {
                    // Regular user: Check if invoice belongs to their vendor
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null || item.VendorId != user.VendorId)
                    {
                        _logger.LogWarning("User {UserId} attempted to access invoice {InvoiceId} from different vendor", userId, id);
                        return Forbid(); // Return 403 Forbidden
                    }
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice {InvoiceId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving invoice" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InvoiceDto dto)
        {
            try
            {
                // Get current user
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var isSuperAdminOrAdmin = userRoles.Contains("SUPER_ADMIN") || userRoles.Contains("ADMIN");

                if (!isSuperAdminOrAdmin)
                {
                    // Regular user: Can only create invoices for their own vendor
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null || user.VendorId == Guid.Empty)
                    {
                        return BadRequest(new { message = "User has no vendor assigned" });
                    }

                    // Force vendorId to be user's vendor
                    dto.VendorId = user.VendorId;
                }

                // Set created by
                dto.CreatedBy = userId;
                dto.CreatedByUserId = userId;

                var created = await _service.CreateAsync(dto);
                _logger.LogInformation("Invoice {InvoiceNumber} created by user {UserId}", created.InvoiceNumber, userId);

                return CreatedAtAction(nameof(GetById), new { id = created.InvoiceId }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(500, new { message = "An error occurred while creating invoice" });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InvoiceDto dto)
        {
            try
            {
                // Check if invoice exists and user has access
                var existingInvoice = await _service.GetByIdAsync(id);
                if (existingInvoice == null)
                    return NotFound(new { message = "Invoice not found" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var isSuperAdminOrAdmin = userRoles.Contains("SUPER_ADMIN") || userRoles.Contains("ADMIN");

                if (!isSuperAdminOrAdmin)
                {
                    // Regular user: Can only update their vendor's invoices
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null || existingInvoice.VendorId != user.VendorId)
                    {
                        _logger.LogWarning("User {UserId} attempted to update invoice {InvoiceId} from different vendor", userId, id);
                        return Forbid();
                    }

                    // Prevent changing vendor
                    dto.VendorId = existingInvoice.VendorId;
                }

                dto.InvoiceId = id;
                var ok = await _service.UpdateAsync(dto);
                if (!ok) return NotFound(new { message = "Failed to update invoice" });

                _logger.LogInformation("Invoice {InvoiceId} updated by user {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice {InvoiceId}", id);
                return StatusCode(500, new { message = "An error occurred while updating invoice" });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")] // Only admin can delete
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var ok = await _service.DeleteAsync(id);
                if (!ok) return NotFound(new { message = "Invoice not found" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Invoice {InvoiceId} deleted by user {UserId}", id, userIdClaim);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice {InvoiceId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting invoice" });
            }
        }

        /// <summary>
        /// Update Invoice Progress Status - Admin/Super Admin Only
        /// PATCH /api/invoices/{id}/status
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> UpdateInvoiceStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (request == null || request.StatusId <= 0)
                {
                    return BadRequest(new { message = "Valid status ID is required" });
                }

                var invoice = await _service.GetByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound(new { message = "Invoice not found" });
                }

                // Create DTO with updated status
                var dto = new InvoiceDto
                {
                    InvoiceId = invoice.InvoiceId,
                    VendorId = invoice.VendorId,
                    CreatedByUserId = invoice.CreatedByUserId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    ProgressStatusId = request.StatusId, // Update status
                    InvoiceAmount = invoice.InvoiceAmount,
                    TaxAmount = invoice.TaxAmount,
                    CreatedBy = invoice.CreatedBy
                };

                var success = await _service.UpdateAsync(dto);
                if (!success)
                {
                    return NotFound(new { message = "Failed to update invoice status" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Invoice {InvoiceId} status updated to {StatusId} by admin {UserId}",
                    id, request.StatusId, userIdClaim);

                return Ok(new
                {
                    success = true,
                    message = "Invoice status updated successfully",
                    statusId = request.StatusId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice status for {InvoiceId}", id);
                return StatusCode(500, new { message = "An error occurred while updating invoice status" });
            }
        }
    }
}
