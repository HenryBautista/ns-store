using NsStore.Application.Common;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Clients;
using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

/// <summary>
/// Covers what the point of sale needs from the client list: finding a person by document number
/// in the same free-text box used for names, and keeping the captured data coherent per type.
/// </summary>
public class ClientServiceTests
{
    private static ClientRequest Person(
        string name = "Maria",
        string? lastName = "Quispe",
        string? ci = null,
        string? nit = null,
        string? city = null,
        string? address = null,
        string? contactName = null) =>
        new(ClientType.Individual, name, lastName, null, ci, nit, null, null, city, address, contactName);

    private static ClientRequest Company(string name = "Comercial Sur", string? nit = null, string? ci = null) =>
        new(ClientType.Company, name, null, null, ci, nit, null, null, null, null, "Ana Contacto");

    private static Task<PagedResult<ClientDto>> SearchAsync(TestHarness harness, string term) =>
        harness.Clients.ListAsync(new PageRequest(term));

    [Fact]
    public async Task A_person_is_found_by_ci()
    {
        using var harness = new TestHarness();
        await harness.Clients.CreateAsync(Person(ci: "8123456"));

        var found = Assert.Single((await SearchAsync(harness, "8123456")).Items);

        Assert.Equal("8123456", found.Ci);
    }

    [Fact]
    public async Task A_company_is_found_by_nit()
    {
        using var harness = new TestHarness();
        await harness.Clients.CreateAsync(Company(nit: "1029384756"));

        var found = Assert.Single((await SearchAsync(harness, "1029384756")).Items);

        Assert.Equal("Comercial Sur", found.FullName);
    }

    [Fact]
    public async Task Searching_by_name_still_works()
    {
        using var harness = new TestHarness();

        // The harness seeds Juan Perez.
        Assert.Single((await SearchAsync(harness, "juan")).Items);
        Assert.Single((await SearchAsync(harness, "perez")).Items);
        Assert.Empty((await SearchAsync(harness, "nobody")).Items);
    }

    [Fact]
    public async Task An_individual_keeps_its_city_and_address()
    {
        using var harness = new TestHarness();

        var created = await harness.Clients.CreateAsync(
            Person(city: "Cochabamba", address: "Av. America 123"));

        var stored = await harness.Clients.GetAsync(created.Id);

        Assert.Equal("Cochabamba", stored.City);
        Assert.Equal("Av. America 123", stored.Address);
    }

    [Fact]
    public async Task Identity_fields_stay_exclusive_per_type()
    {
        using var harness = new TestHarness();

        var person = await harness.Clients.CreateAsync(Person(ci: "9111222", contactName: "Nobody"));
        var company = await harness.Clients.CreateAsync(Company(ci: "9333444", nit: "555666"));

        Assert.Null(person.ContactName);
        Assert.Equal("9111222", person.Ci);
        Assert.Null(company.Ci);
        Assert.Equal("555666", company.Nit);
    }

    [Fact]
    public async Task A_repeated_ci_is_rejected()
    {
        using var harness = new TestHarness();
        await harness.Clients.CreateAsync(Person(ci: "8123456"));

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => harness.Clients.CreateAsync(Person(name: "Otro", ci: "8123456")));

        Assert.Equal(ErrorCodes.DuplicateCi, error.ErrorCode);
    }

    [Fact]
    public async Task Clients_without_a_ci_do_not_collide()
    {
        using var harness = new TestHarness();

        await harness.Clients.CreateAsync(Person(name: "Uno"));
        await harness.Clients.CreateAsync(Person(name: "Dos"));

        Assert.Equal(3, (await harness.Clients.ListAsync(new PageRequest())).Total); // + the seeded Juan Perez
    }

    [Fact]
    public async Task A_company_nit_may_repeat_a_person_ci()
    {
        using var harness = new TestHarness();
        await harness.Clients.CreateAsync(Person(ci: "8123456"));

        var company = await harness.Clients.CreateAsync(Company(nit: "8123456"));

        Assert.Equal("8123456", company.Nit);
    }

    [Fact]
    public async Task Keeping_its_own_ci_on_update_is_not_a_conflict()
    {
        using var harness = new TestHarness();
        var created = await harness.Clients.CreateAsync(Person(ci: "8123456"));

        var updated = await harness.Clients.UpdateAsync(
            created.Id,
            Person(name: "Maria Elena", ci: "8123456", city: "Sacaba"));

        Assert.Equal("8123456", updated.Ci);
        Assert.Equal("Sacaba", updated.City);
    }

    [Fact]
    public async Task Taking_the_ci_of_another_client_on_update_is_rejected()
    {
        using var harness = new TestHarness();
        await harness.Clients.CreateAsync(Person(name: "Uno", ci: "8123456"));
        var second = await harness.Clients.CreateAsync(Person(name: "Dos", ci: "9876543"));

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => harness.Clients.UpdateAsync(second.Id, Person(name: "Dos", ci: "8123456")));

        Assert.Equal(ErrorCodes.DuplicateCi, error.ErrorCode);
    }

    [Fact]
    public async Task A_deleted_client_releases_its_ci()
    {
        using var harness = new TestHarness();
        var created = await harness.Clients.CreateAsync(Person(ci: "8123456"));

        await harness.Clients.DeleteAsync(created.Id);
        var reused = await harness.Clients.CreateAsync(Person(name: "Nueva", ci: "8123456"));

        Assert.Equal("8123456", reused.Ci);
    }
}
