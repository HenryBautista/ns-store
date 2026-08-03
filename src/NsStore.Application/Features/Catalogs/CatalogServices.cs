using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;

namespace NsStore.Application.Features.Catalogs;

public class TrademarkService(IAppDbContext db, TimeProvider clock)
{
    public async Task<PagedResult<TrademarkDto>> ListAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = db.Trademarks.AsNoTracking().AsQueryable();
        if (request.SearchPattern is { } pattern)
        {
            query = query.Where(t => EF.Functions.Like(SearchText.Unaccent(t.Name).ToLower(), pattern));
        }

        var page = await query.OrderBy(t => t.Name).ToPagedResultAsync(request, cancellationToken);
        return new PagedResult<TrademarkDto>(page.Items.Select(t => t.ToDto()).ToList(), page.Page, page.PageSize, page.Total);
    }

    public async Task<TrademarkDto> GetAsync(long id, CancellationToken cancellationToken = default) =>
        (await FindAsync(id, cancellationToken)).ToDto();

    public async Task<TrademarkDto> CreateAsync(NameRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        await EnsureNameAvailableAsync(name, null, cancellationToken);

        var entity = new Trademark { Name = name };
        db.Trademarks.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<TrademarkDto> UpdateAsync(long id, NameRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        var name = request.Name.Trim();
        await EnsureNameAvailableAsync(name, id, cancellationToken);

        entity.Name = name;
        await db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (await db.Products.AnyAsync(p => p.TrademarkId == id, cancellationToken))
        {
            throw new ConflictException(ErrorCodes.Conflict, "Trademark is referenced by active products");
        }

        entity.DeletedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Trademark> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Trademarks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Trademark), id);

    private async Task EnsureNameAvailableAsync(string name, long? excludeId, CancellationToken cancellationToken)
    {
        if (await db.Trademarks.AnyAsync(t => t.Name.ToLower() == name.ToLower() && (excludeId == null || t.Id != excludeId), cancellationToken))
        {
            throw new ConflictException(ErrorCodes.DuplicateName, $"Trademark '{name}' already exists");
        }
    }
}

public class CategoryService(IAppDbContext db, TimeProvider clock)
{
    public async Task<PagedResult<CategoryDto>> ListAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = db.Categories.AsNoTracking().AsQueryable();
        if (request.SearchPattern is { } pattern)
        {
            query = query.Where(c => EF.Functions.Like(SearchText.Unaccent(c.Name).ToLower(), pattern));
        }

        var page = await query.OrderBy(c => c.Name).ToPagedResultAsync(request, cancellationToken);
        return new PagedResult<CategoryDto>(page.Items.Select(c => c.ToDto()).ToList(), page.Page, page.PageSize, page.Total);
    }

    public async Task<CategoryDto> GetAsync(long id, CancellationToken cancellationToken = default) =>
        (await FindAsync(id, cancellationToken)).ToDto();

    public async Task<CategoryDto> CreateAsync(NameRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        await EnsureNameAvailableAsync(name, null, cancellationToken);

        var entity = new Category { Name = name };
        db.Categories.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(long id, NameRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        var name = request.Name.Trim();
        await EnsureNameAvailableAsync(name, id, cancellationToken);

        entity.Name = name;
        await db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (await db.Products.AnyAsync(p => p.CategoryId == id, cancellationToken))
        {
            throw new ConflictException(ErrorCodes.Conflict, "Category is referenced by active products");
        }

        entity.DeletedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Category> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Category), id);

    private async Task EnsureNameAvailableAsync(string name, long? excludeId, CancellationToken cancellationToken)
    {
        if (await db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower() && (excludeId == null || c.Id != excludeId), cancellationToken))
        {
            throw new ConflictException(ErrorCodes.DuplicateName, $"Category '{name}' already exists");
        }
    }
}

public class WarrantyTermService(IAppDbContext db, TimeProvider clock)
{
    public async Task<PagedResult<WarrantyTermDto>> ListAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = db.WarrantyTerms.AsNoTracking().AsQueryable();
        if (request.SearchPattern is { } pattern)
        {
            query = query.Where(w => EF.Functions.Like(SearchText.Unaccent(w.Description).ToLower(), pattern));
        }

        var page = await query.OrderBy(w => w.Description).ToPagedResultAsync(request, cancellationToken);
        return new PagedResult<WarrantyTermDto>(page.Items.Select(w => w.ToDto()).ToList(), page.Page, page.PageSize, page.Total);
    }

    public async Task<WarrantyTermDto> GetAsync(long id, CancellationToken cancellationToken = default) =>
        (await FindAsync(id, cancellationToken)).ToDto();

    public async Task<WarrantyTermDto> CreateAsync(DescriptionRequest request, CancellationToken cancellationToken = default)
    {
        var description = request.Description.Trim();
        await EnsureDescriptionAvailableAsync(description, null, cancellationToken);

        var entity = new WarrantyTerm { Description = description };
        db.WarrantyTerms.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<WarrantyTermDto> UpdateAsync(long id, DescriptionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        var description = request.Description.Trim();
        await EnsureDescriptionAvailableAsync(description, id, cancellationToken);

        entity.Description = description;
        await db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (await db.Products.AnyAsync(p => p.WarrantyTermId == id, cancellationToken))
        {
            throw new ConflictException(ErrorCodes.Conflict, "Warranty term is referenced by active products");
        }

        entity.DeletedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<WarrantyTerm> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.WarrantyTerms.FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(WarrantyTerm), id);

    private async Task EnsureDescriptionAvailableAsync(string description, long? excludeId, CancellationToken cancellationToken)
    {
        if (await db.WarrantyTerms.AnyAsync(w => w.Description.ToLower() == description.ToLower() && (excludeId == null || w.Id != excludeId), cancellationToken))
        {
            throw new ConflictException(ErrorCodes.DuplicateName, $"Warranty term '{description}' already exists");
        }
    }
}

public class SupplierService(IAppDbContext db, TimeProvider clock)
{
    public async Task<PagedResult<SupplierDto>> ListAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = db.Suppliers.AsNoTracking().AsQueryable();
        if (request.SearchPattern is { } pattern)
        {
            query = query.Where(s => EF.Functions.Like(SearchText.Unaccent(s.Name).ToLower(), pattern));
        }

        var page = await query.OrderBy(s => s.Name).ToPagedResultAsync(request, cancellationToken);
        return new PagedResult<SupplierDto>(page.Items.Select(s => s.ToDto()).ToList(), page.Page, page.PageSize, page.Total);
    }

    public async Task<SupplierDto> GetAsync(long id, CancellationToken cancellationToken = default) =>
        (await FindAsync(id, cancellationToken)).ToDto();

    public async Task<SupplierDto> CreateAsync(SupplierRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        await EnsureNameAvailableAsync(name, null, cancellationToken);

        var entity = new Supplier
        {
            Name = name,
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim()
        };

        db.Suppliers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<SupplierDto> UpdateAsync(long id, SupplierRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        var name = request.Name.Trim();
        await EnsureNameAvailableAsync(name, id, cancellationToken);

        entity.Name = name;
        entity.Phone = request.Phone?.Trim();
        entity.Email = request.Email?.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (await db.Purchases.AnyAsync(p => p.SupplierId == id, cancellationToken))
        {
            throw new ConflictException(ErrorCodes.Conflict, "Supplier is referenced by registered purchases");
        }

        entity.DeletedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Supplier> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Supplier), id);

    private async Task EnsureNameAvailableAsync(string name, long? excludeId, CancellationToken cancellationToken)
    {
        if (await db.Suppliers.AnyAsync(s => s.Name.ToLower() == name.ToLower() && (excludeId == null || s.Id != excludeId), cancellationToken))
        {
            throw new ConflictException(ErrorCodes.DuplicateName, $"Supplier '{name}' already exists");
        }
    }
}
