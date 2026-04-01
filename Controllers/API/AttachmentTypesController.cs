using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Interfaces;
using System.Security.Claims;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize] // All authenticated users can access
    [ApiController]
    [Route("api/attachment-types")]
    public class AttachmentTypesController : ControllerBase
    {
        private readonly IAttachmentTypes _service;
        private readonly ILogger<AttachmentTypesController> _logger;

        public AttachmentTypesController(IAttachmentTypes service, ILogger<AttachmentTypesController> logger)
        {
            _service = service;
            _logger = logger;
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

        [HttpPost]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Create([FromBody] AttachmentTypeDto dto)
        {
            Console.WriteLine($"[AttachmentType CREATE] DTO IsRequired: {dto.IsRequired}");
            Console.WriteLine($"[AttachmentType CREATE] DTO Code: {dto.Code}");
            Console.WriteLine($"[AttachmentType CREATE] DTO Name: {dto.Name}");
            Console.WriteLine($"[AttachmentType CREATE] DTO AppliesTo: {dto.AppliesTo}");

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.AttachmentTypeId }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] AttachmentTypeDto dto)
        {
            Console.WriteLine($"[AttachmentType UPDATE] ID: {id}");
            Console.WriteLine($"[AttachmentType UPDATE] DTO IsRequired: {dto.IsRequired}");
            Console.WriteLine($"[AttachmentType UPDATE] DTO Code: {dto.Code}");
            Console.WriteLine($"[AttachmentType UPDATE] DTO Name: {dto.Name}");
            Console.WriteLine($"[AttachmentType UPDATE] DTO AppliesTo: {dto.AppliesTo}");

            dto.AttachmentTypeId = id;
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
