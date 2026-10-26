
namespace AjoLedger.Api.Models;

public class Contribution
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SavingsGroupId { get; set; }

    public Guid SavingsGroupMemberId { get; set; }

    public decimal Amount { get; set; }

    public DateTime ContributedAtUtc { get; set; }

    public Guid IdempotencyKey { get; set; }
}
