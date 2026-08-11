namespace EventsRestApi.Dto.Response
{
    public record PaginatedResult<T>(
        List<T> ItemsForPage,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages);
}
