
using System.ComponentModel.DataAnnotations;

namespace AjoLedger.Api.DTOs;

public class RegisterMemberRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
}
