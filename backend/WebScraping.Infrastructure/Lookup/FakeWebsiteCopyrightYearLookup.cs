using System.Collections.Concurrent;
using WebScraping.Domain.Abstractions;

namespace WebScraping.Infrastructure.Lookup;

public sealed class FakeWebsiteCopyrightYearLookup : IWebsiteCopyrightYearLookup
{
    private readonly Dictionary<string, int?> _years;
    private readonly ConcurrentDictionary<string, byte> _called = new(StringComparer.OrdinalIgnoreCase);

    public FakeWebsiteCopyrightYearLookup()
        : this(new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase)
        {
            ["https://padariacentral.example"] = 2016,
            ["https://cafeesquina.example"] = 2018
        })
    {
    }

    public FakeWebsiteCopyrightYearLookup(IDictionary<string, int?> years)
    {
        _years = new Dictionary<string, int?>(years, StringComparer.OrdinalIgnoreCase);
    }

    public int CallCount { get; private set; }

    public IReadOnlyCollection<string> CalledUrls => _called.Keys.ToList();

    public Func<string, Task<int?>>? Behavior { get; set; }

    public async Task<int?> GetYearAsync(string websiteUrl, CancellationToken cancellationToken = default)
    {
        CallCount++;
        _called.TryAdd(websiteUrl, 0);

        if (Behavior is not null)
        {
            return await Behavior(websiteUrl);
        }

        cancellationToken.ThrowIfCancellationRequested();

        foreach (var (key, value) in _years)
        {
            if (websiteUrl.Contains(key, StringComparison.OrdinalIgnoreCase)
                || key.Contains(websiteUrl, StringComparison.OrdinalIgnoreCase)
                || string.Equals(Normalize(key), Normalize(websiteUrl), StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return null;
    }

    private static string Normalize(string url) => url.Trim().TrimEnd('/').ToLowerInvariant();
}
