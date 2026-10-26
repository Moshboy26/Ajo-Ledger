using System.Data;
using Microsoft.Data.SqlClient;
using AjoLedger.Api.Models;
using Dapper;

namespace AjoLedger.Api.Repositories;

public class DapperSavingsGroupMemberRepository
    : ISavingsGroupMemberRepository
{
    private readonly string _connectionString;

    public DapperSavingsGroupMemberRepository(
        IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AjoLedger")
            ?? throw new InvalidOperationException(
                "AjoLedger connection string is missing.");
    }

    public IEnumerable<SavingsGroupMember> GetByGroup(Guid groupId)
    {
        const string sql = """
            SELECT Id, SavingsGroupId, FullName, PhoneNumber,
                   JoinedAtUtc, IsActive
            FROM dbo.SavingsGroupMembers
            WHERE SavingsGroupId = @GroupId
            ORDER BY JoinedAtUtc;
            """;

        using var connection = new SqlConnection(_connectionString);

        return connection.Query<SavingsGroupMember>(
            sql, new { GroupId = groupId }).ToList();
    }
 
public MemberRegistrationResult RegisterIfEligible(
    SavingsGroupMember member)
{
    using var connection = new SqlConnection(_connectionString);
    connection.Open();

    using var transaction =
        connection.BeginTransaction(IsolationLevel.Serializable);

    try
    {
        const string groupSql = """
            SELECT Id, Name, ContributionAmount, MaxMembers,
                   CreatedAtUtc, IsActive
            FROM dbo.SavingsGroups WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @Id;
            """;

        var group = connection.QuerySingleOrDefault<SavingsGroup>(
            groupSql,
            new { Id = member.SavingsGroupId },
            transaction);

        if (group is null)
        {
            transaction.Rollback();
            return new(MemberRegistrationStatus.GroupNotFound);
        }

        if (!group.IsActive)
        {
            transaction.Rollback();
            return new(MemberRegistrationStatus.GroupInactive);
        }

        const string duplicateSql = """
            SELECT CASE WHEN EXISTS
            (
                SELECT 1
                FROM dbo.SavingsGroupMembers
                WHERE SavingsGroupId = @GroupId
                  AND PhoneNumber = @PhoneNumber
            )
            THEN 1 ELSE 0 END;
            """;

        var phoneExists = connection.ExecuteScalar<bool>(
            duplicateSql,
            new
            {
                GroupId = member.SavingsGroupId,
                member.PhoneNumber
            },
            transaction);

        if (phoneExists)
        {
            transaction.Rollback();
            return new(MemberRegistrationStatus.DuplicatePhone);
        }

        const string countSql = """
            SELECT COUNT(*)
            FROM dbo.SavingsGroupMembers
            WHERE SavingsGroupId = @GroupId
              AND IsActive = 1;
            """;

        var activeCount = connection.ExecuteScalar<int>(
            countSql,
            new { GroupId = member.SavingsGroupId },
            transaction);

        if (activeCount >= group.MaxMembers)
        {
            transaction.Rollback();
            return new(MemberRegistrationStatus.GroupFull);
        }

        const string insertSql = """
            INSERT INTO dbo.SavingsGroupMembers
                (Id, SavingsGroupId, FullName, PhoneNumber,
                 JoinedAtUtc, IsActive)
            VALUES
                (@Id, @SavingsGroupId, @FullName, @PhoneNumber,
                 @JoinedAtUtc, @IsActive);
            """;

        connection.Execute(insertSql, member, transaction);

        transaction.Commit();

return new(
    MemberRegistrationStatus.Registered,
    member);
    }
   catch
{
    try
    {
        if (transaction.Connection is not null)
        {
            transaction.Rollback();
        }
    }
    catch
    {
        // Preserve the original exception if rollback also fails.
    }

    throw;
}
}

}
