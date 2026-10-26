
namespace AjoLedger.Api.Models;

public class LedgerTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SavingsGroupId { get; set; }

    public string Reference { get; set; } = string.Empty;

    public string TransactionType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid IdempotencyKey { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public List<LedgerEntry> Entries { get; set; } = [];
}
