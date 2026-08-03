using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Branches;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Settings;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Sales;

public class SaleService(
    IAppDbContext db,
    InventoryService inventory,
    SerialService serials,
    BranchService branches,
    IStockLockService stockLock,
    IDocumentNumberService documentNumbers,
    SettingsService settings,
    ICurrentUser currentUser,
    TimeProvider clock)
{
    public async Task<PagedResult<SaleListItemDto>> ListAsync(SaleQuery query, CancellationToken cancellationToken = default)
    {
        // Billing is not readable across branches: a seller is pinned to their own whatever they ask.
        query = query with { BranchId = currentUser.ResolveScopedBranch(query.BranchId) };

        var request = new PageRequest(query.Search, query.Page, query.PageSize);
        var sales = Filter(db.Sales.AsNoTracking(), query, request);

        return await ToListResultAsync(
            sales.OrderByDescending(s => s.SaleDate).ThenByDescending(s => s.Id),
            request,
            cancellationToken);
    }

    /// <summary>Credit sales with an outstanding balance (legacy "No pagadas").</summary>
    public async Task<PagedResult<SaleListItemDto>> ListDebtsAsync(SaleQuery query, CancellationToken cancellationToken = default)
    {
        query = query with { BranchId = currentUser.ResolveScopedBranch(query.BranchId) };

        var request = new PageRequest(query.Search, query.Page, query.PageSize);
        var sales = Filter(db.Sales.AsNoTracking(), query with { Status = null }, request)
            .Where(s => s.PaymentStatus == PaymentStatus.Credit && s.TotalPaid < s.TotalAmount);

        return await ToListResultAsync(
            sales.OrderBy(s => s.SaleDate).ThenBy(s => s.Id),
            request,
            cancellationToken);
    }

    /// <summary>
    /// The collections screen: one row per client who still owes, newest debt aside — the order is
    /// worst-overdue first, because that is the order someone works the phone in.
    /// </summary>
    /// <remarks>
    /// Branch scope follows the same policy as every other money read: an admin sees the whole
    /// business consolidated, a seller only their own branch. A client's debt is the business's
    /// debt, not one till's, so the aggregate deliberately spans branches when the caller may see
    /// them — a sale made at one branch can be settled at another.
    /// </remarks>
    public async Task<PagedResult<ClientDebtDto>> ListDebtsByClientAsync(
        ClientDebtQuery query,
        CancellationToken cancellationToken = default)
    {
        var branchId = currentUser.ResolveScopedBranch(query.BranchId);
        var overdueDays = (await settings.GetAsync(cancellationToken)).OverdueDays;
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        var request = new PageRequest(query.Search, query.Page, query.PageSize);

        var owing = Filter(
            db.Sales.AsNoTracking(),
            new SaleQuery(query.Search, null, null, null, BranchId: branchId),
            request)
            .Where(s => s.TotalPaid < s.TotalAmount);

        var grouped = await owing
            .GroupBy(s => s.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                SaleCount = g.Count(),
                TotalAmount = g.Sum(s => s.TotalAmount),
                TotalPaid = g.Sum(s => s.TotalPaid),
                OldestSaleDate = g.Min(s => s.SaleDate)
            })
            .ToListAsync(cancellationToken);

        if (grouped.Count == 0)
        {
            return new PagedResult<ClientDebtDto>([], request.NormalizedPage, request.NormalizedPageSize, 0);
        }

        var clientIds = grouped.Select(g => g.ClientId).ToList();

        // Second query rather than a SelectMany inside the grouping projection: that shape does not
        // translate reliably, and the client count here is already bounded by the page above.
        var lastPayments = await db.Payments.AsNoTracking()
            .Where(p => clientIds.Contains(p.Sale.ClientId))
            .GroupBy(p => p.Sale.ClientId)
            .Select(g => new { ClientId = g.Key, Last = g.Max(p => p.PaymentDate) })
            .ToDictionaryAsync(x => x.ClientId, x => x.Last, cancellationToken);

        var clients = await db.Clients.AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                Name = c.Type == ClientType.Company
                    ? c.Name
                    : (c.Name + " " + (c.LastName ?? "") + " " + (c.MotherLastName ?? "")).Trim(),
                Document = c.Nit ?? c.Ci,
                c.Phone
            })
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var rows = grouped
            .Select(g =>
            {
                var client = clients[g.ClientId];
                var lastPayment = lastPayments.TryGetValue(g.ClientId, out var last) ? last : (DateOnly?)null;
                var since = lastPayment ?? g.OldestSaleDate;
                var days = Math.Max(0, today.DayNumber - since.DayNumber);

                return new ClientDebtDto(
                    g.ClientId,
                    client.Name,
                    client.Document,
                    client.Phone,
                    g.SaleCount,
                    g.TotalAmount,
                    g.TotalPaid,
                    g.TotalAmount - g.TotalPaid,
                    g.OldestSaleDate,
                    lastPayment,
                    days,
                    days > overdueDays);
            })
            .Where(r => query.Status switch
            {
                ClientDebtFilter.Overdue => r.IsOverdue,
                ClientDebtFilter.Current => !r.IsOverdue,
                _ => true
            })
            .OrderByDescending(r => r.IsOverdue)
            .ThenByDescending(r => r.DaysOutstanding)
            .ThenByDescending(r => r.Balance)
            .ToList();

        // Paged in memory: the status filter is only decidable after aggregating, so a SQL-side
        // Skip/Take would page the wrong set.
        return new PagedResult<ClientDebtDto>(
            [.. rows.Skip(request.Skip).Take(request.NormalizedPageSize)],
            request.NormalizedPage,
            request.NormalizedPageSize,
            rows.Count);
    }

    public async Task<PagedResult<SaleListItemDto>> ListByClientAsync(long clientId, PageRequest request, CancellationToken cancellationToken = default)
    {
        if (!await db.Clients.AnyAsync(c => c.Id == clientId, cancellationToken))
        {
            throw new NotFoundException(nameof(Client), clientId);
        }

        var branchId = currentUser.ResolveScopedBranch();

        return await ToListResultAsync(
            db.Sales.AsNoTracking()
                .Where(s => s.ClientId == clientId && (branchId == null || s.BranchId == branchId))
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.Id),
            request,
            cancellationToken);
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
                    i.Serials.OrderBy(x => x.SerialNumber).Select(x => x.SerialNumber).ToList(),
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
            sale.Sale.Number,
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
                i.Serials.OrderBy(x => x.SerialNumber).Select(x => x.SerialNumber).ToList(),
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

            // Serials are judged per product, not per line: T and S are per (branch, product), so
            // two lines of one product could each pass the pick rule alone yet overdraw together.
            // Across the whole request, so one serial cannot be claimed by two lines.
            SerialService.NormalizeBatch([.. request.Items.SelectMany(i => i.SerialNumbers ?? [])]);

            var serialsByProduct = request.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)[.. g.SelectMany(i => i.SerialNumbers ?? [])]);

            // Resolved before the counter is drawn, so a sale rejected for its serials burns no folio.
            var resolved = new Dictionary<long, List<ProductSerial>>();
            foreach (var (productId, quantity) in quantities)
            {
                resolved[productId] = [.. await serials.ResolveOutboundAsync(
                    branchId, products[productId], quantity, serialsByProduct.GetValueOrDefault(productId), ct)];
            }

            // After the stock locks, never before: the branch counter is the second lockable
            // resource, and taking it last keeps a branch's sales from serialising any longer than
            // they must. Read inside the action so a retry gets a fresh number.
            var branch = await db.Branches.FirstAsync(b => b.Id == branchId, ct);
            var sequence = await documentNumbers.NextAsync(branchId, DocumentKind.Sale, ct);

            var sale = new Sale
            {
                SaleDate = request.SaleDate,
                BranchId = branchId,
                BranchSequence = sequence,
                Number = branch.FormatDocumentNumber(sequence),
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

            // sale.Items was built from request.Items in order, so each line keeps the serials that
            // arrived on it — the warranty note has to say which unit went out on which line.
            foreach (var (item, line) in request.Items.Zip(sale.Items))
            {
                foreach (var name in item.SerialNumbers ?? [])
                {
                    var serial = resolved[item.ProductId]
                        .First(s => string.Equals(s.SerialNumber, name.Trim(), StringComparison.OrdinalIgnoreCase));

                    serial.MarkSold(line, now);
                    serials.RecordEvent(serial, SerialEventType.Sold, branchId, "sale", sale.Id);
                }
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
    /// Collects one amount from a client and spreads it across their unpaid sales — over the ones
    /// the caller named, or oldest-first when it named none — issuing a numbered receipt for the
    /// whole act.
    /// </summary>
    /// <remarks>
    /// Oldest-first stays the default because it keeps a debt from ageing forever while the client
    /// keeps paying. Explicit allocations exist because the counter often settles a named invoice,
    /// and guessing at that leaves the receipt disagreeing with what the customer asked for.
    /// The whole thing is one transaction — a partially applied collection would leave the customer
    /// holding a receipt for money the ledger disagrees about.
    /// </remarks>
    public async Task<CollectionReceiptDto> CollectFromClientAsync(
        CollectDebtRequest request,
        CancellationToken cancellationToken = default)
    {
        var branchId = currentUser.RequireWritableBranch();
        var amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);

        if (amount <= 0)
        {
            throw new BadRequestException("Collected amount must be greater than zero");
        }

        var receiptId = await db.ExecuteInTransactionAsync(async ct =>
        {
            await branches.EnsureWritableAsync(branchId, ct);

            if (!await db.Clients.AnyAsync(c => c.Id == request.ClientId, ct))
            {
                throw new NotFoundException(nameof(Client), request.ClientId);
            }

            // Read across branches: a debt belongs to the business, and the client is standing at
            // whichever counter they walked into.
            var owing = await db.Sales
                .Include(s => s.Payments)
                .Where(s => s.ClientId == request.ClientId && s.TotalPaid < s.TotalAmount)
                .OrderBy(s => s.SaleDate)
                .ThenBy(s => s.Id)
                .ToListAsync(ct);

            if (owing.Count == 0)
            {
                throw new BadRequestException($"Client {request.ClientId} has no outstanding balance");
            }

            var outstanding = owing.Sum(s => s.Balance);
            if (amount > outstanding)
            {
                throw new ConflictException(
                    ErrorCodes.PaymentExceedsBalance,
                    $"Collected {amount} exceeds the client's outstanding balance {outstanding}");
            }

            var now = clock.GetUtcNow();
            var paymentDate = request.PaymentDate ?? DateOnly.FromDateTime(now.UtcDateTime);

            // Taken inside the transaction and never cached outside it: ExecuteInTransactionAsync
            // may retry the whole action, and a retry has to read a fresh number.
            var branch = await db.Branches.FirstAsync(b => b.Id == branchId, ct);
            var sequence = await documentNumbers.NextAsync(branchId, DocumentKind.Receipt, ct);

            var receipt = new PaymentReceipt
            {
                ClientId = request.ClientId,
                BranchId = branchId,
                BranchSequence = sequence,
                Number = branch.FormatDocumentNumber(sequence),
                ReceiptDate = paymentDate,
                TotalAmount = amount
            };

            db.PaymentReceipts.Add(receipt);

            foreach (var (sale, applied) in ResolveAllocations(request, owing, amount))
            {
                var payment = sale.RegisterPayment(applied, paymentDate, branchId, currentUser.UserId, now);
                payment.Receipt = receipt;

                db.Payments.Add(payment);
            }

            await db.SaveChangesAsync(ct);
            return receipt.Id;
        }, cancellationToken);

        return await GetCollectionReceiptAsync(receiptId, cancellationToken);
    }

    /// <summary>
    /// Decides what each sale absorbs of a collection: the caller's own breakdown when it sent one,
    /// otherwise the oldest-first walk. Only the split is decided here — whether a sale can actually
    /// take its share is <see cref="Sale.RegisterPayment"/>'s call, against the balance it reads
    /// inside the transaction rather than whatever the caller was looking at.
    /// </summary>
    private static List<(Sale Sale, decimal Applied)> ResolveAllocations(
        CollectDebtRequest request,
        List<Sale> owing,
        decimal amount)
    {
        var result = new List<(Sale, decimal)>();

        if (request.Allocations is not { Count: > 0 } allocations)
        {
            var remaining = amount;
            foreach (var sale in owing)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var applied = Math.Min(remaining, sale.Balance);
                result.Add((sale, applied));
                remaining -= applied;
            }

            return result;
        }

        foreach (var allocation in allocations)
        {
            // Missing covers all three ways a sale can fail to belong here — another client's,
            // already settled, or soft-deleted — and none of them is worth telling apart.
            var sale = owing.FirstOrDefault(s => s.Id == allocation.SaleId)
                ?? throw new NotFoundException(nameof(Sale), allocation.SaleId);

            result.Add((sale, decimal.Round(allocation.Amount, 2, MidpointRounding.AwayFromZero)));
        }

        // The receipt is printed from Amount, so a breakdown that does not add up to it would hand
        // the customer paper that contradicts the ledger.
        var allocated = result.Sum(entry => entry.Item2);
        if (allocated != amount)
        {
            throw new BadRequestException(
                $"Allocations total {allocated}, which does not match the collected amount {amount}");
        }

        return result;
    }

    /// <summary>Reissues a receipt: the customer's copy has to survive being lost.</summary>
    public async Task<CollectionReceiptDto> GetCollectionReceiptAsync(long receiptId, CancellationToken cancellationToken = default)
    {
        var receipt = await db.PaymentReceipts.AsNoTracking()
            .Where(r => r.Id == receiptId)
            .Select(r => new
            {
                r.Id,
                r.Number,
                r.BranchId,
                BranchCode = r.Branch.Code,
                r.ClientId,
                ClientName = r.Client.Type == ClientType.Company
                    ? r.Client.Name
                    : (r.Client.Name + " " + (r.Client.LastName ?? "") + " " + (r.Client.MotherLastName ?? "")).Trim(),
                ClientDocument = r.Client.Nit ?? r.Client.Ci,
                ClientPhone = r.Client.Phone,
                r.ReceiptDate,
                r.TotalAmount,
                CreatedByName = db.Users.Where(u => u.Id == r.CreatedBy).Select(u => u.Username).FirstOrDefault(),
                Allocations = r.Payments
                    .OrderBy(p => p.Sale.SaleDate)
                    .ThenBy(p => p.SaleId)
                    .Select(p => new PaymentAllocationDto(
                        p.SaleId,
                        p.Sale.Number,
                        p.Sale.SaleDate,
                        p.Sale.TotalAmount,
                        p.Amount,
                        p.Sale.TotalAmount - p.Sale.TotalPaid,
                        p.Sale.TotalPaid >= p.Sale.TotalAmount))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(PaymentReceipt), receiptId);

        // What the client still owes now, not at the time of the receipt: a reissued copy should
        // tell them where they actually stand.
        var remainingDebt = await db.Sales.AsNoTracking()
            .Where(s => s.ClientId == receipt.ClientId && s.TotalPaid < s.TotalAmount)
            .SumAsync(s => (decimal?)(s.TotalAmount - s.TotalPaid), cancellationToken) ?? 0m;

        return new CollectionReceiptDto(
            receipt.Id,
            receipt.Number,
            receipt.BranchId,
            receipt.BranchCode,
            receipt.ClientId,
            receipt.ClientName,
            receipt.ClientDocument,
            receipt.ClientPhone,
            receipt.ReceiptDate,
            receipt.TotalAmount,
            remainingDebt,
            receipt.CreatedByName,
            receipt.Allocations);
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

        if (query.BranchId is { } branchId)
        {
            sales = sales.Where(s => s.BranchId == branchId);
        }

        if (query.ClientId is { } clientId)
        {
            sales = sales.Where(s => s.ClientId == clientId);
        }

        return sales;
    }

    /// <summary>
    /// Pages the query and finishes the two day-derived fields in memory. The subtraction is not
    /// pushed to SQL on purpose: <see cref="DateOnly"/> arithmetic translates differently on Npgsql
    /// and on the SQLite the tests run against, and this is a handful of rows either way.
    /// </summary>
    private async Task<PagedResult<SaleListItemDto>> ToListResultAsync(
        IQueryable<Sale> sales,
        PageRequest request,
        CancellationToken cancellationToken)
    {
        var overdueDays = (await settings.GetAsync(cancellationToken)).OverdueDays;
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        var page = await sales.Select(ProjectToRow).ToPagedResultAsync(request, cancellationToken);

        return new PagedResult<SaleListItemDto>(
            [.. page.Items.Select(row => row.ToDto(overdueDays, today))],
            page.Page,
            page.PageSize,
            page.Total);
    }

    /// <summary>The SQL-shaped row behind <see cref="SaleListItemDto"/>.</summary>
    private sealed record SaleListRow(
        long Id,
        DateOnly SaleDate,
        long BranchId,
        string BranchCode,
        string Number,
        long ClientId,
        string ClientName,
        string? ClientDocument,
        InvoiceType InvoiceType,
        PaymentStatus PaymentStatus,
        int TotalQuantity,
        decimal TotalAmount,
        decimal TotalPaid,
        DateOnly? LastPaymentDate,
        string? CreatedByName)
    {
        public SaleListItemDto ToDto(int overdueDays, DateOnly today)
        {
            var balance = TotalAmount - TotalPaid;

            // A settled sale is not "outstanding" for any number of days, however old it is.
            var since = LastPaymentDate ?? SaleDate;
            var days = balance > 0 ? Math.Max(0, today.DayNumber - since.DayNumber) : 0;

            return new SaleListItemDto(
                Id, SaleDate, BranchId, BranchCode, Number, ClientId, ClientName, ClientDocument,
                InvoiceType, PaymentStatus, TotalQuantity, TotalAmount, TotalPaid, balance,
                LastPaymentDate, days, balance > 0 && days > overdueDays, CreatedByName);
        }
    }

    private System.Linq.Expressions.Expression<Func<Sale, SaleListRow>> ProjectToRow =>
        s => new SaleListRow(
            s.Id,
            s.SaleDate,
            s.BranchId,
            s.Branch.Code,
            s.Number,
            s.ClientId,
            s.Client.Type == ClientType.Company
                ? s.Client.Name
                : (s.Client.Name + " " + (s.Client.LastName ?? "") + " " + (s.Client.MotherLastName ?? "")).Trim(),
            s.Client.Nit ?? s.Client.Ci,
            s.InvoiceType,
            s.PaymentStatus,
            s.TotalQuantity,
            s.TotalAmount,
            s.TotalPaid,
            s.Payments.Max(p => (DateOnly?)p.PaymentDate),
            db.Users.Where(u => u.Id == s.CreatedBy).Select(u => u.Username).FirstOrDefault());
}
