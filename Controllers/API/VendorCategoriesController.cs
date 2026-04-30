using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize] // All authenticated users can access
    [ApiController]
    [Route("api/vendor-categories")]
    public class VendorCategoriesController : ControllerBase
    {
        private readonly IVendorCategory _service;

        public VendorCategoriesController(IVendorCategory service)
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

        [HttpGet("by-budget-code/{budgetCodeId:guid}")]
        public async Task<IActionResult> GetByBudgetCode(Guid budgetCodeId)
        {
            var all = await _service.GetAllAsync();
            var filtered = all.Where(x => x.ParentBudgetCodeId == budgetCodeId);
            return Ok(filtered);
        }

        [HttpPost]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Create([FromBody] VendorCategoryDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.VendorCategoryId }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] VendorCategoryDto dto)
        {
            dto.VendorCategoryId = id;
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

        // GET: api/vendor-categories/by-budget-codes?ids=guid1,guid2,...
        [HttpGet("by-budget-codes")]
        public async Task<IActionResult> GetByBudgetCodes([FromQuery] string ids)
        {
            if (string.IsNullOrWhiteSpace(ids)) return BadRequest("No Budget Code IDs provided.");
            var guidList = ids.Split(',')
                .Select(x => Guid.TryParse(x, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();
            if (guidList.Count == 0) return BadRequest("No valid GUIDs.");
            var filtered = await _service.GetByBudgetCodeIdsAsync(guidList);
            return Ok(filtered);
        }
    }
}
