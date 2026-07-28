using Microsoft.EntityFrameworkCore;
using NsStore.Domain.Entities;

namespace NsStore.Application.Common.Interfaces;

/// <summary>
/// Persistence port. Infrastructure implements it with EF Core / PostgreSQL.
/// </summary>
public interface IAppDbContext
{
    DbSet<Branch> Branches { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Trademark> Trademarks { get; }
    DbSet<Category> Categories { get; }
    DbSet<WarrantyTerm> WarrantyTerms { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Product> Products { get; }
    DbSet<StockLevel> StockLevels { get; }
    DbSet<InventoryMovement> InventoryMovements { get; }
    DbSet<Client> Clients { get; }
    DbSet<Purchase> Purchases { get; }
    DbSet<PurchaseItem> PurchaseItems { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleItem> SaleItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Order> Orders { get; }
    DbSet<Quote> Quotes { get; }
    DbSet<StockTransfer> StockTransfers { get; }
    DbSet<StockTransferItem> StockTransferItems { get; }
    DbSet<AppSetting> AppSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Runs <paramref name="action"/> inside a single database transaction.</summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}
