using Microsoft.Extensions.Options;
using WebScraping.Application.Options;
using WebScraping.Infrastructure.Lookup;

namespace WebScraping.Infrastructure.Tests.Support;

public static class GooglePlacesTestFactory
{
    public static (GooglePlacesBusinessLookupSource Source, FakeHttpMessageHandler Handler) Create(
        string apiKey = "test-key")
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://places.googleapis.com/")
        };
        var options = Options.Create(new GooglePlacesOptions
        {
            ApiKey = apiKey,
            UseFakeSource = false
        });
        var source = new GooglePlacesBusinessLookupSource(httpClient, options);
        return (source, handler);
    }
}
