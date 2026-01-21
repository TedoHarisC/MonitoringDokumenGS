using System.Collections.Generic;

namespace MonitoringDokumenGS.Dtos.Common
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;

        public PagedResponse(IEnumerable<T> items, int page, int pageSize, int totalCount, int totalPages)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = totalPages;
        }
    }
}