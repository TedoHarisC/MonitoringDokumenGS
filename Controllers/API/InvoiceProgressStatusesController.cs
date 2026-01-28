using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize] // All authenticated users can access
    [ApiController]
    [Route("api/invoice-progress-statuses")]
    public class InvoiceProgressStatusesController : ControllerBase
    {
        private readonly IInvoiceProgressStatuses _service;

        public InvoiceProgressStatusesController(IInvoiceProgressStatuses service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetPagedAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InvoiceProgressStatusDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ProgressStatusId }, created);
        }

        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] InvoiceProgressStatusDto dto)
        {
            dto.ProgressStatusId = id;
            var ok = await _service.UpdateAsync(dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
