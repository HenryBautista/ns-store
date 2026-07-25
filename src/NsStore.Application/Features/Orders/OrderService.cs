using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Orders;

public class OrderService(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<PagedResult<OrderDto>> ListAsync(OrderQuery query, CancellationToken cancellationToken = default)
    {
        var request = new PageRequest(query.Search, query.Page, query.PageSize);
        var orders = db.Orders.AsNoTracking().AsQueryable();

        if (request.TrimmedSearch is { } search)
        {
            var pattern = $"%{search.ToLower()}%";
            orders = orders.Where(o =>
                EF.Functions.Like(o.ClientName.ToLower(), pattern) ||
                EF.Functions.Like(o.ProductDescription.ToLower(), pattern));
        }

        if (query.Date is { } date)
        {
            orders = orders.Where(o => o.OrderDate == date);
        }

        return await orders
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.Id)
            .Select(ProjectToDto)
            .ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<OrderDto> GetAsync(long id, CancellationToken cancellationToken = default) =>
        await db.Orders.AsNoTracking().Where(o => o.Id == id).Select(ProjectToDto)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(Order), id);

    public async Task<OrderDto> CreateAsync(OrderRequest request, CancellationToken cancellationToken = default)
    {
        var ownerId = currentUser.UserId
            ?? throw new UnauthorizedException(ErrorCodes.Unauthorized, "No authenticated user");

        var order = new Order { OwnerId = ownerId };
        Apply(order, request);
        order.EnsureAdvanceWithinPrice();

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(order.Id, cancellationToken);
    }

    public async Task<OrderDto> UpdateAsync(long id, OrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = await FindAsync(id, cancellationToken);
        EnsureCanEdit(order);

        Apply(order, request);
        order.EnsureAdvanceWithinPrice();

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(order.Id, cancellationToken);
    }

    /// <summary>Admin only — sellers cannot delete, matching the legacy permission model.</summary>
    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var order = await FindAsync(id, cancellationToken);
        order.DeletedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Order> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Order), id);

    /// <summary>A seller may edit only their own orders; an admin may edit any.</summary>
    private void EnsureCanEdit(Order order)
    {
        if (!currentUser.IsAdmin && order.OwnerId != currentUser.UserId)
        {
            throw new ForbiddenException("Only the owner or an admin can edit this order");
        }
    }

    private static void Apply(Order order, OrderRequest request)
    {
        order.OrderDate = request.OrderDate;
        order.ClientName = request.ClientName.Trim();
        order.Phone = request.Phone?.Trim();
        order.ProductDescription = request.ProductDescription.Trim();
        order.Price = decimal.Round(request.Price, 2, MidpointRounding.AwayFromZero);
        order.AdvanceAmount = decimal.Round(request.AdvanceAmount, 2, MidpointRounding.AwayFromZero);
        order.Notes = request.Notes?.Trim();
        order.Status = request.Status ?? order.Status;
    }

    private static readonly System.Linq.Expressions.Expression<Func<Order, OrderDto>> ProjectToDto =
        o => new OrderDto(
            o.Id,
            o.OrderDate,
            o.ClientName,
            o.Phone,
            o.ProductDescription,
            o.Price,
            o.AdvanceAmount,
            o.Price - o.AdvanceAmount,
            o.Notes,
            o.Status,
            o.OwnerId,
            o.Owner.Username);
}
