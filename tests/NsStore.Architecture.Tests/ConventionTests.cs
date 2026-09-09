using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NsStore.Application;

namespace NsStore.Architecture.Tests;

/// <summary>
/// The conventions the compiler cannot reach. Each of these was a line on a delivery checklist
/// that someone had to remember; here it fails at the moment it is broken instead.
/// </summary>
public class ConventionTests
{
    private static readonly Type[] ApplicationTypes =
        typeof(DependencyInjection).Assembly.GetTypes();

    [Fact]
    public void Every_feature_service_is_registered_in_DependencyInjection()
    {
        var services = new ServiceCollection().AddApplication();
        var registered = services.Select(descriptor => descriptor.ServiceType).ToHashSet();

        var missing = ApplicationTypes
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal))
            .Where(type => type.Namespace?.StartsWith("NsStore.Application.Features", StringComparison.Ordinal) == true)
            .Where(type => !registered.Contains(type))
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            missing.Length == 0,
            $"""
             Not registered in Application/DependencyInjection.cs:
             {string.Join(Environment.NewLine, missing.Select(t => "  " + t.FullName))}

             Services are plain classes wired by hand — no MediatR, no assembly scanning — so a new
             one is invisible until it is added there. Add services.AddScoped<TheService>(), and
             add it to TestHarness too rather than hand-wiring it in each test.
             See .agent/decisions.md#d-03.
             """);
    }

    [Fact]
    public void Every_route_that_declares_validation_has_a_validator_to_run()
    {
        // ValidationFilter resolves IValidator<T> and, when there is none, does nothing at all:
        //     if (validator is not null && argument is not null)
        // So .WithValidation<T>() on a type with no validator is a silent no-op — the route says
        // it validates, and nothing does. That is the failure this test exists to catch.
        //
        // Not "every *Request type has a validator": several are nested line items validated
        // through their parent (SaleItemRequest, PurchaseItemRequest, TransferItemRequest,
        // CollectAllocationRequest), and asserting that would be asserting something untrue.
        var declared = Directory
            .EnumerateFiles(Path.Combine(RepoPath.Root, "src", "NsStore.Api", "Endpoints"), "*.cs")
            .SelectMany(file => Regex.Matches(File.ReadAllText(file), @"WithValidation<(\w+)>"))
            .Select(match => match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        var validated = ApplicationTypes
            .Select(type => type.BaseType)
            .Where(baseType => baseType is { IsGenericType: true }
                && baseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            .Select(baseType => baseType!.GetGenericArguments()[0].Name)
            .ToHashSet(StringComparer.Ordinal);

        var missing = declared.Except(validated).OrderBy(name => name, StringComparer.Ordinal).ToArray();

        Assert.True(
            missing.Length == 0,
            $"""
             Routes declare .WithValidation<T>() for types that have no validator:
             {string.Join(Environment.NewLine, missing.Select(name => "  " + name))}

             ValidationFilter skips silently when no IValidator<T> is registered, so the request
             reaches the service unvalidated and a bad body surfaces as a 500 instead of a 400.
             Write XRequestValidator : AbstractValidator<XRequest> next to the DTO; validators are
             auto-registered from the Application assembly.
             """);

        Assert.NotEmpty(declared); // the regex still matches something; guards against a silent pass
    }

    [Fact]
    public void Every_domain_enum_is_mapped_as_a_native_postgres_type()
    {
        // The Npgsql mappings are not introspectable once the data source is built, so this reads
        // the registration source. Crude, but it catches the exact omission that breaks at runtime.
        var registration = RepoPath.Read("src/NsStore.Infrastructure/DependencyInjection.cs");

        var missing = typeof(NsStore.Domain.Enums.UserRole).Assembly.GetTypes()
            .Where(type => type.IsEnum)
            .Where(type => type.Namespace == "NsStore.Domain.Enums")
            .Where(type => !registration.Contains($"MapEnum<{type.Name}>", StringComparison.Ordinal))
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            missing.Length == 0,
            $"""
             Domain enums with no native PostgreSQL mapping:
             {string.Join(Environment.NewLine, missing.Select(t => "  " + t.Name))}

             Domain enums are native PostgreSQL enum types, so each one needs
             npgsql.MapEnum<TheEnum>("the_enum") in Infrastructure/DependencyInjection.cs *and* a
             migration that creates the type. Without the mapping the column fails to read at
             runtime, not at startup. See .agent/decisions.md#d-04.
             """);
    }
}
