
using System.ComponentModel.DataAnnotations;

namespace AjoLedger.Api.DTOs;

public class RecordLedgerTransactionRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Reference { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(Contribution|Payout|Reversal|Adjustment)$")]
    public string TransactionType { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    public Guid IdempotencyKey { get; set; }

    [Required]
    [MinLength(2)]
    public List<LedgerEntryRequest> Entries { get; set; } = [];
}
