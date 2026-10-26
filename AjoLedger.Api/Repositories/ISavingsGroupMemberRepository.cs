
using AjoLedger.Api.Models;

namespace AjoLedger.Api.Repositories;

public interface ISavingsGroupMemberRepository
{
    MemberRegistrationResult RegisterIfEligible(
        SavingsGroupMember member);

    IEnumerable<SavingsGroupMember> GetByGroup(Guid groupId);
}
