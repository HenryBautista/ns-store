using NsStore.Api.Middleware;
using NsStore.Api.Security;
using NsStore.Application.Features.Orders;
using NsStore.Application.Features.Quotes;
using NsStore.Application.Features.Reports;
using NsStore.Application.Features.Settings;

namespace NsStore.Api.Endpoints;

public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/settings").WithTags("Settings");

        group.MapGet("/", async (SettingsService settings, CancellationToken ct) =>
                Results.Ok(await settings.GetAsync(ct)))
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapPut("/", async (UpdateSettingsRequest request, SettingsService settings, CancellationToken ct) =>
                Results.Ok(await settings.UpdateAsync(request, ct)))
            .RequireAuthorization(AuthPolicies.AdminOnly)
            .WithValidation<UpdateSettingsRequest>();

        return app;
    }
}

public static class ReportEndpoints
{
    /// <summary>
    /// Reports expose the structured data each printable view needs; rendering happens in the SPA.
    /// </summary>
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reports")
            .WithTags("Reports")
            .RequireAuthorization(AuthPolicies.Authenticated);

        // branchId is honoured for an admin and ignored for a seller, who is always pinned to
        // their own branch — see BranchScope.ResolveScopedBranch.
        group.MapGet("/dashboard", async (long? branchId, ReportService reports, CancellationToken ct) =>
            Results.Ok(await reports.GetDashboardAsync(branchId, ct)));

        group.MapGet("/sales", async (DateOnly? from, DateOnly? to, string? search, long? branchId, ReportService reports, CancellationToken ct) =>
            Results.Ok(await reports.GetSalesReportAsync(new ReportRange(from, to), search, branchId, ct)));

        group.MapGet("/purchases", async (DateOnly? from, DateOnly? to, string? search, long? branchId, ReportService reports, CancellationToken ct) =>
            Results.Ok(await reports.GetPurchasesReportAsync(new ReportRange(from, to), search, branchId, ct)));

        group.MapGet("/stock", async (string? search, long? branchId, ReportService reports, CancellationToken ct) =>
            Results.Ok(await reports.GetStockReportAsync(search, branchId, ct)));

        group.MapGet("/debts", async (string? search, long? branchId, ReportService reports, CancellationToken ct) =>
            Results.Ok(await reports.GetDebtsReportAsync(search, branchId, ct)));

        group.MapGet("/price-list", async (string? search, long? branchId, ReportService reports, CancellationToken ct) =>
            Results.Ok(await reports.GetPriceListAsync(search, branchId, ct)));

        group.MapGet("/sale-invoice/{saleId:long}", async (long saleId, ReportService reports, CancellationToken ct) =>
                Results.Ok(await reports.GetWarrantyNoteAsync(saleId, ct)))
            .WithSummary("Warranty note data for a sale (standard or credit variant)");

        group.MapGet("/client-statement/{clientId:long}", async (long clientId, ReportService reports, CancellationToken ct) =>
                Results.Ok(await reports.GetClientStatementAsync(clientId, ct)))
            .WithSummary("Statement of account for one client: unpaid sales and the instalments credited to them");

        group.MapGet("/order/{id:long}", async (long id, OrderService orders, CancellationToken ct) =>
            Results.Ok(await orders.GetAsync(id, ct)));

        group.MapGet("/quote/{id:long}", async (long id, QuoteService quotes, CancellationToken ct) =>
            Results.Ok(await quotes.GetAsync(id, ct)));

        return app;
    }
}
