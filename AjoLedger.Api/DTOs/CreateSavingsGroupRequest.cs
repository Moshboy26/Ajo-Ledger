
using System.ComponentModel.DataAnnotations;

namespace AjoLedger.Api.DTOs;

public class CreateSavingsGroupRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "1000000000")]
    public decimal ContributionAmount { get; set; }

    [Range(2, 1000)]
    public int MaxMembers { get; set; }
}
