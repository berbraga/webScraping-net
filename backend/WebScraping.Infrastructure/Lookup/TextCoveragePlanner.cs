using Microsoft.Extensions.Options;
using WebScraping.Application.Options;
using WebScraping.Domain.Abstractions;

namespace WebScraping.Infrastructure.Lookup;

public sealed class TextCoveragePlanner : ITextCoveragePlanner
{
    private readonly SearchOptions _options;

    public TextCoveragePlanner(IOptions<SearchOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyList<CoverageSlice> Build(string region, string query)
    {
        var slices = new List<CoverageSlice>
        {
            new(0, region.Trim(), query.Trim(), "base")
        };

        var sectors = _options.CoverageSectorSuffixes ?? Array.Empty<string>();
        var index = 1;
        foreach (var sector in sectors)
        {
            if (string.IsNullOrWhiteSpace(sector))
            {
                continue;
            }

            var label = sector.Trim();
            slices.Add(new CoverageSlice(
                index,
                $"{label}, {region.Trim()}",
                query.Trim(),
                label));
            index++;
        }

        return slices;
    }
}
