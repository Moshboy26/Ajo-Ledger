
using AjoLedger.Api.Models;
using AjoLedger.Api.Repositories;

namespace AjoLedger.Api.Services;

public class LedgerAccountService : ILedgerAccountService
{
    private readonly ILedgerAccountRepository _repository;

    public LedgerAccountService(ILedgerAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<LedgerAccount> Create(LedgerAccount account)
    {
        account.AccountCode = account.AccountCode.Trim();
        account.Name = account.Name.Trim();
        account.AccountType = account.AccountType.Trim();

        string[] allowedTypes =
        [
            "Asset",
            "Liability",
            "Equity",
            "Income",
            "Expense"
        ];

        if (string.IsNullOrWhiteSpace(account.AccountCode))
        {
            throw new ArgumentException("Account code is required.");
        }

        if (string.IsNullOrWhiteSpace(account.Name))
        {
            throw new ArgumentException("Account name is required.");
        }

        if (!allowedTypes.Contains(
                account.AccountType,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Account type must be Asset, Liability, Equity, Income, or Expense.");
        }

        account.AccountType = allowedTypes.First(
            type => type.Equals(
                account.AccountType,
                StringComparison.OrdinalIgnoreCase));

        return await _repository.Create(account);
    }

    public Task<LedgerAccount?> GetById(
        Guid savingsGroupId,
        Guid accountId)
    {
        return _repository.GetById(savingsGroupId, accountId);
    }

    public Task<IEnumerable<LedgerAccount>> GetByGroup(
        Guid savingsGroupId)
    {
        return _repository.GetByGroup(savingsGroupId);
    }
}
