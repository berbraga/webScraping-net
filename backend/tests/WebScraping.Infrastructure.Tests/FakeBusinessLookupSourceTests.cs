using FluentAssertions;
using WebScraping.Domain.Abstractions;
using WebScraping.Infrastructure.Lookup;

namespace WebScraping.Infrastructure.Tests;

public class FakeBusinessLookupSourceTests
{
    [Fact]
    public async Task FakeBusinessLookupSource_respects_maxResults_100()
    {
        var listings = Enumerable.Range(1, 150)
            .Select(i => new BusinessListing($"Biz {i}", $"place-{i}"))
            .ToList();
        var fake = FakeBusinessLookupSource.WithData(listings);

        var results = await fake.SearchAsync("Centro", "padarias", 100);

        results.Should().HaveCount(100);
    }
}
