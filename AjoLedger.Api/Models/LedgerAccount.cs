
namespace AjoLedger.Api.Models;

public class LedgerAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SavingsGroupId { get; set; }

    public string AccountCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string AccountType { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
}
