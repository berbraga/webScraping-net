using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using WebScraping.Infrastructure.Tests.Support;

namespace WebScraping.Infrastructure.Tests;

public class GooglePlacesBusinessLookupSourceTests
{
    [Fact]
    public async Task SearchAsync_paginates_until_maxResults()
    {
        var (source, handler) = GooglePlacesTestFactory.Create();
        handler.EnqueueJson(HttpStatusCode.OK, BuildPlacesPage(0, 20, "token-1"));
        handler.EnqueueJson(HttpStatusCode.OK, BuildPlacesPage(20, 20, "token-2"));
        handler.EnqueueJson(HttpStatusCode.OK, BuildPlacesPage(40, 10, null));

        var results = await source.SearchAsync("Florianopolis", "restaurantes", 50);

        results.Should().HaveCount(50);
        handler.Requests.Should().HaveCount(3);

        var secondBody = await handler.Requests[1].Content!.ReadAsStringAsync();
        using var secondJson = JsonDocument.Parse(secondBody);
        secondJson.RootElement.GetProperty("pageToken").GetString().Should().Be("token-1");

        var thirdBody = await handler.Requests[2].Content!.ReadAsStringAsync();
        using var thirdJson = JsonDocument.Parse(thirdBody);
        thirdJson.RootElement.GetProperty("pageToken").GetString().Should().Be("token-2");
    }

    [Fact]
    public async Task SearchAsync_requests_include_nextPageToken_in_field_mask()
    {
        var (source, handler) = GooglePlacesTestFactory.Create();
        handler.EnqueueJson(HttpStatusCode.OK, BuildPlacesPage(0, 5, null));

        await source.SearchAsync("Centro", "padarias", 5);

        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].Headers.TryGetValues("X-Goog-FieldMask", out var masks).Should().BeTrue();
        string.Join(",", masks!).Should().Contain("nextPageToken");
        string.Join(",", masks!).Should().Contain("places.id");
        string.Join(",", masks!).Should().Contain("places.displayName");
    }

    [Fact]
    public async Task SearchAsync_stops_when_provider_has_no_nextPageToken()
    {
        var (source, handler) = GooglePlacesTestFactory.Create();
        handler.EnqueueJson(HttpStatusCode.OK, BuildPlacesPage(0, 20, "token-1"));
        handler.EnqueueJson(HttpStatusCode.OK, BuildPlacesPage(20, 15, null));

        var results = await source.SearchAsync("Florianopolis", "restaurantes", 100);

        results.Should().HaveCount(35);
        handler.Requests.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_returns_empty_when_no_places()
    {
        var (source, handler) = GooglePlacesTestFactory.Create();
        handler.EnqueueJson(HttpStatusCode.OK, """{"places":[]}""");

        var results = await source.SearchAsync("Centro", "xyz-inexistente", 50);

        results.Should().BeEmpty();
        handler.Requests.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_deduplicates_by_external_id_across_pages()
    {
        var (source, handler) = GooglePlacesTestFactory.Create();
        handler.EnqueueJson(HttpStatusCode.OK, """
            {
              "places": [
                { "id": "dup-1", "displayName": { "text": "A" } },
                { "id": "unique-1", "displayName": { "text": "B" } }
              ],
              "nextPageToken": "token-1"
            }
            """);
        handler.EnqueueJson(HttpStatusCode.OK, """
            {
              "places": [
                { "id": "dup-1", "displayName": { "text": "A again" } },
                { "id": "unique-2", "displayName": { "text": "C" } }
              ]
            }
            """);

        var results = await source.SearchAsync("Centro", "padarias", 10);

        results.Should().HaveCount(3);
        results.Select(x => x.ExternalId).Should().BeEquivalentTo(["dup-1", "unique-1", "unique-2"]);
    }

    private static string BuildPlacesPage(int startIndex, int count, string? nextPageToken)
    {
        var places = new StringBuilder();
        places.Append("{\"places\":[");
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                places.Append(',');
            }

            var id = startIndex + i;
            places.Append($"{{\"id\":\"place-{id}\",\"displayName\":{{\"text\":\"Business {id}\"}}}}");
        }

        places.Append(']');
        if (!string.IsNullOrEmpty(nextPageToken))
        {
            places.Append($",\"nextPageToken\":\"{nextPageToken}\"");
        }

        places.Append('}');
        return places.ToString();
    }
}
