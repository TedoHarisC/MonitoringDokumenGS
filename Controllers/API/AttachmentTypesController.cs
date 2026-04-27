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
            // DataTables server-side
            int draw = 1;
            int start = (page - 1) * pageSize;
            int length = pageSize;
            string? search = null;
            if (Request.Query.ContainsKey("draw"))
                int.TryParse(Request.Query["draw"], out draw);
            if (Request.Query.ContainsKey("start"))
                int.TryParse(Request.Query["start"], out start);
            if (Request.Query.ContainsKey("length"))
                int.TryParse(Request.Query["length"], out length);
            if (Request.Query.ContainsKey("search[value]"))
                search = Request.Query["search[value]"].ToString();

            int pageNum = (start / (length > 0 ? length : 10)) + 1;
            int pageSizeNum = length;
            var result = await _service.GetPagedAsync(pageNum, pageSizeNum, search);
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
