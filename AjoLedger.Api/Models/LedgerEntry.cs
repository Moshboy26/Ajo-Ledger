
namespace AjoLedger.Api.Models;

public class LedgerEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SavingsGroupId { get; set; }

    public Guid LedgerTransactionId { get; set; }

    public Guid LedgerAccountId { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
