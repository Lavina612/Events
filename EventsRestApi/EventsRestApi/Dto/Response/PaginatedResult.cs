namespace EventsRestApi.Dto.Response
{
    public record PaginatedResult<T>(
        List<T> ItemsForPage,
        int TotalCount,
        int Page,
        int PageSize)
    {
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    }
}
