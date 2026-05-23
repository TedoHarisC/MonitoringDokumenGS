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
        private readonly IAuditLog _auditLog;
        public UangMukaController(IUangMuka service, IAuditLog auditLog)
        {
            _service = service;
            _auditLog = auditLog;
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
            [FromQuery] string? coaText,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? search
        )
        {
            var all = await _service.GetAllAsync();
            var query = all.AsQueryable();

            // --- Filtering by user role ---
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            bool isAdmin = roleClaim == "SUPER_ADMIN" || roleClaim == "ADMIN";
            if (!isAdmin && Guid.TryParse(userIdClaim, out var userId))
            {
                query = query.Where(x => x.CreatedBy == userId);
            }

            if (!string.IsNullOrEmpty(jenis))
                query = query.Where(x => x.Jenis == jenis);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(x => (x.Status ?? "").Contains(status));
            if (!string.IsNullOrEmpty(atasNama))
                query = query.Where(x => x.AtasNama.Contains(atasNama));
            // Filtering by BudgetCodeId (now many-to-many)
            if (!string.IsNullOrEmpty(budgetCode) && Guid.TryParse(budgetCode, out var budgetCodeGuid))
                query = query.Where(x => (x.UangMukaBudgetCodes ?? new List<Models.Transaction.UangMukaBudgetCode>()).Any(bc => bc.BudgetCodeId == budgetCodeGuid));
            if (!string.IsNullOrEmpty(coaText) && int.TryParse(coaText, out var coaTextId))
                query = query.Where(x => (x.UangMukaCoaTexts ?? new List<Models.Transaction.UangMukaCoaText>()).Any(ct => ct.CoaTextId == coaTextId));
            if (startDate.HasValue)
                query = query.Where(x => x.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(x => x.EndDate <= endDate.Value);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(x => x.AtasNama.Contains(search));
            var result = query
                .OrderByDescending(x => x.CreatedAt)
                .Take(30)
                .Select(x => Mappings.Transaction.UangMukaMappings.ToDto(x))
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
                query = query.Where(x => x.AtasNama.Contains(search));
            var result = query
                .OrderByDescending(x => x.CreatedAt)
                .Take(30)
                .Select(x => Mappings.Transaction.UangMukaMappings.ToDto(x))
                .ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = Mappings.Transaction.UangMukaMappings.ToDto(entity);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Dtos.Transaction.UangMukaDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Map DTO to entity
            var entity = new Models.Transaction.UangMuka
            {
                Jenis = model.Jenis ?? string.Empty,
                UangMukaRelatedId = model.UangMukaRelatedId,
                NoSAP = model.NoSAP,
                Amount = model.Amount,
                AtasNama = model.AtasNama ?? string.Empty,
                Deskripsi = model.Deskripsi ?? string.Empty,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                StatusId = model.StatusId ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = model.CreatedBy,
                UangMukaBudgetCodes = (model.BudgetCodeIds ?? new List<Guid>()).Select(bc => new Models.Transaction.UangMukaBudgetCode { BudgetCodeId = bc }).ToList(),
                UangMukaCoaTexts = (model.CoaTextIds ?? new List<int>()).Select(ct => new Models.Transaction.UangMukaCoaText { CoaTextId = ct }).ToList()
            };
            var created = await _service.CreateAsync(entity);
            var dto = Mappings.Transaction.UangMukaMappings.ToDto(created);
            return CreatedAtAction(nameof(GetById), new { id = dto.UangMukaId }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Dtos.Transaction.UangMukaDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Map DTO to entity
            var entity = new Models.Transaction.UangMuka
            {
                Jenis = model.Jenis ?? string.Empty,
                UangMukaRelatedId = model.UangMukaRelatedId,
                NoSAP = model.NoSAP,
                Amount = model.Amount,
                AtasNama = model.AtasNama ?? string.Empty,
                Deskripsi = model.Deskripsi ?? string.Empty,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                StatusId = model.StatusId ?? string.Empty,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = model.UpdatedBy,
                UangMukaBudgetCodes = (model.BudgetCodeIds ?? new List<Guid>()).Select(bc => new Models.Transaction.UangMukaBudgetCode { BudgetCodeId = bc }).ToList(),
                UangMukaCoaTexts = (model.CoaTextIds ?? new List<int>()).Select(ct => new Models.Transaction.UangMukaCoaText { CoaTextId = ct }).ToList()
            };
            var updated = await _service.UpdateAsync(id, entity);
            var dto = Mappings.Transaction.UangMukaMappings.ToDto(updated);
            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(string id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();
            var history = await _auditLog.GetAuditHistoryAsync(id, "UangMuka");
            if (history == null || !history.Any())
            {
                return NotFound(new { message = "No history found for this Uang Muka" });
            }
            return Ok(history);
        }
    }
}
