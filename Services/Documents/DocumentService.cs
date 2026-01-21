using System;
using System.Collections.Generic;
using System.Linq;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Documents;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Services.Documents
{
    public class DocumentService : IDocumentService
    {
        private readonly List<DocumentResponse> _documents = new();
        private readonly object _lock = new();

        public IEnumerable<DocumentResponse> GetAll()
        {
            lock (_lock)
            {
                return _documents.ToList();
            }
        }

        public PagedResponse<DocumentResponse> GetPaged(int page, int pageSize)
        {
            lock (_lock)
            {
                return _documents.ToPagedResponse(page, pageSize);
            }
        }

        public DocumentResponse? GetById(int id)
        {
            lock (_lock)
            {
                return _documents.FirstOrDefault(d => d.Id == id);
            }
        }

        public DocumentResponse Create(CreateDocumentRequest request)
        {
            lock (_lock)
            {
                var nextId = _documents.Count == 0 ? 1 : _documents.Max(d => d.Id) + 1;
                var newDoc = new DocumentResponse
                {
                    Id = nextId,
                    Title = request.Title,
                    Description = request.Description,
                    DueDate = request.DueDate,
                    CreatedAt = DateTime.UtcNow
                };
                _documents.Add(newDoc);
                return newDoc;
            }
        }

        public int Count()
        {
            lock (_lock)
            {
                return _documents.Count;
            }
        }
    }
}