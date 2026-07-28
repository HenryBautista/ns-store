using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Features.Clients;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Sales;
using NsStore.Application.Features.Settings;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;
using NsStore.Infrastructure.Persistence;

namespace NsStore.Application.Tests;

/// <summary>
/// Wires the real services against an in-memory SQLite database. Postgres-only behaviour
/// (row locking, native enums) is covered by integration tests, not here.
/// </summary>
public sealed class TestHarness : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestHarness(long userId = 1, UserRole role = UserRole.Admin)
    {
        CurrentUser = new FakeCurrentUser(userId, role);
        Clock = new FakeTimeProvider(new DateTimeOffset(2026, 7, 24, 12, 0, 0, TimeSpan.Zero));

        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new AuditInterceptor(CurrentUser, Clock))
            .Options;

        Db = new AppDbContext(options);
        Db.Database.EnsureCreated();

        Settings = new SettingsService(Db, CurrentUser, Clock);
        Products = new ProductService(Db, Settings, Clock);
        Inventory = new InventoryService(Db, StockLock, Clock);
        Purchases = new PurchaseService(Db, Inventory, StockLock, Clock);
        Sales = new SaleService(Db, Inventory, StockLock, CurrentUser, Clock);
        Clients = new ClientService(Db, Clock);

        Seed();
    }

    public AppDbContext Db { get; }
    public FakeCurrentUser CurrentUser { get; }
    public FakeTimeProvider Clock { get; }
    public IStockLockService StockLock { get; } = new NoOpStockLock();
    public SettingsService Settings { get; }
    public ProductService Products { get; }
    public InventoryService Inventory { get; }
    public PurchaseService Purchases { get; }
    public SaleService Sales { get; }
    public ClientService Clients { get; }

    public DateOnly Today => DateOnly.FromDateTime(Clock.GetUtcNow().UtcDateTime);

    private void Seed()
    {
        Db.Users.Add(new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = "x",
            FirstName = "Ada",
            LastName = "Admin",
            Role = UserRole.Admin
        });

        Db.AppSettings.AddRange(
            new AppSetting { Key = AppSettingKeys.VatRate, Value = "16" },
            new AppSetting { Key = AppSettingKeys.DefaultMarginPct, Value = "30" },
            new AppSetting { Key = AppSettingKeys.Currency, Value = "BOB" });

        Db.Suppliers.Add(new Supplier { Id = 1, Name = "Distribuidora Central" });
        Db.Clients.Add(new Client { Id = 1, Type = ClientType.Individual, Name = "Juan", LastName = "Perez" });
        Db.SaveChanges();
    }

    public async Task<long> CreateProductAsync(string name = "SSD 1TB")
    {
        var product = await Products.CreateAsync(new ProductRequest(name, null, null, null, null, null, null));
        return product.Id;
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}

public class FakeCurrentUser(long? userId, UserRole role) : ICurrentUser
{
    public long? UserId { get; set; } = userId;
    public string? Username => "admin";
    public UserRole? Role { get; set; } = role;
    public bool IsAuthenticated => UserId is not null;
    public bool IsAdmin => Role == UserRole.Admin;
}

public class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}

public class NoOpStockLock : IStockLockService
{
    public Task LockAsync(IReadOnlyCollection<long> productIds, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
