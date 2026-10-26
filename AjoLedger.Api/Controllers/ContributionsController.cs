
using AjoLedger.Api.DTOs;
using AjoLedger.Api.Models;
using AjoLedger.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AjoLedger.Api.Controllers;

[ApiController]
[Route("api/savings-groups/{groupId:guid}/members/{memberId:guid}/contributions")]
public class ContributionsController : ControllerBase
{
    private readonly ContributionService _service;

    public ContributionsController(ContributionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Record(
        Guid groupId,
        Guid memberId,
        [FromBody] RecordContributionRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Contribution amount must be greater than zero."
            });
        }

        if (request.IdempotencyKey == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "A valid, non-empty idempotency key is required."
            });
        }

        var result = await _service.Record(
        groupId,
        memberId,
        request.Amount,
        request.IdempotencyKey);

        if (result is null)
        {
            return NotFound(new
            {
                message = "The savings group or active member was not found."
            });
        }

        if (result.Status == ContributionCreateStatus.KeyReusedWithDifferentAmount)
        {
            return Conflict(new
            {
                message = "This idempotency key has already been used with a different amount."
            });
        }

        if (result.Status == ContributionCreateStatus.AlreadyExists)
        {
            return Ok(result.Contribution);
        }

        return CreatedAtAction(
            nameof(GetByMember),
            new { groupId, memberId },
            result.Contribution);
    }

    [HttpGet]
    public IActionResult GetByMember(Guid groupId, Guid memberId)
    {
        return Ok(_service.GetByMember(groupId, memberId));
    }
}
