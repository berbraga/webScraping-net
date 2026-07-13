using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebScraping.Domain.Abstractions;
using WebScraping.Infrastructure.Lookup;
using WebScraping.Infrastructure.Persistence;

namespace WebScraping.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Testing:UseInMemoryStores", "true");
        builder.UseSetting("GooglePlaces:UseFakeSource", "true");
        builder.UseSetting("GooglePlaces:ApiKey", "");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IBusinessLookupSource>();
            services.RemoveAll<ISearchRepository>();
            services.RemoveAll<IBusinessRepository>();
            services.RemoveAll<InMemorySearchRepository>();
            services.RemoveAll<InMemoryBusinessRepository>();

            services.AddSingleton<InMemorySearchRepository>();
            services.AddSingleton<InMemoryBusinessRepository>();
            services.AddSingleton<ISearchRepository>(sp => sp.GetRequiredService<InMemorySearchRepository>());
            services.AddSingleton<IBusinessRepository>(sp => sp.GetRequiredService<InMemoryBusinessRepository>());
            services.AddSingleton<IBusinessLookupSource>(_ => new FakeBusinessLookupSource());
        });
    }
}

public class SearchesEndpointTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public SearchesEndpointTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<JsonElement> WaitForTerminalAsync(string id, TimeSpan? timeout = null)
    {
        var limit = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(15));
        JsonElement body = default;
        while (DateTime.UtcNow < limit)
        {
            var get = await _client.GetAsync($"/api/searches/{id}");
            get.StatusCode.Should().Be(HttpStatusCode.OK);
            body = await get.Content.ReadFromJsonAsync<JsonElement>();
            var status = body.GetProperty("status").GetString();
            if (status is "completed" or "failed" or "cancelled")
            {
                return body;
            }

            await Task.Delay(50);
        }

        throw new TimeoutException($"Search {id} did not reach a terminal status. Last: {body}");
    }

    [Fact]
    public async Task Post_and_list_businesses_returns_names()
    {
        var create = await _client.PostAsJsonAsync("/api/searches", new
        {
            region = "Centro, São Paulo",
            query = "padarias",
            maxResults = 10
        });

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var summary = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = summary.GetProperty("id").GetString();
        id.Should().NotBeNullOrWhiteSpace();

        var terminal = await WaitForTerminalAsync(id!);
        terminal.GetProperty("totalFound").GetInt32().Should().BeGreaterThan(0);

        var list = await _client.GetAsync($"/api/searches/{id}/businesses");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await list.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("total").GetInt32().Should().BeGreaterThan(0);
        body.GetProperty("items")[0].GetProperty("name").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("items")[0].TryGetProperty("siteCreationYear", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Completed_search_includes_siteCreationYear_for_known_sites()
    {
        var create = await _client.PostAsJsonAsync("/api/searches", new
        {
            region = "Centro, São Paulo",
            query = "padarias",
            maxResults = 3
        });

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var summary = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = summary.GetProperty("id").GetString()!;
        await WaitForTerminalAsync(id);

        var list = await _client.GetAsync($"/api/searches/{id}/businesses");
        var body = await list.Content.ReadFromJsonAsync<JsonElement>();
        var items = body.GetProperty("items").EnumerateArray().ToList();
        var withWebsite = items.First(i => i.TryGetProperty("website", out var w) && w.ValueKind == JsonValueKind.String);
        withWebsite.TryGetProperty("siteCreationYear", out var year).Should().BeTrue();
        year.ValueKind.Should().BeOneOf(JsonValueKind.Number, JsonValueKind.Null);
    }

    [Fact]
    public async Task Post_with_empty_region_returns_400()
    {
        var create = await _client.PostAsJsonAsync("/api/searches", new
        {
            region = " ",
            query = "padarias"
        });

        create.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_with_maxResults_100_returns_matching_totalFound()
    {
        var create = await _client.PostAsJsonAsync("/api/searches", new
        {
            region = "Florianopolis",
            query = "restaurantes",
            maxResults = 100
        });

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var summary = await create.Content.ReadFromJsonAsync<JsonElement>();
        summary.GetProperty("maxResults").GetInt32().Should().Be(100);
        var id = summary.GetProperty("id").GetString()!;

        var terminal = await WaitForTerminalAsync(id);
        terminal.GetProperty("status").GetString().Should().Be("completed");
        terminal.GetProperty("maxResults").GetInt32().Should().Be(100);
        terminal.GetProperty("totalFound").GetInt32().Should().Be(100);
    }
}

public class SearchProgressCancelTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public SearchProgressCancelTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Cancel_endpoint_and_progress_fields_work()
    {
        var create = await _client.PostAsJsonAsync("/api/searches", new
        {
            region = "Centro",
            query = "padarias",
            maxResults = 10
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var summary = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = summary.GetProperty("id").GetString()!;

        await Task.Delay(50);
        var cancel = await _client.PostAsync($"/api/searches/{id}/cancel", null);

        if (cancel.StatusCode == HttpStatusCode.Conflict)
        {
            var completed = await _client.GetAsync($"/api/searches/{id}");
            var body = await completed.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("status").GetString().Should().BeOneOf("completed", "failed");
            body.GetProperty("processedCount").GetInt32().Should().BeGreaterThanOrEqualTo(0);
            body.GetProperty("totalFound").GetInt32().Should().BeGreaterThanOrEqualTo(0);
            return;
        }

        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelled = await cancel.Content.ReadFromJsonAsync<JsonElement>();
        cancelled.GetProperty("status").GetString().Should().Be("cancelled");

        var get = await _client.GetAsync($"/api/searches/{id}");
        var progress = await get.Content.ReadFromJsonAsync<JsonElement>();
        progress.TryGetProperty("processedCount", out _).Should().BeTrue();
    }
}

public class ExportEndpointTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ExportEndpointTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Export_returns_csv_with_header()
    {
        var create = await _client.PostAsJsonAsync("/api/searches", new
        {
            region = "Centro",
            query = "padarias",
            maxResults = 10
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var summary = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = summary.GetProperty("id").GetString()!;

        var limit = DateTime.UtcNow + TimeSpan.FromSeconds(15);
        while (DateTime.UtcNow < limit)
        {
            var get = await _client.GetAsync($"/api/searches/{id}");
            var body = await get.Content.ReadFromJsonAsync<JsonElement>();
            var status = body.GetProperty("status").GetString();
            if (status is "completed" or "failed" or "cancelled")
            {
                break;
            }

            await Task.Delay(50);
        }

        var export = await _client.GetAsync($"/api/searches/{id}/export");
        export.StatusCode.Should().Be(HttpStatusCode.OK);
        export.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var text = await export.Content.ReadAsStringAsync();
        text.Should().Contain("Nome,Telefone,Site,Criação do site,Avaliacao");
        text.Should().Contain("Padaria Central");
    }
}
