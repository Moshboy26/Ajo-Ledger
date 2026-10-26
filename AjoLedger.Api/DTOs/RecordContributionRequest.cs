
using System.ComponentModel.DataAnnotations;

namespace AjoLedger.Api.DTOs;

public class RecordContributionRequest
{
    [Range(typeof(decimal), "300", "99999999999999999.99")]
    public decimal Amount { get; set; }

    public Guid IdempotencyKey { get; set; }
}
