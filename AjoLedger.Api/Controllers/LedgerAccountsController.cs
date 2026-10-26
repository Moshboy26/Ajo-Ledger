
using AjoLedger.Api.DTOs;
using AjoLedger.Api.Models;
using AjoLedger.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AjoLedger.Api.Controllers;

[ApiController]
[Route("api/savings-groups/{groupId:guid}/ledger-accounts")]
public class LedgerAccountsController : ControllerBase
{
    private readonly ILedgerAccountService _service;

    public LedgerAccountsController(ILedgerAccountService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid groupId,
        CreateLedgerAccountRequest request)
    {
        var account = new LedgerAccount
        {
            SavingsGroupId = groupId,
            AccountCode = request.AccountCode,
            Name = request.Name,
            AccountType = request.AccountType
        };

        try
        {
            var created = await _service.Create(account);

            return CreatedAtAction(
                nameof(GetById),
                new { groupId, accountId = created.Id },
                created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return NotFound(new
            {
                message = "Savings group not found."
            });
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return Conflict(new
            {
                message = "An account with this code already exists in this savings group."
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetByGroup(Guid groupId)
    {
        var accounts = await _service.GetByGroup(groupId);
        return Ok(accounts);
    }

    [HttpGet("{accountId:guid}")]
    public async Task<IActionResult> GetById(
        Guid groupId,
        Guid accountId)
    {
        var account = await _service.GetById(groupId, accountId);

        if (account is null)
        {
            return NotFound(new
            {
                message = "Ledger account not found in this savings group."
            });
        }

        return Ok(account);
    }
}
