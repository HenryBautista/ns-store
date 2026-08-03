# Graph Report - ns-store  (2026-08-03)

## Corpus Check
- 153 files · ~72,441 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1421 nodes · 3359 edges · 81 communities (68 shown, 13 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 194 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `7435dbfb`
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
- .Apply
- UniqueClientCi
- AppClaimTypes.cs
- .ExecuteInTransactionAsync
- CatalogDtos.cs
- .CreateAsync
- ReportDtos.cs
- ProductSerialTests
- PageRequest
- ProductDtos.cs
- BranchDtos.cs
- IReadOnlyCollection
- OrderDtos.cs
- UserDtos.cs
- IServiceCollection
- DbSet
- Func
- MigrationBuilder
- InlineData
- Theory
- long

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Enums` - 46 edges
2. `TestHarness` - 44 edges
3. `DemoDataSeeder` - 38 edges
4. `NsStore.Application.Common` - 37 edges
5. `NsStore.Domain.Common` - 36 edges
6. `AppDbContext` - 33 edges
7. `NsStore.Domain.Entities` - 33 edges
8. `IAppDbContext` - 28 edges
9. `SerializedInventoryTests` - 26 edges
10. `NsStore.Application.Common.Models` - 25 edges

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

## Communities (81 total, 13 thin omitted)

### Community 0 - "PagedResult"
Cohesion: 0.41
Nodes (3): Fact, Task, SerialServiceTests

### Community 1 - "NsStore.Infrastructure.csproj"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - ".CreateProductAsync"
Cohesion: 0.11
Nodes (14): InlineData, Fact, Task, AccentInsensitiveSearchTests, Fact, Task, BranchServiceTests, Fact (+6 more)

### Community 3 - ".ReadyProductAsync"
Cohesion: 0.41
Nodes (3): Fact, Task, SaleServiceTests

### Community 4 - ".MapReportEndpoints"
Cohesion: 0.09
Nodes (29): Amount, Balance, ClientDto, ClientRequest, ClientStatementDto, DashboardDto, DebtsReportDto, Paid (+21 more)

### Community 5 - "UserService"
Cohesion: 0.29
Nodes (10): CreateUserRequest, IEndpointRouteBuilder, UserEndpoints, CancellationToken, Task, User, UserRole, UserService (+2 more)

### Community 6 - "Ports.cs"
Cohesion: 0.10
Nodes (19): HttpContext, IEndpointRouteBuilder, AuthEndpoints, CancellationToken, Func, Task, AccessToken, IssuedRefreshToken (+11 more)

### Community 7 - "ReportDtos.cs"
Cohesion: 0.20
Nodes (10): DateTimeOffset, IHasCreationAudit, DateTimeOffset, List, ProductSerial, ProductSerialEvent, SerialEventType, EntityTypeBuilder (+2 more)

### Community 8 - "NsStore.Application.Common.Interfaces"
Cohesion: 0.15
Nodes (16): IEndpointRouteBuilder, InventoryEndpoints, BranchAvailabilityDto, InventoryMovementDto, KardexQuery, KardexRowDto, StockLevelDto, StockQuery (+8 more)

### Community 9 - "AuditableEntity"
Cohesion: 0.27
Nodes (10): OrderDto, OrderQuery, OrderRequest, IEndpointRouteBuilder, OrderEndpoints, CancellationToken, Expression, Order (+2 more)

### Community 10 - ".UpdateAsync"
Cohesion: 0.15
Nodes (10): InvoiceType, DateOnly, DateTimeOffset, Fact, InlineData, long, Theory, OrderTests (+2 more)

### Community 11 - "IEndpointRouteBuilder"
Cohesion: 0.55
Nodes (3): Fact, Task, CrossBranchAvailabilityTests

### Community 12 - "Client"
Cohesion: 0.12
Nodes (18): IEntityTypeConfiguration, DateOnly, Order, Quote, List, User, OrderStatus, EntityTypeBuilder (+10 more)

### Community 13 - "ProductService"
Cohesion: 0.28
Nodes (10): PriceSuggestionDto, ProductDto, ProductRequest, SetPricesRequest, ProductEndpoints, CancellationToken, Expression, Product (+2 more)

### Community 14 - "IEntityTypeConfiguration"
Cohesion: 0.50
Nodes (3): DateTimeOffset, Guid, RefreshToken

### Community 15 - "IAppDbContext"
Cohesion: 0.19
Nodes (10): DateOnly, List, Purchase, PurchaseItem, ClientType, MovementType, PaymentStatus, ProductSerialStatus (+2 more)

### Community 16 - "docker-compose 'api' service (src/NsStore.Api/Dockerfile)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "AbstractValidator"
Cohesion: 0.12
Nodes (24): Applied, ClientDebtDto, ClientDebtQuery, CollectDebtRequest, CollectionReceiptDto, CreateSaleRequest, RegisterPaymentRequest, SaleItemDto (+16 more)

### Community 18 - "ClientServiceTests"
Cohesion: 0.30
Nodes (5): ClientDto, ClientRequest, Fact, Task, ClientServiceTests

### Community 19 - "AuthPolicies.cs"
Cohesion: 0.67
Nodes (3): string, AuthCookies, AuthPolicies

### Community 20 - ".UpdateAsync"
Cohesion: 0.28
Nodes (10): QuoteEndpoints, CancellationToken, Expression, Quote, Task, QuoteDto, QuoteQuery, QuoteRequest (+2 more)

### Community 21 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 22 - "DemoDataSeeder"
Cohesion: 0.10
Nodes (36): At, BaseCosts, BranchId, Date, Dictionary, IEnumerable, MarginPct, Movement (+28 more)

### Community 23 - "SaleTests"
Cohesion: 0.22
Nodes (7): CreatePurchaseRequest, PurchaseDto, PurchaseItemDto, PurchaseItemRequest, PurchaseListItemDto, PurchaseQuery, CreatePurchaseRequestValidator

### Community 24 - "NsStore.Domain.Enums"
Cohesion: 0.14
Nodes (8): NsStore.Application.Common.Interfaces, NsStore.Domain.Tests, NsStore.Domain.Enums, NsStore.Domain.Common, NsStore.Domain.Entities, ClientRequestValidator, string, ErrorCodes

### Community 25 - "AppExceptionHandler"
Cohesion: 0.19
Nodes (9): Detail, ErrorCode, IExceptionHandler, CancellationToken, Exception, HttpContext, ValueTask, AppExceptionHandler (+1 more)

### Community 26 - "Sale"
Cohesion: 0.17
Nodes (15): ProductSerialDto, RegisterSerialsRequest, SerialDriftDto, SerialEventDto, SerialEventType, SerialLookupDto, SerialQuery, CancellationToken (+7 more)

### Community 27 - "Exceptions.cs"
Cohesion: 0.26
Nodes (10): Exception, IDictionary, AppException, BadRequestException, ConflictException, ForbiddenException, NotFoundException, UnauthorizedException (+2 more)

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
Cohesion: 0.08
Nodes (24): CancellationToken, IReadOnlyCollection, Task, DocumentKind, IDocumentNumberService, IStockLockService, StockKey, CreateTransferRequest (+16 more)

### Community 33 - "DatabaseInitializer"
Cohesion: 0.20
Nodes (8): IPasswordHasher, CancellationToken, string, Task, DatabaseInitializer, int, string, PasswordHasher

### Community 34 - "BranchService"
Cohesion: 0.29
Nodes (9): BranchDto, BranchRequest, IEndpointRouteBuilder, BranchEndpoints, Branch, CancellationToken, Expression, Task (+1 more)

### Community 35 - "Migration"
Cohesion: 0.06
Nodes (19): Migration, MigrationBuilder, MigrationBuilder, InitialSchema, MigrationBuilder, UniqueClientCi, MigrationBuilder, string (+11 more)

### Community 36 - ".SaveChangesAsync"
Cohesion: 0.12
Nodes (19): CategoryDto, DescriptionRequest, NameRequest, IEndpointRouteBuilder, CatalogEndpoints, CancellationToken, Category, Supplier (+11 more)

### Community 37 - "NsStore.Infrastructure.Persistence.Migrations"
Cohesion: 0.06
Nodes (20): NsStore.Infrastructure.Persistence.Migrations, NsStore.Infrastructure.Persistence, string, DemoDataCatalog, ModelBuilder, InitialSchema, ModelBuilder, UniqueClientCi (+12 more)

### Community 38 - "TestHarness"
Cohesion: 0.52
Nodes (3): Fact, Task, TransferServiceTests

### Community 39 - "Atomic sales transaction"
Cohesion: 0.67
Nodes (3): Atomic sales transaction, Credit sales / installment payments, Stock movement ledger (inventory_movements + stock_levels cache)

### Community 40 - "NsStore.Api.Middleware"
Cohesion: 0.10
Nodes (17): NsStore.Api.Endpoints, NsStore.Application.Features.Users, NsStore.Application, NsStore.Application.Features.Auth, NsStore.Application.Features.Catalogs, NsStore.Api.Security, NsStore.Application.Features.Quotes, NsStore.Application.Features.Orders (+9 more)

### Community 41 - "NsStore.Application.Features.Sales"
Cohesion: 0.18
Nodes (10): NsStore.Application.Common.Models, NsStore.Application.Tests, NsStore.Application.Features.Inventory, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Application.Common, NsStore.Application.Features.Sales, NsStore.Application.Features.Clients (+2 more)

### Community 45 - ".StockedInMainAsync"
Cohesion: 0.33
Nodes (5): ClaimsPrincipal, CurrentUser, BranchScope, ICurrentUser, UserRole

### Community 46 - "ICurrentUser"
Cohesion: 0.10
Nodes (25): ICurrentUser, IDisposable, IDocumentNumberService, IReadOnlyCollection, IStockLockService, long, ProductSerialStatus, SettingsService (+17 more)

### Community 47 - "Deploy — development / demo server"
Cohesion: 0.29
Nodes (6): Demo dataset, Deploy — development / demo server, Deploying, First-time setup, Operating notes, Why one origin

### Community 48 - "StockTransfer"
Cohesion: 0.27
Nodes (7): DateOnly, List, StockTransfer, StockTransferItem, EntityTypeBuilder, StockTransferConfiguration, StockTransferItemConfiguration

### Community 49 - "AppDbContextModelSnapshot"
Cohesion: 0.50
Nodes (3): ModelSnapshot, ModelBuilder, AppDbContextModelSnapshot

### Community 50 - "List"
Cohesion: 0.17
Nodes (12): DateTimeOffset, InventoryMovement, StockLevel, List, Product, EntityTypeBuilder, InventoryMovementConfiguration, ProductConfiguration (+4 more)

### Community 51 - "Product"
Cohesion: 0.56
Nodes (3): Fact, Task, ReportServiceTests

### Community 52 - ".AddInfrastructure"
Cohesion: 0.25
Nodes (5): NsStore.Infrastructure, NsStore.Infrastructure.Security, IConfiguration, IServiceCollection, DependencyInjection

### Community 53 - "DocumentNumberingTests"
Cohesion: 0.20
Nodes (10): IReadOnlyDictionary, IEndpointRouteBuilder, SettingsEndpoints, CancellationToken, int, Task, SettingsDto, SettingsService (+2 more)

### Community 54 - ".ReadyProductAsync"
Cohesion: 0.12
Nodes (15): ClientDebtDto, ClientDebtFilter, ClientDebtQuery, CollectAllocationRequest, CollectionReceiptDto, CreateSaleRequest, PaymentAllocationDto, PaymentDto (+7 more)

### Community 55 - "AppDbContext"
Cohesion: 0.28
Nodes (13): DbSet, IAppDbContext, Branch, Client, DateOnly, DateTimeOffset, List, Payment (+5 more)

### Community 56 - "CatalogEndpoints"
Cohesion: 0.11
Nodes (17): NsStore.Infrastructure.Persistence.Configurations, DateTimeOffset, AuditableEntity, DateTimeOffset, string, AppSetting, AppSettingKeys, Category (+9 more)

### Community 57 - "AppSetting"
Cohesion: 0.22
Nodes (7): ProductSerialDto, RegisterSerialsRequest, SerialDriftDto, SerialEventDto, SerialLookupDto, SerialSaleReferenceDto, RegisterSerialsRequestValidator

### Community 58 - ".TwoDebtsAsync"
Cohesion: 0.29
Nodes (6): decimal, Newer, Older, Fact, Task, CollectionTests

### Community 60 - ".Apply"
Cohesion: 0.16
Nodes (11): AbstractValidator, LoginRequest, LoginRequestValidator, StockAdjustmentRequest, StockAdjustmentRequestValidator, CreateUserRequest, UpdateUserBranchRequest, UpdateUserRequest (+3 more)

### Community 61 - "UniqueClientCi"
Cohesion: 0.22
Nodes (8): SaleDto, Fact, Task, BranchScopingTests, Fact, Task, DocumentNumberingTests, SaleDtoAssertions

### Community 62 - "AppClaimTypes.cs"
Cohesion: 0.33
Nodes (4): DateOnly, TimeProvider, TimeSpan, BusinessClock

### Community 63 - ".ExecuteInTransactionAsync"
Cohesion: 0.05
Nodes (34): AppSetting, DbContext, DbSet, Func, IAppDbContext, IDesignTimeDbContextFactory, Payment, PaymentReceipt (+26 more)

### Community 64 - "CatalogDtos.cs"
Cohesion: 0.21
Nodes (11): CatalogMapping, CategoryDto, DescriptionRequest, NameRequest, SupplierDto, SupplierRequest, TrademarkDto, WarrantyTermDto (+3 more)

### Community 65 - ".CreateAsync"
Cohesion: 0.29
Nodes (8): CreatePurchaseRequest, PurchaseDto, PurchaseListItemDto, PurchaseQuery, PurchaseEndpoints, CancellationToken, Task, PurchaseService

### Community 66 - "ReportDtos.cs"
Cohesion: 0.17
Nodes (11): ClientStatementDto, ClientStatementSaleDto, DashboardDto, DebtsReportDto, PriceListReportDto, PriceListRowDto, PurchasesReportDto, ReportRange (+3 more)

### Community 67 - "ProductSerialTests"
Cohesion: 0.42
Nodes (3): DateTimeOffset, Fact, ProductSerialTests

### Community 68 - "PageRequest"
Cohesion: 0.22
Nodes (7): CancellationToken, int, IQueryable, Task, PageRequest, QueryableExtensions, SerialQuery

### Community 69 - "ProductDtos.cs"
Cohesion: 0.29
Nodes (6): PriceSuggestionDto, ProductDto, ProductRequest, SetPricesRequest, ProductRequestValidator, SetPricesRequestValidator

### Community 70 - "BranchDtos.cs"
Cohesion: 0.33
Nodes (4): BranchDto, BranchRequest, UpdateBranchStatusRequest, BranchRequestValidator

### Community 72 - "OrderDtos.cs"
Cohesion: 0.33
Nodes (4): OrderDto, OrderQuery, OrderRequest, OrderRequestValidator

### Community 73 - "UserDtos.cs"
Cohesion: 0.40
Nodes (4): UpdateUserRoleRequest, UpdateUserStatusRequest, UserDto, UserMapping

## Knowledge Gaps
- **114 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)`, `Microsoft.EntityFrameworkCore.Design (10.0.10)`, `Microsoft.OpenApi (2.11.0)` (+109 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **13 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NsStore.Application.Common` connect `NsStore.Application.Features.Sales` to `PageRequest`, `.MapReportEndpoints`, `NsStore.Infrastructure.Persistence.Migrations`, `NsStore.Api.Middleware`, `.UpdateAsync`, `NsStore.Domain.Enums`, `Exceptions.cs`, `CancellationToken`, `AppClaimTypes.cs`, `.ExecuteInTransactionAsync`?**
  _High betweenness centrality (0.186) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Enums` connect `NsStore.Domain.Enums` to `ReportDtos.cs`, `NsStore.Infrastructure.Persistence.Migrations`, `NsStore.Api.Middleware`, `OrderDtos.cs`, `UserDtos.cs`, `NsStore.Application.Features.Sales`, `IAppDbContext`, `ClientServiceTests`, `.AddInfrastructure`, `AppDbContext`, `.ReadyProductAsync`, `SaleTests`, `AppSetting`?**
  _High betweenness centrality (0.182) - this node is a cross-community bridge._
- **Why does `TestHarness` connect `ICurrentUser` to `PagedResult`, `.CreateAsync`, `.CreateProductAsync`, `.ReadyProductAsync`, `BranchService`, `.MapReportEndpoints`, `Sale`, `TestHarness`, `NsStore.Application.Common.Interfaces`, `NsStore.Application.Features.Sales`, `IEndpointRouteBuilder`, `ProductService`, `AbstractValidator`, `ClientServiceTests`, `Product`, `.TwoDebtsAsync`, `UniqueClientCi`, `.ExecuteInTransactionAsync`?**
  _High betweenness centrality (0.166) - this node is a cross-community bridge._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)` to the rest of the system?**
  _114 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `NsStore.Infrastructure.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `.CreateProductAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.11498257839721254 - nodes in this community are weakly interconnected._
- **Should `.MapReportEndpoints` be split into smaller, more focused modules?**
  _Cohesion score 0.08672699849170437 - nodes in this community are weakly interconnected._