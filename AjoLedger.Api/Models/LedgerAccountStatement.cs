
namespace AjoLedger.Api.Models;

public class LedgerAccountStatement
{
    public Guid SavingsGroupId { get; set; }
    public Guid LedgerAccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal Balance { get; set; }
    public List<LedgerAccountStatementEntry> Entries { get; set; } = [];
}

public class LedgerAccountStatementEntry
{
    public Guid LedgerEntryId { get; set; }
    public Guid LedgerTransactionId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
