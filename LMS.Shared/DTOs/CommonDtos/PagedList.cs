namespace LMS.Shared.DTOs.CommonDtos;

public sealed record PagedList<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);