
using AjoLedger.Api.Models;
using AjoLedger.Api.Repositories;

namespace AjoLedger.Api.Services;

public class SavingsGroupMemberService
{
    private readonly ISavingsGroupMemberRepository _members;

    public SavingsGroupMemberService(
        ISavingsGroupMemberRepository members)
    {
        _members = members;
    }

    public MemberRegistrationResult Register(
        Guid groupId,
        string fullName,
        string phoneNumber)
    {
        var member = new SavingsGroupMember
        {
            SavingsGroupId = groupId,
            FullName = fullName.Trim(),
            PhoneNumber = phoneNumber.Trim()
        };

        return _members.RegisterIfEligible(member);
    }

    public IEnumerable<SavingsGroupMember> GetByGroup(
        Guid groupId)
    {
        return _members.GetByGroup(groupId);
    }
}
