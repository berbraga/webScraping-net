using FluentAssertions;
using WebScraping.Domain.Entities;

namespace WebScraping.Domain.Tests;

public class SearchValidationTests
{
    [Fact]
    public void Create_rejects_empty_region()
    {
        var act = () => Search.Create("  ", "padarias", 10, DateTime.UtcNow);
        act.Should().Throw<ArgumentException>().WithParameterName("region");
    }

    [Fact]
    public void Create_rejects_empty_query()
    {
        var act = () => Search.Create("São Paulo", " ", 10, DateTime.UtcNow);
        act.Should().Throw<ArgumentException>().WithParameterName("query");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(201)]
    public void EnsureMaxResults_rejects_out_of_bounds(int maxResults)
    {
        var act = () => Search.EnsureMaxResults(maxResults);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_trims_values_and_sets_pending()
    {
        var search = Search.Create("  Centro  ", " padarias ", 25, DateTime.UtcNow);
        search.Region.Should().Be("Centro");
        search.Query.Should().Be("padarias");
        search.MaxResults.Should().Be(25);
        search.Status.Should().Be(SearchStatus.Pending);
    }
}
