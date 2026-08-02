# Graph Report - ns-store  (2026-08-01)

## Corpus Check
- 137 files · ~57,637 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1194 nodes · 2877 edges · 66 communities (54 shown, 12 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 223 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `8836a47a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .SaveChangesAsync
- NsStore.Infrastructure.csproj
- .CreateProductAsync
- TokenService
- ReportDtos.cs
- AbstractValidator
- .LockAsync
- .IssueTokensAsync
- NsStore.Application.Common.Interfaces
- AuditableEntity
- Order
- .CreateAsync
- IEntityTypeConfiguration
- ProductService
- User
- StockTransfer
- docker-compose 'api' service (src/NsStore.Api/Dockerfile)
- IAppDbContext
- ClientServiceTests
- AuthPolicies.cs
- QuoteService.cs
- http
- DemoDataSeeder
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
- .StockedInMainAsync
- Deploy — development / demo server
- .AddInfrastructure
- AppDbContextModelSnapshot
- ErrorCodes.cs
- Ports.cs
- AppClaimTypes.cs
- PasswordHasher
- ValidationFilter.cs
- .MapAuthEndpoints
- AppSetting
- SaleDtos.cs
- .TwoDebtsAsync
- TokenService
- .IssueTokensAsync
- Branch
- DocumentNumberingTests
- PageRequest
- Ports.cs
- PasswordHasher

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Entities` - 43 edges
2. `DemoDataSeeder` - 38 edges
3. `TestHarness` - 37 edges
4. `NsStore.Domain.Enums` - 37 edges
5. `NsStore.Domain.Common` - 35 edges
6. `AppDbContext` - 31 edges
7. `NsStore.Application.Common.Interfaces` - 30 edges
8. `IAppDbContext` - 27 edges
9. `Branch` - 27 edges
10. `NsStore.Application.Common` - 21 edges

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

## Communities (66 total, 12 thin omitted)

### Community 0 - ".SaveChangesAsync"
Cohesion: 0.08
Nodes (29): InventoryEndpoints, CancellationToken, IQueryable, Task, PagedResult, QueryableExtensions, BranchAvailabilityDto, InventoryMovementDto (+21 more)

### Community 1 - "NsStore.Infrastructure.csproj"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - ".CreateProductAsync"
Cohesion: 0.17
Nodes (9): Fact, Task, BranchServiceTests, Fact, Task, InventoryReportingTests, Fact, Task (+1 more)

### Community 3 - "TokenService"
Cohesion: 0.41
Nodes (3): Fact, Task, SaleServiceTests

### Community 4 - "ReportDtos.cs"
Cohesion: 0.09
Nodes (27): Amount, Balance, IReadOnlyDictionary, Paid, Quantity, IEndpointRouteBuilder, ClientStatementDto, ClientStatementSaleDto (+19 more)

### Community 5 - "AbstractValidator"
Cohesion: 0.18
Nodes (15): IEndpointRouteBuilder, UserEndpoints, CreateUserRequest, UpdateUserBranchRequest, UpdateUserRequest, UpdateUserRoleRequest, UpdateUserStatusRequest, UserDto (+7 more)

### Community 6 - ".LockAsync"
Cohesion: 0.05
Nodes (39): HttpContext, IEndpointRouteBuilder, AuthEndpoints, CancellationToken, IReadOnlyCollection, Task, User, AccessToken (+31 more)

### Community 7 - ".IssueTokensAsync"
Cohesion: 0.55
Nodes (3): Fact, Task, BranchScopingTests

### Community 8 - "NsStore.Application.Common.Interfaces"
Cohesion: 0.13
Nodes (11): NsStore.Application.Common.Interfaces, NsStore.Application, NsStore.Infrastructure, NsStore.Infrastructure.Persistence, NsStore.Infrastructure.Security, IConfiguration, Program, IServiceCollection (+3 more)

### Community 9 - "AuditableEntity"
Cohesion: 0.09
Nodes (23): NsStore.Infrastructure.Persistence.Configurations, DateTimeOffset, AuditableEntity, DateTimeOffset, IHasCreationAudit, DateTimeOffset, string, AppSetting (+15 more)

### Community 10 - "Order"
Cohesion: 0.23
Nodes (10): OrderEndpoints, OrderDto, OrderQuery, OrderRequest, CancellationToken, Expression, Task, OrderService (+2 more)

### Community 11 - ".CreateAsync"
Cohesion: 0.22
Nodes (10): CreatePurchaseRequest, PurchaseDto, PurchaseItemDto, PurchaseItemRequest, PurchaseListItemDto, PurchaseQuery, CancellationToken, Task (+2 more)

### Community 12 - "IEntityTypeConfiguration"
Cohesion: 0.17
Nodes (11): EntityTypeBuilder, Client, Purchase, PurchaseItem, Quote, ClientConfiguration, PaymentConfiguration, PaymentReceiptConfiguration (+3 more)

### Community 13 - "ProductService"
Cohesion: 0.13
Nodes (19): AbstractValidator, IEndpointRouteBuilder, ProductEndpoints, DescriptionRequestValidator, NameRequestValidator, SupplierRequestValidator, ClientRequestValidator, StockAdjustmentRequestValidator (+11 more)

### Community 14 - "User"
Cohesion: 0.23
Nodes (7): IEntityTypeConfiguration, DateTimeOffset, Guid, RefreshToken, EntityTypeBuilder, RefreshTokenConfiguration, UserConfiguration

### Community 15 - "StockTransfer"
Cohesion: 0.29
Nodes (7): DateOnly, List, StockTransfer, StockTransferItem, EntityTypeBuilder, StockTransferConfiguration, StockTransferItemConfiguration

### Community 16 - "docker-compose 'api' service (src/NsStore.Api/Dockerfile)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "IAppDbContext"
Cohesion: 0.26
Nodes (11): AuditableEntity, InvoiceType, List, PaymentStatus, Client, DateOnly, DateTimeOffset, Payment (+3 more)

### Community 18 - "ClientServiceTests"
Cohesion: 0.17
Nodes (11): IEndpointRouteBuilder, ClientEndpoints, ClientDto, ClientRequest, CancellationToken, Task, ClientService, Client (+3 more)

### Community 19 - "AuthPolicies.cs"
Cohesion: 0.67
Nodes (3): string, AuthCookies, AuthPolicies

### Community 20 - "QuoteService.cs"
Cohesion: 0.24
Nodes (12): IEndpointRouteBuilder, QuoteEndpoints, CancellationToken, Expression, Task, QuoteDto, QuoteQuery, QuoteRequest (+4 more)

### Community 21 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 22 - "DemoDataSeeder"
Cohesion: 0.19
Nodes (12): BaseCosts, MarginPct, Products, Random, CancellationToken, int, List, string (+4 more)

### Community 23 - "SaleTests"
Cohesion: 0.08
Nodes (21): ClaimsPrincipal, InlineData, CurrentUser, BranchScope, UserRole, ICurrentUser, ClientType, InvoiceType (+13 more)

### Community 24 - "NsStore.Domain.Enums"
Cohesion: 0.12
Nodes (9): NsStore.Domain.Tests, NsStore.Application.Features.Clients, NsStore.Domain.Enums, NsStore.Domain.Common, NsStore.Domain.Entities, string, AppClaimTypes, string (+1 more)

### Community 25 - "AppExceptionHandler"
Cohesion: 0.19
Nodes (9): Detail, ErrorCode, IExceptionHandler, CancellationToken, Exception, HttpContext, ValueTask, AppExceptionHandler (+1 more)

### Community 26 - "AppDbContext"
Cohesion: 0.09
Nodes (22): DbContext, IDesignTimeDbContextFactory, Category, Client, DbSet, InventoryMovement, ModelBuilder, Order (+14 more)

### Community 27 - "Exceptions.cs"
Cohesion: 0.26
Nodes (10): Exception, IDictionary, AppException, BadRequestException, ConflictException, ForbiddenException, NotFoundException, UnauthorizedException (+2 more)

### Community 28 - ".CreateAsync"
Cohesion: 0.08
Nodes (34): Expression, IQueryable, IReadOnlyList, PagedResult, PageRequest, IEndpointRouteBuilder, PurchaseEndpoints, SaleEndpoints (+26 more)

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
Cohesion: 0.07
Nodes (15): Migration, MigrationBuilder, MigrationBuilder, InitialSchema, MigrationBuilder, UniqueClientCi, MigrationBuilder, string (+7 more)

### Community 36 - "CatalogEndpoints"
Cohesion: 0.13
Nodes (19): NsStore.Application.Features.Catalogs, IEndpointRouteBuilder, CatalogEndpoints, int, PageRequest, CatalogMapping, CategoryDto, DescriptionRequest (+11 more)

### Community 37 - "NsStore.Infrastructure.Persistence.Migrations"
Cohesion: 0.07
Nodes (13): NsStore.Infrastructure.Persistence.Migrations, ModelBuilder, InitialSchema, ModelBuilder, UniqueClientCi, ModelBuilder, AddBranches, ModelBuilder (+5 more)

### Community 38 - "NsStore.Application.Features.Users"
Cohesion: 0.28
Nodes (5): NsStore.Application.Features.Users, NsStore.Application.Features.Auth, LoginRequest, LoginResponse, LoginRequestValidator

### Community 39 - "Atomic sales transaction"
Cohesion: 0.67
Nodes (3): Atomic sales transaction, Credit sales / installment payments, Stock movement ledger (inventory_movements + stock_levels cache)

### Community 40 - "NsStore.Application.Common.Models"
Cohesion: 0.24
Nodes (8): NsStore.Api.Endpoints, NsStore.Application.Common.Models, NsStore.Api.Security, NsStore.Application.Features.Quotes, NsStore.Application.Features.Orders, NsStore.Api.Middleware, string, RateLimitPolicies

### Community 41 - "NsStore.Application.Features.Inventory"
Cohesion: 0.17
Nodes (10): NsStore.Application.Tests, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Application.Features.Sales, NsStore.Application.Features.Purchases, NsStore.Application.Features.Settings, ReportEndpoints, SettingsEndpoints (+2 more)

### Community 45 - "TestHarness"
Cohesion: 0.25
Nodes (6): At, Date, IEnumerable, DateOnly, Quantity, Workspace

### Community 46 - ".StockedInMainAsync"
Cohesion: 0.11
Nodes (18): Category, Client, DbSet, InventoryMovement, Order, Product, Purchase, PurchaseItem (+10 more)

### Community 47 - "Deploy — development / demo server"
Cohesion: 0.29
Nodes (6): Demo dataset, Deploy — development / demo server, Deploying, First-time setup, Operating notes, Why one origin

### Community 48 - ".AddInfrastructure"
Cohesion: 0.55
Nodes (3): Fact, Task, CrossBranchAvailabilityTests

### Community 49 - "AppDbContextModelSnapshot"
Cohesion: 0.50
Nodes (3): ModelSnapshot, ModelBuilder, AppDbContextModelSnapshot

### Community 50 - "ErrorCodes.cs"
Cohesion: 0.58
Nodes (3): Fact, Task, ReportServiceTests

### Community 51 - "Ports.cs"
Cohesion: 0.16
Nodes (16): BranchId, Dictionary, Movement, ProductId, DateTimeOffset, InventoryMovement, StockLevel, List (+8 more)

### Community 52 - "AppClaimTypes.cs"
Cohesion: 0.67
Nodes (3): Product, SaleItem, SaleItemConfiguration

### Community 53 - "PasswordHasher"
Cohesion: 0.50
Nodes (3): CancellationToken, Func, Task

### Community 54 - "ValidationFilter.cs"
Cohesion: 0.23
Nodes (5): NsStore.Application.Features.Inventory, NsStore.Application.Common, NsStore.Application.Features.Branches, RouteHandlerBuilder, ValidationFilterExtensions

### Community 58 - ".TwoDebtsAsync"
Cohesion: 0.09
Nodes (26): BranchService, ClientService, decimal, IDisposable, InventoryService, long, Newer, Older (+18 more)

### Community 61 - "Branch"
Cohesion: 0.25
Nodes (5): Branch, List, User, EntityTypeBuilder, BranchConfiguration

## Knowledge Gaps
- **86 isolated node(s):** `ClientStatementSaleDto`, `PriceListRowDto`, `SaleItemRequest`, `PaymentDto`, `ClientDebtFilter` (+81 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **12 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TestHarness` connect `.TwoDebtsAsync` to `.CreateProductAsync`, `TokenService`, `ReportDtos.cs`, `.LockAsync`, `.IssueTokensAsync`, `NsStore.Application.Features.Inventory`, `.AddInfrastructure`, `ClientServiceTests`, `ErrorCodes.cs`, `SaleTests`, `AppDbContext`, `.CreateAsync`?**
  _High betweenness centrality (0.149) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Entities` connect `NsStore.Domain.Enums` to `CatalogEndpoints`, `ReportDtos.cs`, `NsStore.Application.Features.Users`, `.LockAsync`, `NsStore.Application.Common.Models`, `NsStore.Application.Features.Inventory`, `AbstractValidator`, `AuditableEntity`, `NsStore.Application.Common.Interfaces`, `IEntityTypeConfiguration`, `User`, `StockTransfer`, `IAppDbContext`, `Ports.cs`, `QuoteService.cs`, `ValidationFilter.cs`, `Branch`?**
  _High betweenness centrality (0.126) - this node is a cross-community bridge._
- **Why does `AppDbContext` connect `AppDbContext` to `NsStore.Application.Common.Interfaces`, `AuditableEntity`, `.StockedInMainAsync`, `IAppDbContext`, `AppClaimTypes.cs`, `PasswordHasher`, `.TwoDebtsAsync`, `Branch`?**
  _High betweenness centrality (0.102) - this node is a cross-community bridge._
- **What connects `ClientStatementSaleDto`, `PriceListRowDto`, `SaleItemRequest` to the rest of the system?**
  _86 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.SaveChangesAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.08244680851063829 - nodes in this community are weakly interconnected._
- **Should `NsStore.Infrastructure.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `ReportDtos.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.09393939393939393 - nodes in this community are weakly interconnected._