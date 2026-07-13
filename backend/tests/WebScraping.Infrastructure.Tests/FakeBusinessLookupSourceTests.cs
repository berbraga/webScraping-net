using FluentAssertions;
using WebScraping.Application.Options;
using WebScraping.Domain.Abstractions;
using WebScraping.Infrastructure.Lookup;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace WebScraping.Infrastructure.Tests;

public class FakeBusinessLookupSourceTests
{
    [Fact]
    public async Task FakeBusinessLookupSource_caps_each_call_at_60()
    {
        var listings = Enumerable.Range(1, 150)
            .Select(i => new BusinessListing($"Biz {i}", $"place-{i}"))
            .ToList();
        var fake = FakeBusinessLookupSource.WithData(listings);

        var results = await fake.SearchAsync("Centro", "padarias", 100);

        results.Should().HaveCount(60);
    }

    [Fact]
    public async Task FakeBusinessLookupSource_slice_windows_return_distinct_pages()
    {
        var listings = Enumerable.Range(1, 150)
            .Select(i => new BusinessListing($"Biz {i}", $"place-{i}"))
            .ToList();
        var fake = FakeBusinessLookupSource.WithData(listings);

        var page0 = await fake.SearchAsync("Centro", "padarias", 60, coverageSliceIndex: 0);
        var page1 = await fake.SearchAsync("Centro", "padarias", 60, coverageSliceIndex: 1);

        page0.Should().HaveCount(60);
        page1.Should().HaveCount(60);
        page0[0].ExternalId.Should().Be("place-1");
        page1[0].ExternalId.Should().Be("place-61");
    }
}

public class TextCoveragePlannerTests
{
    [Fact]
    public void Build_includes_base_and_configured_sectors()
    {
        var planner = new TextCoveragePlanner(MsOptions.Create(new SearchOptions
        {
            CoverageSectorSuffixes = ["norte", "sul"]
        }));

        var slices = planner.Build("Florianopolis", "restaurante");

        slices.Should().HaveCount(3);
        slices[0].Index.Should().Be(0);
        slices[0].EffectiveRegion.Should().Be("Florianopolis");
        slices[0].Label.Should().Be("base");
        slices[1].EffectiveRegion.Should().Be("norte, Florianopolis");
        slices[1].EffectiveQuery.Should().Be("restaurante");
        slices[2].EffectiveRegion.Should().Be("sul, Florianopolis");
    }
}
