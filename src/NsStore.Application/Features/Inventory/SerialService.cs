using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Inventory;

/// <summary>
/// Owns which physical units back a branch's stock, and the one rule that decides how many of them
/// a movement has to name.
/// </summary>
/// <remarks>
/// <para>
/// <b>The pick rule.</b> Switching tracking on for a product that already has stock must not force
/// the shop to walk the shelves counting serials, so a branch may hold units that carry no serial.
/// With <c>T</c> units of stock and <c>S</c> of them identified, <c>U = T - S</c> are anonymous.
/// Moving <c>Q</c> units out therefore requires naming at least <c>max(0, Q - U)</c> serials and at
/// most <c>min(Q, S)</c>.
/// </para>
/// <para>
/// This keeps <c>S &lt;= T</c> — the invariant everything here rests on — and it self-tightens: while
/// anonymous units remain they are spent first for free, and once the shelf has rotated (<c>T == S</c>)
/// every unit sold must be named. The window is empty only when <c>Q &gt; T</c>, which is exactly when
/// <see cref="StockLevel.Apply"/> would reject the movement anyway, so it can never wedge a seller
/// who actually has the goods.
/// </para>
/// <para>
/// <b>Sequencing (load-bearing).</b> Every caller must already hold the pessimistic lock on the
/// <c>(branch, product)</c> stock row and be inside its transaction before calling in here. That is
/// what makes two tills picking the same serial safe: the second blocks until the first commits,
/// re-reads the unit as sold and is rejected. The <c>Version</c> token catches whatever slips past.
/// </para>
/// </remarks>
public class SerialService(IAppDbContext db, ICurrentUser currentUser, IStockLockService stockLock, TimeProvider clock)
{
    public async Task<PagedResult<ProductSerialDto>> ListAsync(
        SerialQuery query,
        CancellationToken cancellationToken = default)
    {
        var request = query.ToPageRequest();
        var branchId = currentUser.ResolveReadableBranch(query.BranchId);
        var serials = db.ProductSerials.AsNoTracking().Where(s => s.BranchId == branchId);

        if (query.ProductId is { } productId)
        {
            serials = serials.Where(s => s.ProductId == productId);
        }

        if (query.Status is { } status)
        {
            serials = serials.Where(s => s.Status == status);
        }

        if (request.SearchPattern is { } pattern)
        {
            serials = serials.Where(s => EF.Functions.Like(SearchText.Unaccent(s.SerialNumber).ToLower(), pattern));
        }

        return await serials
            .OrderBy(s => s.SerialNumber)
            .Select(s => new ProductSerialDto(
                s.Id,
                s.ProductId,
                s.Product.Name,
                s.SerialNumber,
                s.Status,
                s.BranchId,
                s.Branch.Code,
                s.ReceivedAt,
                s.SoldAt))
            .ToPagedResultAsync(request, cancellationToken);
    }

    /// <summary>
    /// Answers "did we sell this unit, and to whom?" — the reason the feature exists.
    /// </summary>
    /// <remarks>
    /// No branch guard: a unit sold at one counter gets claimed at another, and refusing to look it
    /// up would defeat the point. The product is read through <c>IgnoreQueryFilters</c> because a
    /// discontinued product is soft-deleted, and a warranty claim on an old unit is precisely when
    /// its name still has to resolve.
    /// </remarks>
    public async Task<SerialLookupDto> LookupAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(serialNumber);
        if (normalized.Length == 0)
        {
            throw new BadRequestException("A serial number is required");
        }

        var lowered = normalized.ToLower();

        return await db.ProductSerials.AsNoTracking()
            .Where(s => s.SerialNumber.ToLower() == lowered)
            .Select(s => new SerialLookupDto(
                s.Id,
                s.SerialNumber,
                s.ProductId,
                db.Products.IgnoreQueryFilters()
                    .Where(p => p.Id == s.ProductId)
                    .Select(p => p.Name)
                    .FirstOrDefault()!,
                db.Products.IgnoreQueryFilters()
                    .Where(p => p.Id == s.ProductId)
                    .Select(p => p.PartNumber)
                    .FirstOrDefault(),
                db.Products.IgnoreQueryFilters()
                    .Where(p => p.Id == s.ProductId && p.WarrantyTerm != null)
                    .Select(p => p.WarrantyTerm!.Description)
                    .FirstOrDefault(),
                s.Status,
                s.BranchId,
                s.Branch.Code,
                s.ReceivedAt,
                s.SaleItem == null
                    ? null
                    : new SerialSaleReferenceDto(
                        s.SaleItem.SaleId,
                        s.SaleItem.Sale.Number,
                        s.SaleItem.Sale.SaleDate,
                        s.SaleItem.Sale.ClientId,
                        s.SaleItem.Sale.Client.FullName,
                        s.SaleItem.Sale.Client.Nit ?? s.SaleItem.Sale.Client.Ci,
                        s.SaleItem.UnitPrice)))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(ProductSerial), normalized);
    }

    public async Task<IReadOnlyList<SerialEventDto>> GetHistoryAsync(long serialId, CancellationToken cancellationToken = default)
    {
        if (!await db.ProductSerials.AnyAsync(s => s.Id == serialId, cancellationToken))
        {
            throw new NotFoundException(nameof(ProductSerial), serialId);
        }

        return await db.ProductSerialEvents.AsNoTracking()
            .Where(e => e.SerialId == serialId)
            // By id, not CreatedAt: ledger ids are monotonic so the order is the same, and SQLite
            // (the test provider) cannot ORDER BY a DateTimeOffset.
            .OrderBy(e => e.Id)
            .Select(e => new SerialEventDto(
                e.Id,
                e.EventType,
                e.BranchId,
                e.Branch.Code,
                e.ReferenceType,
                e.ReferenceId,
                e.Notes,
                e.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Names units the branch already counted. Moves no stock and writes no ledger entry — the
    /// units were always on the shelf.
    /// </summary>
    public async Task<IReadOnlyList<ProductSerialDto>> RegisterAsync(
        RegisterSerialsRequest request,
        CancellationToken cancellationToken = default)
    {
        var branchId = currentUser.RequireWritableBranch(request.BranchId);

        var ids = await db.ExecuteInTransactionAsync(async ct =>
        {
            // Reads T, so it takes the same lock every stock-moving path does.
            await stockLock.LockAsync([new StockKey(branchId, request.ProductId)], ct);

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
                ?? throw new NotFoundException(nameof(Product), request.ProductId);

            if (!product.IsSerialized)
            {
                throw new BadRequestException(
                    $"Product {product.Id} is not tracked by serial number",
                    ErrorCodes.SerialsNotTracked);
            }

            var names = NormalizeBatch(request.SerialNumbers);
            await EnsureUnusedAsync(names, ct);

            var onHand = await StockQuantityAsync(branchId, product.Id, ct);
            var identified = await IdentifiedCountAsync(branchId, product.Id, ct);

            if (identified + names.Count > onHand)
            {
                throw new ConflictException(
                    ErrorCodes.SerialStockExceeded,
                    $"Branch {branchId} holds {onHand} of product {product.Id} with {identified} already " +
                    $"identified; registering {names.Count} more would exceed the count on hand");
            }

            var now = clock.GetUtcNow();
            var created = names
                .Select(name => Build(product.Id, name, branchId, purchaseItemId: null, now))
                .ToList();

            db.ProductSerials.AddRange(created);
            foreach (var serial in created)
            {
                RecordEvent(serial, SerialEventType.Registered, branchId, "manual", referenceId: null);
            }

            await db.SaveChangesAsync(ct);
            return created.Select(s => s.Id).ToList();
        }, cancellationToken);

        return await db.ProductSerials.AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .OrderBy(s => s.SerialNumber)
            .Select(s => new ProductSerialDto(
                s.Id,
                s.ProductId,
                s.Product.Name,
                s.SerialNumber,
                s.Status,
                s.BranchId,
                s.Branch.Code,
                s.ReceivedAt,
                s.SoldAt))
            .ToListAsync(cancellationToken);
    }

    /// <summary>Pairs holding more identified units than stock. Healthy systems return nothing.</summary>
    public async Task<IReadOnlyList<SerialDriftDto>> GetDriftAsync(CancellationToken cancellationToken = default) =>
        // Filtered before the projection, not after: a predicate over a projected subquery is not
        // something either provider will translate.
        await db.StockLevels.AsNoTracking()
            .Where(s => db.ProductSerials.Count(x =>
                x.ProductId == s.ProductId &&
                x.BranchId == s.BranchId &&
                x.Status == ProductSerialStatus.InStock) > s.Quantity)
            .OrderBy(s => s.BranchId)
            .ThenBy(s => s.Product.Name)
            .Select(s => new SerialDriftDto(
                s.BranchId,
                s.Branch.Code,
                s.ProductId,
                s.Product.Name,
                s.Quantity,
                db.ProductSerials.Count(x =>
                    x.ProductId == s.ProductId &&
                    x.BranchId == s.BranchId &&
                    x.Status == ProductSerialStatus.InStock)))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Creates the units an inbound movement brings in. Inbound is all-or-nothing: goods arriving
    /// today have no excuse for missing a serial, so a tracked product needs exactly one per unit.
    /// </summary>
    internal async Task<IReadOnlyList<ProductSerial>> CreateInboundAsync(
        long branchId,
        Product product,
        int quantity,
        IReadOnlyList<string>? serialNumbers,
        long? purchaseItemId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (!product.IsSerialized)
        {
            EnsureNoneSupplied(product, serialNumbers);
            return [];
        }

        var names = NormalizeBatch(serialNumbers ?? []);
        if (names.Count != quantity)
        {
            throw new BadRequestException(
                $"Product {product.Id} is tracked by serial number: {quantity} units need {quantity} " +
                $"serials, got {names.Count}",
                ErrorCodes.SerialCountMismatch);
        }

        await EnsureUnusedAsync(names, cancellationToken);

        var created = names
            .Select(name => Build(product.Id, name, branchId, purchaseItemId, now))
            .ToList();

        db.ProductSerials.AddRange(created);
        return created;
    }

    /// <summary>
    /// Resolves the units an outbound movement takes, applying the pick rule. Returns them tracked
    /// and unchanged — the caller decides whether they were sold, transferred or written off.
    /// </summary>
    /// <remarks>
    /// <paramref name="quantity"/> is the total across every line of this product in the request,
    /// not one line's worth: T and S are per (branch, product), so the window has to be judged
    /// against the whole movement or two lines could each pass alone and overdraw together.
    /// </remarks>
    internal async Task<IReadOnlyList<ProductSerial>> ResolveOutboundAsync(
        long branchId,
        Product product,
        int quantity,
        IReadOnlyList<string>? serialNumbers,
        CancellationToken cancellationToken)
    {
        if (!product.IsSerialized)
        {
            EnsureNoneSupplied(product, serialNumbers);
            return [];
        }

        var names = NormalizeBatch(serialNumbers ?? []);

        var onHand = await StockQuantityAsync(branchId, product.Id, cancellationToken);

        if (quantity > onHand)
        {
            // Doomed for want of stock, not for want of serials. Stay quiet and let
            // StockLevel.Apply say INSUFFICIENT_STOCK, which is the answer a seller can act on —
            // "at most 0 serials" would just describe the symptom.
            return [];
        }

        var identified = await IdentifiedCountAsync(branchId, product.Id, cancellationToken);
        var anonymous = onHand - identified;

        var minimum = Math.Max(0, quantity - anonymous);
        var maximum = Math.Min(quantity, identified);

        if (names.Count < minimum)
        {
            throw new ConflictException(
                ErrorCodes.SerialSelectionRequired,
                $"Selling {quantity} of product {product.Id} from branch {branchId} needs at least " +
                $"{minimum} serials (picked {names.Count}); the branch holds {onHand} units, " +
                $"{identified} of them identified");
        }

        if (names.Count > maximum)
        {
            throw new BadRequestException(
                $"Selling {quantity} of product {product.Id} accepts at most {maximum} serials, " +
                $"got {names.Count}",
                ErrorCodes.SerialCountMismatch);
        }

        if (names.Count == 0)
        {
            return [];
        }

        var lowered = names.Select(n => n.ToLower()).ToList();
        var found = await db.ProductSerials
            .Where(s => lowered.Contains(s.SerialNumber.ToLower()))
            .ToListAsync(cancellationToken);

        foreach (var name in names)
        {
            var serial = found.FirstOrDefault(s => string.Equals(s.SerialNumber, name, StringComparison.OrdinalIgnoreCase));

            if (serial is null ||
                serial.ProductId != product.Id ||
                serial.BranchId != branchId ||
                serial.Status != ProductSerialStatus.InStock)
            {
                throw new ConflictException(
                    ErrorCodes.SerialNotAvailable,
                    $"Serial {name} is not an in-stock unit of product {product.Id} in branch {branchId}");
            }
        }

        return found;
    }

    /// <summary>Appends to the unit's history. The caller saves.</summary>
    internal void RecordEvent(
        ProductSerial serial,
        SerialEventType eventType,
        long branchId,
        string referenceType,
        long? referenceId,
        string? notes = null)
    {
        db.ProductSerialEvents.Add(new ProductSerialEvent
        {
            Serial = serial,
            EventType = eventType,
            BranchId = branchId,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Notes = notes
        });
    }

    /// <summary>Trimmed, never upper-cased: the printed serial has to match the sticker.</summary>
    internal static string Normalize(string serialNumber) => serialNumber.Trim();

    private static ProductSerial Build(long productId, string serialNumber, long branchId, long? purchaseItemId, DateTimeOffset now) =>
        new()
        {
            ProductId = productId,
            SerialNumber = serialNumber,
            Status = ProductSerialStatus.InStock,
            BranchId = branchId,
            PurchaseItemId = purchaseItemId,
            ReceivedAt = now,
            UpdatedAt = now
        };

    private static void EnsureNoneSupplied(Product product, IReadOnlyList<string>? serialNumbers)
    {
        if (serialNumbers is { Count: > 0 })
        {
            throw new BadRequestException(
                $"Product {product.Id} is not tracked by serial number",
                ErrorCodes.SerialsNotTracked);
        }
    }

    /// <summary>
    /// Trims and rejects blanks and repeats. Callers spanning several lines run it over the whole
    /// request first, so the same serial cannot be claimed twice by two lines of one document.
    /// </summary>
    internal static List<string> NormalizeBatch(IReadOnlyList<string> serialNumbers)
    {
        var names = serialNumbers.Select(Normalize).ToList();

        if (names.Any(n => n.Length == 0))
        {
            throw new BadRequestException("A serial number must not be blank");
        }

        var duplicate = names
            .GroupBy(n => n.ToLower())
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate is not null)
        {
            throw new ConflictException(
                ErrorCodes.DuplicateSerialNumber,
                $"Serial {duplicate.First()} appears more than once in the same request");
        }

        return names;
    }

    /// <summary>
    /// Serials are unique across every product and every status, so a sold or written-off unit keeps
    /// its number for good — which is what stops an outside unit being registered under a number we
    /// once issued. Checked here as well as by the index because the raw <c>lower()</c> index is
    /// PostgreSQL-only and the test suite runs on SQLite.
    /// </summary>
    private async Task EnsureUnusedAsync(IReadOnlyList<string> names, CancellationToken cancellationToken)
    {
        var lowered = names.Select(n => n.ToLower()).ToList();

        var taken = await db.ProductSerials.AsNoTracking()
            .Where(s => lowered.Contains(s.SerialNumber.ToLower()))
            .Select(s => s.SerialNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (taken is not null)
        {
            throw new ConflictException(
                ErrorCodes.DuplicateSerialNumber,
                $"Serial {taken} is already registered");
        }
    }

    /// <summary>
    /// Reads the level rather than creating one: a row created here would never have been locked,
    /// so treating a missing row as zero is the honest answer.
    /// </summary>
    private async Task<int> StockQuantityAsync(long branchId, long productId, CancellationToken cancellationToken) =>
        await db.StockLevels.AsNoTracking()
            .Where(s => s.BranchId == branchId && s.ProductId == productId)
            .Select(s => (int?)s.Quantity)
            .FirstOrDefaultAsync(cancellationToken) ?? 0;

    private Task<int> IdentifiedCountAsync(long branchId, long productId, CancellationToken cancellationToken) =>
        db.ProductSerials.AsNoTracking()
            .CountAsync(
                s => s.BranchId == branchId && s.ProductId == productId && s.Status == ProductSerialStatus.InStock,
                cancellationToken);
}
