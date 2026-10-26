
using System.ComponentModel.DataAnnotations;

namespace AjoLedger.Api.DTOs;

public class CreateLedgerAccountRequest
{
    [Required]
    [StringLength(20, MinimumLength = 1)]
    [RegularExpression(@"^[A-Za-z0-9_-]+$",
        ErrorMessage = "Account code can contain only letters, numbers, hyphens, and underscores.")]
    public string AccountCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(Asset|Liability|Equity|Income|Expense)$",
        ErrorMessage = "Account type must be Asset, Liability, Equity, Income, or Expense.")]
    public string AccountType { get; set; } = string.Empty;
}
