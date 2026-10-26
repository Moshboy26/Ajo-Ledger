using AjoLedger.Api.Models;

namespace AjoLedger.Api.Models;

public class ContributionLedgerOutboxItem
{
    public Guid OutboxId { get; set; }

    public Guid ContributionId { get; set; }

    public Guid SavingsGroupId { get; set; }

    public Guid SavingsGroupMemberId { get; set; }

    public decimal Amount { get; set; }

    public DateTime ContributedAtUtc { get; set; }

    public Guid IdempotencyKey { get; set; }

    public Contribution ToContribution()
    {
        return new Contribution
        {
            Id = ContributionId,
            SavingsGroupId = SavingsGroupId,
            SavingsGroupMemberId = SavingsGroupMemberId,
            Amount = Amount,
            ContributedAtUtc = ContributedAtUtc,
            IdempotencyKey = IdempotencyKey
        };
    }
}