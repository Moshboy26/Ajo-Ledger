
using AjoLedger.Api.Models;
using AjoLedger.Api.Repositories;

namespace AjoLedger.Api.Services;

public class LedgerReportService : ILedgerReportService
{
    private readonly ILedgerReportRepository _repository;

    public LedgerReportService(ILedgerReportRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<LedgerAccountBalance>> GetAccountBalances(
        Guid savingsGroupId)
    {
        if (savingsGroupId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid savings group ID is required.");
        }

        return _repository.GetAccountBalances(savingsGroupId);
    }
    
public async Task<TrialBalanceReport> GetTrialBalance(
    Guid savingsGroupId)
{
    if (savingsGroupId == Guid.Empty)
    {
        throw new ArgumentException(
            "A valid savings group ID is required.");
    }

    var accounts = (await _repository
        .GetAccountBalances(savingsGroupId)).ToList();

    return new TrialBalanceReport
    {
        SavingsGroupId = savingsGroupId,
        Accounts = accounts,
        TotalDebits = accounts.Sum(account => account.TotalDebits),
        TotalCredits = accounts.Sum(account => account.TotalCredits)
    };
}
public Task<LedgerAccountStatement?> GetAccountStatement(
    Guid savingsGroupId,
    Guid accountId)
{
    if (savingsGroupId == Guid.Empty || accountId == Guid.Empty)
    {
        throw new ArgumentException(
            "Valid savings group and account IDs are required.");
    }

    return _repository.GetAccountStatement(
        savingsGroupId,
        accountId);
}
}
