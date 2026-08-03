using NsStore.Application.Common;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Clients;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

/// <summary>
/// The counter types without accents. These run against the same predicate production runs: the
/// harness registers a SQLite <c>unaccent</c> function so the query is not rewritten for the tests.
/// </summary>
public class AccentInsensitiveSearchTests
{
    /// <summary>
    /// The fold is a hand-written table, not Unicode normalization — the solution builds with
    /// InvariantGlobalization, where <c>String.Normalize</c> quietly returns its input. This pins
    /// the letters Spanish actually uses so a slipped table entry fails here rather than in a
    /// search that silently returns nothing.
    /// </summary>
    [Theory]
    [InlineData("Teléfono", "telefono")]
    [InlineData("NÚÑEZ", "nunez")]
    [InlineData("Peña", "pena")]
    [InlineData("áéíóúü", "aeiouu")]
    [InlineData("ÁÉÍÓÚÜ", "aeiouu")]
    [InlineData("SSD 1TB", "ssd 1tb")]
    public void A_term_folds_to_lower_case_ascii(string typed, string expected)
    {
        Assert.Equal(expected, SearchText.Normalize(typed));
        Assert.Equal($"%{expected}%", new PageRequest(typed).SearchPattern);
    }

    [Fact]
    public async Task A_product_with_an_accent_is_found_without_typing_one()
    {
        using var harness = new TestHarness();
        await harness.CreateProductAsync("Teléfono Inalámbrico");

        var found = Assert.Single((await harness.Products.ListAsync(new PageRequest("telefono"))).Items);

        Assert.Equal("Teléfono Inalámbrico", found.Name);
    }

    [Fact]
    public async Task Typing_the_accent_still_finds_the_product()
    {
        using var harness = new TestHarness();
        await harness.CreateProductAsync("Teléfono Inalámbrico");

        var found = Assert.Single((await harness.Products.ListAsync(new PageRequest("Teléfono"))).Items);

        Assert.Equal("Teléfono Inalámbrico", found.Name);
    }

    [Fact]
    public async Task An_accented_term_finds_a_product_stored_without_one()
    {
        using var harness = new TestHarness();
        await harness.CreateProductAsync("Cable HDMI Premium");

        var found = Assert.Single((await harness.Products.ListAsync(new PageRequest("prémium"))).Items);

        Assert.Equal("Cable HDMI Premium", found.Name);
    }

    /// <summary>The eñe folds too — Bolivian surnames are the reason this ticket exists.</summary>
    [Fact]
    public async Task A_client_surname_is_found_without_its_tilde_or_its_ene()
    {
        using var harness = new TestHarness();
        await harness.Clients.CreateAsync(new ClientRequest(
            ClientType.Individual, "María", "Núñez", null, null, null, null, null, null, null, null));

        var found = Assert.Single((await harness.Clients.ListAsync(new PageRequest("nunez"))).Items);

        Assert.Equal("Núñez", found.LastName);
    }

    [Fact]
    public async Task Folding_does_not_widen_a_search_into_unrelated_rows()
    {
        using var harness = new TestHarness();
        await harness.CreateProductAsync("Teléfono Inalámbrico");
        await harness.CreateProductAsync("Monitor 24");

        var page = await harness.Products.ListAsync(new PageRequest("monitor"));

        var found = Assert.Single(page.Items);
        Assert.Equal("Monitor 24", found.Name);
    }
}
