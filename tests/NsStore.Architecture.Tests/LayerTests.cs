using System.Reflection;

namespace NsStore.Architecture.Tests;

/// <summary>
/// The dependency rule, enforced instead of merely written down: Api → Application → Domain, and
/// Infrastructure → Application/Domain. Nothing points back up.
/// </summary>
public class LayerTests
{
    private static readonly Assembly Domain = typeof(NsStore.Domain.Common.ErrorCodes).Assembly;
    private static readonly Assembly Application = typeof(NsStore.Application.DependencyInjection).Assembly;
    private static readonly Assembly Infrastructure = typeof(NsStore.Infrastructure.Persistence.AppDbContext).Assembly;

    [Fact]
    public void Domain_depends_on_nothing_of_ours()
    {
        var violations = OwnReferencesOf(Domain);

        Assert.True(
            violations.Length == 0,
            $"""
             NsStore.Domain references {string.Join(", ", violations)}.
             The domain is the bottom of the stack: entities, enums, ErrorCodes and the invariants
             that throw DomainRuleException, with no dependency on anything of ours and no
             infrastructure. If an entity needs data it cannot reach, the use case belongs in
             Application, not in the entity. See .agent/decisions.md#d-03.
             """);
    }

    [Fact]
    public void Application_depends_only_on_Domain()
    {
        var violations = OwnReferencesOf(Application)
            .Where(name => name != Domain.GetName().Name)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"""
             NsStore.Application references {string.Join(", ", violations)}.
             Application depends only on Domain and on the ports in Common/Interfaces
             (IAppDbContext, ICurrentUser, IPasswordHasher, ITokenService, IStockLockService,
             TimeProvider). Reaching for a concrete type from Infrastructure means the port is
             missing — add it to Common/Interfaces and implement it there.
             See .agent/decisions.md#d-03.
             """);
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_Api()
    {
        var violations = OwnReferencesOf(Infrastructure)
            .Where(name => name.StartsWith("NsStore.Api", StringComparison.Ordinal))
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"""
             NsStore.Infrastructure references {string.Join(", ", violations)}.
             Api is the composition root and is allowed to know Infrastructure; the reverse turns
             the dependency graph into a cycle and makes the persistence layer unusable outside a
             web host.
             """);
    }

    /// <summary>
    /// Only our own assemblies. The compiler drops references an assembly never actually uses, so
    /// this reflects real coupling rather than what the csproj happens to list.
    /// </summary>
    private static string[] OwnReferencesOf(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .OfType<string>()
            .Where(name => name.StartsWith("NsStore.", StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
}
