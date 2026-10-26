
namespace AjoLedger.Api.Models;

public sealed record MemberRegistrationResult(
    MemberRegistrationStatus Status,
    SavingsGroupMember? Member = null);
