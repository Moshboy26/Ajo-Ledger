
namespace AjoLedger.Api.Models;

public class LedgerAccountBalance
{
    public Guid LedgerAccountId { get; set; }
    public Guid SavingsGroupId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal Balance { get; set; }
}
