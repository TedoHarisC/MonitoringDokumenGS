using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize] // All authenticated users can access
    [ApiController]
    [Route("api/vendors")]
    public class VendorsController : ControllerBase
    {
        private readonly IVendor _service;

        public VendorsController(IVendor service)
        {
            _service = service;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery(Name = "draw")] int draw = 1,
            [FromQuery(Name = "start")] int start = 0,
            [FromQuery(Name = "length")] int length = 10,
            [FromQuery(Name = "search[value]")] string? search = null)
        {
            // DataTables: start = offset, length = page size
            int page = (start / (length > 0 ? length : 10)) + 1;
            int pageSize = length;

            var paged = await _service.GetPagedAsync(page, pageSize, search);

            return Ok(new
            {
                draw = draw,
                recordsTotal = paged.TotalCount,
                recordsFiltered = paged.TotalCount,
                data = paged.Items
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Create([FromBody] VendorDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            dto.CreatedBy = userId;
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.VendorId }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VendorDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            dto.VendorId = id;
            dto.UpdatedBy = userId;
            var ok = await _service.UpdateAsync(dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
