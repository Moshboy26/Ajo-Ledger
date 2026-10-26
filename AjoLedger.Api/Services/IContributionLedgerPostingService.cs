
using AjoLedger.Api.Models;

namespace AjoLedger.Api.Services;

public interface IContributionLedgerPostingService
{
    Task PostContribution(Contribution contribution);
}
