using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NsStore.Application.Features.Auth;
using NsStore.Application.Features.Branches;
using NsStore.Application.Features.Catalogs;
using NsStore.Application.Features.Clients;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Orders;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Quotes;
using NsStore.Application.Features.Reports;
using NsStore.Application.Features.Sales;
using NsStore.Application.Features.Settings;
using NsStore.Application.Features.Users;

namespace NsStore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AuthService>(ServiceLifetime.Singleton);

        services.AddScoped<AuthService>();
        services.AddScoped<BranchService>();
        services.AddScoped<UserService>();
        services.AddScoped<TrademarkService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<WarrantyTermService>();
        services.AddScoped<SupplierService>();
        services.AddScoped<ProductService>();
        services.AddScoped<InventoryService>();
        services.AddScoped<TransferService>();
        services.AddScoped<ClientService>();
        services.AddScoped<PurchaseService>();
        services.AddScoped<SaleService>();
        services.AddScoped<OrderService>();
        services.AddScoped<QuoteService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<ReportService>();

        return services;
    }
}
