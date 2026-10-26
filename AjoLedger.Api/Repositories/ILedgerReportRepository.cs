
using AjoLedger.Api.Models;

namespace AjoLedger.Api.Repositories;

public interface ILedgerReportRepository
{
    Task<IEnumerable<LedgerAccountBalance>> GetAccountBalances(
        Guid savingsGroupId);
    Task<LedgerAccountStatement?> GetAccountStatement(
    Guid savingsGroupId,
    Guid accountId);    
}
