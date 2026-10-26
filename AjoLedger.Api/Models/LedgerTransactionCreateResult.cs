
namespace AjoLedger.Api.Models;

public record LedgerTransactionCreateResult(
    LedgerTransactionCreateStatus Status,
    LedgerTransaction Transaction);
