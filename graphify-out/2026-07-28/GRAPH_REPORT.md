# Graph Report - ns-store  (2026-07-28)

## Corpus Check
- 118 files · ~35,299 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 915 nodes · 2232 edges · 55 communities (47 shown, 8 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 168 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `11d30429`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .SaveChangesAsync
- NsStore.Infrastructure.csproj
- .CreateProductAsync
- .IssueTokensAsync
- ReportDtos.cs
- AbstractValidator
- Purchase
- TestHarness
- NsStore.Application.Common.Interfaces
- IAppDbContext
- Order
- .CreateAsync
- TransactionConfigurations.cs
- ProductService
- IEntityTypeConfiguration
- ClientServiceTests
- docker-compose 'api' service (src/NsStore.Api/Dockerfile)
- Sale
- ClientService
- AuthPolicies.cs
- .UpdateAsync
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
- Branch
- AddBranches
- CatalogEndpoints
- NsStore.Infrastructure.Persistence.Migrations
- NsStore.Application.Features.Users
- Atomic sales transaction
- NsStore.Api.Middleware
- TestHarness.cs
- Dual price business rule
- Price suggestion formula
- Soft delete + audit columns
- User
- .ReadyProductAsync
- AppSetting
- .AddInfrastructure
- AppDbContextModelSnapshot
- ErrorCodes.cs
- PagedResult
- AppClaimTypes.cs
- DependencyInjection
- ValidationFilter.cs

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Enums` - 43 edges
2. `NsStore.Domain.Entities` - 38 edges
3. `NsStore.Domain.Common` - 36 edges
4. `AppDbContext` - 28 edges
5. `NsStore.Application.Common.Interfaces` - 26 edges
6. `TestHarness` - 26 edges
7. `IAppDbContext` - 24 edges
8. `NsStore.Application.Common` - 22 edges
9. `PagedResult` - 21 edges
10. `NsStore.Application.Common.Models` - 20 edges

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
Cohesion: 0.15
Nodes (16): int, PageRequest, CatalogMapping, CategoryDto, DescriptionRequest, NameRequest, SupplierDto, SupplierRequest (+8 more)

### Community 1 - "NsStore.Infrastructure.csproj"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - ".CreateProductAsync"
Cohesion: 0.17
Nodes (9): Fact, Task, BranchServiceTests, Fact, Task, InventoryReportingTests, Fact, Task (+1 more)

### Community 3 - ".IssueTokensAsync"
Cohesion: 0.07
Nodes (26): ClaimsPrincipal, HttpContext, IEndpointRouteBuilder, AuthEndpoints, CurrentUser, BranchScope, AccessToken, ICurrentUser (+18 more)

### Community 4 - "ReportDtos.cs"
Cohesion: 0.10
Nodes (23): IReadOnlyDictionary, IEndpointRouteBuilder, ReportEndpoints, SettingsEndpoints, DashboardDto, DebtsReportDto, PriceListReportDto, PriceListRowDto (+15 more)

### Community 5 - "AbstractValidator"
Cohesion: 0.09
Nodes (28): AbstractValidator, IEndpointRouteBuilder, UserEndpoints, LoginRequest, LoginRequestValidator, BranchRequestValidator, DescriptionRequestValidator, NameRequestValidator (+20 more)

### Community 6 - "Purchase"
Cohesion: 0.20
Nodes (9): DateOnly, List, Purchase, ClientType, InvoiceType, MovementType, OrderStatus, PaymentStatus (+1 more)

### Community 7 - "TestHarness"
Cohesion: 0.14
Nodes (14): IDisposable, SqliteConnection, Fact, Task, BranchScopingTests, Fact, Task, CrossBranchAvailabilityTests (+6 more)

### Community 8 - "NsStore.Application.Common.Interfaces"
Cohesion: 0.20
Nodes (6): NsStore.Application.Common.Interfaces, NsStore.Application, NsStore.Infrastructure, NsStore.Infrastructure.Persistence, NsStore.Infrastructure.Security, Program

### Community 9 - "IAppDbContext"
Cohesion: 0.18
Nodes (12): NsStore.Infrastructure.Persistence.Configurations, DbSet, IAppDbContext, Category, Supplier, Trademark, WarrantyTerm, EntityTypeBuilder (+4 more)

### Community 10 - "Order"
Cohesion: 0.28
Nodes (9): OrderEndpoints, OrderDto, OrderQuery, OrderRequest, CancellationToken, Expression, Task, OrderService (+1 more)

### Community 11 - ".CreateAsync"
Cohesion: 0.08
Nodes (25): IEndpointRouteBuilder, PurchaseEndpoints, CancellationToken, IReadOnlyCollection, Task, IStockLockService, StockKey, CreatePurchaseRequest (+17 more)

### Community 12 - "TransactionConfigurations.cs"
Cohesion: 0.24
Nodes (7): PurchaseItem, SaleItem, EntityTypeBuilder, ClientConfiguration, OrderConfiguration, PurchaseItemConfiguration, SaleItemConfiguration

### Community 13 - "ProductService"
Cohesion: 0.27
Nodes (10): IEndpointRouteBuilder, ProductEndpoints, PriceSuggestionDto, ProductDto, ProductRequest, SetPricesRequest, CancellationToken, Expression (+2 more)

### Community 14 - "IEntityTypeConfiguration"
Cohesion: 0.23
Nodes (7): IEntityTypeConfiguration, DateTimeOffset, Guid, RefreshToken, EntityTypeBuilder, RefreshTokenConfiguration, UserConfiguration

### Community 15 - "ClientServiceTests"
Cohesion: 0.38
Nodes (3): Fact, Task, ClientServiceTests

### Community 16 - "docker-compose 'api' service (src/NsStore.Api/Dockerfile)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "Sale"
Cohesion: 0.33
Nodes (7): DateOnly, DateTimeOffset, List, Payment, Sale, PaymentConfiguration, SaleConfiguration

### Community 18 - "ClientService"
Cohesion: 0.32
Nodes (8): IEndpointRouteBuilder, ClientEndpoints, ClientDto, ClientRequest, CancellationToken, Task, ClientService, Client

### Community 19 - "AuthPolicies.cs"
Cohesion: 0.67
Nodes (3): string, AuthCookies, AuthPolicies

### Community 20 - ".UpdateAsync"
Cohesion: 0.30
Nodes (9): IEndpointRouteBuilder, QuoteEndpoints, CancellationToken, Expression, Task, QuoteDto, QuoteQuery, QuoteRequest (+1 more)

### Community 21 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 22 - "Product"
Cohesion: 0.26
Nodes (9): DateTimeOffset, InventoryMovement, StockLevel, List, Product, EntityTypeBuilder, InventoryMovementConfiguration, ProductConfiguration (+1 more)

### Community 23 - "SaleTests"
Cohesion: 0.16
Nodes (9): InlineData, DateOnly, DateTimeOffset, Fact, long, OrderTests, ProductTests, SaleTests (+1 more)

### Community 24 - "NsStore.Domain.Enums"
Cohesion: 0.19
Nodes (6): NsStore.Application.Common.Models, NsStore.Domain.Tests, NsStore.Application.Common, NsStore.Domain.Enums, NsStore.Domain.Common, NsStore.Domain.Entities

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
Cohesion: 0.13
Nodes (20): SaleEndpoints, CancellationToken, Func, Task, CreateSaleRequest, PaymentDto, RegisterPaymentRequest, SaleDto (+12 more)

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

### Community 34 - "Branch"
Cohesion: 0.18
Nodes (12): IEndpointRouteBuilder, BranchEndpoints, BranchDto, BranchRequest, UpdateBranchStatusRequest, CancellationToken, Expression, Task (+4 more)

### Community 35 - "AddBranches"
Cohesion: 0.15
Nodes (8): Migration, MigrationBuilder, InitialSchema, MigrationBuilder, UniqueClientCi, MigrationBuilder, string, AddBranches

### Community 37 - "NsStore.Infrastructure.Persistence.Migrations"
Cohesion: 0.13
Nodes (7): NsStore.Infrastructure.Persistence.Migrations, ModelBuilder, InitialSchema, ModelBuilder, UniqueClientCi, ModelBuilder, AddBranches

### Community 38 - "NsStore.Application.Features.Users"
Cohesion: 0.50
Nodes (3): NsStore.Application.Features.Users, NsStore.Application.Features.Auth, LoginResponse

### Community 39 - "Atomic sales transaction"
Cohesion: 0.67
Nodes (3): Atomic sales transaction, Credit sales / installment payments, Stock movement ledger (inventory_movements + stock_levels cache)

### Community 40 - "NsStore.Api.Middleware"
Cohesion: 0.23
Nodes (8): NsStore.Api.Endpoints, NsStore.Application.Features.Catalogs, NsStore.Api.Security, NsStore.Application.Features.Quotes, NsStore.Application.Features.Orders, NsStore.Api.Middleware, string, RateLimitPolicies

### Community 41 - "TestHarness.cs"
Cohesion: 0.26
Nodes (9): NsStore.Application.Tests, NsStore.Application.Features.Inventory, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Application.Features.Sales, NsStore.Application.Features.Clients, NsStore.Application.Features.Purchases, NsStore.Application.Features.Branches (+1 more)

### Community 45 - "User"
Cohesion: 0.20
Nodes (9): DateTimeOffset, AuditableEntity, DateTimeOffset, IHasCreationAudit, DateOnly, Quote, List, User (+1 more)

### Community 46 - ".ReadyProductAsync"
Cohesion: 0.47
Nodes (3): Fact, Task, SaleServiceTests

### Community 47 - "AppSetting"
Cohesion: 0.33
Nodes (5): DateTimeOffset, string, AppSetting, AppSettingKeys, AppSettingConfiguration

### Community 48 - ".AddInfrastructure"
Cohesion: 0.50
Nodes (3): IConfiguration, IServiceCollection, DependencyInjection

### Community 49 - "AppDbContextModelSnapshot"
Cohesion: 0.50
Nodes (3): ModelSnapshot, ModelBuilder, AppDbContextModelSnapshot

### Community 51 - "PagedResult"
Cohesion: 0.12
Nodes (19): InventoryEndpoints, CancellationToken, IQueryable, Task, PagedResult, QueryableExtensions, BranchAvailabilityDto, InventoryMovementDto (+11 more)

## Knowledge Gaps
- **76 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)`, `Microsoft.EntityFrameworkCore.Design (10.0.10)`, `Microsoft.OpenApi (2.11.0)` (+71 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **8 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NsStore.Domain.Enums` connect `NsStore.Domain.Enums` to `.IssueTokensAsync`, `ReportDtos.cs`, `AbstractValidator`, `Purchase`, `NsStore.Infrastructure.Persistence.Migrations`, `NsStore.Api.Middleware`, `TestHarness.cs`, `Order`, `.CreateAsync`, `NsStore.Application.Common.Interfaces`, `ClientService`, `PagedResult`, `.CreateAsync`?**
  _High betweenness centrality (0.147) - this node is a cross-community bridge._
- **Why does `TestHarness` connect `TestHarness` to `.CreateProductAsync`, `Branch`, `ReportDtos.cs`, `.IssueTokensAsync`, `TestHarness.cs`, `.CreateAsync`, `ProductService`, `.ReadyProductAsync`, `ClientServiceTests`, `ClientService`, `PagedResult`, `AppDbContext`, `.CreateAsync`?**
  _High betweenness centrality (0.100) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Common` connect `NsStore.Domain.Enums` to `NsStore.Application.Features.Users`, `NsStore.Application.Common.Interfaces`, `TestHarness.cs`, `User`, `ErrorCodes.cs`, `AppClaimTypes.cs`, `Exceptions.cs`?**
  _High betweenness centrality (0.085) - this node is a cross-community bridge._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)` to the rest of the system?**
  _76 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.SaveChangesAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.1461609620721554 - nodes in this community are weakly interconnected._
- **Should `NsStore.Infrastructure.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `.IssueTokensAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.07020408163265306 - nodes in this community are weakly interconnected._