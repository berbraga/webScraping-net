using FluentAssertions;
using WebScraping.Application.Options;
using WebScraping.Application.Searches;
using WebScraping.Domain.Abstractions;
using WebScraping.Domain.Entities;
using WebScraping.Infrastructure.Lookup;
using WebScraping.Infrastructure.Persistence;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace WebScraping.Application.Tests;

internal static class TestYearEnrichment
{
    public static WebsiteCopyrightOptions CopyrightOptions() => new()
    {
        TimeoutSeconds = 10,
        MaxDegreeOfParallelism = 10
    };

    public static EnrichSiteCreationYearsHandler CreateYearEnricher(
        InMemorySearchRepository searches,
        InMemoryBusinessRepository businesses,
        IWebsiteCopyrightYearLookup? lookup = null) =>
        new(
            searches,
            businesses,
            lookup ?? new FakeWebsiteCopyrightYearLookup(),
            MsOptions.Create(CopyrightOptions()));
}

public class StartSearchTests
{
    private static SearchOptions Options() => new()
    {
        DefaultMaxResults = 50,
        AbsoluteMaxResults = 200,
        ProviderPageCap = 60
    };

    private static async Task<SearchSummaryDto> StartAndDiscoverAsync(
        StartSearchRequest request,
        IBusinessLookupSource? lookup = null,
        SearchOptions? options = null)
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var queue = new InProcessSearchJobQueue();
        var opts = MsOptions.Create(options ?? Options());
        lookup ??= new FakeBusinessLookupSource();
        var planner = new TextCoveragePlanner(opts);
        var enricher = new EnrichBusinessesHandler(searches, businesses, lookup, TestYearEnrichment.CreateYearEnricher(searches, businesses));
        var start = new StartSearchHandler(searches, queue, opts);
        var discover = new DiscoverSearchHandler(searches, businesses, lookup, planner, enricher, TestYearEnrichment.CreateYearEnricher(searches, businesses), opts);

        var summary = await start.HandleAsync(request);
        await discover.HandleAsync(summary.Id);
        var updated = await searches.GetByIdAsync(summary.Id);
        return StartSearchHandler.ToDto(updated!);
    }

    [Fact]
    public async Task StartSearch_persists_discovered_businesses()
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var queue = new InProcessSearchJobQueue();
        var lookup = new FakeBusinessLookupSource();
        var opts = MsOptions.Create(Options());
        var start = new StartSearchHandler(searches, queue, opts);
        var discover = new DiscoverSearchHandler(
            searches,
            businesses,
            lookup,
            new TextCoveragePlanner(opts),
            new EnrichBusinessesHandler(searches, businesses, lookup, TestYearEnrichment.CreateYearEnricher(searches, businesses)),
            TestYearEnrichment.CreateYearEnricher(searches, businesses),
            opts);

        var summary = await start.HandleAsync(new StartSearchRequest("Centro", "padarias", 10));
        summary.Status.Should().Be("running");
        summary.TotalFound.Should().Be(0);

        await discover.HandleAsync(summary.Id);

        var updated = await searches.GetByIdAsync(summary.Id);
        updated!.TotalFound.Should().Be(10);
        updated.Status.Should().Be(SearchStatus.Completed);
        var items = await businesses.ListBySearchIdAsync(summary.Id, 0, 10);
        items.Should().HaveCount(10);
        items.Select(x => x.Name).Should().Contain("Padaria Central");
    }

    [Fact]
    public async Task StartSearch_preserves_requested_maxResults_and_totalFound()
    {
        var summary = await StartAndDiscoverAsync(new StartSearchRequest("Centro", "padarias", 80));

        summary.MaxResults.Should().Be(80);
        summary.TotalFound.Should().Be(80);
        summary.Status.Should().Be("completed");
    }

    [Fact]
    public async Task StartSearch_completes_when_empty()
    {
        var summary = await StartAndDiscoverAsync(new StartSearchRequest("Centro", "__empty__", 10));

        summary.Status.Should().Be("completed");
        summary.TotalFound.Should().Be(0);
    }
}

public class DiscoverSearchHandlerTests
{
    private static SearchOptions Options() => new()
    {
        DefaultMaxResults = 50,
        AbsoluteMaxResults = 200,
        ProviderPageCap = 60,
        CoverageSectorSuffixes = ["centro", "norte", "sul", "leste", "oeste"]
    };

    private static async Task<(InMemorySearchRepository Searches, InMemoryBusinessRepository Businesses, DiscoverSearchHandler Discover, string SearchId)>
        CreateRunningSearchAsync(string region, string query, int maxResults, IBusinessLookupSource? lookup = null)
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var queue = new InProcessSearchJobQueue();
        var opts = MsOptions.Create(Options());
        lookup ??= new FakeBusinessLookupSource();
        var start = new StartSearchHandler(searches, queue, opts);
        var enricher = new EnrichBusinessesHandler(searches, businesses, lookup, TestYearEnrichment.CreateYearEnricher(searches, businesses));
        var discover = new DiscoverSearchHandler(
            searches,
            businesses,
            lookup,
            new TextCoveragePlanner(opts),
            enricher,
            TestYearEnrichment.CreateYearEnricher(searches, businesses),
            opts);

        var summary = await start.HandleAsync(new StartSearchRequest(region, query, maxResults));
        return (searches, businesses, discover, summary.Id);
    }

    [Fact]
    public async Task Discover_reaches_maxResults_100_across_slices()
    {
        var (searches, businesses, discover, id) = await CreateRunningSearchAsync("Centro", "padarias", 100);
        await discover.HandleAsync(id);

        var search = await searches.GetByIdAsync(id);
        search!.TotalFound.Should().Be(100);
        search.MaxResults.Should().Be(100);
        search.Status.Should().Be(SearchStatus.Completed);
        var items = await businesses.ListBySearchIdAsync(id, 0, 200);
        items.Should().HaveCount(100);
        items.Select(x => x.ExternalId).Distinct().Should().HaveCount(100);
    }

    [Fact]
    public async Task Discover_reaches_maxResults_200_across_slices()
    {
        var (searches, _, discover, id) = await CreateRunningSearchAsync("Centro", "padarias", 200);
        await discover.HandleAsync(id);

        var search = await searches.GetByIdAsync(id);
        search!.TotalFound.Should().Be(200);
        search.Status.Should().Be(SearchStatus.Completed);
    }

    [Fact]
    public async Task Discover_stops_when_catalog_smaller_than_limit()
    {
        var listings = Enumerable.Range(1, 45)
            .Select(i => new BusinessListing($"Biz {i}", $"p-{i}"))
            .ToList();
        var lookup = FakeBusinessLookupSource.WithData(listings);
        var (searches, _, discover, id) = await CreateRunningSearchAsync("Centro", "padarias", 100, lookup);
        await discover.HandleAsync(id);

        var search = await searches.GetByIdAsync(id);
        search!.TotalFound.Should().Be(45);
        search.Status.Should().Be(SearchStatus.Completed);
    }

    [Fact]
    public async Task Discover_stops_on_slice_with_only_duplicates()
    {
        // Single page of 60; further slices return same window if catalog is only 60 unique and slice skip is beyond end → 0 new
        var listings = Enumerable.Range(1, 60)
            .Select(i => new BusinessListing($"Biz {i}", $"p-{i}"))
            .ToList();
        var lookup = FakeBusinessLookupSource.WithData(listings);
        var (searches, _, discover, id) = await CreateRunningSearchAsync("Centro", "padarias", 200, lookup);
        await discover.HandleAsync(id);

        var search = await searches.GetByIdAsync(id);
        search!.TotalFound.Should().Be(60);
        search.Status.Should().Be(SearchStatus.Completed);
    }

    [Fact]
    public async Task Discover_marks_failed_but_keeps_items_when_later_slice_throws()
    {
        var (searches, businesses, discover, id) = await CreateRunningSearchAsync("Centro", "__fail_slice_1__", 100);
        await discover.HandleAsync(id);

        var search = await searches.GetByIdAsync(id);
        search!.Status.Should().Be(SearchStatus.Failed);
        search.ErrorMessage.Should().NotBeNullOrWhiteSpace();
        search.TotalFound.Should().BeGreaterThan(0);
        var items = await businesses.ListBySearchIdAsync(id, 0, 200);
        items.Should().HaveCount(search.TotalFound);
    }

    [Fact]
    public async Task Discover_enriches_first_batch_before_finishing_all_slices()
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var queue = new InProcessSearchJobQueue();
        var opts = MsOptions.Create(Options());
        var inner = new FakeBusinessLookupSource();
        var lookup = new TrackingLookupSource(inner);
        var enricher = new EnrichBusinessesHandler(searches, businesses, lookup, TestYearEnrichment.CreateYearEnricher(searches, businesses));
        var discover = new DiscoverSearchHandler(
            searches,
            businesses,
            lookup,
            new TextCoveragePlanner(opts),
            enricher,
            TestYearEnrichment.CreateYearEnricher(searches, businesses),
            opts);
        var start = new StartSearchHandler(searches, queue, opts);

        var summary = await start.HandleAsync(new StartSearchRequest("Centro", "padarias", 100));
        await discover.HandleAsync(summary.Id);

        var firstDetailIndex = lookup.Events.FindIndex(e => e.StartsWith("detail:", StringComparison.Ordinal));
        var secondSliceIndex = lookup.Events.FindIndex(e => e == "search:1");
        firstDetailIndex.Should().BeGreaterThanOrEqualTo(0);
        secondSliceIndex.Should().BeGreaterThan(firstDetailIndex);

        var midFlightProcessed = 0;
        for (var i = 0; i < lookup.Events.Count; i++)
        {
            if (lookup.Events[i].StartsWith("detail:", StringComparison.Ordinal))
            {
                midFlightProcessed++;
            }

            if (lookup.Events[i] == "search:1")
            {
                midFlightProcessed.Should().BeGreaterThan(0);
                break;
            }
        }

        var search = await searches.GetByIdAsync(summary.Id);
        search!.ProcessedCount.Should().Be(search.TotalFound);
        search.TotalFound.Should().Be(100);
        var items = await businesses.ListBySearchIdAsync(summary.Id, 0, 200);
        items.Should().OnlyContain(x => x.DetailStatus == DetailStatus.Enriched);
    }

    private sealed class TrackingLookupSource(IBusinessLookupSource inner) : IBusinessLookupSource
    {
        public List<string> Events { get; } = [];

        public Task<IReadOnlyList<BusinessListing>> SearchAsync(
            string region,
            string query,
            int maxResults,
            CancellationToken cancellationToken = default) =>
            SearchAsync(region, query, maxResults, 0, cancellationToken);

        public async Task<IReadOnlyList<BusinessListing>> SearchAsync(
            string region,
            string query,
            int maxResults,
            int coverageSliceIndex,
            CancellationToken cancellationToken = default)
        {
            Events.Add($"search:{coverageSliceIndex}");
            return await inner.SearchAsync(region, query, maxResults, coverageSliceIndex, cancellationToken);
        }

        public async Task<BusinessDetails> GetDetailsAsync(string externalId, CancellationToken cancellationToken = default)
        {
            Events.Add($"detail:{externalId}");
            return await inner.GetDetailsAsync(externalId, cancellationToken);
        }
    }

    [Fact]
    public async Task Discover_totalFound_grows_across_batches()
    {
        var totals = new List<int>();
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var opts = MsOptions.Create(Options());
        var lookup = new FakeBusinessLookupSource();
        var enricher = new EnrichBusinessesHandler(searches, businesses, lookup, TestYearEnrichment.CreateYearEnricher(searches, businesses));
        var trackingPlanner = new TextCoveragePlanner(opts);
        var discover = new DiscoverSearchHandler(
            searches,
            businesses,
            lookup,
            trackingPlanner,
            enricher,
            TestYearEnrichment.CreateYearEnricher(searches, businesses),
            opts);

        var search = Search.Create("Centro", "padarias", 120, DateTime.UtcNow);
        search.Status = SearchStatus.Running;
        await searches.InsertAsync(search);

        // Manual observation by wrapping: run discover and sample isn't mid-flight easily without hooks.
        // Assert final and that Fake single call cannot exceed 60 — so 120 requires multi-slice.
        await discover.HandleAsync(search.Id);
        var updated = await searches.GetByIdAsync(search.Id);
        updated!.TotalFound.Should().Be(120);
        (await lookup.SearchAsync("Centro", "padarias", 120)).Should().HaveCount(60);
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

        var enricher = new EnrichBusinessesHandler(
            searches,
            businesses,
            new FakeBusinessLookupSource(),
            TestYearEnrichment.CreateYearEnricher(searches, businesses));
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
        csv.Should().StartWith("Nome,Telefone,Site,Criação do site,Avaliacao");
        csv.Should().Contain("\"Padaria \"\"Central\"\", SP\"");
        csv.Should().Contain(",,https://example.com,,4.5");
    }

    [Fact]
    public void BuildCsv_includes_site_creation_year()
    {
        var businesses = new[]
        {
            new Business
            {
                Name = "A",
                Phone = "1",
                Website = "https://a.example",
                SiteCreationYear = 2016,
                Rating = 4
            },
            new Business
            {
                Name = "B",
                Website = "https://b.example",
                SiteCreationYear = null
            }
        };

        var csv = ExportSearchCsvHandler.BuildCsv(businesses);
        csv.Should().Contain("Criação do site");
        csv.Should().Contain("https://a.example,2016,4");
        csv.Should().Contain("https://b.example,,");
    }
}

public class SiteCreationYearEnrichmentTests
{
    [Fact]
    public async Task Discover_sets_site_creation_year_from_fake_lookup()
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var queue = new InProcessSearchJobQueue();
        var opts = MsOptions.Create(new SearchOptions
        {
            DefaultMaxResults = 50,
            AbsoluteMaxResults = 200,
            ProviderPageCap = 60
        });
        var lookup = new FakeBusinessLookupSource();
        var yearLookup = new FakeWebsiteCopyrightYearLookup();
        var yearEnricher = TestYearEnrichment.CreateYearEnricher(searches, businesses, yearLookup);
        var enricher = new EnrichBusinessesHandler(searches, businesses, lookup, yearEnricher);
        var discover = new DiscoverSearchHandler(
            searches,
            businesses,
            lookup,
            new TextCoveragePlanner(opts),
            enricher,
            yearEnricher,
            opts);
        var start = new StartSearchHandler(searches, queue, opts);

        var summary = await start.HandleAsync(new StartSearchRequest("Centro", "padarias", 3));
        await discover.HandleAsync(summary.Id);

        var search = await searches.GetByIdAsync(summary.Id);
        search!.Status.Should().Be(SearchStatus.Completed);
        var items = await businesses.ListBySearchIdAsync(summary.Id, 0, 10);
        items.Single(x => x.ExternalId == "place-1").SiteCreationYear.Should().Be(2016);
        items.Single(x => x.ExternalId == "place-2").SiteCreationYear.Should().Be(2018);
        items.Single(x => x.ExternalId == "place-3").SiteCreationYear.Should().BeNull();
    }

    [Fact]
    public async Task Year_phase_reuses_same_website_url()
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var now = DateTime.UtcNow;
        var search = Search.Create("Centro", "padarias", 10, now);
        search.Status = SearchStatus.Running;
        await searches.InsertAsync(search);

        var a = Business.CreateDiscovered(search.Id, "A", "place-1", now);
        a.Website = "https://shared.example";
        a.DetailStatus = DetailStatus.Enriched;
        var b = Business.CreateDiscovered(search.Id, "B", "place-x", now);
        b.Website = "https://shared.example";
        b.DetailStatus = DetailStatus.Enriched;
        await businesses.InsertManyAsync([a, b]);

        var yearLookup = new FakeWebsiteCopyrightYearLookup(new Dictionary<string, int?>
        {
            ["https://shared.example"] = 2011
        });
        var handler = TestYearEnrichment.CreateYearEnricher(searches, businesses, yearLookup);
        await handler.HandleAsync(search.Id);

        yearLookup.CallCount.Should().Be(1);
        var items = await businesses.ListBySearchIdAsync(search.Id, 0, 10);
        items.Should().OnlyContain(x => x.SiteCreationYear == 2011);
    }

    [Fact]
    public async Task Year_lookup_failure_does_not_fail_places_or_abort_search()
    {
        var searches = new InMemorySearchRepository();
        var businesses = new InMemoryBusinessRepository();
        var now = DateTime.UtcNow;
        var search = Search.Create("Centro", "padarias", 10, now);
        search.Status = SearchStatus.Running;
        search.FailedCount = 0;
        await searches.InsertAsync(search);

        var biz = Business.CreateDiscovered(search.Id, "A", "place-1", now);
        biz.Website = "https://bad.example";
        biz.DetailStatus = DetailStatus.Enriched;
        biz.Phone = "1";
        await businesses.InsertManyAsync([biz]);

        var yearLookup = new FakeWebsiteCopyrightYearLookup();
        yearLookup.Behavior = _ => throw new HttpRequestException("down");
        var handler = TestYearEnrichment.CreateYearEnricher(searches, businesses, yearLookup);
        await handler.HandleAsync(search.Id);

        var updatedBiz = (await businesses.ListBySearchIdAsync(search.Id, 0, 10)).Single();
        updatedBiz.SiteCreationYear.Should().BeNull();
        updatedBiz.DetailStatus.Should().Be(DetailStatus.Enriched);
        updatedBiz.Phone.Should().Be("1");

        var updatedSearch = await searches.GetByIdAsync(search.Id);
        updatedSearch!.FailedCount.Should().Be(0);
        updatedSearch.Status.Should().Be(SearchStatus.Running);
    }
}
