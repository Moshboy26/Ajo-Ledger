using AjoLedger.Api.Models;

namespace AjoLedger.Api.Repositories;

public interface IContributionLedgerOutboxRepository
{
    Task<IReadOnlyList<ContributionLedgerOutboxItem>> GetPendingBatch(
        int batchSize);

    Task MarkProcessed(Guid outboxId);
    Task MarkProcessedByContributionId(Guid contributionId);

    Task RecordFailure(Guid outboxId, string error);
}
