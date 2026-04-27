using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize] // All authenticated users can access
    [ApiController]
    [Route("api/approval-statuses")]
    public class ApprovalStatusController : ControllerBase
    {
        private readonly IApprovalStatus _service;

        public ApprovalStatusController(IApprovalStatus service)
        {
            _service = service;
        }

        /// <summary>
        /// DataTables server-side endpoint
        /// </summary>
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
            var result = await _service.GetPagedAsync(page, pageSize, search);

            // DataTables expects: { draw, recordsTotal, recordsFiltered, data }
            return Ok(new
            {
                draw,
                recordsTotal = result.TotalCount,
                recordsFiltered = result.TotalCount,
                data = result.Items
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Create([FromBody] ApprovalStatusDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ApprovalStatusId }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] ApprovalStatusDto dto)
        {
            dto.ApprovalStatusId = id;
            var ok = await _service.UpdateAsync(dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
