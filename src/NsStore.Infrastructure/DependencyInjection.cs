using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Enums;
using NsStore.Infrastructure.Persistence;
using NsStore.Infrastructure.Security;

namespace NsStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<AuditInterceptor>();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured");

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    // C# enums as native PostgreSQL enum types (values as snake_case).
                    npgsql.MapEnum<UserRole>("user_role");
                    npgsql.MapEnum<ClientType>("client_type");
                    npgsql.MapEnum<InvoiceType>("invoice_type");
                    npgsql.MapEnum<PaymentStatus>("payment_status");
                    npgsql.MapEnum<OrderStatus>("order_status");
                    npgsql.MapEnum<MovementType>("movement_type");
                    npgsql.MapEnum<ProductSerialStatus>("product_serial_status");
                    npgsql.MapEnum<SerialEventType>("serial_event_type");
                    npgsql.EnableRetryOnFailure(3);
                })
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IStockLockService, StockLockService>();
        services.AddScoped<IDocumentNumberService, DocumentNumberService>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<DemoDataSeeder>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
