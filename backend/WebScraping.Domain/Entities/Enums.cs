namespace WebScraping.Domain.Entities;

public enum SearchStatus
{
    Pending,
    Running,
    Completed,
    Cancelled,
    Failed
}

public enum DetailStatus
{
    Pending,
    Enriched,
    Failed,
    Skipped
}
