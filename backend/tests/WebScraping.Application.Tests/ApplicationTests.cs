using FluentAssertions;
using WebScraping.Application.Options;
using WebScraping.Application.Searches;
using WebScraping.Domain.Entities;
using WebScraping.Infrastructure.Lookup;
using WebScraping.Infrastructure.Persistence;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace WebScraping.Application.Tests;

public class StartSearchTests
{
    [Fact]
    public async Task StartSearch_persists_discovered_businesses()
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var queue = new InProcessSearchJobQueue();
        var lookup = new FakeBusinessLookupSource();
        var handler = new StartSearchHandler(
            searches,
            businesses,
            lookup,
            queue,
            MsOptions.Create(new SearchOptions { DefaultMaxResults = 50, AbsoluteMaxResults = 200 }));

        var summary = await handler.HandleAsync(new StartSearchRequest("Centro", "padarias", 10));

        summary.TotalFound.Should().Be(10);
        summary.MaxResults.Should().Be(10);
        summary.Status.Should().Be("running");
        var items = await businesses.ListBySearchIdAsync(summary.Id, 0, 10);
        items.Should().HaveCount(10);
        items.Select(x => x.Name).Should().Contain("Padaria Central");
    }

    [Fact]
    public async Task StartSearch_preserves_requested_maxResults_and_totalFound()
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var queue = new InProcessSearchJobQueue();
        var listings = Enumerable.Range(1, 100)
            .Select(i => new WebScraping.Domain.Abstractions.BusinessListing($"Biz {i}", $"p-{i}"))
            .ToList();
        var lookup = FakeBusinessLookupSource.WithData(listings);
        var handler = new StartSearchHandler(
            searches,
            businesses,
            lookup,
            queue,
            MsOptions.Create(new SearchOptions { DefaultMaxResults = 50, AbsoluteMaxResults = 200 }));

        var summary = await handler.HandleAsync(new StartSearchRequest("Centro", "padarias", 80));

        summary.MaxResults.Should().Be(80);
        summary.TotalFound.Should().Be(80);
    }

    [Fact]
    public async Task StartSearch_completes_when_empty()
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var queue = new InProcessSearchJobQueue();
        var lookup = new FakeBusinessLookupSource();
        var handler = new StartSearchHandler(
            searches,
            businesses,
            lookup,
            queue,
            MsOptions.Create(new SearchOptions()));

        var summary = await handler.HandleAsync(new StartSearchRequest("Centro", "__empty__", 10));

        summary.Status.Should().Be("completed");
        summary.TotalFound.Should().Be(0);
    }
}

public class EnrichBusinessesTests
{
    [Fact]
    public async Task Enrich_fills_details_and_allows_nulls()
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var now = DateTime.UtcNow;
        var search = Search.Create("Centro", "padarias", 10, now);
        search.Status = SearchStatus.Running;
        await searches.InsertAsync(search);

        var b1 = Business.CreateDiscovered(search.Id, "Padaria Central", "place-1", now);
        var b2 = Business.CreateDiscovered(search.Id, "Café", "place-2", now);
        var b3 = Business.CreateDiscovered(search.Id, "Mercado", "place-3", now);
        await businesses.InsertManyAsync([b1, b2, b3]);

        var enricher = new EnrichBusinessesHandler(searches, businesses, new FakeBusinessLookupSource());
        await enricher.EnrichSearchAsync(search.Id);

        var items = await businesses.ListBySearchIdAsync(search.Id, 0, 10);
        items.Should().OnlyContain(x => x.DetailStatus == DetailStatus.Enriched);
        items.Single(x => x.ExternalId == "place-1").Phone.Should().Be("+55 11 3000-0001");
        items.Single(x => x.ExternalId == "place-2").Phone.Should().BeNull();
        items.Single(x => x.ExternalId == "place-3").Website.Should().BeNull();
        items.Single(x => x.ExternalId == "place-3").Rating.Should().BeNull();

        var updated = await searches.GetByIdAsync(search.Id);
        updated!.ProcessedCount.Should().Be(3);
        updated.Status.Should().Be(SearchStatus.Completed);
    }
}

public class CancelSearchTests
{
    [Fact]
    public async Task Cancel_marks_remaining_as_skipped()
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var now = DateTime.UtcNow;
        var search = Search.Create("Centro", "padarias", 10, now);
        search.Status = SearchStatus.Running;
        await searches.InsertAsync(search);

        var pending = Business.CreateDiscovered(search.Id, "Pendente", "place-x", now);
        var done = Business.CreateDiscovered(search.Id, "Pronto", "place-1", now);
        done.DetailStatus = DetailStatus.Enriched;
        done.Phone = "123";
        await businesses.InsertManyAsync([pending, done]);

        var handler = new CancelSearchHandler(searches, businesses);
        var summary = await handler.HandleAsync(search.Id);

        summary.Status.Should().Be("cancelled");
        var items = await businesses.ListBySearchIdAsync(search.Id, 0, 10);
        items.Single(x => x.Name == "Pendente").DetailStatus.Should().Be(DetailStatus.Skipped);
        items.Single(x => x.Name == "Pronto").DetailStatus.Should().Be(DetailStatus.Enriched);
    }
}

public class CsvExportTests
{
    [Fact]
    public void BuildCsv_escapes_and_keeps_empty_optional_fields()
    {
        var businesses = new[]
        {
            new Business
            {
                Name = "Padaria \"Central\", SP",
                Phone = null,
                Website = "https://example.com",
                Rating = 4.5
            }
        };

        var csv = ExportSearchCsvHandler.BuildCsv(businesses);
        csv.Should().StartWith("Nome,Telefone,Site,Avaliacao");
        csv.Should().Contain("\"Padaria \"\"Central\"\", SP\"");
        csv.Should().Contain(",,https://example.com,4.5");
    }
}
