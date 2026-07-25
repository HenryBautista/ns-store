using Microsoft.EntityFrameworkCore;

namespace NsStore.Application.Common.Models;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);

/// <summary>Common collection query string: <c>?search=&amp;page=&amp;pageSize=</c>.</summary>
public record PageRequest(string? Search = null, int Page = 1, int PageSize = 25)
{
    public const int MaxPageSize = 200;

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize => PageSize switch
    {
        < 1 => 25,
        > MaxPageSize => MaxPageSize,
        _ => PageSize
    };

    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;

    public string? TrimmedSearch => string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
}

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Skip)
            .Take(request.NormalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, request.NormalizedPage, request.NormalizedPageSize, total);
    }
}
