using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NsStore.Domain.Enums;

namespace NsStore.Infrastructure.Persistence;

/// <summary>
/// Used only by <c>dotnet ef</c>. The connection string comes from <c>NSSTORE_CONNECTION</c>;
/// the fallback points at the local Docker Compose database and is never used at runtime.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("NSSTORE_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=nsstore;Username=nsstore;Password=nsstore";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MapEnum<UserRole>("user_role");
                npgsql.MapEnum<ClientType>("client_type");
                npgsql.MapEnum<InvoiceType>("invoice_type");
                npgsql.MapEnum<PaymentStatus>("payment_status");
                npgsql.MapEnum<OrderStatus>("order_status");
                npgsql.MapEnum<MovementType>("movement_type");
            })
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options);
    }
}
