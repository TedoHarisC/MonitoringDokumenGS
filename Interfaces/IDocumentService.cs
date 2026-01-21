using System.Collections.Generic;
using MonitoringDokumenGS.Dtos.Documents;
using MonitoringDokumenGS.Dtos.Common;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IDocumentService
    {
        IEnumerable<DocumentResponse> GetAll();
        PagedResponse<DocumentResponse> GetPaged(int page, int pageSize);
        DocumentResponse? GetById(int id);
        DocumentResponse Create(CreateDocumentRequest request);
        int Count();
    }
}