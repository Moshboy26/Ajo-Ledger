
using AjoLedger.Api.Models;

namespace AjoLedger.Api.Services;

public interface ILedgerTransactionService
{
    Task<LedgerTransactionCreateResult> Create(
        LedgerTransaction transaction);

    Task<LedgerTransaction?> GetById(
        Guid savingsGroupId,
        Guid transactionId);

    Task<IEnumerable<LedgerTransaction>> GetByGroup(
        Guid savingsGroupId);
}
