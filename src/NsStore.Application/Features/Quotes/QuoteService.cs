using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;

namespace NsStore.Application.Features.Quotes;

public record QuoteDto(
    long Id,
    DateOnly QuoteDate,
    string ClientName,
    string? Phone,
    string Detail,
    decimal Price,
    string? SupplierName,
    long OwnerId,
    string OwnerUsername);

public record QuoteRequest(
    DateOnly QuoteDate,
    string ClientName,
    string? Phone,
    string Detail,
    decimal Price,
    string? SupplierName);

public record QuoteQuery(string? Search, DateOnly? Date, int Page = 1, int PageSize = 25);

public class QuoteRequestValidator : AbstractValidator<QuoteRequest>
{
    public QuoteRequestValidator()
    {
        RuleFor(x => x.QuoteDate).NotEqual(default(DateOnly));
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.Detail).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SupplierName).MaximumLength(160);
    }
}

public class QuoteService(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<PagedResult<QuoteDto>> ListAsync(QuoteQuery query, CancellationToken cancellationToken = default)
    {
        var request = new PageRequest(query.Search, query.Page, query.PageSize);
        var quotes = db.Quotes.AsNoTracking().AsQueryable();

        if (request.TrimmedSearch is { } search)
        {
            quotes = quotes.Where(q => EF.Functions.Like(q.ClientName.ToLower(), $"%{search.ToLower()}%"));
        }

        if (query.Date is { } date)
        {
            quotes = quotes.Where(q => q.QuoteDate == date);
        }

        return await quotes
            .OrderByDescending(q => q.QuoteDate)
            .ThenByDescending(q => q.Id)
            .Select(ProjectToDto)
            .ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<QuoteDto> GetAsync(long id, CancellationToken cancellationToken = default) =>
        await db.Quotes.AsNoTracking().Where(q => q.Id == id).Select(ProjectToDto)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(Quote), id);

    public async Task<QuoteDto> CreateAsync(QuoteRequest request, CancellationToken cancellationToken = default)
    {
        var ownerId = currentUser.UserId
            ?? throw new UnauthorizedException(ErrorCodes.Unauthorized, "No authenticated user");

        var quote = new Quote { OwnerId = ownerId };
        Apply(quote, request);

        db.Quotes.Add(quote);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(quote.Id, cancellationToken);
    }

    public async Task<QuoteDto> UpdateAsync(long id, QuoteRequest request, CancellationToken cancellationToken = default)
    {
        var quote = await FindAsync(id, cancellationToken);
        if (!currentUser.IsAdmin && quote.OwnerId != currentUser.UserId)
        {
            throw new ForbiddenException("Only the owner or an admin can edit this quote");
        }

        Apply(quote, request);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(quote.Id, cancellationToken);
    }

    /// <summary>Admin only — sellers cannot delete, matching the legacy permission model.</summary>
    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var quote = await FindAsync(id, cancellationToken);
        quote.DeletedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Quote> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Quotes.FirstOrDefaultAsync(q => q.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Quote), id);

    private static void Apply(Quote quote, QuoteRequest request)
    {
        quote.QuoteDate = request.QuoteDate;
        quote.ClientName = request.ClientName.Trim();
        quote.Phone = request.Phone?.Trim();
        quote.Detail = request.Detail.Trim();
        quote.Price = decimal.Round(request.Price, 2, MidpointRounding.AwayFromZero);
        quote.SupplierName = request.SupplierName?.Trim();
    }

    private static readonly System.Linq.Expressions.Expression<Func<Quote, QuoteDto>> ProjectToDto =
        q => new QuoteDto(
            q.Id,
            q.QuoteDate,
            q.ClientName,
            q.Phone,
            q.Detail,
            q.Price,
            q.SupplierName,
            q.OwnerId,
            q.Owner.Username);
}
