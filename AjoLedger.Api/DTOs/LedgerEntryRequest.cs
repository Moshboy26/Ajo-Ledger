
using System.ComponentModel.DataAnnotations;

namespace AjoLedger.Api.DTOs;

public class LedgerEntryRequest
{
    [Required]
    public Guid LedgerAccountId { get; set; }

    [Range(typeof(decimal), "0", "99999999999999999.99")]
    public decimal Debit { get; set; }

    [Range(typeof(decimal), "0", "99999999999999999.99")]
    public decimal Credit { get; set; }
}
