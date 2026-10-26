
using AjoLedger.Api.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AjoLedger.Api.Services;

public class ContributionLedgerOutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ContributionLedgerOutboxWorker> _logger;

    public ContributionLedgerOutboxWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ContributionLedgerOutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Contribution ledger outbox worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingBatch(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An unexpected error occurred while processing the contribution outbox.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation(
            "Contribution ledger outbox worker stopped.");
    }

    private async Task ProcessPendingBatch(
        CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var outboxRepository = scope.ServiceProvider
            .GetRequiredService<IContributionLedgerOutboxRepository>();

        var postingService = scope.ServiceProvider
            .GetRequiredService<ContributionLedgerPostingService>();

        var pendingItems = await outboxRepository.GetPendingBatch(20);

        foreach (var item in pendingItems)
        {
            stoppingToken.ThrowIfCancellationRequested();

            try
            {
                await postingService.PostContribution(
                    item.ToContribution());

                await outboxRepository.MarkProcessed(item.OutboxId);

                _logger.LogInformation(
                    "Successfully posted contribution {ContributionId} to the ledger.",
                    item.ContributionId);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to post contribution {ContributionId}. A retry will be scheduled.",
                    item.ContributionId);

                try
                {
                    await outboxRepository.RecordFailure(
                        item.OutboxId,
                        exception.Message);
                }
                catch (Exception recordException)
                {
                    _logger.LogError(
                        recordException,
                        "Could not record the retry state for outbox item {OutboxId}.",
                        item.OutboxId);
                }
            }
        }
    }
}
