
using AjoLedger.Api.Models;

namespace AjoLedger.Api.Repositories;

public interface IContributionRepository
{
    ContributionCreateResult Create(
        Guid groupId,
        Guid memberId,
        decimal amount,
        Guid idempotencyKey
    );

    IEnumerable<Contribution> GetByMember(
        Guid groupId,
        Guid memberId
    );
}
