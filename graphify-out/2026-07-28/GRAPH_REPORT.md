# Graph Report - ns-store  (2026-07-28)

## Corpus Check
- 117 files · ~33,992 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 894 nodes · 2173 edges · 53 communities (47 shown, 6 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 166 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `1745f61a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .SaveChangesAsync
- NsStore.Infrastructure.csproj
- .CreateProductAsync
- .IssueTokensAsync
- .MapReportEndpoints
- AbstractValidator
- Purchase
- TestHarness
- NsStore.Application.Common.Interfaces
- AuditableEntity
- Order
- .CreateAsync
- TransactionConfigurations.cs
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
- NsStore.Domain.Enums
- AppExceptionHandler
- AppDbContext
- Exceptions.cs
- PagedResult
- .SavingChangesAsync
- .InvokeAsync
- CLAUDE.md
- .Apply
- DatabaseInitializer
- BranchService
- AddBranches
- CatalogEndpoints
- AppSetting
- NsStore.Application.Features.Users
- Atomic sales transaction
- NsStore.Api.Security
- TestHarness.cs
- Dual price business rule
- Price suggestion formula
- Soft delete + audit columns
- NsStore.Infrastructure.Persistence.Migrations
- .ReadyProductAsync
- .AddInfrastructure
- Quote
- AppDbContextModelSnapshot
- ErrorCodes.cs
- AppClaimTypes.cs
- NsStore.Application.Common

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Enums` - 42 edges
2. `NsStore.Domain.Entities` - 38 edges
3. `NsStore.Domain.Common` - 36 edges
4. `AppDbContext` - 28 edges
5. `NsStore.Application.Common.Interfaces` - 26 edges
6. `IAppDbContext` - 24 edges
7. `TestHarness` - 24 edges
8. `NsStore.Application.Common` - 21 edges
9. `PagedResult` - 21 edges
10. `NsStore.Application.Common.Models` - 19 edges

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

## Communities (53 total, 6 thin omitted)

### Community 0 - ".SaveChangesAsync"
Cohesion: 0.15
Nodes (15): CatalogMapping, CategoryDto, DescriptionRequest, NameRequest, SupplierDto, SupplierRequest, TrademarkDto, WarrantyTermDto (+7 more)

### Community 1 - "NsStore.Infrastructure.csproj"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - ".CreateProductAsync"
Cohesion: 0.17
Nodes (9): Fact, Task, BranchServiceTests, Fact, Task, InventoryReportingTests, Fact, Task (+1 more)

### Community 3 - ".IssueTokensAsync"
Cohesion: 0.06
Nodes (33): HttpContext, IEndpointRouteBuilder, AuthEndpoints, CancellationToken, IReadOnlyCollection, Task, AccessToken, IPasswordHasher (+25 more)

### Community 4 - ".MapReportEndpoints"
Cohesion: 0.10
Nodes (22): IReadOnlyDictionary, IEndpointRouteBuilder, ReportEndpoints, SettingsEndpoints, DashboardDto, DebtsReportDto, PriceListReportDto, PurchasesReportDto (+14 more)

### Community 5 - "AbstractValidator"
Cohesion: 0.09
Nodes (27): AbstractValidator, IEndpointRouteBuilder, UserEndpoints, LoginRequest, LoginRequestValidator, DescriptionRequestValidator, NameRequestValidator, SupplierRequestValidator (+19 more)

### Community 6 - "Purchase"
Cohesion: 0.18
Nodes (10): DateOnly, List, Purchase, ClientType, InvoiceType, MovementType, OrderStatus, PaymentStatus (+2 more)

### Community 7 - "TestHarness"
Cohesion: 0.13
Nodes (17): ClaimsPrincipal, IDisposable, SqliteConnection, CurrentUser, BranchScope, ICurrentUser, UserRole, Fact (+9 more)

### Community 8 - "NsStore.Application.Common.Interfaces"
Cohesion: 0.21
Nodes (6): NsStore.Application.Common.Interfaces, NsStore.Application, NsStore.Infrastructure, NsStore.Infrastructure.Persistence, NsStore.Infrastructure.Security, Program

### Community 9 - "AuditableEntity"
Cohesion: 0.19
Nodes (10): DateTimeOffset, AuditableEntity, Category, Supplier, WarrantyTerm, EntityTypeBuilder, CategoryConfiguration, SupplierConfiguration (+2 more)

### Community 10 - "Order"
Cohesion: 0.28
Nodes (9): OrderEndpoints, OrderDto, OrderQuery, OrderRequest, CancellationToken, Expression, Task, OrderService (+1 more)

### Community 11 - ".CreateAsync"
Cohesion: 0.18
Nodes (12): IEndpointRouteBuilder, PurchaseEndpoints, CreatePurchaseRequest, PurchaseDto, PurchaseItemDto, PurchaseItemRequest, PurchaseListItemDto, PurchaseQuery (+4 more)

### Community 12 - "TransactionConfigurations.cs"
Cohesion: 0.18
Nodes (9): PurchaseItem, SaleItem, EntityTypeBuilder, ClientConfiguration, OrderConfiguration, PaymentConfiguration, PurchaseItemConfiguration, SaleConfiguration (+1 more)

### Community 13 - "ProductService"
Cohesion: 0.29
Nodes (9): ProductEndpoints, PriceSuggestionDto, ProductDto, ProductRequest, SetPricesRequest, CancellationToken, Expression, Task (+1 more)

### Community 14 - "IEntityTypeConfiguration"
Cohesion: 0.17
Nodes (10): NsStore.Infrastructure.Persistence.Configurations, IEntityTypeConfiguration, DateTimeOffset, Guid, RefreshToken, List, User, EntityTypeBuilder (+2 more)

### Community 15 - "ClientServiceTests"
Cohesion: 0.38
Nodes (3): Fact, Task, ClientServiceTests

### Community 16 - "docker-compose 'api' service (src/NsStore.Api/Dockerfile)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "IAppDbContext"
Cohesion: 0.26
Nodes (10): DbSet, IAppDbContext, Branch, DateOnly, DateTimeOffset, List, Payment, Sale (+2 more)

### Community 18 - "ClientService"
Cohesion: 0.32
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
Cohesion: 0.19
Nodes (11): DateTimeOffset, IHasCreationAudit, DateTimeOffset, InventoryMovement, StockLevel, List, Product, EntityTypeBuilder (+3 more)

### Community 23 - "SaleTests"
Cohesion: 0.18
Nodes (8): InlineData, DateOnly, DateTimeOffset, Fact, long, OrderTests, SaleTests, Theory

### Community 24 - "NsStore.Domain.Enums"
Cohesion: 0.20
Nodes (5): NsStore.Domain.Tests, NsStore.Application.Features.Clients, NsStore.Domain.Enums, NsStore.Domain.Common, NsStore.Domain.Entities

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
Nodes (37): IReadOnlyList, IEndpointRouteBuilder, InventoryEndpoints, SaleEndpoints, CancellationToken, Func, Task, CancellationToken (+29 more)

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

### Community 35 - "AddBranches"
Cohesion: 0.15
Nodes (8): Migration, MigrationBuilder, InitialSchema, MigrationBuilder, UniqueClientCi, MigrationBuilder, string, AddBranches

### Community 37 - "AppSetting"
Cohesion: 0.33
Nodes (5): DateTimeOffset, string, AppSetting, AppSettingKeys, AppSettingConfiguration

### Community 38 - "NsStore.Application.Features.Users"
Cohesion: 0.50
Nodes (3): NsStore.Application.Features.Users, NsStore.Application.Features.Auth, LoginResponse

### Community 39 - "Atomic sales transaction"
Cohesion: 0.67
Nodes (3): Atomic sales transaction, Credit sales / installment payments, Stock movement ledger (inventory_movements + stock_levels cache)

### Community 40 - "NsStore.Api.Security"
Cohesion: 0.22
Nodes (9): NsStore.Api.Endpoints, NsStore.Application.Common.Models, NsStore.Application.Features.Catalogs, NsStore.Api.Security, NsStore.Application.Features.Quotes, NsStore.Application.Features.Orders, NsStore.Api.Middleware, string (+1 more)

### Community 41 - "TestHarness.cs"
Cohesion: 0.22
Nodes (10): NsStore.Application.Tests, NsStore.Application.Features.Inventory, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Application.Features.Sales, NsStore.Application.Features.Purchases, NsStore.Application.Features.Settings, IServiceCollection (+2 more)

### Community 45 - "NsStore.Infrastructure.Persistence.Migrations"
Cohesion: 0.13
Nodes (7): NsStore.Infrastructure.Persistence.Migrations, ModelBuilder, InitialSchema, ModelBuilder, UniqueClientCi, ModelBuilder, AddBranches

### Community 46 - ".ReadyProductAsync"
Cohesion: 0.47
Nodes (3): Fact, Task, SaleServiceTests

### Community 47 - ".AddInfrastructure"
Cohesion: 0.50
Nodes (3): IConfiguration, IServiceCollection, DependencyInjection

### Community 48 - "Quote"
Cohesion: 0.67
Nodes (3): DateOnly, Quote, QuoteConfiguration

### Community 49 - "AppDbContextModelSnapshot"
Cohesion: 0.50
Nodes (3): ModelSnapshot, ModelBuilder, AppDbContextModelSnapshot

### Community 54 - "NsStore.Application.Common"
Cohesion: 0.21
Nodes (4): NsStore.Application.Common, NsStore.Application.Features.Branches, RouteHandlerBuilder, ValidationFilterExtensions

## Knowledge Gaps
- **76 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)`, `Microsoft.EntityFrameworkCore.Design (10.0.10)`, `Microsoft.OpenApi (2.11.0)` (+71 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **6 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NsStore.Domain.Enums` connect `NsStore.Domain.Enums` to `.IssueTokensAsync`, `AbstractValidator`, `Purchase`, `NsStore.Api.Security`, `TestHarness.cs`, `Order`, `.CreateAsync`, `NsStore.Application.Common.Interfaces`, `NsStore.Infrastructure.Persistence.Migrations`, `ClientService`, `NsStore.Application.Common`, `PagedResult`?**
  _High betweenness centrality (0.146) - this node is a cross-community bridge._
- **Why does `TestHarness` connect `TestHarness` to `.CreateProductAsync`, `.IssueTokensAsync`, `BranchService`, `.MapReportEndpoints`, `TestHarness.cs`, `.CreateAsync`, `ProductService`, `.ReadyProductAsync`, `ClientServiceTests`, `ClientService`, `AppDbContext`, `PagedResult`?**
  _High betweenness centrality (0.090) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Common` connect `NsStore.Domain.Enums` to `NsStore.Application.Features.Users`, `NsStore.Api.Security`, `AuditableEntity`, `NsStore.Application.Common.Interfaces`, `TestHarness.cs`, `ErrorCodes.cs`, `QuoteService.cs`, `AppClaimTypes.cs`, `NsStore.Application.Common`, `Product`, `Exceptions.cs`?**
  _High betweenness centrality (0.089) - this node is a cross-community bridge._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)` to the rest of the system?**
  _76 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.SaveChangesAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.1497584541062802 - nodes in this community are weakly interconnected._
- **Should `NsStore.Infrastructure.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `.IssueTokensAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.05844155844155844 - nodes in this community are weakly interconnected._