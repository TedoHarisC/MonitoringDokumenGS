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
    [Route("api/contracts")]
    public class ContractsController : ControllerBase
    {
        private readonly IContract _service;
        private readonly IUser _userService;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(IContract service, IUser userService, ILogger<ContractsController> logger)
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

                IEnumerable<ContractDto> result;

                if (isSuperAdminOrAdmin)
                {
                    // Super Admin or Admin: Get all contracts
                    result = await _service.GetAllAsync();
                    _logger.LogInformation("User {UserId} with role {Roles} accessed all contracts", userId, string.Join(", ", userRoles));
                }
                else
                {
                    // Regular user: Get only their vendor's contracts
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null || user.VendorId == Guid.Empty)
                    {
                        _logger.LogWarning("User {UserId} has no vendor assigned", userId);
                        return Ok(new List<ContractDto>()); // Return empty list if no vendor assigned
                    }

                    result = await _service.GetAllByVendorAsync(user.VendorId);
                    _logger.LogInformation("User {UserId} with vendor {VendorId} accessed their contracts", userId, user.VendorId);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contracts");
                return StatusCode(500, new { message = "An error occurred while retrieving contracts" });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var item = await _service.GetByIdAsync(id);
                if (item == null) return NotFound(new { message = "Contract not found" });

                // Check if user has access to this contract
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var isSuperAdminOrAdmin = userRoles.Contains("SUPER_ADMIN") || userRoles.Contains("ADMIN");

                if (!isSuperAdminOrAdmin)
                {
                    // Regular user: Check if contract belongs to their vendor
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null || item.VendorId != user.VendorId)
                    {
                        _logger.LogWarning("User {UserId} attempted to access contract {ContractId} from different vendor", userId, id);
                        return Forbid(); // Return 403 Forbidden
                    }
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract {ContractId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving contract" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContractDto dto)
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
                    // Regular user: Can only create contracts for their own vendor
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
                _logger.LogInformation("Contract {ContractNumber} created by user {UserId}", created.ContractNumber, userId);

                return CreatedAtAction(nameof(GetById), new { id = created.ContractId }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract");
                return StatusCode(500, new { message = "An error occurred while creating contract" });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ContractDto dto)
        {
            try
            {
                // Check if contract exists and user has access
                var existingContract = await _service.GetByIdAsync(id);
                if (existingContract == null)
                    return NotFound(new { message = "Contract not found" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var isSuperAdminOrAdmin = userRoles.Contains("SUPER_ADMIN") || userRoles.Contains("ADMIN");

                if (!isSuperAdminOrAdmin)
                {
                    // Regular user: Can only update their vendor's contracts
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null || existingContract.VendorId != user.VendorId)
                    {
                        _logger.LogWarning("User {UserId} attempted to update contract {ContractId} from different vendor", userId, id);
                        return Forbid();
                    }

                    // Prevent changing vendor
                    dto.VendorId = existingContract.VendorId;
                }

                dto.ContractId = id;
                var ok = await _service.UpdateAsync(dto);
                if (!ok) return NotFound(new { message = "Failed to update contract" });

                _logger.LogInformation("Contract {ContractId} updated by user {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract {ContractId}", id);
                return StatusCode(500, new { message = "An error occurred while updating contract" });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")] // Only admin can delete
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var ok = await _service.DeleteAsync(id);
                if (!ok) return NotFound(new { message = "Contract not found" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Contract {ContractId} deleted by user {UserId}", id, userIdClaim);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract {ContractId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting contract" });
            }
        }
    }
}
