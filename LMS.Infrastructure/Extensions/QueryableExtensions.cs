using LMS.Shared.DTOs.CommonDtos;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        PageParameters pageParameters)
    {
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageParameters.PageNumber - 1) * pageParameters.PageSize)
            .Take(pageParameters.PageSize)
            .ToListAsync();

        return new PagedList<T>(
            items,
            pageParameters.PageNumber,
            pageParameters.PageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageParameters.PageSize));
    }
}