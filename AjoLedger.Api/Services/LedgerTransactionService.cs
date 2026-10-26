
using AjoLedger.Api.Models;
using AjoLedger.Api.Repositories;

namespace AjoLedger.Api.Services;

public class LedgerTransactionService : ILedgerTransactionService
{
    private readonly ILedgerTransactionRepository _repository;
    private readonly ILedgerAccountRepository _accountRepository;
    private readonly ISavingsGroupRepository _groupRepository;

    private static readonly string[] AllowedTransactionTypes =
    [
        "Contribution",
        "Payout",
        "Reversal",
        "Adjustment"
    ];

    public LedgerTransactionService(
        ILedgerTransactionRepository repository,
        ILedgerAccountRepository accountRepository,
        ISavingsGroupRepository groupRepository)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _groupRepository = groupRepository;
    }

    public async Task<LedgerTransactionCreateResult> Create(
        LedgerTransaction transaction)
    {
        if (transaction.SavingsGroupId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid savings group ID is required.");
        }

        if (transaction.IdempotencyKey == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid idempotency key is required.");
        }

        if (string.IsNullOrWhiteSpace(transaction.Reference) ||
            transaction.Reference.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Reference is required and must not exceed 100 characters.");
        }

        transaction.Reference = transaction.Reference.Trim();

        if (!AllowedTransactionTypes.Contains(
                transaction.TransactionType,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Transaction type must be Contribution, Payout, Reversal, or Adjustment.");
        }

        transaction.TransactionType = AllowedTransactionTypes.First(
            type => type.Equals(
                transaction.TransactionType,
                StringComparison.OrdinalIgnoreCase));

        if (transaction.Description?.Length > 250)
        {
            throw new ArgumentException(
                "Description must not exceed 250 characters.");
        }

        if (transaction.Entries is null || transaction.Entries.Count < 2)
        {
            throw new ArgumentException(
                "A transaction must contain at least two ledger entries.");
        }

        if (transaction.Entries.Any(entry =>
                entry.LedgerAccountId == Guid.Empty ||
                entry.Debit < 0 ||
                entry.Credit < 0 ||
                (entry.Debit > 0 && entry.Credit > 0) ||
                (entry.Debit == 0 && entry.Credit == 0)))
        {
            throw new ArgumentException(
                "Each entry must have a valid account and exactly one positive debit or credit.");
        }

        decimal totalDebits = transaction.Entries.Sum(entry => entry.Debit);
        decimal totalCredits = transaction.Entries.Sum(entry => entry.Credit);

        if (totalDebits <= 0 || totalDebits != totalCredits)
        {
            throw new ArgumentException(
                "Total debits must equal total credits and be greater than zero.");
        }

        var group = _groupRepository.GetById(
    transaction.SavingsGroupId);

        if (group is null)
        {
            throw new KeyNotFoundException("Savings group not found.");
        }

        if (!group.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot record ledger transactions for an inactive savings group.");
        }

        var distinctAccountIds = transaction.Entries
            .Select(entry => entry.LedgerAccountId)
            .Distinct()
            .ToList();

        foreach (var accountId in distinctAccountIds)
        {
            var account = await _accountRepository.GetById(
                transaction.SavingsGroupId,
                accountId);

            if (account is null)
            {
                throw new KeyNotFoundException(
                    $"Ledger account {accountId} was not found in this savings group.");
            }

            if (!account.IsActive)
            {
                throw new InvalidOperationException(
                    $"Ledger account {account.AccountCode} is inactive.");
            }
        }

        return await _repository.Create(transaction);
    }

    public Task<LedgerTransaction?> GetById(
        Guid savingsGroupId,
        Guid transactionId)
    {
        return _repository.GetById(savingsGroupId, transactionId);
    }

    public Task<IEnumerable<LedgerTransaction>> GetByGroup(
        Guid savingsGroupId)
    {
        return _repository.GetByGroup(savingsGroupId);
    }
}
