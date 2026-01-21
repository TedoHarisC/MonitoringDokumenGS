using System;
using System.Collections.Generic;
using System.Linq;
using MonitoringDokumenGS.Dtos.Common;

namespace MonitoringDokumenGS.Extensions
{
    public static class PaginationExtensions
    {
        /// <summary>
        /// Converts IEnumerable to PagedResponse with pagination metadata.
        /// </summary>
        public static PagedResponse<T> ToPagedResponse<T>(this IEnumerable<T> source, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var items = source.ToList();
            var totalCount = items.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var skip = (page - 1) * pageSize;
            var pagedItems = items.Skip(skip).Take(pageSize).ToList();

            return new PagedResponse<T>(
                pagedItems,
                page,
                pageSize,
                totalCount,
                totalPages
            );
        }

        /// <summary>
        /// Converts IQueryable to PagedResponse with pagination metadata (deferred execution).
        /// </summary>
        public static PagedResponse<T> ToPagedResponse<T>(this IQueryable<T> source, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalCount = source.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var skip = (page - 1) * pageSize;
            var pagedItems = source.Skip(skip).Take(pageSize).ToList();

            return new PagedResponse<T>(
                pagedItems,
                page,
                pageSize,
                totalCount,
                totalPages
            );
        }
    }
}
