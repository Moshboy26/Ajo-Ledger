
namespace AjoLedger.Api.Models;

public class TrialBalanceReport
{
    public Guid SavingsGroupId { get; set; }
    public List<LedgerAccountBalance> Accounts { get; set; } = [];
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced => TotalDebits == TotalCredits;
    public decimal Difference => TotalDebits - TotalCredits;
}
