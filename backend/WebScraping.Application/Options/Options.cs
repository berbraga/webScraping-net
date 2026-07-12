namespace WebScraping.Application.Options;

public sealed class SearchOptions
{
    public const string SectionName = "Search";

    public int DefaultMaxResults { get; set; } = 50;
    public int AbsoluteMaxResults { get; set; } = 200;
}

public sealed class MongoOptions
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "webscraping";
}

public sealed class GooglePlacesOptions
{
    public const string SectionName = "GooglePlaces";

    public string ApiKey { get; set; } = string.Empty;
    public bool UseFakeSource { get; set; }
}
