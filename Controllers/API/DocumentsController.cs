using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Documents;
using MonitoringDokumenGS.Interfaces;
using System;
using System.Collections.Generic;

namespace MonitoringDokumenGS.Controllers.API
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _service;

        public DocumentsController(IDocumentService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<PagedResponse<DocumentResponse>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1)
                return BadRequest(new { message = "page must be >= 1" });
            if (pageSize < 1 || pageSize > 100)
                return BadRequest(new { message = "pageSize must be between 1 and 100" });

            var result = _service.GetPaged(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public ActionResult<DocumentResponse> GetById(int id)
        {
            var doc = _service.GetById(id);
            if (doc is null)
                return NotFound();

            return Ok(doc);
        }

        [HttpPost]
        public ActionResult<DocumentResponse> Create([FromBody] CreateDocumentRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var newDoc = _service.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = newDoc.Id }, newDoc);
        }
    }
}