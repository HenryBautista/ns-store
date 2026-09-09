using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NsStore.Application;

namespace NsStore.Application.Tests;

/// <summary>
/// A service that is registered but absent from <see cref="TestHarness"/> gets hand-wired in
/// whichever test needs it first, and from then on every test wires it slightly differently.
/// This keeps the harness honest as services are added.
/// </summary>
public class HarnessCoverageTests
{
    /// <summary>
    /// Services registered before this test existed and never added to the harness. The point is
    /// not to fix them here — it is that nothing *new* joins the list. Removing a name from this
    /// set (by adding the service to TestHarness) is always welcome; adding one is the thing to
    /// push back on.
    /// </summary>
    private static readonly HashSet<string> KnownAbsent =
    [
        "AuthService",
        "CategoryService",
        "OrderService",
        "QuoteService",
        "SupplierService",
        "TrademarkService",
        "UserService",
        "WarrantyTermService",
    ];

    [Fact]
    public void TestHarness_exposes_every_registered_service_except_the_known_baseline()
    {
        var registered = new ServiceCollection().AddApplication()
            .Select(descriptor => descriptor.ServiceType)
            .Where(type => type.Namespace?.StartsWith("NsStore.Application.Features", StringComparison.Ordinal) == true)
            .Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal))
            .ToArray();

        var exposed = typeof(TestHarness)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.PropertyType)
            .ToHashSet();

        var missing = registered
            .Where(type => !exposed.Contains(type))
            .Select(type => type.Name)
            .Where(name => !KnownAbsent.Contains(name))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            missing.Length == 0,
            $"""
             Registered services missing from TestHarness:
             {string.Join(Environment.NewLine, missing.Select(name => "  " + name))}

             Add a property to TestHarness that builds the service with the harness's own
             AppDbContext, FakeCurrentUser, FakeTimeProvider and NoOpStockLock, instead of
             hand-wiring it inside a test. Otherwise the next test wires it differently and the
             two disagree about what "the real service" means.
             """);

        var resurrected = KnownAbsent
            .Where(name => exposed.Any(type => type.Name == name))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            resurrected.Length == 0,
            $"""
             These are in TestHarness now but still listed as known-absent:
             {string.Join(Environment.NewLine, resurrected.Select(name => "  " + name))}

             Delete them from KnownAbsent in this file. A baseline that outlives the gap it
             describes stops meaning anything.
             """);
    }
}
