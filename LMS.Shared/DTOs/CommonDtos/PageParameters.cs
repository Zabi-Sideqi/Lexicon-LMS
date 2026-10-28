namespace LMS.Shared.DTOs.CommonDtos;

public class PageParameters
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;
    private const int DefaultPageNumber = 1;

    private int _pageNumber = DefaultPageNumber;
    private int _pageSize = DefaultPageSize;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? DefaultPageNumber : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? DefaultPageSize : (value > MaxPageSize ? MaxPageSize : value);
    }

    public string? SearchTerm { get; set; }
    public string? SortOrder { get; set; }
}