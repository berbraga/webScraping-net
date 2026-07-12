using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using WebScraping.Application.Options;
using WebScraping.Application.Searches;
using WebScraping.Domain.Abstractions;
using WebScraping.Infrastructure.Lookup;
using WebScraping.Infrastructure.Persistence;
using WebScraping.Infrastructure.Workers;

namespace WebScraping.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebScrapingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SearchOptions>(configuration.GetSection(SearchOptions.SectionName));
        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));
        services.Configure<GooglePlacesOptions>(configuration.GetSection(GooglePlacesOptions.SectionName));

        var useTestingStores = configuration.GetValue("Testing:UseInMemoryStores", false);
        var useFakeSource = configuration.GetValue("GooglePlaces:UseFakeSource", false);
        var apiKey = configuration["GooglePlaces:ApiKey"];

        services.AddSingleton<ISearchJobQueue, InProcessSearchJobQueue>();
        services.AddScoped<StartSearchHandler>();
        services.AddScoped<CancelSearchHandler>();
        services.AddScoped<EnrichBusinessesHandler>();
        services.AddScoped<ExportSearchCsvHandler>();

        if (useTestingStores)
        {
            services.AddSingleton<InMemorySearchRepository>();
            services.AddSingleton<InMemoryBusinessRepository>();
            services.AddSingleton<ISearchRepository>(sp => sp.GetRequiredService<InMemorySearchRepository>());
            services.AddSingleton<IBusinessRepository>(sp => sp.GetRequiredService<InMemoryBusinessRepository>());
        }
        else
        {
            services.AddSingleton<IMongoClient>(sp =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoOptions>>().Value;
                return new MongoClient(options.ConnectionString);
            });
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoOptions>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(options.DatabaseName);
            });
            services.AddSingleton<MongoIndexInitializer>();
            services.AddScoped<ISearchRepository, MongoSearchRepository>();
            services.AddScoped<IBusinessRepository, MongoBusinessRepository>();
        }

        if (useTestingStores || useFakeSource || string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddSingleton<IBusinessLookupSource>(_ => new FakeBusinessLookupSource());
        }
        else
        {
            services.AddHttpClient<IBusinessLookupSource, GooglePlacesBusinessLookupSource>();
        }

        services.AddHostedService<SearchEnrichmentWorker>();
        return services;
    }
}
