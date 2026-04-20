using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models.Transaction;

namespace MonitoringDokumenGS.Controllers.API
{
    [ApiController]
    [Route("api/uang-muka")]
    [Authorize]
    public class UangMukaController : ControllerBase
    {
        private readonly IUangMuka _service;
        public UangMukaController(IUangMuka service)
        {
            _service = service;
        }

        /// <summary>
        /// Get Uang Muka with optional filters (jenis, status, atasNama, budgetCode, search)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFiltered(
            [FromQuery] string? jenis,
            [FromQuery] string? status,
            [FromQuery] string? atasNama,
            [FromQuery] string? budgetCode,
            [FromQuery] string? search
        )
        {
            var all = await _service.GetAllAsync();
            var query = all.AsQueryable();
            if (!string.IsNullOrEmpty(jenis))
                query = query.Where(x => x.Jenis == jenis);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(x => (x.Status ?? "").Contains(status));
            if (!string.IsNullOrEmpty(atasNama))
                query = query.Where(x => x.AtasNama.Contains(atasNama));
            if (!string.IsNullOrEmpty(budgetCode))
                query = query.Where(x => (x.BudgetCode != null && x.BudgetCode.Code.Contains(budgetCode)));
            if (!string.IsNullOrEmpty(search))
                query = query.Where(x => x.AtasNama.Contains(search) || (x.BudgetCode != null && x.BudgetCode.Code.Contains(search)));
            var result = query
                .OrderByDescending(x => x.CreatedAt)
                .Take(30)
                .Select(x => new
                {
                    uangMukaId = x.UangMukaId,
                    atasNama = x.AtasNama,
                    amount = x.Amount,
                    jenis = x.Jenis,
                    status = x.Status,
                    startDate = x.StartDate,
                    endDate = x.EndDate,
                    budgetCode = x.BudgetCode == null ? null : new
                    {
                        code = x.BudgetCode.Code,
                        description = x.BudgetCode.Description,
                        budgetCodeId = x.BudgetCode.BudgetCodeId
                    },
                    coaText = x.CoaText == null ? null : new
                    {
                        vendorCategoryId = x.CoaText.VendorCategoryId,
                        name = x.CoaText.Name,
                        parentBudgetCodeLabel = x.CoaText.Name
                    },
                    relatedUangMuka = x.RelatedUangMuka == null ? null : new
                    {
                        id = x.RelatedUangMuka.UangMukaId,
                        atasNama = x.RelatedUangMuka.AtasNama,
                        amount = x.RelatedUangMuka.Amount,
                        jenis = x.RelatedUangMuka.Jenis
                    }
                })
                .ToList();
            return Ok(result);
        }

        /// <summary>
        /// Get Advanced Uang Muka with status 'Butuh Realisasi' for Realisasi selection (Select2)
        /// </summary>
        [HttpGet("advanced-for-realisasi")]
        public async Task<IActionResult> GetAdvancedForRealisasi([FromQuery] string? search)
        {
            var all = await _service.GetAllAsync();
            var query = all.Where(x => x.Jenis == "Advanced" && (x.Status ?? "") == "Butuh Realisasi");
            if (!string.IsNullOrEmpty(search))
                query = query.Where(x => x.AtasNama.Contains(search) || (x.BudgetCode != null && x.BudgetCode.Code.Contains(search)));
            var result = query
                .OrderByDescending(x => x.CreatedAt)
                .Take(30)
                .Select(x => new
                {
                    id = x.UangMukaId,
                    atasNama = x.AtasNama,
                    amount = x.Amount,
                    budgetCode = x.BudgetCode != null ? x.BudgetCode.Code : null
                })
                .ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UangMuka model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _service.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.UangMukaId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UangMuka model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _service.UpdateAsync(id, model);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
