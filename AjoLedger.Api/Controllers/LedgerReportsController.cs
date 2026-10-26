
using AjoLedger.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AjoLedger.Api.Controllers;

[ApiController]
[Route("api/savings-groups/{groupId:guid}/ledger-reports")]
public class LedgerReportsController : ControllerBase
{
    private readonly ILedgerReportService _service;

    public LedgerReportsController(ILedgerReportService service)
    {
        _service = service;
    }

    [HttpGet("account-balances")]
    public async Task<IActionResult> GetAccountBalances(Guid groupId)
    {
        if (groupId == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "A valid savings group ID is required."
            });
        }

        var balances = await _service.GetAccountBalances(groupId);

        return Ok(balances);
    }
    
[HttpGet("trial-balance")]
public async Task<IActionResult> GetTrialBalance(Guid groupId)
{
    if (groupId == Guid.Empty)
    {
        return BadRequest(new
        {
            message = "A valid savings group ID is required."
        });
    }

    var report = await _service.GetTrialBalance(groupId);

    return Ok(report);
}


[HttpGet("accounts/{accountId:guid}/statement")]
public async Task<IActionResult> GetAccountStatement(
    Guid groupId,
    Guid accountId)
{
    if (groupId == Guid.Empty || accountId == Guid.Empty)
    {
        return BadRequest(new
        {
            message = "Valid savings group and account IDs are required."
        });
    }

    var statement = await _service.GetAccountStatement(
        groupId,
        accountId);

    if (statement is null)
    {
        return NotFound(new
        {
            message = "Ledger account not found in this savings group."
        });
    }

    return Ok(statement);
}

}
