using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebScraping.Application.Searches;
using WebScraping.Domain.Abstractions;

namespace WebScraping.Infrastructure.Workers;

public sealed class SearchEnrichmentWorker : BackgroundService
{
    private readonly ISearchJobQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SearchEnrichmentWorker> _logger;

    public SearchEnrichmentWorker(
        ISearchJobQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<SearchEnrichmentWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var searchId in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var enricher = scope.ServiceProvider.GetRequiredService<EnrichBusinessesHandler>();
                await enricher.EnrichSearchAsync(searchId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enrich search {SearchId}", searchId);
            }
        }
    }
}
