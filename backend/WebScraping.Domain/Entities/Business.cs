namespace WebScraping.Domain.Entities;

public sealed class Business
{
    public string Id { get; set; } = string.Empty;
    public string SearchId { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public double? Rating { get; set; }
    public DetailStatus DetailStatus { get; set; } = DetailStatus.Pending;
    public string? DetailError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static Business CreateDiscovered(
        string searchId,
        string name,
        string? externalId,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(searchId))
        {
            throw new ArgumentException("searchId is required.", nameof(searchId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("name is required.", nameof(name));
        }

        return new Business
        {
            Id = Guid.NewGuid().ToString("N"),
            SearchId = searchId,
            ExternalId = string.IsNullOrWhiteSpace(externalId) ? null : externalId.Trim(),
            Name = name.Trim(),
            DetailStatus = DetailStatus.Pending,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
}
