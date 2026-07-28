# Graph Report - ns-store  (2026-07-28)

## Corpus Check
- 130 files · ~43,096 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 999 nodes · 2461 edges · 55 communities (47 shown, 8 thin omitted)
- Extraction: 93% EXTRACTED · 7% INFERRED · 0% AMBIGUOUS · INFERRED: 182 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5b7d39e6`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .SaveChangesAsync
- NsStore.Infrastructure.csproj
- .CreateProductAsync
- Ports.cs
- .MapReportEndpoints
- AbstractValidator
- Purchase
- .ReadyProductAsync
- NsStore.Application.Common.Interfaces
- AuditableEntity
- Order
- .CreateAsync
- IEntityTypeConfiguration
- ProductService
- User
- IAppDbContext
- docker-compose 'api' service (src/NsStore.Api/Dockerfile)
- Branch
- ClientServiceTests
- AuthPolicies.cs
- QuoteService.cs
- http
- Product
- SaleTests
- NsStore.Domain.Enums
- AppExceptionHandler
- AppDbContext
- Exceptions.cs
- .CreateAsync
- .SavingChangesAsync
- .InvokeAsync
- CLAUDE.md
- .Apply
- DatabaseInitializer
- BranchService
- Migration
- CatalogEndpoints
- NsStore.Infrastructure.Persistence.Migrations
- NsStore.Application.Features.Users
- Atomic sales transaction
- NsStore.Application.Common.Models
- NsStore.Application.Features.Inventory
- Dual price business rule
- Price suggestion formula
- Soft delete + audit columns
- TestHarness
- DocumentNumberingTests
- .AddInfrastructure
- AppDbContextModelSnapshot
- ErrorCodes.cs
- PagedResult
- AppClaimTypes.cs
- ValidationFilter.cs
- AppSetting
- DependencyInjection

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Enums` - 48 edges
2. `NsStore.Domain.Entities` - 42 edges
3. `NsStore.Domain.Common` - 40 edges
4. `TestHarness` - 33 edges
5. `AppDbContext` - 30 edges
6. `NsStore.Application.Common.Interfaces` - 29 edges
7. `IAppDbContext` - 26 edges
8. `NsStore.Application.Common` - 24 edges
9. `NsStore.Application.Common.Models` - 22 edges
10. `PagedResult` - 22 edges

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

## Communities (55 total, 8 thin omitted)

### Community 0 - ".SaveChangesAsync"
Cohesion: 0.12
Nodes (21): NsStore.Application.Features.Catalogs, CancellationToken, int, IQueryable, Task, PageRequest, QueryableExtensions, CatalogMapping (+13 more)

### Community 1 - "NsStore.Infrastructure.csproj"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - ".CreateProductAsync"
Cohesion: 0.13
Nodes (12): Fact, Task, BranchServiceTests, Fact, Task, InventoryReportingTests, Fact, Task (+4 more)

### Community 3 - "Ports.cs"
Cohesion: 0.06
Nodes (28): ClaimsPrincipal, HttpContext, IEndpointRouteBuilder, AuthEndpoints, CurrentUser, BranchScope, AccessToken, ICurrentUser (+20 more)

### Community 4 - ".MapReportEndpoints"
Cohesion: 0.10
Nodes (22): IReadOnlyDictionary, IEndpointRouteBuilder, ReportEndpoints, SettingsEndpoints, DashboardDto, DebtsReportDto, PriceListReportDto, PurchasesReportDto (+14 more)

### Community 5 - "AbstractValidator"
Cohesion: 0.14
Nodes (20): AbstractValidator, IEndpointRouteBuilder, UserEndpoints, DescriptionRequestValidator, NameRequestValidator, SupplierRequestValidator, ClientRequestValidator, CreateUserRequest (+12 more)

### Community 6 - "Purchase"
Cohesion: 0.22
Nodes (8): DateOnly, List, Purchase, ClientType, MovementType, OrderStatus, PaymentStatus, PurchaseConfiguration

### Community 7 - ".ReadyProductAsync"
Cohesion: 0.55
Nodes (3): Fact, Task, BranchScopingTests

### Community 8 - "NsStore.Application.Common.Interfaces"
Cohesion: 0.18
Nodes (6): NsStore.Application.Common.Interfaces, NsStore.Application, NsStore.Infrastructure, NsStore.Infrastructure.Persistence, NsStore.Infrastructure.Security, Program

### Community 9 - "AuditableEntity"
Cohesion: 0.18
Nodes (12): NsStore.Infrastructure.Persistence.Configurations, DateTimeOffset, AuditableEntity, Category, Supplier, Trademark, WarrantyTerm, EntityTypeBuilder (+4 more)

### Community 10 - "Order"
Cohesion: 0.24
Nodes (10): OrderEndpoints, OrderDto, OrderQuery, OrderRequest, CancellationToken, Expression, Task, OrderService (+2 more)

### Community 11 - ".CreateAsync"
Cohesion: 0.07
Nodes (30): IEndpointRouteBuilder, PurchaseEndpoints, CancellationToken, IReadOnlyCollection, Task, DocumentKind, IDocumentNumberService, IStockLockService (+22 more)

### Community 12 - "IEntityTypeConfiguration"
Cohesion: 0.18
Nodes (12): IEntityTypeConfiguration, DateOnly, Quote, PurchaseItem, SaleItem, EntityTypeBuilder, ClientConfiguration, OrderConfiguration (+4 more)

### Community 13 - "ProductService"
Cohesion: 0.24
Nodes (11): ProductEndpoints, PriceSuggestionDto, ProductDto, ProductRequest, SetPricesRequest, CancellationToken, Expression, Task (+3 more)

### Community 14 - "User"
Cohesion: 0.22
Nodes (8): DateTimeOffset, Guid, RefreshToken, List, User, EntityTypeBuilder, RefreshTokenConfiguration, UserConfiguration

### Community 15 - "IAppDbContext"
Cohesion: 0.24
Nodes (9): DbSet, IAppDbContext, DateOnly, List, StockTransfer, StockTransferItem, EntityTypeBuilder, StockTransferConfiguration (+1 more)

### Community 16 - "docker-compose 'api' service (src/NsStore.Api/Dockerfile)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "Branch"
Cohesion: 0.22
Nodes (9): Branch, DateOnly, DateTimeOffset, List, Payment, Sale, EntityTypeBuilder, BranchConfiguration (+1 more)

### Community 18 - "ClientServiceTests"
Cohesion: 0.17
Nodes (11): IEndpointRouteBuilder, ClientEndpoints, ClientDto, ClientRequest, CancellationToken, Task, ClientService, Client (+3 more)

### Community 19 - "AuthPolicies.cs"
Cohesion: 0.67
Nodes (3): string, AuthCookies, AuthPolicies

### Community 20 - "QuoteService.cs"
Cohesion: 0.27
Nodes (10): IEndpointRouteBuilder, QuoteEndpoints, CancellationToken, Expression, Task, QuoteDto, QuoteQuery, QuoteRequest (+2 more)

### Community 21 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 22 - "Product"
Cohesion: 0.19
Nodes (11): DateTimeOffset, IHasCreationAudit, DateTimeOffset, InventoryMovement, StockLevel, List, Product, EntityTypeBuilder (+3 more)

### Community 23 - "SaleTests"
Cohesion: 0.14
Nodes (11): NsStore.Domain.Tests, InlineData, InvoiceType, DateOnly, DateTimeOffset, Fact, long, OrderTests (+3 more)

### Community 24 - "NsStore.Domain.Enums"
Cohesion: 0.17
Nodes (6): NsStore.Application.Common, NsStore.Application.Features.Clients, NsStore.Domain.Enums, NsStore.Application.Features.Branches, NsStore.Domain.Common, NsStore.Domain.Entities

### Community 25 - "AppExceptionHandler"
Cohesion: 0.19
Nodes (9): Detail, ErrorCode, IExceptionHandler, CancellationToken, Exception, HttpContext, ValueTask, AppExceptionHandler (+1 more)

### Community 26 - "AppDbContext"
Cohesion: 0.18
Nodes (9): DbContext, IDesignTimeDbContextFactory, CancellationToken, DbSet, Func, ModelBuilder, Task, AppDbContext (+1 more)

### Community 27 - "Exceptions.cs"
Cohesion: 0.26
Nodes (10): Exception, IDictionary, AppException, BadRequestException, ConflictException, ForbiddenException, NotFoundException, UnauthorizedException (+2 more)

### Community 28 - ".CreateAsync"
Cohesion: 0.12
Nodes (21): SaleEndpoints, CancellationToken, Func, Task, CreateSaleRequest, PaymentDto, RegisterPaymentRequest, SaleDto (+13 more)

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
Cohesion: 0.48
Nodes (3): DateTimeOffset, Fact, StockLevelTests

### Community 33 - "DatabaseInitializer"
Cohesion: 0.53
Nodes (4): CancellationToken, string, Task, DatabaseInitializer

### Community 34 - "BranchService"
Cohesion: 0.23
Nodes (10): IEndpointRouteBuilder, BranchEndpoints, BranchDto, BranchRequest, UpdateBranchStatusRequest, CancellationToken, Expression, Task (+2 more)

### Community 35 - "Migration"
Cohesion: 0.09
Nodes (13): Migration, MigrationBuilder, InitialSchema, MigrationBuilder, UniqueClientCi, MigrationBuilder, string, AddBranches (+5 more)

### Community 37 - "NsStore.Infrastructure.Persistence.Migrations"
Cohesion: 0.09
Nodes (11): NsStore.Infrastructure.Persistence.Migrations, ModelBuilder, InitialSchema, ModelBuilder, UniqueClientCi, ModelBuilder, AddBranches, ModelBuilder (+3 more)

### Community 38 - "NsStore.Application.Features.Users"
Cohesion: 0.50
Nodes (3): NsStore.Application.Features.Users, NsStore.Application.Features.Auth, LoginResponse

### Community 39 - "Atomic sales transaction"
Cohesion: 0.67
Nodes (3): Atomic sales transaction, Credit sales / installment payments, Stock movement ledger (inventory_movements + stock_levels cache)

### Community 40 - "NsStore.Application.Common.Models"
Cohesion: 0.25
Nodes (8): NsStore.Api.Endpoints, NsStore.Application.Common.Models, NsStore.Api.Security, NsStore.Application.Features.Quotes, NsStore.Application.Features.Orders, NsStore.Api.Middleware, string, RateLimitPolicies

### Community 41 - "NsStore.Application.Features.Inventory"
Cohesion: 0.29
Nodes (8): NsStore.Application.Tests, NsStore.Application.Features.Inventory, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Application.Features.Sales, NsStore.Application.Features.Purchases, NsStore.Application.Features.Settings, PriceListRowDto

### Community 45 - "TestHarness"
Cohesion: 0.15
Nodes (14): IDisposable, SqliteConnection, Fact, Task, CrossBranchAvailabilityTests, DateOnly, DateTimeOffset, long (+6 more)

### Community 46 - "DocumentNumberingTests"
Cohesion: 0.58
Nodes (3): Fact, Task, DocumentNumberingTests

### Community 48 - ".AddInfrastructure"
Cohesion: 0.50
Nodes (3): IConfiguration, IServiceCollection, DependencyInjection

### Community 49 - "AppDbContextModelSnapshot"
Cohesion: 0.50
Nodes (3): ModelSnapshot, ModelBuilder, AppDbContextModelSnapshot

### Community 51 - "PagedResult"
Cohesion: 0.09
Nodes (27): IEndpointRouteBuilder, InventoryEndpoints, PagedResult, BranchAvailabilityDto, InventoryMovementDto, KardexQuery, KardexRowDto, StockAdjustmentRequest (+19 more)

### Community 56 - "AppSetting"
Cohesion: 0.33
Nodes (5): DateTimeOffset, string, AppSetting, AppSettingKeys, AppSettingConfiguration

## Knowledge Gaps
- **78 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)`, `Microsoft.EntityFrameworkCore.Design (10.0.10)`, `Microsoft.OpenApi (2.11.0)` (+73 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **8 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NsStore.Domain.Enums` connect `NsStore.Domain.Enums` to `Ports.cs`, `AbstractValidator`, `Purchase`, `NsStore.Infrastructure.Persistence.Migrations`, `NsStore.Application.Common.Models`, `NsStore.Application.Features.Inventory`, `Order`, `.CreateAsync`, `NsStore.Application.Common.Interfaces`, `ClientServiceTests`, `PagedResult`, `SaleTests`, `.CreateAsync`?**
  _High betweenness centrality (0.164) - this node is a cross-community bridge._
- **Why does `TestHarness` connect `TestHarness` to `.CreateProductAsync`, `BranchService`, `.MapReportEndpoints`, `Ports.cs`, `.ReadyProductAsync`, `NsStore.Application.Features.Inventory`, `.CreateAsync`, `ProductService`, `DocumentNumberingTests`, `ClientServiceTests`, `PagedResult`, `AppDbContext`, `.CreateAsync`?**
  _High betweenness centrality (0.118) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Common` connect `NsStore.Domain.Enums` to `.SaveChangesAsync`, `NsStore.Application.Features.Users`, `NsStore.Application.Common.Interfaces`, `AuditableEntity`, `NsStore.Application.Features.Inventory`, `ErrorCodes.cs`, `QuoteService.cs`, `AppClaimTypes.cs`, `Product`, `SaleTests`, `Exceptions.cs`?**
  _High betweenness centrality (0.084) - this node is a cross-community bridge._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)` to the rest of the system?**
  _78 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.SaveChangesAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.11515151515151516 - nodes in this community are weakly interconnected._
- **Should `NsStore.Infrastructure.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `.CreateProductAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.1273532668881506 - nodes in this community are weakly interconnected._