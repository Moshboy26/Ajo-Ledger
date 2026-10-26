
using AjoLedger.Api.DTOs;
using AjoLedger.Api.Models;
using AjoLedger.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AjoLedger.Api.Controllers;

[ApiController]
[Route("api/savings-groups")]
public class SavingsGroupsController : ControllerBase
{
    private readonly SavingsGroupService _service;

    public SavingsGroupsController(SavingsGroupService service)
    {
        _service = service;
    }

    [HttpPost]
    public ActionResult<SavingsGroup> Create(
        CreateSavingsGroupRequest request)
    {
        var group = _service.Create(
            request.Name,
            request.ContributionAmount,
            request.MaxMembers);

        return CreatedAtAction(
            nameof(GetById),
            new { id = group.Id },
            group);
    }

    [HttpGet]
    public ActionResult<IEnumerable<SavingsGroup>> GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<SavingsGroup> GetById(Guid id)
    {
        var group = _service.GetById(id);

        if (group is null)
        {
            return NotFound();
        }

        return Ok(group);
    }
    
[HttpPut("{id:guid}")]
public ActionResult<SavingsGroup> Update(
    Guid id,
    UpdateSavingsGroupRequest request)
{
    var updated = _service.Update(
        id,
        request.Name,
        request.ContributionAmount,
        request.MaxMembers);

    if (!updated)
    {
        return NotFound();
    }

    return Ok(_service.GetById(id));
}

[HttpPatch("{id:guid}/deactivate")]
public IActionResult Deactivate(Guid id)
{
    var deactivated = _service.Deactivate(id);

    if (!deactivated)
    {
        return NotFound();
    }

    return NoContent();
}

}
