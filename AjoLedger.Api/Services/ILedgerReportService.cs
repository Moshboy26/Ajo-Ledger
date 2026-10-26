
using AjoLedger.Api.Models;

namespace AjoLedger.Api.Services;

public interface ILedgerReportService
{
    Task<IEnumerable<LedgerAccountBalance>> GetAccountBalances(
        Guid savingsGroupId);
        Task<TrialBalanceReport> GetTrialBalance(Guid savingsGroupId);
       Task<LedgerAccountStatement?> GetAccountStatement(
    Guid savingsGroupId,
    Guid accountId); 
}
