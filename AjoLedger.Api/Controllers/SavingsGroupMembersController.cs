
using AjoLedger.Api.DTOs;
using AjoLedger.Api.Models;
using AjoLedger.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AjoLedger.Api.Controllers;

[ApiController]
[Route("api/savings-groups/{groupId:guid}/members")]
public class SavingsGroupMembersController : ControllerBase
{
    private readonly SavingsGroupMemberService _service;

    public SavingsGroupMembersController(
        SavingsGroupMemberService service)
    {
        _service = service;
    }

    [HttpPost]
    public ActionResult<SavingsGroupMember> Register(
        Guid groupId,
        RegisterMemberRequest request)
    {
        var result = _service.Register(
            groupId,
            request.FullName,
            request.PhoneNumber);

        switch (result.Status)
        {
            case MemberRegistrationStatus.Registered:
                return StatusCode(
                    StatusCodes.Status201Created,
                    result.Member);

            case MemberRegistrationStatus.GroupNotFound:
                return NotFound(new
                {
                    message = "Savings group was not found."
                });

            case MemberRegistrationStatus.GroupInactive:
                return Conflict(new
                {
                    message =
                        "Cannot join an inactive savings group."
                });

            case MemberRegistrationStatus.DuplicatePhone:
                return Conflict(new
                {
                    message =
                        "This phone number is already registered in the group."
                });

            case MemberRegistrationStatus.GroupFull:
                return Conflict(new
                {
                    message =
                        "The savings group has reached its member limit."
                });

            default:
                return StatusCode(
                    StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<SavingsGroupMember>> GetByGroup(
        Guid groupId)
    {
        return Ok(_service.GetByGroup(groupId));
    }
}
