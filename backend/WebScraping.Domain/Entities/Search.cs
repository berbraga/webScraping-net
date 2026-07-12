namespace WebScraping.Domain.Entities;

public sealed class Search
{
    public string Id { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int MaxResults { get; set; }
    public SearchStatus Status { get; set; } = SearchStatus.Pending;
    public int TotalFound { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public static Search Create(string region, string query, int maxResults, DateTime utcNow)
    {
        var normalizedRegion = NormalizeRequired(region, nameof(region));
        var normalizedQuery = NormalizeRequired(query, nameof(query));
        EnsureMaxResults(maxResults);

        return new Search
        {
            Id = Guid.NewGuid().ToString("N"),
            Region = normalizedRegion,
            Query = normalizedQuery,
            MaxResults = maxResults,
            Status = SearchStatus.Pending,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} is required.", fieldName);
        }

        return value.Trim();
    }

    public static void EnsureMaxResults(int maxResults, int absoluteMax = 200)
    {
        if (maxResults <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxResults), "maxResults must be greater than zero.");
        }

        if (maxResults > absoluteMax)
        {
            throw new ArgumentOutOfRangeException(nameof(maxResults), $"maxResults cannot exceed {absoluteMax}.");
        }
    }
}
