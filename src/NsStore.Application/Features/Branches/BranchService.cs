using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;

namespace NsStore.Application.Features.Branches;

public class BranchService(IAppDbContext db, TimeProvider clock)
{
    public async Task<PagedResult<BranchDto>> ListAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = db.Branches.AsNoTracking().AsQueryable();
        if (request.TrimmedSearch is { } search)
        {
            var pattern = $"%{search.ToLower()}%";
            query = query.Where(b =>
                EF.Functions.Like(b.Code.ToLower(), pattern) ||
                EF.Functions.Like(b.Name.ToLower(), pattern));
        }

        return await query
            .OrderBy(b => b.Code)
            .Select(ProjectToDto)
            .ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<BranchDto> GetAsync(long id, CancellationToken cancellationToken = default) =>
        await db.Branches.AsNoTracking().Where(b => b.Id == id).Select(ProjectToDto)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(Branch), id);

    /// <summary>
    /// Creating a branch fans out a <see cref="StockLevel"/> row for every live product. That is not
    /// tidiness: <c>SELECT … FOR UPDATE</c> only locks rows that exist, so a branch with a sparse
    /// grid can be oversold under concurrency no matter what the lock service does.
    /// </summary>
    public async Task<BranchDto> CreateAsync(BranchRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await EnsureCodeAvailableAsync(code, null, ct);

            var now = clock.GetUtcNow();
            var branch = new Branch
            {
                Code = code,
                Name = request.Name.Trim(),
                Address = request.Address?.Trim(),
                Phone = request.Phone?.Trim(),
                IsActive = true
            };

            db.Branches.Add(branch);
            await db.SaveChangesAsync(ct);

            var productIds = await db.Products.Select(p => p.Id).ToListAsync(ct);
            foreach (var productId in productIds)
            {
                db.StockLevels.Add(new StockLevel
                {
                    BranchId = branch.Id,
                    ProductId = productId,
                    Quantity = 0,
                    UpdatedAt = now
                });
            }

            await db.SaveChangesAsync(ct);
            return ToDto(branch);
        }, cancellationToken);
    }

    public async Task<BranchDto> UpdateAsync(long id, BranchRequest request, CancellationToken cancellationToken = default)
    {
        var branch = await FindAsync(id, cancellationToken);
        var code = request.Code.Trim().ToUpperInvariant();
        await EnsureCodeAvailableAsync(code, id, cancellationToken);

        // Documents keep the folio rendered at issue time, so renaming the code is safe for history.
        branch.Code = code;
        branch.Name = request.Name.Trim();
        branch.Address = request.Address?.Trim();
        branch.Phone = request.Phone?.Trim();

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(branch);
    }

    public async Task<BranchDto> SetStatusAsync(long id, bool isActive, CancellationToken cancellationToken = default)
    {
        var branch = await FindAsync(id, cancellationToken);
        branch.IsActive = isActive;
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(branch);
    }

    /// <summary>
    /// Soft delete, refused while the branch still owns anything. Deactivating is the normal move;
    /// this exists for a branch created by mistake.
    /// </summary>
    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var branch = await FindAsync(id, cancellationToken);

        if (await db.StockLevels.AnyAsync(s => s.BranchId == id && s.Quantity != 0, cancellationToken))
        {
            throw new ConflictException(ErrorCodes.Conflict, "Branch still holds stock");
        }

        if (await db.Users.AnyAsync(u => u.BranchId == id, cancellationToken))
        {
            throw new ConflictException(ErrorCodes.Conflict, "Branch still has users assigned");
        }

        if (await db.Sales.AnyAsync(s => s.BranchId == id, cancellationToken) ||
            await db.Purchases.AnyAsync(p => p.BranchId == id, cancellationToken))
        {
            throw new ConflictException(ErrorCodes.Conflict, "Branch is referenced by registered sales or purchases");
        }

        branch.DeletedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Throws unless the branch exists and is open for business.</summary>
    internal async Task EnsureWritableAsync(long branchId, CancellationToken cancellationToken)
    {
        var isActive = await db.Branches.AsNoTracking()
            .Where(b => b.Id == branchId)
            .Select(b => (bool?)b.IsActive)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), branchId);

        if (!isActive)
        {
            throw new ConflictException(ErrorCodes.BranchInactive, $"Branch {branchId} is inactive");
        }
    }

    private async Task<Branch> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Branches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Branch), id);

    /// <summary>
    /// Mirrors the <c>ux_branches_code_active</c> partial unique index. The index is raw SQL in the
    /// migration, so <c>EnsureCreated()</c> never builds it and the SQLite suite has no safety net —
    /// this check is what the tests actually exercise.
    /// </summary>
    private async Task EnsureCodeAvailableAsync(string code, long? excludeId, CancellationToken cancellationToken)
    {
        var taken = await db.Branches
            .AnyAsync(b => b.Code.ToLower() == code.ToLower() && (excludeId == null || b.Id != excludeId), cancellationToken);

        if (taken)
        {
            throw new ConflictException(ErrorCodes.DuplicateBranchCode, $"Branch code '{code}' already exists");
        }
    }

    private static BranchDto ToDto(Branch b) => new(b.Id, b.Code, b.Name, b.Address, b.Phone, b.IsActive);

    internal static readonly System.Linq.Expressions.Expression<Func<Branch, BranchDto>> ProjectToDto =
        b => new BranchDto(b.Id, b.Code, b.Name, b.Address, b.Phone, b.IsActive);
}
