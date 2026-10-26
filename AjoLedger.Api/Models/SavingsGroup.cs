
namespace AjoLedger.Api.Models;

public class SavingsGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public decimal ContributionAmount { get; set; }

    public int MaxMembers { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
