
using AjoLedger.Api.Models;

namespace AjoLedger.Api.Repositories;

public interface ILedgerAccountRepository
{
    Task<LedgerAccount> Create(LedgerAccount account);

    Task<LedgerAccount?> GetById(
        Guid savingsGroupId,
        Guid accountId);

    Task<IEnumerable<LedgerAccount>> GetByGroup(
        Guid savingsGroupId);
}
