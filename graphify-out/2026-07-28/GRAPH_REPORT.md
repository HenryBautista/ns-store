# Graph Report - ns-store  (2026-07-27)

## Corpus Check
- 101 files · ~24,834 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 778 nodes · 1855 edges · 42 communities (39 shown, 3 thin omitted)
- Extraction: 91% EXTRACTED · 9% INFERRED · 0% AMBIGUOUS · INFERRED: 170 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `1057ef9c`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .SaveChangesAsync
- NsStore.Infrastructure.csproj
- TestHarness
- User
- .MapReportEndpoints
- .UpdateAsync
- .CreateAsync
- .ReadyProductAsync
- NsStore.Domain.Enums
- IAppDbContext
- Order
- .CreateAsync
- Quote
- ProductService
- IEntityTypeConfiguration
- PagedResult
- docker-compose 'api' service (src/NsStore.Api/Dockerfile)
- Sale
- Client
- AuthPolicies.cs
- QuoteService.cs
- http
- Product
- SaleTests
- Purchase
- AppExceptionHandler
- AppDbContext
- Exceptions.cs
- AbstractValidator
- .SavingChangesAsync
- .InvokeAsync
- CLAUDE.md
- .Apply
- DatabaseInitializer
- AppSetting
- InitialSchema
- .AddInfrastructure
- AppDbContextModelSnapshot
- Atomic sales transaction
- Dual price business rule
- Price suggestion formula
- Soft delete + audit columns

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Enums` - 38 edges
2. `NsStore.Domain.Entities` - 35 edges
3. `AppDbContext` - 27 edges
4. `NsStore.Domain.Common` - 25 edges
5. `NsStore.Application.Common.Interfaces` - 24 edges
6. `IAppDbContext` - 23 edges
7. `PagedResult` - 19 edges
8. `TestHarness` - 19 edges
9. `NsStore.Application.Common.Models` - 16 edges
10. `PageRequest` - 16 edges

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

## Communities (42 total, 3 thin omitted)

### Community 0 - ".SaveChangesAsync"
Cohesion: 0.14
Nodes (16): IEndpointRouteBuilder, CatalogEndpoints, CatalogMapping, CategoryDto, DescriptionRequest, NameRequest, SupplierDto, SupplierRequest (+8 more)

### Community 1 - "NsStore.Infrastructure.csproj"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - "TestHarness"
Cohesion: 0.08
Nodes (25): IDisposable, SqliteConnection, CancellationToken, IReadOnlyCollection, Task, IStockLockService, CancellationToken, IReadOnlyCollection (+17 more)

### Community 3 - "User"
Cohesion: 0.08
Nodes (27): ClaimsPrincipal, HttpContext, IEndpointRouteBuilder, AuthEndpoints, CurrentUser, AccessToken, ICurrentUser, IPasswordHasher (+19 more)

### Community 4 - ".MapReportEndpoints"
Cohesion: 0.11
Nodes (21): IReadOnlyDictionary, IEndpointRouteBuilder, ReportEndpoints, SettingsEndpoints, DashboardDto, DebtsReportDto, PriceListReportDto, PurchasesReportDto (+13 more)

### Community 5 - ".UpdateAsync"
Cohesion: 0.19
Nodes (13): IEndpointRouteBuilder, UserEndpoints, CreateUserRequest, UpdateUserRequest, UpdateUserRoleRequest, UpdateUserStatusRequest, UserDto, UserMapping (+5 more)

### Community 6 - ".CreateAsync"
Cohesion: 0.12
Nodes (21): IReadOnlyList, IEndpointRouteBuilder, SaleEndpoints, CancellationToken, Func, Task, CreateSaleRequest, PaymentDto (+13 more)

### Community 7 - ".ReadyProductAsync"
Cohesion: 0.47
Nodes (3): Fact, Task, SaleServiceTests

### Community 8 - "NsStore.Domain.Enums"
Cohesion: 0.06
Nodes (41): NsStore.Application.Common.Interfaces, NsStore.Api.Endpoints, NsStore.Application.Common.Models, NsStore.Application.Tests, NsStore.Application.Features.Inventory, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Domain.Tests (+33 more)

### Community 9 - "IAppDbContext"
Cohesion: 0.18
Nodes (13): DbSet, IAppDbContext, DateTimeOffset, AuditableEntity, Category, Supplier, Trademark, WarrantyTerm (+5 more)

### Community 10 - "Order"
Cohesion: 0.27
Nodes (9): OrderEndpoints, OrderDto, OrderQuery, OrderRequest, CancellationToken, Expression, Task, OrderService (+1 more)

### Community 11 - ".CreateAsync"
Cohesion: 0.20
Nodes (11): PurchaseEndpoints, CreatePurchaseRequest, PurchaseDto, PurchaseItemDto, PurchaseItemRequest, PurchaseListItemDto, PurchaseQuery, CancellationToken (+3 more)

### Community 12 - "Quote"
Cohesion: 0.22
Nodes (8): DateOnly, Quote, SaleItem, EntityTypeBuilder, ClientConfiguration, OrderConfiguration, QuoteConfiguration, SaleItemConfiguration

### Community 13 - "ProductService"
Cohesion: 0.31
Nodes (9): ProductEndpoints, PriceSuggestionDto, ProductDto, ProductRequest, SetPricesRequest, CancellationToken, Expression, Task (+1 more)

### Community 14 - "IEntityTypeConfiguration"
Cohesion: 0.23
Nodes (7): IEntityTypeConfiguration, DateTimeOffset, Guid, RefreshToken, EntityTypeBuilder, RefreshTokenConfiguration, UserConfiguration

### Community 15 - "PagedResult"
Cohesion: 0.13
Nodes (19): IEndpointRouteBuilder, InventoryEndpoints, CancellationToken, int, IQueryable, Task, PagedResult, PageRequest (+11 more)

### Community 16 - "docker-compose 'api' service (src/NsStore.Api/Dockerfile)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "Sale"
Cohesion: 0.33
Nodes (7): DateOnly, DateTimeOffset, List, Payment, Sale, PaymentConfiguration, SaleConfiguration

### Community 18 - "Client"
Cohesion: 0.31
Nodes (8): IEndpointRouteBuilder, ClientEndpoints, ClientDto, ClientRequest, CancellationToken, Task, ClientService, Client

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
Cohesion: 0.23
Nodes (9): NsStore.Infrastructure.Persistence.Configurations, DateTimeOffset, InventoryMovement, StockLevel, Product, EntityTypeBuilder, InventoryMovementConfiguration, ProductConfiguration (+1 more)

### Community 23 - "SaleTests"
Cohesion: 0.22
Nodes (7): InlineData, DateOnly, DateTimeOffset, Fact, OrderTests, SaleTests, Theory

### Community 24 - "Purchase"
Cohesion: 0.15
Nodes (12): DateOnly, List, Purchase, PurchaseItem, ClientType, InvoiceType, MovementType, OrderStatus (+4 more)

### Community 25 - "AppExceptionHandler"
Cohesion: 0.19
Nodes (9): Detail, ErrorCode, IExceptionHandler, CancellationToken, Exception, HttpContext, ValueTask, AppExceptionHandler (+1 more)

### Community 26 - "AppDbContext"
Cohesion: 0.18
Nodes (9): DbContext, IDesignTimeDbContextFactory, CancellationToken, DbSet, Func, ModelBuilder, Task, AppDbContext (+1 more)

### Community 27 - "Exceptions.cs"
Cohesion: 0.26
Nodes (10): Exception, IDictionary, AppException, BadRequestException, ConflictException, ForbiddenException, NotFoundException, UnauthorizedException (+2 more)

### Community 28 - "AbstractValidator"
Cohesion: 0.15
Nodes (11): AbstractValidator, LoginRequest, LoginRequestValidator, DescriptionRequestValidator, NameRequestValidator, SupplierRequestValidator, ClientRequestValidator, OrderRequestValidator (+3 more)

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
Cohesion: 0.67
Nodes (3): CancellationToken, Task, DatabaseInitializer

### Community 34 - "AppSetting"
Cohesion: 0.33
Nodes (5): DateTimeOffset, string, AppSetting, AppSettingKeys, AppSettingConfiguration

### Community 35 - "InitialSchema"
Cohesion: 0.50
Nodes (3): Migration, MigrationBuilder, InitialSchema

### Community 37 - ".AddInfrastructure"
Cohesion: 0.50
Nodes (3): IConfiguration, IServiceCollection, DependencyInjection

### Community 38 - "AppDbContextModelSnapshot"
Cohesion: 0.50
Nodes (3): ModelSnapshot, ModelBuilder, AppDbContextModelSnapshot

### Community 39 - "Atomic sales transaction"
Cohesion: 0.67
Nodes (3): Atomic sales transaction, Credit sales / installment payments, Stock movement ledger (inventory_movements + stock_levels cache)

## Knowledge Gaps
- **75 isolated node(s):** `graphify`, `What this is`, `Commands`, `Cross-cutting mechanics worth knowing before editing`, `Inventory and sales invariants` (+70 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NsStore.Domain.Enums` connect `NsStore.Domain.Enums` to `User`, `.UpdateAsync`, `.CreateAsync`, `Order`, `.CreateAsync`, `PagedResult`, `Client`, `Purchase`?**
  _High betweenness centrality (0.125) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Entities` connect `NsStore.Domain.Enums` to `.SaveChangesAsync`, `AppSetting`, `User`, `.MapReportEndpoints`, `.UpdateAsync`, `Quote`, `IEntityTypeConfiguration`, `QuoteService.cs`, `Product`?**
  _High betweenness centrality (0.084) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Common` connect `NsStore.Domain.Enums` to `IAppDbContext`, `Exceptions.cs`, `QuoteService.cs`?**
  _High betweenness centrality (0.069) - this node is a cross-community bridge._
- **What connects `graphify`, `What this is`, `Commands` to the rest of the system?**
  _75 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.SaveChangesAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.14102564102564102 - nodes in this community are weakly interconnected._
- **Should `NsStore.Infrastructure.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `TestHarness` be split into smaller, more focused modules?**
  _Cohesion score 0.08181818181818182 - nodes in this community are weakly interconnected._