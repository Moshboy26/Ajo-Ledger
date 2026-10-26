
using AjoLedger.Api.Models;

namespace AjoLedger.Api.Repositories;

public interface ILedgerTransactionRepository
{
    Task<LedgerTransactionCreateResult> Create(
        LedgerTransaction transaction);

    Task<LedgerTransaction?> GetById(
        Guid savingsGroupId,
        Guid transactionId);

    Task<IEnumerable<LedgerTransaction>> GetByGroup(
        Guid savingsGroupId);
}
