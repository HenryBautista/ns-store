using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Settings;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Products;

public class ProductService(IAppDbContext db, SettingsService settingsService, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<PagedResult<ProductDto>> ListAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var branchId = currentUser.RequireBranch();
        var query = db.Products.AsNoTracking().AsQueryable();
        if (request.TrimmedSearch is { } search)
        {
            var pattern = $"%{search.ToLower()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name.ToLower(), pattern) ||
                (p.PartNumber != null && EF.Functions.Like(p.PartNumber.ToLower(), pattern)));
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(ProjectToDto(branchId))
            .ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<ProductDto> GetAsync(long id, CancellationToken cancellationToken = default) =>
        await db.Products.AsNoTracking().Where(p => p.Id == id).Select(ProjectToDto(currentUser.RequireBranch()))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(Product), id);

    public async Task<ProductDto> CreateAsync(ProductRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureReferencesExistAsync(request, cancellationToken);

        var now = clock.GetUtcNow();
        var branchIds = await db.Branches.Where(b => b.IsActive).Select(b => b.Id).ToListAsync(cancellationToken);

        var product = new Product
        {
            Name = request.Name.Trim(),
            PartNumber = request.PartNumber?.Trim(),
            Description = request.Description?.Trim(),
            SerialNumber = request.SerialNumber?.Trim(),
            TrademarkId = request.TrademarkId,
            CategoryId = request.CategoryId,
            WarrantyTermId = request.WarrantyTermId,
            // Prices start at 0 and are set in the pricing module.
            PriceWithInvoice = 0m,
            PriceWithoutInvoice = 0m,
            // One stock row per active branch from creation. Rows may sit at 0 and are never
            // deleted: a missing row makes SELECT … FOR UPDATE lock nothing, which silently
            // reintroduces the oversell race no existing test would catch.
            StockLevels = branchIds
                .Select(branchId => new StockLevel { BranchId = branchId, Quantity = 0, UpdatedAt = now })
                .ToList()
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(product.Id, cancellationToken);
    }

    public async Task<ProductDto> UpdateAsync(long id, ProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await FindAsync(id, cancellationToken);
        await EnsureReferencesExistAsync(request, cancellationToken);

        product.Name = request.Name.Trim();
        product.PartNumber = request.PartNumber?.Trim();
        product.Description = request.Description?.Trim();
        product.SerialNumber = request.SerialNumber?.Trim();
        product.TrademarkId = request.TrademarkId;
        product.CategoryId = request.CategoryId;
        product.WarrantyTermId = request.WarrantyTermId;

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(product.Id, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await FindAsync(id, cancellationToken);
        product.DeletedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductDto> SetPricesAsync(long id, SetPricesRequest request, CancellationToken cancellationToken = default)
    {
        var product = await FindAsync(id, cancellationToken);
        product.PriceWithInvoice = decimal.Round(request.PriceWithInvoice, 2, MidpointRounding.AwayFromZero);
        product.PriceWithoutInvoice = decimal.Round(request.PriceWithoutInvoice, 2, MidpointRounding.AwayFromZero);

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(product.Id, cancellationToken);
    }

    /// <summary>
    /// withoutInvoice = lastCost × (1 + margin); withInvoice = withoutInvoice × (1 + vat).
    /// Margin and VAT come from <c>app_settings</c>, never from constants.
    /// </summary>
    public async Task<PriceSuggestionDto> GetPriceSuggestionAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        var settings = await settingsService.GetAsync(cancellationToken);

        var lastCost = await db.InventoryMovements.AsNoTracking()
            .Where(m => m.ProductId == id && m.MovementType == MovementType.Purchase && m.UnitCost != null)
            // Ledger ids are monotonic, so the highest id is the most recent purchase cost.
            .OrderByDescending(m => m.Id)
            .Select(m => m.UnitCost)
            .FirstOrDefaultAsync(cancellationToken);

        decimal? withoutInvoice = null;
        decimal? withInvoice = null;
        if (lastCost is { } cost)
        {
            withoutInvoice = decimal.Round(cost * (1 + settings.DefaultMarginPct / 100m), 2, MidpointRounding.AwayFromZero);
            withInvoice = decimal.Round(withoutInvoice.Value * (1 + settings.VatRate / 100m), 2, MidpointRounding.AwayFromZero);
        }

        return new PriceSuggestionDto(
            product.Id,
            lastCost,
            settings.DefaultMarginPct,
            settings.VatRate,
            withoutInvoice,
            withInvoice,
            product.PriceWithInvoice,
            product.PriceWithoutInvoice);
    }

    private async Task<Product> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Product), id);

    private async Task EnsureReferencesExistAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        if (request.TrademarkId is { } trademarkId &&
            !await db.Trademarks.AnyAsync(t => t.Id == trademarkId, cancellationToken))
        {
            throw new NotFoundException(nameof(Trademark), trademarkId);
        }

        if (request.CategoryId is { } categoryId &&
            !await db.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken))
        {
            throw new NotFoundException(nameof(Category), categoryId);
        }

        if (request.WarrantyTermId is { } warrantyTermId &&
            !await db.WarrantyTerms.AnyAsync(w => w.Id == warrantyTermId, cancellationToken))
        {
            throw new NotFoundException(nameof(WarrantyTerm), warrantyTermId);
        }
    }

    /// <summary>
    /// A method rather than a static field: the projection now closes over a runtime branch id, and
    /// a static field cannot capture one. <c>Sum</c> over a filtered set instead of
    /// <c>FirstOrDefault</c> yields 0 for a missing row without nullable-int gymnastics, and
    /// translates cleanly on both Npgsql and SQLite.
    /// </summary>
    internal static System.Linq.Expressions.Expression<Func<Product, ProductDto>> ProjectToDto(long branchId) =>
        p => new ProductDto(
            p.Id,
            p.Name,
            p.PartNumber,
            p.Description,
            p.SerialNumber,
            p.TrademarkId,
            p.Trademark != null ? p.Trademark.Name : null,
            p.CategoryId,
            p.Category != null ? p.Category.Name : null,
            p.WarrantyTermId,
            p.WarrantyTerm != null ? p.WarrantyTerm.Description : null,
            p.PriceWithInvoice,
            p.PriceWithoutInvoice,
            p.StockLevels.Where(s => s.BranchId == branchId).Sum(s => (int?)s.Quantity) ?? 0);
}
