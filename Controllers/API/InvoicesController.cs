using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Transaction;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
    [ApiController]
    [Route("api/invoices")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoice _service;

        public InvoicesController(IInvoice service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InvoiceDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.InvoiceId }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InvoiceDto dto)
        {
            dto.InvoiceId = id;
            var ok = await _service.UpdateAsync(dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
