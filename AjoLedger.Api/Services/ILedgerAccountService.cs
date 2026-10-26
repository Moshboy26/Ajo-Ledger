
using AjoLedger.Api.Models;

namespace AjoLedger.Api.Services;

public interface ILedgerAccountService
{
    Task<LedgerAccount> Create(LedgerAccount account);

    Task<LedgerAccount?> GetById(
        Guid savingsGroupId,
        Guid accountId);

    Task<IEnumerable<LedgerAccount>> GetByGroup(
        Guid savingsGroupId);
}
