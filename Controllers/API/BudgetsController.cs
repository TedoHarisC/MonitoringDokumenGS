using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize] // All authenticated users can access
    [ApiController]
    [Route("api/budgets")]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudget _service;

        public BudgetsController(IBudget service)
        {
            _service = service;
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
                recordsFiltered = paged.TotalCount, // Untuk DataTables, recordsFiltered = total setelah filter
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
        public async Task<IActionResult> Create([FromBody] BudgetDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.BudgetId }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BudgetDto dto)
        {
            dto.BudgetId = id;
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
