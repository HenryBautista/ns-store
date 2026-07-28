# Graph Report - ns-store  (2026-07-28)

## Corpus Check
- 105 files · ~27,013 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 809 nodes · 1920 edges · 42 communities (38 shown, 4 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 146 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `f7ff81ae`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .SaveChangesAsync
- NsStore.Infrastructure.csproj
- TestHarness
- .IssueTokensAsync
- ReportDtos.cs
- .UpdateAsync
- Purchase
- AbstractValidator
- NsStore.Domain.Enums
- Category
- .UpdateAsync
- .CreateAsync
- Sale
- ProductService
- IEntityTypeConfiguration
- ClientServiceTests
- docker-compose 'api' service (src/NsStore.Api/Dockerfile)
- IAppDbContext
- ClientService
- AuthPolicies.cs
- QuoteService.cs
- http
- Product
- SaleTests
- AuditableEntity
- AppExceptionHandler
- AppDbContext
- Exceptions.cs
- PagedResult
- .SavingChangesAsync
- .InvokeAsync
- CLAUDE.md
- .Apply
- DatabaseInitializer
- InitialSchema
- CatalogEndpoints
- .AddInfrastructure
- AppDbContextModelSnapshot
- Atomic sales transaction
- Dual price business rule
- Price suggestion formula
- Soft delete + audit columns

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Enums` - 40 edges
2. `NsStore.Domain.Entities` - 35 edges
3. `NsStore.Domain.Common` - 28 edges
4. `AppDbContext` - 27 edges
5. `NsStore.Application.Common.Interfaces` - 24 edges
6. `IAppDbContext` - 23 edges
7. `PagedResult` - 20 edges
8. `TestHarness` - 20 edges
9. `NsStore.Application.Common.Models` - 17 edges
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

## Communities (42 total, 4 thin omitted)

### Community 0 - ".SaveChangesAsync"
Cohesion: 0.15
Nodes (14): CatalogMapping, CategoryDto, DescriptionRequest, NameRequest, SupplierDto, SupplierRequest, TrademarkDto, WarrantyTermDto (+6 more)

### Community 1 - "NsStore.Infrastructure.csproj"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - "TestHarness"
Cohesion: 0.12
Nodes (16): IDisposable, SqliteConnection, Fact, Task, InventoryReportingTests, Fact, Task, PurchaseAndPricingTests (+8 more)

### Community 3 - ".IssueTokensAsync"
Cohesion: 0.08
Nodes (25): ClaimsPrincipal, HttpContext, IEndpointRouteBuilder, AuthEndpoints, CurrentUser, AccessToken, ICurrentUser, IPasswordHasher (+17 more)

### Community 4 - "ReportDtos.cs"
Cohesion: 0.11
Nodes (22): IReadOnlyDictionary, IEndpointRouteBuilder, ReportEndpoints, SettingsEndpoints, DashboardDto, DebtsReportDto, PriceListReportDto, PriceListRowDto (+14 more)

### Community 5 - ".UpdateAsync"
Cohesion: 0.19
Nodes (13): IEndpointRouteBuilder, UserEndpoints, CreateUserRequest, UpdateUserRequest, UpdateUserRoleRequest, UpdateUserStatusRequest, UserDto, UserMapping (+5 more)

### Community 6 - "Purchase"
Cohesion: 0.15
Nodes (12): Supplier, DateOnly, List, Purchase, ClientType, InvoiceType, MovementType, OrderStatus (+4 more)

### Community 7 - "AbstractValidator"
Cohesion: 0.15
Nodes (11): AbstractValidator, LoginRequest, LoginRequestValidator, DescriptionRequestValidator, NameRequestValidator, SupplierRequestValidator, ClientRequestValidator, OrderRequestValidator (+3 more)

### Community 8 - "NsStore.Domain.Enums"
Cohesion: 0.05
Nodes (42): NsStore.Application.Common.Interfaces, NsStore.Api.Endpoints, NsStore.Application.Common.Models, NsStore.Application.Tests, NsStore.Application.Features.Inventory, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Domain.Tests (+34 more)

### Community 9 - "Category"
Cohesion: 0.17
Nodes (11): NsStore.Infrastructure.Persistence.Configurations, DateTimeOffset, string, AppSetting, AppSettingKeys, Category, WarrantyTerm, EntityTypeBuilder (+3 more)

### Community 10 - ".UpdateAsync"
Cohesion: 0.28
Nodes (9): IEndpointRouteBuilder, OrderEndpoints, OrderDto, OrderQuery, OrderRequest, CancellationToken, Expression, Task (+1 more)

### Community 11 - ".CreateAsync"
Cohesion: 0.08
Nodes (23): PurchaseEndpoints, CancellationToken, IReadOnlyCollection, Task, IStockLockService, CreatePurchaseRequest, PurchaseDto, PurchaseItemDto (+15 more)

### Community 12 - "Sale"
Cohesion: 0.17
Nodes (13): PurchaseItem, DateOnly, DateTimeOffset, List, Payment, Sale, SaleItem, EntityTypeBuilder (+5 more)

### Community 13 - "ProductService"
Cohesion: 0.31
Nodes (9): ProductEndpoints, PriceSuggestionDto, ProductDto, ProductRequest, SetPricesRequest, CancellationToken, Expression, Task (+1 more)

### Community 14 - "IEntityTypeConfiguration"
Cohesion: 0.23
Nodes (7): IEntityTypeConfiguration, DateTimeOffset, Guid, RefreshToken, EntityTypeBuilder, RefreshTokenConfiguration, UserConfiguration

### Community 15 - "ClientServiceTests"
Cohesion: 0.38
Nodes (3): Fact, Task, ClientServiceTests

### Community 16 - "docker-compose 'api' service (src/NsStore.Api/Dockerfile)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "IAppDbContext"
Cohesion: 0.27
Nodes (9): DbSet, IAppDbContext, DateOnly, Order, Quote, List, User, OrderConfiguration (+1 more)

### Community 18 - "ClientService"
Cohesion: 0.32
Nodes (8): IEndpointRouteBuilder, ClientEndpoints, ClientDto, ClientRequest, CancellationToken, Task, ClientService, Client

### Community 19 - "AuthPolicies.cs"
Cohesion: 0.67
Nodes (3): string, AuthCookies, AuthPolicies

### Community 20 - "QuoteService.cs"
Cohesion: 0.30
Nodes (9): QuoteEndpoints, CancellationToken, Expression, Task, QuoteDto, QuoteQuery, QuoteRequest, QuoteRequestValidator (+1 more)

### Community 21 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 22 - "Product"
Cohesion: 0.29
Nodes (8): DateTimeOffset, InventoryMovement, StockLevel, Product, EntityTypeBuilder, InventoryMovementConfiguration, ProductConfiguration, StockLevelConfiguration

### Community 23 - "SaleTests"
Cohesion: 0.20
Nodes (7): InlineData, DateOnly, DateTimeOffset, Fact, OrderTests, SaleTests, Theory

### Community 24 - "AuditableEntity"
Cohesion: 0.25
Nodes (6): DateTimeOffset, AuditableEntity, DateTimeOffset, IHasCreationAudit, Trademark, TrademarkConfiguration

### Community 25 - "AppExceptionHandler"
Cohesion: 0.19
Nodes (9): Detail, ErrorCode, IExceptionHandler, CancellationToken, Exception, HttpContext, ValueTask, AppExceptionHandler (+1 more)

### Community 26 - "AppDbContext"
Cohesion: 0.18
Nodes (9): DbContext, IDesignTimeDbContextFactory, CancellationToken, DbSet, Func, ModelBuilder, Task, AppDbContext (+1 more)

### Community 27 - "Exceptions.cs"
Cohesion: 0.26
Nodes (10): Exception, IDictionary, AppException, BadRequestException, ConflictException, ForbiddenException, NotFoundException, UnauthorizedException (+2 more)

### Community 28 - "PagedResult"
Cohesion: 0.07
Nodes (40): IReadOnlyList, IEndpointRouteBuilder, InventoryEndpoints, IEndpointRouteBuilder, SaleEndpoints, CancellationToken, Func, Task (+32 more)

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

### Community 35 - "InitialSchema"
Cohesion: 0.24
Nodes (5): Migration, MigrationBuilder, InitialSchema, MigrationBuilder, UniqueClientCi

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
- **75 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)`, `Microsoft.EntityFrameworkCore.Design (10.0.10)`, `Microsoft.OpenApi (2.11.0)` (+70 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **4 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NsStore.Domain.Enums` connect `NsStore.Domain.Enums` to `.IssueTokensAsync`, `ReportDtos.cs`, `.UpdateAsync`, `Purchase`, `.CreateAsync`, `ClientService`, `PagedResult`?**
  _High betweenness centrality (0.142) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Entities` connect `NsStore.Domain.Enums` to `.SaveChangesAsync`, `.IssueTokensAsync`, `ReportDtos.cs`, `.UpdateAsync`, `Category`, `Sale`, `IEntityTypeConfiguration`, `QuoteService.cs`, `Product`?**
  _High betweenness centrality (0.080) - this node is a cross-community bridge._
- **Why does `TestHarness` connect `TestHarness` to `.IssueTokensAsync`, `ReportDtos.cs`, `NsStore.Domain.Enums`, `.CreateAsync`, `ProductService`, `ClientServiceTests`, `ClientService`, `AppDbContext`, `PagedResult`?**
  _High betweenness centrality (0.076) - this node is a cross-community bridge._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)` to the rest of the system?**
  _75 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `NsStore.Infrastructure.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `TestHarness` be split into smaller, more focused modules?**
  _Cohesion score 0.11627906976744186 - nodes in this community are weakly interconnected._
- **Should `.IssueTokensAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.07878787878787878 - nodes in this community are weakly interconnected._