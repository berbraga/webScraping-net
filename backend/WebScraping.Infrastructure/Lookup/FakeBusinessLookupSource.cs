using WebScraping.Domain.Abstractions;

namespace WebScraping.Infrastructure.Lookup;

public sealed class FakeBusinessLookupSource : IBusinessLookupSource
{
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
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(query, "__empty__", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IReadOnlyList<BusinessListing>>(Array.Empty<BusinessListing>());
        }

        var take = Math.Max(maxResults, 0);
        IReadOnlyList<BusinessListing> results = _listings.Take(take).ToList();
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
