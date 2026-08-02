# Graph Report - ns-store  (2026-08-02)

## Corpus Check
- 139 files · ~58,623 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1146 nodes · 2947 edges · 62 communities (55 shown, 7 thin omitted)
- Extraction: 93% EXTRACTED · 7% INFERRED · 0% AMBIGUOUS · INFERRED: 201 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `003eac86`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PagedResult
- NsStore.Infrastructure.csproj
- .CreateProductAsync
- .ReadyProductAsync
- .GetAsync
- UserService
- Ports.cs
- .CreateAsync
- NsStore.Application.Common.Interfaces
- AuditableEntity
- .UpdateAsync
- .CreateAsync
- IEntityTypeConfiguration
- AbstractValidator
- RefreshToken
- .ReadyProductAsync
- docker-compose 'api' service (src/NsStore.Api/Dockerfile)
- SaleDtos.cs
- ClientServiceTests
- AuthPolicies.cs
- .UpdateAsync
- http
- DemoDataSeeder
- SaleTests
- NsStore.Domain.Entities
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
- .SaveChangesAsync
- NsStore.Infrastructure.Persistence.Migrations
- TransferServiceTests
- Atomic sales transaction
- NsStore.Application/DependencyInjection.cs
- NsStore.Domain.Enums
- Dual price business rule
- Price suggestion formula
- Soft delete + audit columns
- TestHarness
- ICurrentUser
- Deploy — development / demo server
- StockTransfer
- AppDbContextModelSnapshot
- .ResolveAllocations
- Branch
- .AddInfrastructure
- .ExecuteInTransactionAsync
- .ToPagedResultAsync
- .NextAsync
- CatalogEndpoints
- QueryEnum.cs
- .TwoDebtsAsync
- PageRequest
- AppClaimTypes.cs
- ErrorCodes.cs

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Enums` - 52 edges
2. `NsStore.Domain.Entities` - 43 edges
3. `NsStore.Domain.Common` - 42 edges
4. `DemoDataSeeder` - 38 edges
5. `TestHarness` - 37 edges
6. `AppDbContext` - 31 edges
7. `NsStore.Application.Common.Interfaces` - 30 edges
8. `NsStore.Application.Common` - 27 edges
9. `IAppDbContext` - 27 edges
10. `Branch` - 27 edges

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

## Communities (62 total, 7 thin omitted)

### Community 0 - "PagedResult"
Cohesion: 0.18
Nodes (14): PagedResult, BranchAvailabilityDto, InventoryMovementDto, KardexQuery, KardexRowDto, StockAdjustmentRequest, StockLevelDto, StockQuery (+6 more)

### Community 1 - "NsStore.Infrastructure.csproj"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - ".CreateProductAsync"
Cohesion: 0.13
Nodes (12): Fact, Task, BranchServiceTests, Fact, Task, InventoryReportingTests, Fact, Task (+4 more)

### Community 3 - ".ReadyProductAsync"
Cohesion: 0.41
Nodes (3): Fact, Task, SaleServiceTests

### Community 4 - ".GetAsync"
Cohesion: 0.08
Nodes (28): Amount, Balance, IReadOnlyDictionary, Paid, IEndpointRouteBuilder, ReportEndpoints, SettingsEndpoints, ClientStatementDto (+20 more)

### Community 5 - "UserService"
Cohesion: 0.18
Nodes (15): IEndpointRouteBuilder, UserEndpoints, CreateUserRequest, UpdateUserBranchRequest, UpdateUserRequest, UpdateUserRoleRequest, UpdateUserStatusRequest, UserDto (+7 more)

### Community 6 - "Ports.cs"
Cohesion: 0.05
Nodes (37): HttpContext, IEndpointRouteBuilder, AuthEndpoints, AccessToken, DocumentKind, IDocumentNumberService, IPasswordHasher, IssuedRefreshToken (+29 more)

### Community 7 - ".CreateAsync"
Cohesion: 0.18
Nodes (12): IEndpointRouteBuilder, InventoryEndpoints, CreateTransferRequest, TransferDto, TransferItemDto, TransferItemRequest, TransferListItemDto, TransferQuery (+4 more)

### Community 8 - "NsStore.Application.Common.Interfaces"
Cohesion: 0.15
Nodes (8): NsStore.Application.Common.Interfaces, NsStore.Application, NsStore.Infrastructure, NsStore.Infrastructure.Persistence, NsStore.Infrastructure.Security, Program, string, DemoDataCatalog

### Community 9 - "AuditableEntity"
Cohesion: 0.09
Nodes (21): NsStore.Infrastructure.Persistence.Configurations, DateTimeOffset, AuditableEntity, DateTimeOffset, IHasCreationAudit, DateTimeOffset, string, AppSetting (+13 more)

### Community 10 - ".UpdateAsync"
Cohesion: 0.25
Nodes (9): OrderEndpoints, OrderDto, OrderQuery, OrderRequest, CancellationToken, Expression, Task, OrderService (+1 more)

### Community 11 - ".CreateAsync"
Cohesion: 0.18
Nodes (12): IEndpointRouteBuilder, PurchaseEndpoints, CreatePurchaseRequest, PurchaseDto, PurchaseItemDto, PurchaseItemRequest, PurchaseListItemDto, PurchaseQuery (+4 more)

### Community 12 - "IEntityTypeConfiguration"
Cohesion: 0.11
Nodes (20): IEntityTypeConfiguration, DateOnly, Order, Quote, DateOnly, List, Purchase, PurchaseItem (+12 more)

### Community 13 - "AbstractValidator"
Cohesion: 0.15
Nodes (18): AbstractValidator, ProductEndpoints, DescriptionRequestValidator, NameRequestValidator, SupplierRequestValidator, ClientRequestValidator, StockAdjustmentRequestValidator, PriceSuggestionDto (+10 more)

### Community 14 - "RefreshToken"
Cohesion: 0.29
Nodes (4): DateTimeOffset, Guid, RefreshToken, EntityTypeBuilder

### Community 15 - ".ReadyProductAsync"
Cohesion: 0.55
Nodes (3): Fact, Task, BranchScopingTests

### Community 16 - "docker-compose 'api' service (src/NsStore.Api/Dockerfile)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "SaleDtos.cs"
Cohesion: 0.14
Nodes (12): ClientDebtDto, ClientDebtFilter, ClientDebtQuery, CollectAllocationRequest, CreateSaleRequest, PaymentAllocationDto, PaymentDto, RegisterPaymentRequest (+4 more)

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
Cohesion: 0.14
Nodes (19): At, BaseCosts, Date, IEnumerable, MarginPct, Products, Random, CancellationToken (+11 more)

### Community 23 - "SaleTests"
Cohesion: 0.16
Nodes (10): InlineData, InvoiceType, DateOnly, DateTimeOffset, Fact, long, OrderTests, ProductTests (+2 more)

### Community 24 - "NsStore.Domain.Entities"
Cohesion: 0.15
Nodes (8): NsStore.Application.Common.Models, NsStore.Application.Features.Inventory, NsStore.Domain.Tests, NsStore.Application.Common, NsStore.Application.Features.Branches, NsStore.Domain.Common, NsStore.Application.Features.Settings, NsStore.Domain.Entities

### Community 25 - "AppExceptionHandler"
Cohesion: 0.19
Nodes (9): Detail, ErrorCode, IExceptionHandler, CancellationToken, Exception, HttpContext, ValueTask, AppExceptionHandler (+1 more)

### Community 26 - "AppDbContext"
Cohesion: 0.15
Nodes (20): DbContext, IDesignTimeDbContextFactory, DbSet, IAppDbContext, Client, DateOnly, DateTimeOffset, List (+12 more)

### Community 27 - "Exceptions.cs"
Cohesion: 0.26
Nodes (10): Exception, IDictionary, AppException, BadRequestException, ConflictException, ForbiddenException, NotFoundException, UnauthorizedException (+2 more)

### Community 28 - ".CreateAsync"
Cohesion: 0.21
Nodes (10): SaleEndpoints, CancellationToken, Func, Task, CollectionReceiptDto, SaleDto, CancellationToken, IReadOnlyList (+2 more)

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
Nodes (15): Migration, MigrationBuilder, InitialSchema, MigrationBuilder, UniqueClientCi, MigrationBuilder, string, AddBranches (+7 more)

### Community 36 - ".SaveChangesAsync"
Cohesion: 0.15
Nodes (14): CatalogMapping, CategoryDto, DescriptionRequest, NameRequest, SupplierDto, SupplierRequest, TrademarkDto, WarrantyTermDto (+6 more)

### Community 37 - "NsStore.Infrastructure.Persistence.Migrations"
Cohesion: 0.07
Nodes (13): NsStore.Infrastructure.Persistence.Migrations, ModelBuilder, InitialSchema, ModelBuilder, UniqueClientCi, ModelBuilder, AddBranches, ModelBuilder (+5 more)

### Community 38 - "TransferServiceTests"
Cohesion: 0.52
Nodes (3): Fact, Task, TransferServiceTests

### Community 39 - "Atomic sales transaction"
Cohesion: 0.67
Nodes (3): Atomic sales transaction, Credit sales / installment payments, Stock movement ledger (inventory_movements + stock_levels cache)

### Community 40 - "NsStore.Application/DependencyInjection.cs"
Cohesion: 0.12
Nodes (15): NsStore.Api.Endpoints, NsStore.Application.Features.Users, NsStore.Application.Features.Auth, NsStore.Application.Features.Catalogs, NsStore.Api.Security, NsStore.Application.Features.Quotes, NsStore.Application.Features.Orders, NsStore.Api.Middleware (+7 more)

### Community 41 - "NsStore.Domain.Enums"
Cohesion: 0.26
Nodes (9): NsStore.Application.Tests, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Application.Features.Sales, NsStore.Application.Features.Clients, NsStore.Domain.Enums, NsStore.Application.Features.Purchases, ClientStatementSaleDto (+1 more)

### Community 45 - "TestHarness"
Cohesion: 0.15
Nodes (14): IDisposable, SqliteConnection, Fact, Task, CrossBranchAvailabilityTests, Fact, Task, DocumentNumberingTests (+6 more)

### Community 46 - "ICurrentUser"
Cohesion: 0.31
Nodes (6): ClaimsPrincipal, CurrentUser, BranchScope, ICurrentUser, UserRole, FakeCurrentUser

### Community 47 - "Deploy — development / demo server"
Cohesion: 0.29
Nodes (6): Demo dataset, Deploy — development / demo server, Deploying, First-time setup, Operating notes, Why one origin

### Community 48 - "StockTransfer"
Cohesion: 0.27
Nodes (7): DateOnly, List, StockTransfer, StockTransferItem, EntityTypeBuilder, StockTransferConfiguration, StockTransferItemConfiguration

### Community 49 - "AppDbContextModelSnapshot"
Cohesion: 0.50
Nodes (3): ModelSnapshot, ModelBuilder, AppDbContextModelSnapshot

### Community 50 - ".ResolveAllocations"
Cohesion: 0.33
Nodes (4): Applied, CollectDebtRequestValidator, CollectDebtRequest, List

### Community 51 - "Branch"
Cohesion: 0.16
Nodes (17): BranchId, Dictionary, Movement, ProductId, Branch, DateTimeOffset, InventoryMovement, StockLevel (+9 more)

### Community 52 - ".AddInfrastructure"
Cohesion: 0.50
Nodes (3): IConfiguration, IServiceCollection, DependencyInjection

### Community 53 - ".ExecuteInTransactionAsync"
Cohesion: 0.50
Nodes (3): CancellationToken, Func, Task

### Community 54 - ".ToPagedResultAsync"
Cohesion: 0.33
Nodes (4): CancellationToken, IQueryable, Task, QueryableExtensions

### Community 55 - ".NextAsync"
Cohesion: 0.50
Nodes (3): CancellationToken, IReadOnlyCollection, Task

### Community 58 - ".TwoDebtsAsync"
Cohesion: 0.29
Nodes (6): decimal, Newer, Older, Fact, Task, CollectionTests

### Community 59 - "PageRequest"
Cohesion: 0.27
Nodes (9): int, PageRequest, SaleListItemDto, SaleQuery, DateOnly, Expression, IQueryable, SaleListRow (+1 more)

## Knowledge Gaps
- **87 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)`, `Microsoft.EntityFrameworkCore.Design (10.0.10)`, `Microsoft.OpenApi (2.11.0)` (+82 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **7 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NsStore.Domain.Enums` connect `NsStore.Domain.Enums` to `PagedResult`, `UserService`, `Ports.cs`, `NsStore.Infrastructure.Persistence.Migrations`, `NsStore.Application/DependencyInjection.cs`, `NsStore.Application.Common.Interfaces`, `.UpdateAsync`, `.CreateAsync`, `SaleDtos.cs`, `ClientServiceTests`, `NsStore.Domain.Entities`, `AppDbContext`?**
  _High betweenness centrality (0.164) - this node is a cross-community bridge._
- **Why does `TestHarness` connect `TestHarness` to `PagedResult`, `.CreateProductAsync`, `.ReadyProductAsync`, `BranchService`, `.GetAsync`, `Ports.cs`, `.CreateAsync`, `AppDbContext`, `NsStore.Domain.Enums`, `TransferServiceTests`, `.CreateAsync`, `AbstractValidator`, `ICurrentUser`, `.ReadyProductAsync`, `ClientServiceTests`, `.TwoDebtsAsync`, `PageRequest`?**
  _High betweenness centrality (0.137) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Common` connect `NsStore.Domain.Entities` to `NsStore.Application/DependencyInjection.cs`, `AuditableEntity`, `NsStore.Application.Common.Interfaces`, `NsStore.Domain.Enums`, `StockTransfer`, `AppDbContext`, `Exceptions.cs`, `AppClaimTypes.cs`, `ErrorCodes.cs`?**
  _High betweenness centrality (0.076) - this node is a cross-community bridge._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)`, `Microsoft.AspNetCore.OpenApi (10.0.10)` to the rest of the system?**
  _87 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `NsStore.Infrastructure.csproj` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `.CreateProductAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.13225371120107962 - nodes in this community are weakly interconnected._
- **Should `.GetAsync` be split into smaller, more focused modules?**
  _Cohesion score 0.08383838383838384 - nodes in this community are weakly interconnected._