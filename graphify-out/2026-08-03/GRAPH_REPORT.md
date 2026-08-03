# Graph Report - ns-store  (2026-08-03)

## Corpus Check
- 148 files · ~68,848 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1373 nodes · 3277 edges · 85 communities (58 shown, 27 thin omitted)
- Extraction: 93% EXTRACTED · 7% INFERRED · 0% AMBIGUOUS · INFERRED: 216 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `cd0a0d72`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PagedResult
- NsStore.Infrastructure.csproj
- .CreateProductAsync
- .ReadyProductAsync
- .MapReportEndpoints
- UserService
- Ports.cs
- ReportDtos.cs
- NsStore.Application.Common.Interfaces
- AuditableEntity
- .UpdateAsync
- IEndpointRouteBuilder
- Client
- ProductService
- IEntityTypeConfiguration
- IAppDbContext
- docker-compose 'api' service (src/NsStore.Api/Dockerfile)
- AbstractValidator
- ClientServiceTests
- AuthPolicies.cs
- .UpdateAsync
- http
- DemoDataSeeder
- SaleTests
- NsStore.Domain.Enums
- AppExceptionHandler
- Sale
- Exceptions.cs
- CancellationToken
- .SavingChangesAsync
- .InvokeAsync
- CLAUDE.md
- .Apply
- DatabaseInitializer
- BranchService
- Migration
- .SaveChangesAsync
- NsStore.Infrastructure.Persistence.Migrations
- TestHarness
- Atomic sales transaction
- NsStore.Api.Middleware
- NsStore.Application.Features.Sales
- Dual price business rule
- Price suggestion formula
- Soft delete + audit columns
- .StockedInMainAsync
- ICurrentUser
- Deploy — development / demo server
- StockTransfer
- AppDbContextModelSnapshot
- List
- Product
- .AddInfrastructure
- DocumentNumberingTests
- .ReadyProductAsync
- AppDbContext
- CatalogEndpoints
- AppSetting
- .TwoDebtsAsync
- DateOnly
- DemoDataCatalog.cs
- UniqueClientCi
- AppClaimTypes.cs
- ErrorCodes.cs
- AddBranchDocumentNumbering
- AddStockTransfers
- AddPaymentReceipts
- AddSerializedInventory
- IQueryable
- IReadOnlyList
- IServiceCollection
- IReadOnlyCollection
- long
- ValueTask
- IServiceCollection
- DateOnly
- IQueryable
- IReadOnlyList
- List
- PagedResult
- Product
- PurchaseService
- SaleService
- Expression
- TransferService

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Enums` - 49 edges
2. `TestHarness` - 44 edges
3. `NsStore.Domain.Common` - 41 edges
4. `NsStore.Domain.Entities` - 41 edges
5. `DemoDataSeeder` - 38 edges
6. `AppDbContext` - 32 edges
7. `IAppDbContext` - 29 edges
8. `SerializedInventoryTests` - 25 edges
9. `NsStore.Application.Common.Interfaces` - 25 edges
10. `Product` - 25 edges

## Surprising Connections (you probably didn't know these)
- `Database__MigrateOnStartup setting` --semantically_similar_to--> `Verify model has no pending migrations (dotnet-ef check)`  [INFERRED] [semantically similar]
  README.md → .github/workflows/ci.yml
- `docker-compose 'api' service (src/NsStore.Api/Dockerfile)` --shares_data_with--> `ConnectionStrings__Default env var`  [INFERRED]
  docker-compose.yml → README.md
- `docker-compose 'api' service (src/NsStore.Api/Dockerfile)` --shares_data_with--> `Jwt__SigningKey env var`  [INFERRED]
  docker-compose.yml → README.md
- `docker-compose 'api' service (src/NsStore.Api/Dockerfile)` --shares_data_with--> `Cors__AllowedOrigins__0 env var`  [INFERRED]
  docker-compose.yml → README.md
- `docker-compose 'api' service (src/NsStore.Api/Dockerfile)` --shares_data_with--> `Seed__Admin__Username env var`  [INFERRED]
  docker-compose.yml → README.md

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Layered solution architecture (Api -> Application -> Domain, Infrastructure -> Application/Domain)** — readme_api_layer, readme_application_layer, readme_domain_layer, readme_infrastructure_layer [EXTRACTED 0.95]
- **Business rules worth knowing (pricing, stock, sales, credit, soft delete)** — readme_dual_price, readme_price_suggestion, readme_stock_ledger, readme_atomic_sales, readme_credit_sales, readme_soft_delete [EXTRACTED 0.90]
- **API service environment configuration shared with README config table** — docker_compose_api_service, readme_config_connectionstrings_default, readme_config_jwt_signingkey, readme_config_cors_allowedorigins_0, readme_config_seed_admin_username, readme_config_seed_admin_password [INFERRED 0.85]

## Communities (85 total, 27 thin omitted)

### Community 0 - "PagedResult"
Cohesion: 0.13
Nodes (19): PageRequest, BranchAvailabilityDto, InventoryMovementDto, KardexQuery, KardexRowDto, StockAdjustmentRequest, StockLevelDto, StockQuery (+11 more)

### Community 1 - "NsStore.Infrastructure.csproj"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - ".CreateProductAsync"
Cohesion: 0.13
Nodes (13): PurchaseDto, Fact, Task, BranchServiceTests, Fact, Task, InventoryReportingTests, Fact (+5 more)

### Community 3 - ".ReadyProductAsync"
Cohesion: 0.41
Nodes (3): Fact, Task, SaleServiceTests

### Community 4 - ".MapReportEndpoints"
Cohesion: 0.08
Nodes (30): Amount, Balance, NsStore.Application.Features.Settings, IReadOnlyDictionary, Paid, IEndpointRouteBuilder, ReportEndpoints, SettingsEndpoints (+22 more)

### Community 5 - "UserService"
Cohesion: 0.18
Nodes (15): IEndpointRouteBuilder, UserEndpoints, CreateUserRequest, UpdateUserBranchRequest, UpdateUserRequest, UpdateUserRoleRequest, UpdateUserStatusRequest, UserDto (+7 more)

### Community 6 - "Ports.cs"
Cohesion: 0.06
Nodes (32): HttpContext, IEndpointRouteBuilder, AuthEndpoints, CancellationToken, IReadOnlyCollection, Task, AccessToken, DocumentKind (+24 more)

### Community 7 - "ReportDtos.cs"
Cohesion: 0.09
Nodes (22): ClaimsPrincipal, IHasCreationAudit, CurrentUser, BranchScope, ICurrentUser, Branch, DateTimeOffset, List (+14 more)

### Community 8 - "NsStore.Application.Common.Interfaces"
Cohesion: 0.17
Nodes (5): NsStore.Application.Common.Interfaces, NsStore.Infrastructure, NsStore.Infrastructure.Persistence, string, DemoDataCatalog

### Community 9 - "AuditableEntity"
Cohesion: 0.12
Nodes (17): AppSetting, Branch, Category, Client, DbSet, InventoryMovement, Order, Quote (+9 more)

### Community 10 - ".UpdateAsync"
Cohesion: 0.23
Nodes (10): OrderEndpoints, OrderDto, OrderQuery, OrderRequest, CancellationToken, Expression, Task, OrderService (+2 more)

### Community 11 - "IEndpointRouteBuilder"
Cohesion: 0.17
Nodes (13): IEndpointRouteBuilder, PurchaseEndpoints, CreatePurchaseRequest, PurchaseDto, PurchaseItemDto, PurchaseItemRequest, PurchaseListItemDto, PurchaseQuery (+5 more)

### Community 12 - "Client"
Cohesion: 0.15
Nodes (13): IEntityTypeConfiguration, DateOnly, Quote, RefreshTokenConfiguration, UserConfiguration, EntityTypeBuilder, ClientConfiguration, OrderConfiguration (+5 more)

### Community 13 - "ProductService"
Cohesion: 0.06
Nodes (42): AbstractValidator, PageRequest, DescriptionRequestValidator, NameRequestValidator, SupplierRequestValidator, ClientRequestValidator, ProductSerialDto, RegisterSerialsRequest (+34 more)

### Community 14 - "IEntityTypeConfiguration"
Cohesion: 0.29
Nodes (4): DateTimeOffset, Guid, RefreshToken, EntityTypeBuilder

### Community 15 - "IAppDbContext"
Cohesion: 0.24
Nodes (9): AuditableEntity, Branch, DateOnly, List, Supplier, Purchase, PurchaseItem, PurchaseConfiguration (+1 more)

### Community 16 - "docker-compose 'api' service (src/NsStore.Api/Dockerfile)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "AbstractValidator"
Cohesion: 0.13
Nodes (24): Applied, ClientDebtDto, ClientDebtQuery, CollectDebtRequest, CollectionReceiptDto, CreateSaleRequest, Expression, RegisterPaymentRequest (+16 more)

### Community 18 - "ClientServiceTests"
Cohesion: 0.17
Nodes (11): IEndpointRouteBuilder, ClientEndpoints, ClientDto, ClientRequest, CancellationToken, Task, ClientService, Client (+3 more)

### Community 19 - "AuthPolicies.cs"
Cohesion: 0.67
Nodes (3): string, AuthCookies, AuthPolicies

### Community 20 - ".UpdateAsync"
Cohesion: 0.27
Nodes (10): IEndpointRouteBuilder, QuoteEndpoints, CancellationToken, Expression, Task, QuoteDto, QuoteQuery, QuoteRequest (+2 more)

### Community 21 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 22 - "DemoDataSeeder"
Cohesion: 0.19
Nodes (12): BaseCosts, MarginPct, Products, Random, CancellationToken, int, List, string (+4 more)

### Community 23 - "SaleTests"
Cohesion: 0.16
Nodes (10): InlineData, InvoiceType, DateOnly, DateTimeOffset, Fact, long, OrderTests, ProductTests (+2 more)

### Community 24 - "NsStore.Domain.Enums"
Cohesion: 0.22
Nodes (4): NsStore.Domain.Tests, NsStore.Domain.Enums, NsStore.Domain.Common, NsStore.Domain.Entities

### Community 26 - "Sale"
Cohesion: 0.42
Nodes (9): Branch, Client, DateOnly, DateTimeOffset, List, Payment, PaymentReceipt, Sale (+1 more)

### Community 27 - "Exceptions.cs"
Cohesion: 0.11
Nodes (18): Detail, ErrorCode, Exception, HttpContext, IDictionary, IExceptionHandler, CancellationToken, AppExceptionHandler (+10 more)

### Community 28 - "CancellationToken"
Cohesion: 0.15
Nodes (8): NsStore.Application.Common.Models, NsStore.Application.Common, NsStore.Application.Features.Clients, NsStore.Application.Features.Catalogs, NsStore.Application.Features.Branches, RouteHandlerBuilder, QueryEnum, ValidationFilterExtensions

### Community 29 - ".SavingChangesAsync"
Cohesion: 0.29
Nodes (7): DbContextEventData, InterceptionResult, SaveChangesInterceptor, CancellationToken, DbContext, ValueTask, AuditInterceptor

### Community 30 - ".InvokeAsync"
Cohesion: 0.33
Nodes (5): EndpointFilterDelegate, EndpointFilterInvocationContext, IEndpointFilter, ValueTask, ValidationFilter

### Community 31 - "CLAUDE.md"
Cohesion: 0.20
Nodes (8): Architecture, Commands, Configuration, Cross-cutting mechanics worth knowing before editing, graphify, Inventory and sales invariants, Tests, What this is

### Community 32 - ".Apply"
Cohesion: 0.16
Nodes (16): ProductSerial, ProductSerialDto, RegisterSerialsRequest, SerialDriftDto, SerialEventDto, SerialLookupDto, SerialQuery, InventoryEndpoints (+8 more)

### Community 33 - "DatabaseInitializer"
Cohesion: 0.53
Nodes (4): CancellationToken, string, Task, DatabaseInitializer

### Community 34 - "BranchService"
Cohesion: 0.23
Nodes (10): IEndpointRouteBuilder, BranchEndpoints, BranchDto, BranchRequest, UpdateBranchStatusRequest, CancellationToken, Expression, Task (+2 more)

### Community 35 - "Migration"
Cohesion: 0.07
Nodes (17): Migration, MigrationBuilder, MigrationBuilder, InitialSchema, MigrationBuilder, UniqueClientCi, MigrationBuilder, string (+9 more)

### Community 36 - ".SaveChangesAsync"
Cohesion: 0.10
Nodes (26): IEndpointRouteBuilder, CatalogEndpoints, CancellationToken, Func, Task, CancellationToken, int, IQueryable (+18 more)

### Community 37 - "NsStore.Infrastructure.Persistence.Migrations"
Cohesion: 0.06
Nodes (15): NsStore.Infrastructure.Persistence.Migrations, ModelBuilder, InitialSchema, ModelBuilder, UniqueClientCi, ModelBuilder, AddBranches, ModelBuilder (+7 more)

### Community 38 - "TestHarness"
Cohesion: 0.08
Nodes (29): AppDbContext, BranchService, ClientService, ICurrentUser, IDisposable, IDocumentNumberService, IReadOnlyCollection, IStockLockService (+21 more)

### Community 39 - "Atomic sales transaction"
Cohesion: 0.67
Nodes (3): Atomic sales transaction, Credit sales / installment payments, Stock movement ledger (inventory_movements + stock_levels cache)

### Community 40 - "NsStore.Api.Middleware"
Cohesion: 0.22
Nodes (8): NsStore.Api.Endpoints, NsStore.Api.Security, NsStore.Application.Features.Quotes, NsStore.Application.Features.Orders, NsStore.Api.Middleware, string, RateLimitPolicies, Program

### Community 41 - "NsStore.Application.Features.Sales"
Cohesion: 0.21
Nodes (8): NsStore.Application.Tests, NsStore.Application.Features.Inventory, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Application.Features.Sales, NsStore.Application.Features.Purchases, ClientStatementSaleDto, PriceListRowDto

### Community 45 - ".StockedInMainAsync"
Cohesion: 0.16
Nodes (12): PageRequest, CreateTransferRequest, TransferDto, TransferItemDto, TransferItemRequest, TransferListItemDto, TransferQuery, CancellationToken (+4 more)

### Community 46 - "ICurrentUser"
Cohesion: 0.16
Nodes (8): Fact, ProductSerialStatus, SaleDto, Task, SerializedInventoryTests, Task, SerialServiceTests, Task

### Community 47 - "Deploy — development / demo server"
Cohesion: 0.29
Nodes (6): Demo dataset, Deploy — development / demo server, Deploying, First-time setup, Operating notes, Why one origin

### Community 48 - "StockTransfer"
Cohesion: 0.29
Nodes (7): DateOnly, List, StockTransfer, StockTransferItem, EntityTypeBuilder, StockTransferConfiguration, StockTransferItemConfiguration

### Community 49 - "AppDbContextModelSnapshot"
Cohesion: 0.40
Nodes (3): ModelSnapshot, ModelBuilder, AppDbContextModelSnapshot

### Community 50 - "List"
Cohesion: 0.13
Nodes (19): BranchId, Dictionary, Movement, ProductId, DateTimeOffset, InventoryMovement, StockLevel, Category (+11 more)

### Community 51 - "Product"
Cohesion: 0.29
Nodes (6): EntityTypeBuilder, InventoryMovement, StockLevel, InventoryMovementConfiguration, ProductConfiguration, StockLevelConfiguration

### Community 52 - ".AddInfrastructure"
Cohesion: 0.25
Nodes (5): NsStore.Application, IConfiguration, IServiceCollection, DependencyInjection, DependencyInjection

### Community 53 - "DocumentNumberingTests"
Cohesion: 0.40
Nodes (3): NsStore.Infrastructure.Security, string, JwtOptions

### Community 54 - ".ReadyProductAsync"
Cohesion: 0.24
Nodes (6): At, Date, IEnumerable, DateOnly, Quantity, Workspace

### Community 55 - "AppDbContext"
Cohesion: 0.09
Nodes (21): DbContext, IDesignTimeDbContextFactory, AppSetting, Branch, Category, Client, DbSet, InventoryMovement (+13 more)

### Community 56 - "CatalogEndpoints"
Cohesion: 0.13
Nodes (15): NsStore.Infrastructure.Persistence.Configurations, DateTimeOffset, string, AppSetting, AppSettingKeys, Category, Supplier, Trademark (+7 more)

### Community 57 - "AppSetting"
Cohesion: 0.50
Nodes (3): CancellationToken, Func, Task

### Community 58 - ".TwoDebtsAsync"
Cohesion: 0.29
Nodes (6): decimal, Newer, Older, Fact, Task, CollectionTests

### Community 59 - "DateOnly"
Cohesion: 0.32
Nodes (5): NsStore.Application.Features.Users, NsStore.Application.Features.Auth, LoginRequest, LoginResponse, LoginRequestValidator

### Community 60 - "DemoDataCatalog.cs"
Cohesion: 0.18
Nodes (9): DateTimeOffset, AuditableEntity, DateTimeOffset, IHasCreationAudit, Branch, List, User, EntityTypeBuilder (+1 more)

### Community 61 - "UniqueClientCi"
Cohesion: 0.22
Nodes (8): SaleDto, Fact, Task, BranchScopingTests, Fact, Task, DocumentNumberingTests, SaleDtoAssertions

## Knowledge Gaps
- **97 isolated node(s):** `TransferItemRequest`, `TransferItemDto`, `PurchaseItemRequest`, `PurchaseItemDto`, `ProductSerialDto` (+92 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **27 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NsStore.Domain.Enums` connect `NsStore.Domain.Enums` to `UserService`, `Ports.cs`, `ReportDtos.cs`, `NsStore.Api.Middleware`, `NsStore.Application.Features.Sales`, `.UpdateAsync`, `NsStore.Application.Common.Interfaces`, `NsStore.Infrastructure.Persistence.Migrations`, `ProductService`, `AppDbContextModelSnapshot`, `ClientServiceTests`, `Sale`, `CancellationToken`?**
  _High betweenness centrality (0.237) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Common` connect `NsStore.Domain.Enums` to `AddBranchDocumentNumbering`, `AddStockTransfers`, `NsStore.Application.Common.Interfaces`, `Exceptions.cs`, `NsStore.Application.Features.Sales`, `DemoDataCatalog.cs`, `.UpdateAsync`, `Sale`, `DateOnly`, `CancellationToken`?**
  _High betweenness centrality (0.131) - this node is a cross-community bridge._
- **Why does `TestHarness` connect `TestHarness` to `PagedResult`, `.Apply`, `.CreateProductAsync`, `.ReadyProductAsync`, `NsStore.Application.Features.Sales`, `IEndpointRouteBuilder`, `.StockedInMainAsync`, `ICurrentUser`, `AbstractValidator`, `ClientServiceTests`, `.TwoDebtsAsync`, `UniqueClientCi`?**
  _High betweenness centrality (0.119) - this node is a cross-community bridge._
- **What connects `TransferItemRequest`, `TransferItemDto`, `PurchaseItemRequest` to the rest of the system?**
  _97 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PagedResult` be split into smaller, more focused modules?**
  _Cohesion score 0.12903225806451613 - nodes in this community are weakly interconnected._
- **Should `NsStore.Infrastructure.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `.CreateProductAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.12692307692307692 - nodes in this community are weakly interconnected._