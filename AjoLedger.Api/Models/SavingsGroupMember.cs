
namespace AjoLedger.Api.Models;

public class SavingsGroupMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SavingsGroupId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
