# Graph Report - ns-store  (2026-07-26)

## Corpus Check
- 99 files · ~23,758 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 778 nodes · 1807 edges · 44 communities (36 shown, 8 thin omitted)
- Extraction: 91% EXTRACTED · 9% INFERRED · 0% AMBIGUOUS · INFERRED: 154 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `625a49e4`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Catalog & Inventory Services
- Project & NuGet Config
- Stock Lock & Purchase/Sale Tests
- Auth & JWT Tokens
- Settings & Reports Services
- User Management & Password Hashing
- Sale Service & DTOs
- API Endpoint Route Mapping
- Domain Entities & Core Services Hub
- Catalog Entities & EF Configurations
- Order Service & Policies
- Purchase Service & DTOs
- Transaction EF Configurations
- Product Service & DTOs
- Current User & Identity/Refresh Tokens
- FluentValidation Validators
- Docs & Business Rationale (README/CI/Compose)
- Program Bootstrap & Infrastructure Wiring
- Client Service & DTOs
- Application DI & Reports/Tests Namespaces
- Quote Service
- launchSettings Configuration
- Inventory/Product Entities & EF Configs
- Order & Sale Domain Tests
- Domain Enums & Purchase Entity
- Exception Handler Middleware
- AppDbContext & Design-Time Factory
- Application/Domain Exceptions
- IAppDbContext & Sale Entity
- Audit Interceptor (EF SaveChanges)
- Validation Filter (Endpoint Filter)
- DateTimeOffset
- Stock Level Domain Tests
- Database Initializer & Seeding
- IQueryable
- Initial Schema Migration (Up/Down)
- int
- Infrastructure DI
- EF Model Snapshot
- Sales Business Rules (Ledger/Atomic/Credit)
- Fact
- Dual Price Rule
- Price Suggestion Formula
- Soft Delete Rule

## God Nodes (most connected - your core abstractions)
1. `NsStore.Domain.Entities` - 33 edges
2. `NsStore.Domain.Enums` - 31 edges
3. `AppDbContext` - 27 edges
4. `NsStore.Domain.Common` - 25 edges
5. `IAppDbContext` - 23 edges
6. `NsStore.Application.Common.Interfaces` - 21 edges
7. `TestHarness` - 18 edges
8. `Sale` - 16 edges
9. `User` - 16 edges
10. `Product` - 15 edges

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

## Communities (44 total, 8 thin omitted)

### Community 0 - "Catalog & Inventory Services"
Cohesion: 0.15
Nodes (15): IEndpointRouteBuilder, CatalogEndpoints, CategoryDto, DescriptionRequest, NameRequest, SupplierDto, SupplierRequest, TrademarkDto (+7 more)

### Community 1 - "Project & NuGet Config"
Cohesion: 0.05
Nodes (41): EFCore.NamingConventions (10.0.1), FluentValidation (12.1.1), FluentValidation.DependencyInjectionExtensions (12.1.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.OpenApi (10.0.10), Microsoft.EntityFrameworkCore (10.0.10), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Sqlite (10.0.10) (+33 more)

### Community 2 - "Stock Lock & Purchase/Sale Tests"
Cohesion: 0.11
Nodes (18): CancellationToken, IReadOnlyCollection, Task, IStockLockService, CancellationToken, IReadOnlyCollection, Task, StockLockService (+10 more)

### Community 3 - "Auth & JWT Tokens"
Cohesion: 0.08
Nodes (25): ClaimsPrincipal, HttpContext, IEndpointRouteBuilder, AuthEndpoints, CurrentUser, AccessToken, ICurrentUser, IPasswordHasher (+17 more)

### Community 4 - "Settings & Reports Services"
Cohesion: 0.19
Nodes (14): int, ReportEndpoints, DashboardDto, DebtsReportDto, PriceListReportDto, PriceListRowDto, PurchasesReportDto, ReportRange (+6 more)

### Community 5 - "User Management & Password Hashing"
Cohesion: 0.21
Nodes (11): IEndpointRouteBuilder, UserEndpoints, CreateUserRequest, UpdateUserRequest, UserDto, UserMapping, CancellationToken, Task (+3 more)

### Community 6 - "Sale Service & DTOs"
Cohesion: 0.11
Nodes (25): IReadOnlyList, SaleEndpoints, CancellationToken, Func, Task, CancellationToken, int, IQueryable (+17 more)

### Community 7 - "API Endpoint Route Mapping"
Cohesion: 0.14
Nodes (13): AbstractValidator, LoginRequest, LoginRequestValidator, DescriptionRequestValidator, NameRequestValidator, SupplierRequestValidator, ClientRequestValidator, OrderRequestValidator (+5 more)

### Community 8 - "Domain Entities & Core Services Hub"
Cohesion: 0.05
Nodes (42): NsStore.Application.Common.Interfaces, NsStore.Api.Endpoints, NsStore.Application.Common.Models, NsStore.Application.Tests, NsStore.Application.Features.Inventory, NsStore.Application.Features.Reports, NsStore.Application.Features.Products, NsStore.Domain.Tests (+34 more)

### Community 9 - "Catalog Entities & EF Configurations"
Cohesion: 0.14
Nodes (16): NsStore.Infrastructure.Persistence.Configurations, CatalogMapping, DateTimeOffset, AuditableEntity, DateTimeOffset, AppSetting, Category, Supplier (+8 more)

### Community 10 - "Order Service & Policies"
Cohesion: 0.27
Nodes (9): OrderEndpoints, OrderDto, OrderQuery, OrderRequest, CancellationToken, Expression, Task, OrderService (+1 more)

### Community 11 - "Purchase Service & DTOs"
Cohesion: 0.12
Nodes (17): Fact, IEndpointRouteBuilder, PurchaseEndpoints, CreatePurchaseRequest, PurchaseDto, PurchaseItemDto, PurchaseItemRequest, PurchaseListItemDto (+9 more)

### Community 12 - "Transaction EF Configurations"
Cohesion: 0.18
Nodes (13): Client, DateOnly, DateTimeOffset, List, Payment, Sale, SaleItem, EntityTypeBuilder (+5 more)

### Community 13 - "Product Service & DTOs"
Cohesion: 0.31
Nodes (9): ProductEndpoints, PriceSuggestionDto, ProductDto, ProductRequest, SetPricesRequest, CancellationToken, Expression, Task (+1 more)

### Community 14 - "Current User & Identity/Refresh Tokens"
Cohesion: 0.16
Nodes (12): IEntityTypeConfiguration, DateOnly, Quote, DateTimeOffset, Guid, RefreshToken, List, User (+4 more)

### Community 15 - "FluentValidation Validators"
Cohesion: 0.16
Nodes (16): DateTimeOffset, IQueryable, PageRequest, Product, IEndpointRouteBuilder, InventoryEndpoints, InventoryMovementDto, KardexRowDto (+8 more)

### Community 16 - "Docs & Business Rationale (README/CI/Compose)"
Cohesion: 0.13
Nodes (18): docker-compose 'api' service (src/NsStore.Api/Dockerfile), docker-compose 'db' service (postgres:17-alpine), docs/new-app/03-frontend.md (Frontend plan: React + TS + Vite), docs/new-app/README.md (new build plan, English), docs/README.md (legacy analysis, Spanish), Verify model has no pending migrations (dotnet-ef check), CI Workflow (build-and-test), NsStore.Api layer (endpoints, auth, ProblemDetails, rate limiting, DI, config) (+10 more)

### Community 17 - "Program Bootstrap & Infrastructure Wiring"
Cohesion: 0.22
Nodes (9): IReadOnlyDictionary, IEndpointRouteBuilder, SettingsEndpoints, CancellationToken, Task, SettingsDto, SettingsService, UpdateSettingsRequest (+1 more)

### Community 18 - "Client Service & DTOs"
Cohesion: 0.16
Nodes (14): IDisposable, SqliteConnection, IEndpointRouteBuilder, ClientEndpoints, ClientDto, ClientRequest, CancellationToken, Task (+6 more)

### Community 19 - "Application DI & Reports/Tests Namespaces"
Cohesion: 0.67
Nodes (3): string, AuthCookies, AuthPolicies

### Community 20 - "Quote Service"
Cohesion: 0.30
Nodes (9): IEndpointRouteBuilder, QuoteEndpoints, CancellationToken, Expression, Task, QuoteDto, QuoteQuery, QuoteRequest (+1 more)

### Community 21 - "launchSettings Configuration"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 22 - "Inventory/Product Entities & EF Configs"
Cohesion: 0.25
Nodes (10): DbSet, IAppDbContext, DateTimeOffset, InventoryMovement, StockLevel, Product, EntityTypeBuilder, InventoryMovementConfiguration (+2 more)

### Community 23 - "Order & Sale Domain Tests"
Cohesion: 0.22
Nodes (7): InlineData, DateOnly, DateTimeOffset, Fact, OrderTests, SaleTests, Theory

### Community 24 - "Domain Enums & Purchase Entity"
Cohesion: 0.15
Nodes (12): DateOnly, List, Purchase, PurchaseItem, ClientType, InvoiceType, MovementType, OrderStatus (+4 more)

### Community 25 - "Exception Handler Middleware"
Cohesion: 0.19
Nodes (9): Detail, ErrorCode, IExceptionHandler, CancellationToken, Exception, HttpContext, ValueTask, AppExceptionHandler (+1 more)

### Community 26 - "AppDbContext & Design-Time Factory"
Cohesion: 0.18
Nodes (9): DbContext, IDesignTimeDbContextFactory, CancellationToken, DbSet, Func, ModelBuilder, Task, AppDbContext (+1 more)

### Community 27 - "Application/Domain Exceptions"
Cohesion: 0.26
Nodes (10): Exception, IDictionary, AppException, BadRequestException, ConflictException, ForbiddenException, NotFoundException, UnauthorizedException (+2 more)

### Community 29 - "Audit Interceptor (EF SaveChanges)"
Cohesion: 0.29
Nodes (7): DbContextEventData, InterceptionResult, SaveChangesInterceptor, CancellationToken, DbContext, ValueTask, AuditInterceptor

### Community 30 - "Validation Filter (Endpoint Filter)"
Cohesion: 0.33
Nodes (5): EndpointFilterDelegate, EndpointFilterInvocationContext, IEndpointFilter, ValueTask, ValidationFilter

### Community 32 - "Stock Level Domain Tests"
Cohesion: 0.48
Nodes (3): DateTimeOffset, Fact, StockLevelTests

### Community 33 - "Database Initializer & Seeding"
Cohesion: 0.67
Nodes (3): CancellationToken, Task, DatabaseInitializer

### Community 35 - "Initial Schema Migration (Up/Down)"
Cohesion: 0.50
Nodes (3): Migration, MigrationBuilder, InitialSchema

### Community 37 - "Infrastructure DI"
Cohesion: 0.50
Nodes (3): IConfiguration, IServiceCollection, DependencyInjection

### Community 38 - "EF Model Snapshot"
Cohesion: 0.50
Nodes (3): ModelSnapshot, ModelBuilder, AppDbContextModelSnapshot

### Community 39 - "Sales Business Rules (Ledger/Atomic/Credit)"
Cohesion: 0.67
Nodes (3): Atomic sales transaction, Credit sales / installment payments, Stock movement ledger (inventory_movements + stock_levels cache)

## Knowledge Gaps
- **68 isolated node(s):** `PurchaseItemRequest`, `PurchaseItemDto`, `PriceListRowDto`, `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10)` (+63 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **8 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NsStore.Domain.Enums` connect `Domain Entities & Core Services Hub` to `Auth & JWT Tokens`, `Sale Service & DTOs`, `Order Service & Policies`, `Transaction EF Configurations`, `Client Service & DTOs`, `Domain Enums & Purchase Entity`?**
  _High betweenness centrality (0.098) - this node is a cross-community bridge._
- **Why does `NsStore.Domain.Entities` connect `Domain Entities & Core Services Hub` to `Catalog & Inventory Services`, `Auth & JWT Tokens`, `Catalog Entities & EF Configurations`, `Transaction EF Configurations`, `Current User & Identity/Refresh Tokens`, `Program Bootstrap & Infrastructure Wiring`, `Inventory/Product Entities & EF Configs`, `IAppDbContext & Sale Entity`?**
  _High betweenness centrality (0.091) - this node is a cross-community bridge._
- **Why does `TestHarness` connect `Client Service & DTOs` to `Stock Lock & Purchase/Sale Tests`, `Auth & JWT Tokens`, `Sale Service & DTOs`, `Domain Entities & Core Services Hub`, `Purchase Service & DTOs`, `Product Service & DTOs`, `FluentValidation Validators`, `Program Bootstrap & Infrastructure Wiring`, `AppDbContext & Design-Time Factory`?**
  _High betweenness centrality (0.091) - this node is a cross-community bridge._
- **What connects `PurchaseItemRequest`, `PurchaseItemDto`, `PriceListRowDto` to the rest of the system?**
  _68 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Catalog & Inventory Services` be split into smaller, more focused modules?**
  _Cohesion score 0.14775510204081632 - nodes in this community are weakly interconnected._
- **Should `Project & NuGet Config` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._
- **Should `Stock Lock & Purchase/Sale Tests` be split into smaller, more focused modules?**
  _Cohesion score 0.1111111111111111 - nodes in this community are weakly interconnected._