using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Features.Branches;
using NsStore.Application.Features.Clients;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Reports;
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

    public TestHarness(long userId = 1, UserRole role = UserRole.Admin, long branchId = MainBranchId)
    {
        CurrentUser = new FakeCurrentUser(userId, role, branchId);
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
        Branches = new BranchService(Db, Clock);
        Products = new ProductService(Db, Settings, CurrentUser, Clock);
        Inventory = new InventoryService(Db, CurrentUser, StockLock, Clock);
        // The real numbering service, not a fake: it detects the provider, and numbering is the
        // piece most likely to carry an off-by-one.
        DocumentNumbers = new DocumentNumberService(Db);
        Purchases = new PurchaseService(Db, Inventory, Branches, StockLock, DocumentNumbers, CurrentUser, Clock);
        Sales = new SaleService(Db, Inventory, Branches, StockLock, DocumentNumbers, Settings, CurrentUser, Clock);
        Transfers = new TransferService(Db, Inventory, Branches, StockLock, DocumentNumbers, CurrentUser, Clock);
        Clients = new ClientService(Db, Clock);
        Reports = new ReportService(Db, Sales, Purchases, Inventory, Settings, Clients, CurrentUser, Clock);

        Seed();
    }

    /// <summary>Seeded branches. A second one exists from the start: every cross-branch test needs a target.</summary>
    public const long MainBranchId = 1;

    public const long SouthBranchId = 2;

    public AppDbContext Db { get; }
    public FakeCurrentUser CurrentUser { get; }
    public FakeTimeProvider Clock { get; }
    public IStockLockService StockLock { get; } = new NoOpStockLock();
    public IDocumentNumberService DocumentNumbers { get; }
    public SettingsService Settings { get; }
    public BranchService Branches { get; }
    public ProductService Products { get; }
    public InventoryService Inventory { get; }
    public PurchaseService Purchases { get; }
    public SaleService Sales { get; }
    public TransferService Transfers { get; }
    public ClientService Clients { get; }
    public ReportService Reports { get; }

    public DateOnly Today => DateOnly.FromDateTime(Clock.GetUtcNow().UtcDateTime);

    private void Seed()
    {
        // Branches first: User.BranchId is NOT NULL with an FK and SQLite enforces foreign keys.
        Db.Branches.AddRange(
            new Branch { Id = MainBranchId, Code = "MAIN", Name = "Casa Matriz" },
            new Branch { Id = SouthBranchId, Code = "SUR", Name = "Sucursal Sur" });
        Db.SaveChanges();

        Db.Users.Add(new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = "x",
            FirstName = "Ada",
            LastName = "Admin",
            Role = UserRole.Admin,
            BranchId = MainBranchId
        });

        Db.AppSettings.AddRange(
            new AppSetting { Key = AppSettingKeys.VatRate, Value = "16" },
            new AppSetting { Key = AppSettingKeys.DefaultMarginPct, Value = "30" },
            new AppSetting { Key = AppSettingKeys.Currency, Value = "BOB" },
            new AppSetting { Key = AppSettingKeys.OverdueDays, Value = "15" });

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

public class FakeCurrentUser(long? userId, UserRole role, long branchId) : ICurrentUser
{
    public long? UserId { get; set; } = userId;
    public string? Username => "admin";
    public UserRole? Role { get; set; } = role;
    public bool IsAuthenticated => UserId is not null;
    public bool IsAdmin => Role == UserRole.Admin;

    // Settable so a test can move an admin between branches, the same way it flips the role.
    public long? HomeBranchId { get; set; } = branchId;
    public long? ActiveBranchId { get; set; } = branchId;
}

public class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}

public class NoOpStockLock : IStockLockService
{
    public Task LockAsync(IReadOnlyCollection<StockKey> keys, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
