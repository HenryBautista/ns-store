using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Branches;
using NsStore.Application.Features.Inventory;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Sales;

public class SaleService(
    IAppDbContext db,
    InventoryService inventory,
    BranchService branches,
    IStockLockService stockLock,
    ICurrentUser currentUser,
    TimeProvider clock)
{
    public async Task<PagedResult<SaleListItemDto>> ListAsync(SaleQuery query, CancellationToken cancellationToken = default)
    {
        var request = new PageRequest(query.Search, query.Page, query.PageSize);
        var sales = Filter(db.Sales.AsNoTracking(), query, request);

        return await sales
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.Id)
            .Select(ProjectToListItem)
            .ToPagedResultAsync(request, cancellationToken);
    }

    /// <summary>Credit sales with an outstanding balance (legacy "No pagadas").</summary>
    public async Task<PagedResult<SaleListItemDto>> ListDebtsAsync(SaleQuery query, CancellationToken cancellationToken = default)
    {
        var request = new PageRequest(query.Search, query.Page, query.PageSize);
        var sales = Filter(db.Sales.AsNoTracking(), query with { Status = null }, request)
            .Where(s => s.PaymentStatus == PaymentStatus.Credit && s.TotalPaid < s.TotalAmount);

        return await sales
            .OrderBy(s => s.SaleDate)
            .ThenBy(s => s.Id)
            .Select(ProjectToListItem)
            .ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<PagedResult<SaleListItemDto>> ListByClientAsync(long clientId, PageRequest request, CancellationToken cancellationToken = default)
    {
        if (!await db.Clients.AnyAsync(c => c.Id == clientId, cancellationToken))
        {
            throw new NotFoundException(nameof(Client), clientId);
        }

        return await db.Sales.AsNoTracking()
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.Id)
            .Select(ProjectToListItem)
            .ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<SaleDto> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var sale = await db.Sales.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new
            {
                Sale = s,
                s.Client,
                BranchCode = s.Branch.Code,
                CreatedByName = db.Users.Where(u => u.Id == s.CreatedBy).Select(u => u.Username).FirstOrDefault(),
                Items = s.Items.Select(i => new SaleItemDto(
                    i.Id,
                    i.ProductId,
                    i.Product.Name,
                    i.Product.PartNumber,
                    i.Product.SerialNumber,
                    i.Product.WarrantyTerm != null ? i.Product.WarrantyTerm.Description : null,
                    i.Quantity,
                    i.UnitPrice,
                    i.Subtotal)).ToList(),
                Payments = s.Payments
                    .OrderBy(p => p.PaymentDate)
                    .Select(p => new PaymentDto(
                        p.Id,
                        p.SaleId,
                        p.BranchId,
                        p.Amount,
                        p.PaymentDate,
                        p.CreatedAt,
                        db.Users.Where(u => u.Id == p.CreatedBy).Select(u => u.Username).FirstOrDefault()))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Sale), id);

        return new SaleDto(
            sale.Sale.Id,
            sale.Sale.SaleDate,
            sale.Sale.BranchId,
            sale.BranchCode,
            sale.Sale.ClientId,
            sale.Client.FullName,
            sale.Client.Nit,
            sale.Client.Ci,
            sale.Client.Phone,
            sale.Sale.InvoiceType,
            sale.Sale.PaymentStatus,
            sale.Sale.TotalQuantity,
            sale.Sale.TotalAmount,
            sale.Sale.TotalPaid,
            sale.Sale.Balance,
            sale.Sale.CreatedBy,
            sale.CreatedByName,
            sale.Sale.CreatedAt,
            sale.Items,
            sale.Payments);
    }

    public async Task<IReadOnlyList<SaleItemDto>> GetItemsAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!await db.Sales.AnyAsync(s => s.Id == id, cancellationToken))
        {
            throw new NotFoundException(nameof(Sale), id);
        }

        return await db.SaleItems.AsNoTracking()
            .Where(i => i.SaleId == id)
            .Select(i => new SaleItemDto(
                i.Id,
                i.ProductId,
                i.Product.Name,
                i.Product.PartNumber,
                i.Product.SerialNumber,
                i.Product.WarrantyTerm != null ? i.Product.WarrantyTerm.Description : null,
                i.Quantity,
                i.UnitPrice,
                i.Subtotal))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// The POS operation: price by invoice type, lock and validate stock, write header + lines +
    /// ledger movements, decrement stock and record the initial payment — all in one transaction.
    /// </summary>
    public async Task<SaleDto> CreateAsync(CreateSaleRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new BadRequestException("A sale requires at least one item");
        }

        // The active branch is the only source of truth for a write; the request body carries none,
        // so body and header can never contradict each other.
        var branchId = currentUser.RequireWritableBranch();

        var saleId = await db.ExecuteInTransactionAsync(async ct =>
        {
            await branches.EnsureWritableAsync(branchId, ct);

            if (!await db.Clients.AnyAsync(c => c.Id == request.ClientId, ct))
            {
                throw new NotFoundException(nameof(Client), request.ClientId);
            }

            // Same product on several lines: one stock movement, one validation.
            var quantities = request.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            var productIds = quantities.Keys.ToList();
            await stockLock.LockAsync([.. productIds.Select(id => new StockKey(branchId, id))], ct);

            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, ct);

            var missing = productIds.FirstOrDefault(id => !products.ContainsKey(id));
            if (missing != 0)
            {
                throw new NotFoundException(nameof(Product), missing);
            }

            var now = clock.GetUtcNow();
            var sale = new Sale
            {
                SaleDate = request.SaleDate,
                BranchId = branchId,
                ClientId = request.ClientId,
                InvoiceType = request.InvoiceType,
                PaymentStatus = request.PaymentStatus
            };

            foreach (var item in request.Items)
            {
                var product = products[item.ProductId];
                var unitPrice = product.PriceFor(request.InvoiceType);
                if (unitPrice <= 0)
                {
                    throw new ConflictException(
                        ErrorCodes.PriceNotSet,
                        $"Product {product.Id} has no sale price for the requested invoice type");
                }

                sale.Items.Add(new SaleItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    Subtotal = decimal.Round(unitPrice * item.Quantity, 2, MidpointRounding.AwayFromZero)
                });
            }

            sale.TotalQuantity = sale.Items.Sum(i => i.Quantity);
            sale.TotalAmount = sale.Items.Sum(i => i.Subtotal);
            sale.TotalPaid = 0m;

            db.Sales.Add(sale);
            await db.SaveChangesAsync(ct);

            foreach (var (productId, quantity) in quantities)
            {
                var stock = await inventory.GetOrCreateStockLevelAsync(branchId, productId, now, ct);
                // Throws INSUFFICIENT_STOCK if the sale would drive the level below zero.
                stock.Apply(-quantity, now);

                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = branchId,
                    ProductId = productId,
                    MovementType = MovementType.Sale,
                    QuantityDelta = -quantity,
                    ReferenceType = "sale",
                    ReferenceId = sale.Id
                });
            }

            var initialPaid = ResolveInitialPaid(request, sale.TotalAmount);
            if (initialPaid > 0)
            {
                var payment = sale.RegisterPayment(initialPaid, request.SaleDate, branchId, currentUser.UserId, now);
                db.Payments.Add(payment);
            }

            await db.SaveChangesAsync(ct);
            return sale.Id;
        }, cancellationToken);

        return await GetAsync(saleId, cancellationToken);
    }

    public async Task<SaleDto> RegisterPaymentAsync(long saleId, RegisterPaymentRequest request, CancellationToken cancellationToken = default)
    {
        // The collecting branch, which is not necessarily the branch that made the sale — a credit
        // can be settled anywhere, and the till that has to balance is the one that took the money.
        var branchId = currentUser.RequireWritableBranch();

        await db.ExecuteInTransactionAsync(async ct =>
        {
            await branches.EnsureWritableAsync(branchId, ct);

            var sale = await db.Sales
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == saleId, ct)
                ?? throw new NotFoundException(nameof(Sale), saleId);

            var now = clock.GetUtcNow();
            var amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
            var paymentDate = request.PaymentDate ?? DateOnly.FromDateTime(now.UtcDateTime);

            var payment = sale.RegisterPayment(amount, paymentDate, branchId, currentUser.UserId, now);
            db.Payments.Add(payment);

            await db.SaveChangesAsync(ct);
            return sale.Id;
        }, cancellationToken);

        return await GetAsync(saleId, cancellationToken);
    }

    /// <summary>
    /// A cash sale ("contado") is fully paid; a credit sale may carry an initial payment that,
    /// if it covers the total, settles the sale immediately.
    /// </summary>
    private static decimal ResolveInitialPaid(CreateSaleRequest request, decimal totalAmount)
    {
        if (request.PaymentStatus == PaymentStatus.Paid)
        {
            return totalAmount;
        }

        var requested = decimal.Round(request.InitialPaid ?? 0m, 2, MidpointRounding.AwayFromZero);
        if (requested > totalAmount)
        {
            throw new ConflictException(
                ErrorCodes.PaymentExceedsBalance,
                $"Initial payment {requested} exceeds the sale total {totalAmount}");
        }

        return requested;
    }

    private static IQueryable<Sale> Filter(IQueryable<Sale> sales, SaleQuery query, PageRequest request)
    {
        if (request.TrimmedSearch is { } search)
        {
            var pattern = $"%{search.ToLower()}%";
            sales = sales.Where(s =>
                EF.Functions.Like(s.Client.Name.ToLower(), pattern) ||
                (s.Client.LastName != null && EF.Functions.Like(s.Client.LastName.ToLower(), pattern)) ||
                (s.Client.MotherLastName != null && EF.Functions.Like(s.Client.MotherLastName.ToLower(), pattern)));
        }

        if (query.From is { } from)
        {
            sales = sales.Where(s => s.SaleDate >= from);
        }

        if (query.To is { } to)
        {
            sales = sales.Where(s => s.SaleDate <= to);
        }

        if (query.Status is { } status)
        {
            sales = sales.Where(s => s.PaymentStatus == status);
        }

        return sales;
    }

    private System.Linq.Expressions.Expression<Func<Sale, SaleListItemDto>> ProjectToListItem =>
        s => new SaleListItemDto(
            s.Id,
            s.SaleDate,
            s.BranchId,
            s.Branch.Code,
            s.ClientId,
            s.Client.Type == ClientType.Company
                ? s.Client.Name
                : (s.Client.Name + " " + (s.Client.LastName ?? "") + " " + (s.Client.MotherLastName ?? "")).Trim(),
            s.InvoiceType,
            s.PaymentStatus,
            s.TotalQuantity,
            s.TotalAmount,
            s.TotalPaid,
            s.TotalAmount - s.TotalPaid,
            db.Users.Where(u => u.Id == s.CreatedBy).Select(u => u.Username).FirstOrDefault());
}
