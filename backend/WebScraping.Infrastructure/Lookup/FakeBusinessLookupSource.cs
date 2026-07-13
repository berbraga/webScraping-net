using WebScraping.Domain.Abstractions;

namespace WebScraping.Infrastructure.Lookup;

public sealed class FakeBusinessLookupSource : IBusinessLookupSource
{
    public const int ProviderPageCap = 60;
    private const int DefaultCatalogSize = 200;

    private readonly List<BusinessListing> _listings;
    private readonly Dictionary<string, BusinessDetails> _details;

    public FakeBusinessLookupSource()
    {
        _listings = BuildDefaultCatalog();
        _details = new Dictionary<string, BusinessDetails>(StringComparer.OrdinalIgnoreCase)
        {
            ["place-1"] = new BusinessDetails("+55 11 3000-0001", "https://padariacentral.example", 4.5),
            ["place-2"] = new BusinessDetails(null, "https://cafeesquina.example", 4.2),
            ["place-3"] = new BusinessDetails("+55 11 3000-0003", null, null)
        };
    }

    public static FakeBusinessLookupSource WithData(
        IEnumerable<BusinessListing> listings,
        IDictionary<string, BusinessDetails>? details = null)
    {
        return new FakeBusinessLookupSource(listings, details);
    }

    private FakeBusinessLookupSource(
        IEnumerable<BusinessListing> listings,
        IDictionary<string, BusinessDetails>? details)
    {
        _listings = listings.ToList();
        _details = details is null
            ? new Dictionary<string, BusinessDetails>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, BusinessDetails>(details, StringComparer.OrdinalIgnoreCase);
    }

    public Task<IReadOnlyList<BusinessListing>> SearchAsync(
        string region,
        string query,
        int maxResults,
        CancellationToken cancellationToken = default) =>
        SearchAsync(region, query, maxResults, coverageSliceIndex: 0, cancellationToken);

    public Task<IReadOnlyList<BusinessListing>> SearchAsync(
        string region,
        string query,
        int maxResults,
        int coverageSliceIndex,
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(query, "__empty__", StringComparison.OrdinalIgnoreCase)
            || string.Equals(region, "__empty__", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IReadOnlyList<BusinessListing>>(Array.Empty<BusinessListing>());
        }

        if (string.Equals(query, "__fail_slice_1__", StringComparison.OrdinalIgnoreCase)
            && coverageSliceIndex >= 1)
        {
            throw new InvalidOperationException("Simulated provider failure on coverage slice.");
        }

        var take = Math.Min(Math.Max(maxResults, 0), ProviderPageCap);
        var skip = Math.Max(coverageSliceIndex, 0) * ProviderPageCap;
        IReadOnlyList<BusinessListing> results = _listings.Skip(skip).Take(take).ToList();
        return Task.FromResult(results);
    }

    public Task<BusinessDetails> GetDetailsAsync(string externalId, CancellationToken cancellationToken = default)
    {
        if (_details.TryGetValue(externalId, out var details))
        {
            return Task.FromResult(details);
        }

        return Task.FromResult(new BusinessDetails(null, null, null));
    }

    private static List<BusinessListing> BuildDefaultCatalog()
    {
        var listings = new List<BusinessListing>(DefaultCatalogSize)
        {
            new("Padaria Central", "place-1"),
            new("Café da Esquina", "place-2"),
            new("Mercado Bom Preço", "place-3")
        };

        for (var i = 4; i <= DefaultCatalogSize; i++)
        {
            listings.Add(new BusinessListing($"Comércio Fake {i}", $"place-{i}"));
        }

        return listings;
    }
}
