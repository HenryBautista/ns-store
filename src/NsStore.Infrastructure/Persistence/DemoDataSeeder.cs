using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Infrastructure.Persistence;

/// <summary>
/// Fills an empty install with six months of plausible history for a laptop-parts store, so the
/// dashboards, kardex, reports and collections screens can be demonstrated against real numbers.
/// </summary>
/// <remarks>
/// <para>
/// Runs only behind <c>Seed:Demo:Enabled</c> and only when the catalog is empty, so a redeploy never
/// overwrites what someone entered while testing. <c>Seed:Demo:Reset</c> is the one path that
/// destroys data, and it exists for the manual workflow.
/// </para>
/// <para>
/// It writes against <see cref="AppDbContext"/> rather than through <c>PurchaseService</c> /
/// <c>SaleService</c>, because those resolve the branch from <c>ICurrentUser</c>, which has no
/// meaning outside a request. That makes the invariants this class's responsibility, so it mirrors
/// them explicitly: stock moves only through <see cref="StockLevel.Apply"/>, every move writes its
/// ledger row, folios come from <see cref="IDocumentNumberService"/>, and sale prices are derived
/// from the configured margin and VAT instead of being listed as literals.
/// </para>
/// <para>
/// Events are inserted in chronological order on purpose. The kardex and the "last purchase cost"
/// projection both order the ledger by id (<c>InventoryService.ProjectStockAsync</c>), so insertion
/// order <em>is</em> the timeline; getting it wrong would show a sale before the stock that covered
/// it existed.
/// </para>
/// <para>
/// There is no unit test for this class, and it is not an oversight. The suite runs on SQLite,
/// where EF stores <c>decimal</c> in TEXT columns, so the <c>ck_sales_total_paid_within_total</c>
/// check compares its operands as strings: a perfectly valid sale of 160.00 with 60.00 paid is
/// rejected because <c>'60' &gt; '160'</c>. Any partially paid sale trips it, so the seeder cannot
/// run there at all. It is exercised against real PostgreSQL instead — it runs at API startup, so
/// a failure keeps <c>/health</c> from ever answering and the deploy job's smoke test fails.
/// </para>
/// </remarks>
public class DemoDataSeeder(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    IDocumentNumberService documentNumbers,
    IConfiguration configuration,
    ILogger<DemoDataSeeder> logger,
    TimeProvider clock)
{
    public const string EnabledKey = "Seed:Demo:Enabled";
    public const string ResetKey = "Seed:Demo:Reset";

    /// <summary>Fixed so a reset reproduces the same dataset — demos stay comparable run to run.</summary>
    private const int RandomSeed = 20260801;

    private const int SaleCount = 200;
    private const int RestockPurchaseCount = 34;
    private const int TransferCount = 6;
    private const int AdjustmentCount = 10;
    private const int LowStockProductCount = 8;
    private const int OutOfStockProductCount = 5;

    /// <summary>The one definition of the store's offset, shared with every "today" the app computes.</summary>
    private static readonly TimeSpan BoliviaOffset = BusinessClock.Offset;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue(EnabledKey, false))
        {
            return;
        }

        var reset = configuration.GetValue(ResetKey, false);

        if (!reset && await db.Products.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            logger.LogInformation(
                "Demo data is enabled but the catalog is not empty; leaving it untouched. Set {Key}=true to rebuild it",
                ResetKey);
            return;
        }

        await db.ExecuteInTransactionAsync(async ct =>
        {
            if (reset)
            {
                await ClearAsync(ct);
            }

            await BuildAsync(ct);
            return 0;
        }, cancellationToken);

        logger.LogInformation(
            "Seeded demo data: {Products} products, {Sales} sales, {Purchases} purchases",
            DemoDataCatalog.Products.Length,
            SaleCount,
            RestockPurchaseCount);
    }

    /// <summary>
    /// Wipes the demo dataset in foreign-key order. The bootstrap admin, the default branch and
    /// <c>app_settings</c> survive — they are install state, not demo state.
    /// </summary>
    private async Task ClearAsync(CancellationToken cancellationToken)
    {
        logger.LogWarning("Resetting demo data: every transactional and catalog row will be deleted");

        // Children first; ExecuteDeleteAsync bypasses the change tracker, so query filters are the
        // only thing that could hide a row — hence IgnoreQueryFilters on the soft-deletable ones.
        await db.Payments.ExecuteDeleteAsync(cancellationToken);
        await db.SaleItems.ExecuteDeleteAsync(cancellationToken);
        await db.Sales.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.PurchaseItems.ExecuteDeleteAsync(cancellationToken);
        await db.Purchases.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.StockTransferItems.ExecuteDeleteAsync(cancellationToken);
        await db.StockTransfers.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.Orders.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.Quotes.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.InventoryMovements.ExecuteDeleteAsync(cancellationToken);
        await db.StockLevels.ExecuteDeleteAsync(cancellationToken);
        await db.Clients.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.Products.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.Suppliers.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.WarrantyTerms.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.Categories.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.Trademarks.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);

        // Demo sellers only: whoever was bootstrapped as admin keeps their account. Resolved to ids
        // first because ExecuteDelete cannot filter through a navigation.
        var sellerUsernames = DemoDataCatalog.Sellers.Select(s => s.Username).ToArray();
        var sellerIds = await db.Users.IgnoreQueryFilters()
            .Where(u => sellerUsernames.Contains(u.Username))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        await db.RefreshTokens.Where(t => sellerIds.Contains(t.UserId)).ExecuteDeleteAsync(cancellationToken);
        await db.Users.IgnoreQueryFilters()
            .Where(u => sellerIds.Contains(u.Id))
            .ExecuteDeleteAsync(cancellationToken);

        await db.Branches
            .Where(b => b.Code == DemoDataCatalog.SecondBranchCode)
            .ExecuteDeleteAsync(cancellationToken);

        // Folios restart from one; otherwise a reset dataset carries the previous run's numbering.
        await db.Branches.ExecuteUpdateAsync(
            b => b
                .SetProperty(x => x.SaleSequence, 0L)
                .SetProperty(x => x.PurchaseSequence, 0L)
                .SetProperty(x => x.TransferSequence, 0L),
            cancellationToken);

        // ExecuteDelete leaves the tracker holding entities that no longer exist.
        db.ChangeTracker.Clear();
    }

    private async Task BuildAsync(CancellationToken cancellationToken)
    {
        // Created inside the action: the Npgsql execution strategy may retry the whole unit of work,
        // and a Random carried in from outside would resume mid-sequence.
        var random = new Random(RandomSeed);
        var today = clock.Today();
        var start = today.AddMonths(-6);

        var branches = await SeedBranchesAsync(cancellationToken);
        var sellers = await SeedSellersAsync(branches, cancellationToken);
        var (trademarks, categories, warranties, suppliers) = await SeedCatalogsAsync(cancellationToken);
        var (products, baseCosts) = await SeedProductsAsync(trademarks, categories, warranties, cancellationToken);
        var clients = await SeedClientsAsync(random, cancellationToken);

        var stock = await SeedDenseStockGridAsync(branches, products, cancellationToken);

        var workspace = new Workspace
        {
            Random = random,
            Start = start,
            Today = today,
            Branches = branches,
            Sellers = sellers,
            Suppliers = suppliers,
            Products = products,
            BaseCosts = baseCosts,
            Clients = clients,
            Stock = stock
        };

        // Opening stock first, dated on day zero, so every later sale is covered in the ledger's
        // own chronological reading — not just in the final quantity.
        await SeedOpeningPurchasesAsync(workspace, cancellationToken);
        await SeedRestockPurchasesAsync(workspace, cancellationToken);
        await SeedTransfersAsync(workspace, cancellationToken);
        await SeedSalesAsync(workspace, cancellationToken);
        await SeedAdjustmentsAsync(workspace, cancellationToken);
        await SeedStockExtremesAsync(workspace, cancellationToken);
        await SeedOrdersAndQuotesAsync(workspace, cancellationToken);

        await BackdateLedgerAsync(workspace, cancellationToken);
    }

    // ---------------------------------------------------------------- reference data

    private async Task<List<Branch>> SeedBranchesAsync(CancellationToken cancellationToken)
    {
        var existing = await db.Branches.OrderBy(b => b.Id).ToListAsync(cancellationToken);

        if (existing.All(b => b.Code != DemoDataCatalog.SecondBranchCode))
        {
            var (code, name, address, phone) = DemoDataCatalog.SecondBranch;
            var second = new Branch { Code = code, Name = name, Address = address, Phone = phone, IsActive = true };
            db.Branches.Add(second);
            await db.SaveChangesAsync(cancellationToken);
            existing.Add(second);
        }

        // The bootstrap branch has no street address; the reports letterhead reads better with one.
        var main = existing.First();
        if (string.IsNullOrWhiteSpace(main.Address))
        {
            main.Address = "Av. Ayacucho 458 entre Ecuador y Colombia, Cochabamba";
            main.Phone = "+591 4 4501234";
            await db.SaveChangesAsync(cancellationToken);
        }

        return existing;
    }

    private async Task<List<User>> SeedSellersAsync(List<Branch> branches, CancellationToken cancellationToken)
    {
        var hash = passwordHasher.Hash(DemoDataCatalog.SellerPassword);
        var sellers = new List<User>();

        foreach (var (username, firstName, lastName, motherLastName, branchCode) in DemoDataCatalog.Sellers)
        {
            var existing = await db.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

            if (existing is not null)
            {
                sellers.Add(existing);
                continue;
            }

            var seller = new User
            {
                Username = username,
                PasswordHash = hash,
                FirstName = firstName,
                LastName = lastName,
                MotherLastName = motherLastName,
                Role = UserRole.Seller,
                IsActive = true,
                BranchId = BranchByCode(branches, branchCode).Id
            };

            db.Users.Add(seller);
            sellers.Add(seller);
        }

        await db.SaveChangesAsync(cancellationToken);
        return sellers;
    }

    private async Task<(Dictionary<string, Trademark>, Dictionary<string, Category>, Dictionary<string, WarrantyTerm>, List<Supplier>)>
        SeedCatalogsAsync(CancellationToken cancellationToken)
    {
        var trademarks = DemoDataCatalog.Trademarks.ToDictionary(n => n, n => new Trademark { Name = n });
        var categories = DemoDataCatalog.Categories.ToDictionary(n => n, n => new Category { Name = n });
        var warranties = DemoDataCatalog.WarrantyTerms.ToDictionary(d => d, d => new WarrantyTerm { Description = d });
        var suppliers = DemoDataCatalog.Suppliers
            .Select(s => new Supplier { Name = s.Name, Phone = s.Phone, Email = s.Email })
            .ToList();

        db.Trademarks.AddRange(trademarks.Values);
        db.Categories.AddRange(categories.Values);
        db.WarrantyTerms.AddRange(warranties.Values);
        db.Suppliers.AddRange(suppliers);
        await db.SaveChangesAsync(cancellationToken);

        return (trademarks, categories, warranties, suppliers);
    }

    private async Task<(List<Product> Products, Dictionary<long, decimal> BaseCosts)> SeedProductsAsync(
        Dictionary<string, Trademark> trademarks,
        Dictionary<string, Category> categories,
        Dictionary<string, WarrantyTerm> warranties,
        CancellationToken cancellationToken)
    {
        // Margin and VAT are business parameters, not constants: DatabaseInitializer seeds them and
        // an admin can change them from the Settings screen, so the demo prices have to follow.
        var (marginPct, vatPct) = await ReadPricingSettingsAsync(cancellationToken);

        var products = new List<Product>(DemoDataCatalog.Products.Length);
        var costs = new List<decimal>(DemoDataCatalog.Products.Length);

        foreach (var (category, trademark, name, partNumber, cost, warranty) in DemoDataCatalog.Products)
        {
            var withoutInvoice = Round(cost * (1m + marginPct / 100m));

            products.Add(new Product
            {
                Name = name,
                PartNumber = partNumber,
                CategoryId = categories[category].Id,
                TrademarkId = trademarks[trademark].Id,
                WarrantyTermId = warranties[DemoDataCatalog.WarrantyTerms[warranty]].Id,
                PriceWithoutInvoice = withoutInvoice,
                PriceWithInvoice = Round(withoutInvoice * (1m + vatPct / 100m))
            });

            costs.Add(cost);
        }

        db.Products.AddRange(products);
        await db.SaveChangesAsync(cancellationToken);

        // Ids only exist after the save, which is why the cost map is built in a second pass.
        var baseCosts = products
            .Select((p, i) => (p.Id, Cost: costs[i]))
            .ToDictionary(x => x.Id, x => x.Cost);

        return (products, baseCosts);
    }

    private async Task<(decimal MarginPct, decimal VatPct)> ReadPricingSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await db.AppSettings
            .Where(s => s.Key == AppSettingKeys.DefaultMarginPct || s.Key == AppSettingKeys.VatRate)
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        return (Percent(AppSettingKeys.DefaultMarginPct, 30m), Percent(AppSettingKeys.VatRate, 16m));

        decimal Percent(string key, decimal fallback) =>
            settings.TryGetValue(key, out var raw) && decimal.TryParse(raw, out var value) ? value : fallback;
    }

    private async Task<List<Client>> SeedClientsAsync(Random random, CancellationToken cancellationToken)
    {
        var clients = new List<Client>();
        var ci = 3_240_000;
        var nit = 1_020_400_010;

        foreach (var (firstName, lastName, motherLastName, city) in DemoDataCatalog.IndividualClients)
        {
            ci += random.Next(1_000, 9_000);
            clients.Add(new Client
            {
                Type = ClientType.Individual,
                Name = firstName,
                LastName = lastName,
                MotherLastName = motherLastName,
                Ci = ci.ToString(),
                Phone = RandomPhone(random),
                Email = $"{firstName.Split(' ')[0].ToLowerInvariant()}.{lastName.ToLowerInvariant()}@gmail.com",
                City = city,
                Address = RandomAddress(random)
            });
        }

        foreach (var (name, contactName, city) in DemoDataCatalog.CompanyClients)
        {
            nit += random.Next(10_000, 90_000);
            clients.Add(new Client
            {
                Type = ClientType.Company,
                Name = name,
                Nit = nit.ToString(),
                ContactName = contactName,
                Phone = RandomPhone(random),
                City = city,
                Address = RandomAddress(random)
            });
        }

        db.Clients.AddRange(clients);
        await db.SaveChangesAsync(cancellationToken);
        return clients;
    }

    /// <summary>
    /// Every (branch, product) pair gets its row up front. The grid has to stay dense because
    /// <c>SELECT … FOR UPDATE</c> can only lock rows that exist — a missing one turns the
    /// pessimistic lock into a no-op and lets oversell back in.
    /// </summary>
    private async Task<Dictionary<(long BranchId, long ProductId), StockLevel>> SeedDenseStockGridAsync(
        List<Branch> branches,
        List<Product> products,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var grid = new Dictionary<(long, long), StockLevel>(branches.Count * products.Count);

        foreach (var branch in branches)
        {
            foreach (var product in products)
            {
                var level = new StockLevel
                {
                    BranchId = branch.Id,
                    ProductId = product.Id,
                    Quantity = 0,
                    UpdatedAt = now
                };

                db.StockLevels.Add(level);
                grid[(branch.Id, product.Id)] = level;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return grid;
    }

    // ---------------------------------------------------------------- purchases

    /// <summary>
    /// Day-zero inventory, split by supplier into a handful of documents per branch rather than one
    /// 150-line receipt, which is what an actual opening count looks like.
    /// </summary>
    private async Task SeedOpeningPurchasesAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        const int ChunkSize = 32;

        foreach (var branch in workspace.Branches)
        {
            var isMain = branch.Id == workspace.Branches[0].Id;
            var chunkIndex = 0;

            foreach (var chunk in workspace.Products.Chunk(ChunkSize))
            {
                var lines = chunk
                    .Select(p => (Product: p, Quantity: isMain
                        ? workspace.Random.Next(12, 41)
                        : workspace.Random.Next(5, 21)))
                    .ToList();

                await CreatePurchaseAsync(
                    workspace,
                    branch,
                    workspace.Start.AddDays(chunkIndex),
                    workspace.Suppliers[chunkIndex % workspace.Suppliers.Count],
                    lines,
                    cancellationToken);

                chunkIndex++;
            }
        }
    }

    private async Task SeedRestockPurchasesAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        var dates = SpreadDates(workspace, RestockPurchaseCount, fromDay: 10);

        foreach (var date in dates)
        {
            var branch = PickBranch(workspace);
            var lineCount = workspace.Random.Next(3, 11);

            var lines = PickDistinctProducts(workspace, lineCount)
                .Select(p => (Product: p, Quantity: workspace.Random.Next(3, 16)))
                .ToList();

            await CreatePurchaseAsync(
                workspace,
                branch,
                date,
                workspace.Suppliers[workspace.Random.Next(workspace.Suppliers.Count)],
                lines,
                cancellationToken);
        }
    }

    private async Task CreatePurchaseAsync(
        Workspace workspace,
        Branch branch,
        DateOnly date,
        Supplier supplier,
        List<(Product Product, int Quantity)> lines,
        CancellationToken cancellationToken)
    {
        if (lines.Count == 0)
        {
            return;
        }

        var at = At(date, workspace.Random);
        var sequence = await documentNumbers.NextAsync(branch.Id, DocumentKind.Purchase, cancellationToken);
        var invoiceType = workspace.Random.Next(100) < 60 ? InvoiceType.WithInvoice : InvoiceType.WithoutInvoice;

        var purchase = new Purchase
        {
            PurchaseDate = date,
            BranchId = branch.Id,
            BranchSequence = sequence,
            Number = branch.FormatDocumentNumber(sequence),
            SupplierId = supplier.Id,
            InvoiceType = invoiceType,
            PaymentStatus = workspace.Random.Next(100) < 85 ? PaymentStatus.Paid : PaymentStatus.Credit,
            CreatedBy = PickSeller(workspace, branch).Id
        };

        foreach (var (product, quantity) in lines)
        {
            var unitCost = CostOf(workspace, product);
            purchase.Items.Add(new PurchaseItem
            {
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = unitCost,
                Subtotal = Round(unitCost * quantity)
            });
        }

        purchase.TotalQuantity = purchase.Items.Sum(i => i.Quantity);
        purchase.TotalAmount = purchase.Items.Sum(i => i.Subtotal);
        db.Purchases.Add(purchase);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var item in purchase.Items)
        {
            MoveStock(
                workspace,
                branch.Id,
                item.ProductId,
                item.Quantity,
                MovementType.Purchase,
                at,
                unitCost: item.UnitPrice,
                referenceType: "purchase",
                referenceId: purchase.Id);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------- transfers

    private async Task SeedTransfersAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        if (workspace.Branches.Count < 2)
        {
            return;
        }

        var dates = SpreadDates(workspace, TransferCount, fromDay: 20);

        foreach (var (date, index) in dates.Select((d, i) => (d, i)))
        {
            // Alternate the direction so the kardex shows both TransferIn and TransferOut per branch.
            var origin = workspace.Branches[index % 2];
            var destination = workspace.Branches[(index + 1) % 2];

            var lines = PickDistinctProducts(workspace, workspace.Random.Next(2, 6))
                .Select(p => (Product: p, Quantity: Math.Min(workspace.Random.Next(2, 9), Available(workspace, origin.Id, p.Id))))
                .Where(l => l.Quantity > 0)
                .ToList();

            if (lines.Count == 0)
            {
                continue;
            }

            var at = At(date, workspace.Random);
            var sequence = await documentNumbers.NextAsync(origin.Id, DocumentKind.Transfer, cancellationToken);

            var transfer = new StockTransfer
            {
                TransferDate = date,
                OriginBranchId = origin.Id,
                DestinationBranchId = destination.Id,
                BranchSequence = sequence,
                Number = origin.FormatDocumentNumber(sequence),
                TotalQuantity = lines.Sum(l => l.Quantity),
                Notes = DemoDataCatalog.TransferNotes[workspace.Random.Next(DemoDataCatalog.TransferNotes.Length)],
                CreatedBy = PickSeller(workspace, origin).Id
            };

            foreach (var (product, quantity) in lines)
            {
                transfer.Items.Add(new StockTransferItem { ProductId = product.Id, Quantity = quantity });
            }

            db.StockTransfers.Add(transfer);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var (product, quantity) in lines)
            {
                // Two ledger rows, not one: dispatching and receiving are distinct physical events
                // at two different counters, and the kardex reports them separately.
                MoveStock(workspace, origin.Id, product.Id, -quantity, MovementType.TransferOut, at,
                    referenceType: "transfer", referenceId: transfer.Id);
                MoveStock(workspace, destination.Id, product.Id, quantity, MovementType.TransferIn, at,
                    referenceType: "transfer", referenceId: transfer.Id);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    // ---------------------------------------------------------------- sales

    private async Task SeedSalesAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        // From day 8, after the last opening receipt, so no sale predates the stock that covered it.
        var dates = SpreadDates(workspace, SaleCount, fromDay: 8);
        var creditSales = new List<(Sale Sale, DateOnly Date)>();

        foreach (var date in dates)
        {
            var branch = PickBranch(workspace);
            var seller = PickSeller(workspace, branch);
            var invoiceType = workspace.Random.Next(100) < 45 ? InvoiceType.WithInvoice : InvoiceType.WithoutInvoice;

            var lines = PickDistinctProducts(workspace, workspace.Random.Next(1, 5))
                .Select(p => (Product: p, Quantity: Math.Min(workspace.Random.Next(1, 4), Available(workspace, branch.Id, p.Id))))
                .Where(l => l.Quantity > 0)
                .ToList();

            if (lines.Count == 0)
            {
                continue;
            }

            var at = At(date, workspace.Random);
            var sequence = await documentNumbers.NextAsync(branch.Id, DocumentKind.Sale, cancellationToken);
            var isCredit = workspace.Random.Next(100) < 25;

            var sale = new Sale
            {
                SaleDate = date,
                BranchId = branch.Id,
                BranchSequence = sequence,
                Number = branch.FormatDocumentNumber(sequence),
                ClientId = workspace.Clients[workspace.Random.Next(workspace.Clients.Count)].Id,
                InvoiceType = invoiceType,
                PaymentStatus = isCredit ? PaymentStatus.Credit : PaymentStatus.Paid,
                CreatedBy = seller.Id
            };

            foreach (var (product, quantity) in lines)
            {
                // The sale's invoice type selects which of the two stored prices every line uses.
                var unitPrice = product.PriceFor(invoiceType);
                sale.Items.Add(new SaleItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Subtotal = Round(unitPrice * quantity)
                });
            }

            sale.TotalQuantity = sale.Items.Sum(i => i.Quantity);
            sale.TotalAmount = sale.Items.Sum(i => i.Subtotal);
            sale.TotalPaid = isCredit ? 0m : sale.TotalAmount;

            db.Sales.Add(sale);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var item in sale.Items)
            {
                MoveStock(workspace, branch.Id, item.ProductId, -item.Quantity, MovementType.Sale, at,
                    referenceType: "sale", referenceId: sale.Id);
            }

            await db.SaveChangesAsync(cancellationToken);

            if (isCredit)
            {
                creditSales.Add((sale, date));
            }
        }

        await SeedPaymentsAsync(workspace, creditSales, cancellationToken);
    }

    /// <summary>
    /// Instalments on the credit sales. Roughly half stay open on purpose: the collections screen
    /// and the overdue report (15 days) need debtors to have something to show.
    /// </summary>
    private async Task SeedPaymentsAsync(
        Workspace workspace,
        List<(Sale Sale, DateOnly Date)> creditSales,
        CancellationToken cancellationToken)
    {
        foreach (var (sale, saleDate) in creditSales)
        {
            var roll = workspace.Random.Next(100);

            // 30% settled, 40% partially paid, 30% untouched and ageing.
            var instalments = roll switch
            {
                < 30 => workspace.Random.Next(1, 4),
                < 70 => workspace.Random.Next(1, 3),
                _ => 0
            };

            if (instalments == 0)
            {
                continue;
            }

            var settle = roll < 30;
            var remaining = sale.Balance;

            for (var i = 0; i < instalments && remaining > 0; i++)
            {
                var isLast = i == instalments - 1;
                var amount = settle && isLast
                    ? remaining
                    : Round(remaining * (decimal)(workspace.Random.NextDouble() * 0.4 + 0.2));

                if (amount <= 0)
                {
                    break;
                }

                amount = Math.Min(amount, remaining);

                var paymentDate = MinDate(saleDate.AddDays(workspace.Random.Next(5, 45) * (i + 1)), workspace.Today);

                // Money can be received at a different counter than the one that sold — that is
                // what makes a till balance.
                var receivingBranch = PickBranch(workspace);

                sale.RegisterPayment(
                    amount,
                    paymentDate,
                    receivingBranch.Id,
                    PickSeller(workspace, receivingBranch).Id,
                    At(paymentDate, workspace.Random));

                remaining = sale.Balance;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------- adjustments

    private async Task SeedAdjustmentsAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        var dates = SpreadDates(workspace, AdjustmentCount, fromDay: 150);

        foreach (var date in dates)
        {
            var branch = PickBranch(workspace);
            var product = workspace.Products[workspace.Random.Next(workspace.Products.Count)];
            var available = Available(workspace, branch.Id, product.Id);

            // Shrinkage is never allowed to drive the row negative; StockLevel.Apply would throw.
            var delta = workspace.Random.Next(100) < 60
                ? -Math.Min(workspace.Random.Next(1, 4), available)
                : workspace.Random.Next(1, 5);

            if (delta == 0)
            {
                continue;
            }

            MoveStock(workspace, branch.Id, product.Id, delta, MovementType.Adjustment, At(date, workspace.Random),
                referenceType: "manual",
                notes: DemoDataCatalog.AdjustmentNotes[workspace.Random.Next(DemoDataCatalog.AdjustmentNotes.Length)]);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Drives a few products to zero and a few below the SPA's low-stock threshold, so the
    /// dashboard and the inventory badges are not a uniform wall of green.
    /// </summary>
    private async Task SeedStockExtremesAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        var main = workspace.Branches[0];
        var date = workspace.Today.AddDays(-workspace.Random.Next(1, 7));
        var at = At(date, workspace.Random);

        var candidates = PickDistinctProducts(workspace, LowStockProductCount + OutOfStockProductCount).ToList();

        foreach (var (product, index) in candidates.Select((p, i) => (p, i)))
        {
            var available = Available(workspace, main.Id, product.Id);
            var target = index < OutOfStockProductCount ? 0 : workspace.Random.Next(1, 5);
            var delta = target - available;

            if (delta == 0)
            {
                continue;
            }

            MoveStock(workspace, main.Id, product.Id, delta, MovementType.Adjustment, at,
                referenceType: "manual",
                notes: "Ajuste por conteo físico de cierre");
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------- orders and quotes

    private async Task SeedOrdersAndQuotesAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        var orderDates = SpreadDates(workspace, DemoDataCatalog.OrderRequests.Length, fromDay: 60);
        var statuses = new[] { OrderStatus.Pending, OrderStatus.Delivered, OrderStatus.Cancelled };

        foreach (var ((description, price), index) in DemoDataCatalog.OrderRequests.Select((r, i) => (r, i)))
        {
            var client = workspace.Clients[workspace.Random.Next(workspace.Clients.Count)];
            var branch = PickBranch(workspace);
            var advance = Round(price * (decimal)(workspace.Random.NextDouble() * 0.5));

            var order = new Order
            {
                OrderDate = orderDates[index],
                BranchId = branch.Id,
                ClientName = client.FullName,
                Phone = client.Phone,
                ProductDescription = description,
                Price = price,
                AdvanceAmount = advance,
                Status = statuses[workspace.Random.Next(statuses.Length)],
                OwnerId = PickSeller(workspace, branch).Id
            };

            order.EnsureAdvanceWithinPrice();
            db.Orders.Add(order);
        }

        var quoteDates = SpreadDates(workspace, DemoDataCatalog.Quotes.Length, fromDay: 90);

        foreach (var ((detail, price), index) in DemoDataCatalog.Quotes.Select((q, i) => (q, i)))
        {
            var client = workspace.Clients[workspace.Random.Next(workspace.Clients.Count)];
            var branch = PickBranch(workspace);

            db.Quotes.Add(new Quote
            {
                QuoteDate = quoteDates[index],
                BranchId = branch.Id,
                ClientName = client.FullName,
                Phone = client.Phone,
                Detail = detail,
                Price = price,
                SupplierName = workspace.Random.Next(100) < 60
                    ? workspace.Suppliers[workspace.Random.Next(workspace.Suppliers.Count)].Name
                    : null,
                OwnerId = PickSeller(workspace, branch).Id
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------- ledger mechanics

    /// <summary>
    /// The single place stock changes: the cache moves through <see cref="StockLevel.Apply"/> and
    /// the ledger row that explains it is written in the same breath. Nothing else in this class
    /// touches <c>StockLevel.Quantity</c>.
    /// </summary>
    private void MoveStock(
        Workspace workspace,
        long branchId,
        long productId,
        int quantityDelta,
        MovementType movementType,
        DateTimeOffset at,
        decimal? unitCost = null,
        string? referenceType = null,
        long? referenceId = null,
        string? notes = null)
    {
        var level = workspace.Stock[(branchId, productId)];
        level.Apply(quantityDelta, at);

        var movement = new InventoryMovement
        {
            BranchId = branchId,
            ProductId = productId,
            MovementType = movementType,
            QuantityDelta = quantityDelta,
            UnitCost = unitCost,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Notes = notes
        };

        db.InventoryMovements.Add(movement);
        workspace.Ledger.Add((movement, at));
    }

    /// <summary>
    /// The audit interceptor stamps <c>CreatedAt</c> at insert time, so every ledger row would
    /// otherwise read "today". A second pass restores the business timestamps — safe here because
    /// <see cref="InventoryMovement"/> is not an <c>AuditableEntity</c>, so a modification does not
    /// trigger the <c>UpdatedAt</c> stamp.
    /// </summary>
    private async Task BackdateLedgerAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        foreach (var (movement, at) in workspace.Ledger)
        {
            movement.CreatedAt = at;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------- helpers

    private int Available(Workspace workspace, long branchId, long productId) =>
        workspace.Stock[(branchId, productId)].Quantity;

    private static Branch BranchByCode(List<Branch> branches, string code) =>
        branches.First(b => string.Equals(b.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>Weighted towards the main store, which is where most of the counter traffic is.</summary>
    private static Branch PickBranch(Workspace workspace) =>
        workspace.Branches.Count < 2 || workspace.Random.Next(100) < 70
            ? workspace.Branches[0]
            : workspace.Branches[1];

    private static User PickSeller(Workspace workspace, Branch branch)
    {
        var local = workspace.Sellers.Where(s => s.BranchId == branch.Id).ToList();
        var pool = local.Count > 0 ? local : workspace.Sellers;
        return pool[workspace.Random.Next(pool.Count)];
    }

    private static IEnumerable<Product> PickDistinctProducts(Workspace workspace, int count)
    {
        var picked = new HashSet<long>();

        while (picked.Count < count && picked.Count < workspace.Products.Count)
        {
            var product = workspace.Products[workspace.Random.Next(workspace.Products.Count)];
            if (picked.Add(product.Id))
            {
                yield return product;
            }
        }
    }

    /// <summary>Dates spread over the window and returned in order, because insertion order is the timeline.</summary>
    private static List<DateOnly> SpreadDates(Workspace workspace, int count, int fromDay)
    {
        var span = workspace.Today.DayNumber - workspace.Start.AddDays(fromDay).DayNumber;
        if (span <= 0)
        {
            return Enumerable.Repeat(workspace.Today, count).ToList();
        }

        return Enumerable.Range(0, count)
            .Select(_ => workspace.Start.AddDays(fromDay + workspace.Random.Next(span + 1)))
            .Order()
            .ToList();
    }

    /// <summary>
    /// Business hours in Bolivian time, so the ledger does not read as a 03:00 batch job. Returned
    /// as UTC because Npgsql rejects any non-zero offset for <c>timestamp with time zone</c>; the
    /// instant is the same one, and the SPA renders it back in local time.
    /// </summary>
    private static DateTimeOffset At(DateOnly date, Random random) =>
        new DateTimeOffset(date.ToDateTime(new TimeOnly(random.Next(9, 19), random.Next(0, 60))), BoliviaOffset)
            .ToUniversalTime();

    private static DateOnly MinDate(DateOnly candidate, DateOnly ceiling) =>
        candidate > ceiling ? ceiling : candidate;

    /// <summary>Costs drift ±8% between receipts, which is what makes "last purchase cost" meaningful.</summary>
    private static decimal CostOf(Workspace workspace, Product product)
    {
        var drift = 1m + (decimal)(workspace.Random.NextDouble() * 0.16 - 0.08);
        return Round(workspace.BaseCosts[product.Id] * drift);
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static string RandomPhone(Random random) =>
        random.Next(100) < 60
            ? $"+591 7{random.Next(1000000, 9999999)}"
            : $"+591 4 4{random.Next(100000, 999999)}";

    private static string RandomAddress(Random random) =>
        $"{DemoDataCatalog.Streets[random.Next(DemoDataCatalog.Streets.Length)]} #{random.Next(100, 2000)}";

    /// <summary>Everything the generation phases share, so they read as steps rather than as plumbing.</summary>
    private sealed class Workspace
    {
        public required Random Random { get; init; }
        public required DateOnly Start { get; init; }
        public required DateOnly Today { get; init; }
        public required List<Branch> Branches { get; init; }
        public required List<User> Sellers { get; init; }
        public required List<Supplier> Suppliers { get; init; }
        public required List<Product> Products { get; init; }

        /// <summary>Catalog cost per product id — the anchor every purchase price drifts around.</summary>
        public required Dictionary<long, decimal> BaseCosts { get; init; }

        public required List<Client> Clients { get; init; }
        public required Dictionary<(long BranchId, long ProductId), StockLevel> Stock { get; init; }

        /// <summary>Movements paired with the moment they represent, for the backdating pass.</summary>
        public List<(InventoryMovement Movement, DateTimeOffset At)> Ledger { get; } = [];
    }
}
