namespace EventsRestApi.Dto
{
    public class PaginatedResult<T>
    {
        public List<T> ItemsForPage { get; set; }

        public int TotalCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public PaginatedResult(
            List<T> itemsForPage,
            int totalCount,
            int page,
            int pageSize,
            int totalPages)
        {
            ItemsForPage = itemsForPage;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
            TotalPages = totalPages;
        }
    }
}
