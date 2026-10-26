
using AjoLedger.Api.Models;
using AjoLedger.Api.Repositories;
using Microsoft.Extensions.Logging;

namespace AjoLedger.Api.Services;

public class ContributionService
{
    private readonly ISavingsGroupRepository _groups;
    private readonly ISavingsGroupMemberRepository _members;
    private readonly IContributionRepository _contributions;
    private readonly IContributionLedgerPostingService _postingService;
    private readonly IContributionLedgerOutboxRepository _outboxRepository;
    private readonly ILogger<ContributionService> _logger;

    public ContributionService(
        ISavingsGroupRepository groups,
        ISavingsGroupMemberRepository members,
        IContributionRepository contributions,
        IContributionLedgerPostingService postingService,
        IContributionLedgerOutboxRepository outboxRepository,
        ILogger<ContributionService> logger)
    {
        _groups = groups;
        _members = members;
        _contributions = contributions;
        _postingService = postingService;
        _outboxRepository = outboxRepository;
        _logger = logger;
    }

    public async Task<ContributionCreateResult?> Record(
        Guid groupId,
        Guid memberId,
        decimal amount,
        Guid idempotencyKey)
    {
        var group = _groups.GetById(groupId);

        if (group is null || !group.IsActive)
        {
            return null;
        }

        var member = _members
            .GetByGroup(groupId)
            .FirstOrDefault(m => m.Id == memberId && m.IsActive);

        if (member is null)
        {
            return null;
        }

        var result = _contributions.Create(
            groupId,
            memberId,
            amount,
            idempotencyKey);

        if (result.Status == ContributionCreateStatus.Created ||
            result.Status == ContributionCreateStatus.AlreadyExists)
        {
            try
            {
                await _postingService.PostContribution(
                    result.Contribution);

                await MarkOutboxProcessed(
                    result.Contribution.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Contribution {ContributionId} was saved, but " +
                    "ledger posting or outbox completion failed. " +
                    "The outbox worker will retry.",
                    result.Contribution.Id);
            }
        }

        return result;
    }

    private Task MarkOutboxProcessed(Guid contributionId)
    {
        return _outboxRepository.MarkProcessedByContributionId(
            contributionId);
    }

    public IEnumerable<Contribution> GetByMember(
        Guid groupId,
        Guid memberId)
    {
        return _contributions.GetByMember(groupId, memberId);
    }
}
