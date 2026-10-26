
namespace AjoLedger.Api.Models;

public record ContributionCreateResult(
    ContributionCreateStatus Status,
    Contribution Contribution
);
