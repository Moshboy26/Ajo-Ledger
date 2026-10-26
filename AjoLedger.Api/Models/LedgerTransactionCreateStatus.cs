
namespace AjoLedger.Api.Models;

public enum LedgerTransactionCreateStatus
{
    Created,
    AlreadyExists,
    IdempotencyKeyReusedWithDifferentPayload,
    ReferenceAlreadyExists
}
