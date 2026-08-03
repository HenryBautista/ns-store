# Graph Report - ns-store  (2026-08-03)

## Corpus Check
- 153 files · ~72,441 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1300 nodes · 3497 edges · 66 communities (61 shown, 5 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 271 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `0c30921e`
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
- IReadOnlyCollection
- IServiceCollection

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Enums` - 63 edges
2. `NsStore.Domain.Common` - 47 edges
3. `NsStore.Domain.Entities` - 47 edges
4. `TestHarness` - 44 edges
5. `DemoDataSeeder` - 38 edges
6. `NsStore.Application.Common` - 37 edges
7. `AppDbContext` - 33 edges
8. `NsStore.Application.Common.Interfaces` - 31 edges
9. `IAppDbContext` - 29 edges
10. `Branch` - 29 edges

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

## Communities (66 total, 5 thin omitted)

### Community 0 - "PagedResult"
Cohesion: 0.41
Nodes (3): Fact, Task, SerialServiceTests

### Community 1 - "NsStore.Infrastructure.csproj"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - ".CreateProductAsync"
Cohesion: 0.14
Nodes (12): Fact, Task, AccentInsensitiveSearchTests, Fact, Task, BranchServiceTests, Fact, Task (+4 more)

### Community 3 - ".ReadyProductAsync"
Cohesion: 0.41
Nodes (3): Fact, Task, SaleServiceTests

### Community 4 - ".MapReportEndpoints"
Cohesion: 0.07
Nodes (32): Amount, Balance, IReadOnlyDictionary, Paid, IEndpointRouteBuilder, ReportEndpoints, SettingsEndpoints, string (+24 more)

### Community 5 - "UserService"
Cohesion: 0.09
Nodes (28): AbstractValidator, IEndpointRouteBuilder, UserEndpoints, LoginRequest, LoginRequestValidator, BranchRequestValidator, DescriptionRequestValidator, NameRequestValidator (+20 more)

### Community 6 - "Ports.cs"
Cohesion: 0.26
Nodes (9): HttpContext, IEndpointRouteBuilder, AuthEndpoints, AuthResult, CancellationToken, DateTimeOffset, Guid, Task (+1 more)

### Community 7 - "ReportDtos.cs"
Cohesion: 0.15
Nodes (12): DateTimeOffset, IHasCreationAudit, DateTimeOffset, List, ProductSerial, ProductSerialEvent, EntityTypeBuilder, ProductSerialConfiguration (+4 more)

### Community 8 - "NsStore.Application.Common.Interfaces"
Cohesion: 0.16
Nodes (14): BranchAvailabilityDto, InventoryMovementDto, KardexQuery, KardexRowDto, StockAdjustmentRequest, StockLevelDto, StockQuery, CancellationToken (+6 more)

### Community 9 - "AuditableEntity"
Cohesion: 0.33
Nodes (5): DocumentKind, IDocumentNumberService, CancellationToken, Task, DocumentNumberService

### Community 10 - ".UpdateAsync"
Cohesion: 0.12
Nodes (18): OrderEndpoints, OrderDto, OrderQuery, OrderRequest, CancellationToken, Expression, Task, OrderService (+10 more)

### Community 11 - "IEndpointRouteBuilder"
Cohesion: 0.55
Nodes (3): Fact, Task, CrossBranchAvailabilityTests

### Community 12 - "Client"
Cohesion: 0.10
Nodes (20): NsStore.Infrastructure.Persistence.Configurations, IEntityTypeConfiguration, DateOnly, List, Purchase, PurchaseItem, EntityTypeBuilder, BranchConfiguration (+12 more)

### Community 13 - "ProductService"
Cohesion: 0.29
Nodes (9): ProductEndpoints, PriceSuggestionDto, ProductDto, ProductRequest, SetPricesRequest, CancellationToken, Expression, Task (+1 more)

### Community 14 - "IEntityTypeConfiguration"
Cohesion: 0.24
Nodes (6): DateTimeOffset, Guid, RefreshToken, EntityTypeBuilder, RefreshTokenConfiguration, UserConfiguration

### Community 15 - "IAppDbContext"
Cohesion: 0.22
Nodes (7): ClientType, InvoiceType, MovementType, OrderStatus, PaymentStatus, SerialEventType, ProductTests

### Community 16 - "docker-compose 'api' service (src/NsStore.Api/Dockerfile)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "AbstractValidator"
Cohesion: 0.07
Nodes (38): Applied, SaleEndpoints, CancellationToken, Func, Task, CancellationToken, IQueryable, Task (+30 more)

### Community 18 - "ClientServiceTests"
Cohesion: 0.18
Nodes (10): IEndpointRouteBuilder, ClientEndpoints, ClientDto, ClientRequest, CancellationToken, Task, ClientService, Fact (+2 more)

### Community 19 - "AuthPolicies.cs"
Cohesion: 0.67
Nodes (3): string, AuthCookies, AuthPolicies

### Community 20 - ".UpdateAsync"
Cohesion: 0.30
Nodes (9): IEndpointRouteBuilder, QuoteEndpoints, CancellationToken, Expression, Task, QuoteDto, QuoteQuery, QuoteRequest (+1 more)

### Community 21 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 22 - "DemoDataSeeder"
Cohesion: 0.15
Nodes (17): At, Date, IEnumerable, MarginPct, Random, CancellationToken, DateOnly, DateTimeOffset (+9 more)

### Community 23 - "SaleTests"
Cohesion: 0.14
Nodes (15): IEndpointRouteBuilder, PurchaseEndpoints, CancellationToken, IReadOnlyCollection, Task, CreatePurchaseRequest, PurchaseDto, PurchaseItemDto (+7 more)

### Community 24 - "NsStore.Domain.Enums"
Cohesion: 0.12
Nodes (9): NsStore.Application.Common.Interfaces, NsStore.Application.Common.Models, NsStore.Domain.Tests, NsStore.Application.Features.Users, NsStore.Application.Common, NsStore.Application.Features.Branches, NsStore.Domain.Common, NsStore.Application.Features.Settings (+1 more)

### Community 25 - "AppExceptionHandler"
Cohesion: 0.19
Nodes (9): Detail, ErrorCode, IExceptionHandler, CancellationToken, Exception, HttpContext, ValueTask, AppExceptionHandler (+1 more)

### Community 26 - "Sale"
Cohesion: 0.33
Nodes (6): CancellationToken, DateTimeOffset, IReadOnlyList, List, Task, SerialService

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
Cohesion: 0.16
Nodes (12): IEndpointRouteBuilder, InventoryEndpoints, CreateTransferRequest, TransferDto, TransferItemDto, TransferItemRequest, TransferListItemDto, TransferQuery (+4 more)

### Community 33 - "DatabaseInitializer"
Cohesion: 0.53
Nodes (4): CancellationToken, string, Task, DatabaseInitializer

### Community 34 - "BranchService"
Cohesion: 0.27
Nodes (9): IEndpointRouteBuilder, BranchEndpoints, BranchDto, BranchRequest, UpdateBranchStatusRequest, CancellationToken, Expression, Task (+1 more)

### Community 35 - "Migration"
Cohesion: 0.06
Nodes (19): Migration, MigrationBuilder, InitialSchema, MigrationBuilder, UniqueClientCi, MigrationBuilder, string, AddBranches (+11 more)

### Community 36 - ".SaveChangesAsync"
Cohesion: 0.13
Nodes (18): IEndpointRouteBuilder, CatalogEndpoints, int, PageRequest, CatalogMapping, CategoryDto, DescriptionRequest, NameRequest (+10 more)

### Community 37 - "NsStore.Infrastructure.Persistence.Migrations"
Cohesion: 0.06
Nodes (21): NsStore.Infrastructure.Persistence.Migrations, NsStore.Domain.Enums, NsStore.Infrastructure.Persistence, string, DemoDataCatalog, ModelBuilder, InitialSchema, ModelBuilder (+13 more)

### Community 38 - "TestHarness"
Cohesion: 0.15
Nodes (14): IDisposable, SqliteConnection, Fact, Task, DocumentNumberingTests, DateOnly, DateTimeOffset, long (+6 more)

### Community 39 - "Atomic sales transaction"
Cohesion: 0.67
Nodes (3): Atomic sales transaction, Credit sales / installment payments, Stock movement ledger (inventory_movements + stock_levels cache)

### Community 40 - "NsStore.Api.Middleware"
Cohesion: 0.12
Nodes (13): NsStore.Api.Endpoints, NsStore.Application, NsStore.Infrastructure, NsStore.Application.Features.Catalogs, NsStore.Api.Security, NsStore.Application.Features.Quotes, NsStore.Application.Features.Orders, NsStore.Infrastructure.Security (+5 more)

### Community 41 - "NsStore.Application.Features.Sales"
Cohesion: 0.25
Nodes (9): NsStore.Application.Tests, NsStore.Application.Features.Inventory, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Application.Features.Sales, NsStore.Application.Features.Clients, NsStore.Application.Features.Purchases, ClientStatementSaleDto (+1 more)

### Community 45 - ".StockedInMainAsync"
Cohesion: 0.31
Nodes (6): ClaimsPrincipal, CurrentUser, BranchScope, ICurrentUser, UserRole, FakeCurrentUser

### Community 46 - "ICurrentUser"
Cohesion: 0.26
Nodes (5): ProductSerialStatus, Fact, Task, SerializedInventoryTests, Task

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
Cohesion: 0.21
Nodes (12): BranchId, Dictionary, Movement, ProductId, DateTimeOffset, InventoryMovement, StockLevel, List (+4 more)

### Community 51 - "Product"
Cohesion: 0.56
Nodes (3): Fact, Task, ReportServiceTests

### Community 52 - ".AddInfrastructure"
Cohesion: 0.50
Nodes (3): IConfiguration, IServiceCollection, DependencyInjection

### Community 53 - "DocumentNumberingTests"
Cohesion: 0.19
Nodes (7): AccessToken, IssuedRefreshToken, ITokenService, string, JwtOptions, TokenService, SymmetricSecurityKey

### Community 54 - ".ReadyProductAsync"
Cohesion: 0.29
Nodes (4): IPasswordHasher, int, string, PasswordHasher

### Community 55 - "AppDbContext"
Cohesion: 0.14
Nodes (26): DbContext, IDesignTimeDbContextFactory, DbSet, IAppDbContext, DateTimeOffset, AuditableEntity, DateTimeOffset, string (+18 more)

### Community 56 - "CatalogEndpoints"
Cohesion: 0.17
Nodes (12): BaseCosts, Products, Category, Supplier, Trademark, WarrantyTerm, EntityTypeBuilder, AppSettingConfiguration (+4 more)

### Community 57 - "AppSetting"
Cohesion: 0.24
Nodes (7): ProductSerialDto, RegisterSerialsRequest, SerialDriftDto, SerialEventDto, SerialLookupDto, SerialQuery, SerialSaleReferenceDto

### Community 58 - ".TwoDebtsAsync"
Cohesion: 0.29
Nodes (6): decimal, Newer, Older, Fact, Task, CollectionTests

### Community 59 - "DateOnly"
Cohesion: 0.17
Nodes (6): NsStore.Application.Features.Auth, LoginResponse, string, AppClaimTypes, string, ErrorCodes

### Community 60 - ".Apply"
Cohesion: 0.48
Nodes (3): DateTimeOffset, Fact, StockLevelTests

### Community 61 - "UniqueClientCi"
Cohesion: 0.55
Nodes (3): Fact, Task, BranchScopingTests

### Community 62 - "AppClaimTypes.cs"
Cohesion: 0.33
Nodes (4): DateOnly, TimeProvider, TimeSpan, BusinessClock

### Community 63 - ".ExecuteInTransactionAsync"
Cohesion: 0.50
Nodes (3): CancellationToken, Func, Task

### Community 71 - "IReadOnlyCollection"
Cohesion: 0.20
Nodes (9): IStockLockService, StockKey, CancellationToken, IReadOnlyCollection, Task, StockLockService, CancellationToken, IReadOnlyCollection (+1 more)

## Knowledge Gaps
- **88 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)`, `Microsoft.EntityFrameworkCore.Design (10.0.10)`, `Microsoft.OpenApi (2.11.0)` (+83 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **5 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NsStore.Domain.Enums` connect `NsStore.Infrastructure.Persistence.Migrations` to `UserService`, `NsStore.Api.Middleware`, `AuditableEntity`, `NsStore.Application.Common.Interfaces`, `.UpdateAsync`, `NsStore.Application.Features.Sales`, `IAppDbContext`, `AbstractValidator`, `ClientServiceTests`, `AppDbContext`, `SaleTests`, `NsStore.Domain.Enums`, `AppSetting`?**
  _High betweenness centrality (0.197) - this node is a cross-community bridge._
- **Why does `TestHarness` connect `TestHarness` to `PagedResult`, `.CreateProductAsync`, `.ReadyProductAsync`, `.MapReportEndpoints`, `NsStore.Application.Common.Interfaces`, `AuditableEntity`, `IEndpointRouteBuilder`, `ProductService`, `AbstractValidator`, `ClientServiceTests`, `SaleTests`, `Sale`, `.Apply`, `BranchService`, `NsStore.Application.Features.Sales`, `.StockedInMainAsync`, `ICurrentUser`, `Product`, `AppDbContext`, `.TwoDebtsAsync`, `UniqueClientCi`, `IReadOnlyCollection`?**
  _High betweenness centrality (0.153) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Entities` connect `NsStore.Domain.Enums` to `.SaveChangesAsync`, `UserService`, `NsStore.Infrastructure.Persistence.Migrations`, `AuditableEntity`, `NsStore.Application.Features.Sales`, `Client`, `IEntityTypeConfiguration`, `StockTransfer`, `AppDbContext`, `CatalogEndpoints`?**
  _High betweenness centrality (0.075) - this node is a cross-community bridge._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)` to the rest of the system?**
  _88 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `NsStore.Infrastructure.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `.CreateProductAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.13513513513513514 - nodes in this community are weakly interconnected._
- **Should `.MapReportEndpoints` be split into smaller, more focused modules?**
  _Cohesion score 0.06821480406386067 - nodes in this community are weakly interconnected._