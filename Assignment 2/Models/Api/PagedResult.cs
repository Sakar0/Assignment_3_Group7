using System.Collections.Generic;

namespace Assignment2.Models.Api
{
    /// <summary>
    /// Paged wrapper for API collection responses.
    /// Prevents unbounded result sets from the read-only catalogue API.
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
