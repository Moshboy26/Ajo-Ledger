
using AjoLedger.Api.Models;
using AjoLedger.Api.Repositories;

namespace AjoLedger.Api.Services;

public class ContributionLedgerPostingService
    : IContributionLedgerPostingService{
    private readonly ILedgerAccountRepository _accountRepository;
    private readonly ILedgerTransactionService _transactionService;

    public ContributionLedgerPostingService(
        ILedgerAccountRepository accountRepository,
        ILedgerTransactionService transactionService)
    {
        _accountRepository = accountRepository;
        _transactionService = transactionService;
    }

    public async Task PostContribution(Contribution contribution)
    {
        var accounts = await _accountRepository.GetByGroup(
            contribution.SavingsGroupId);

        var cashAccount = accounts.FirstOrDefault(account =>
            account.AccountCode == "CASH-001");

        var savingsAccount = accounts.FirstOrDefault(account =>
            account.AccountCode == "SAV-001");

        if (cashAccount is null || savingsAccount is null)
        {
            throw new InvalidOperationException(
                "The savings group must have active CASH-001 and SAV-001 ledger accounts before contributions can be posted.");
        }

        if (!cashAccount.IsActive || !savingsAccount.IsActive)
        {
            throw new InvalidOperationException(
                "Both ledger accounts must be active before posting a contribution.");
        }

        var transaction = new LedgerTransaction
        {
            SavingsGroupId = contribution.SavingsGroupId,
            Reference = $"CONTRIB-{contribution.Id}",
            TransactionType = "Contribution",
            Description = $"Contribution from member {contribution.SavingsGroupMemberId}",
            IdempotencyKey = contribution.IdempotencyKey,
            Entries =
            [
                new LedgerEntry
                {
                    LedgerAccountId = cashAccount.Id,
                    Debit = contribution.Amount,
                    Credit = 0
                },
                new LedgerEntry
                {
                    LedgerAccountId = savingsAccount.Id,
                    Debit = 0,
                    Credit = contribution.Amount
                }
            ]
        };

        var result = await _transactionService.Create(transaction);

        if (result.Status ==
            LedgerTransactionCreateStatus.IdempotencyKeyReusedWithDifferentPayload)
        {
            throw new InvalidOperationException(
                "The contribution's idempotency key conflicts with an existing ledger transaction.");
        }

        if (result.Status ==
            LedgerTransactionCreateStatus.ReferenceAlreadyExists)
        {
            throw new InvalidOperationException(
                "The contribution's ledger reference already exists.");
        }
    }
}
