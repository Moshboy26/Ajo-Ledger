
using System.Data;
using AjoLedger.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AjoLedger.Api.Repositories;

public class DapperContributionRepository : IContributionRepository
{
    private readonly string _connectionString;

    public DapperContributionRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AjoLedger")
            ?? throw new InvalidOperationException(
                "Connection string 'AjoLedger' was not found."
            );
    }

    public ContributionCreateResult Create(
        Guid groupId,
        Guid memberId,
        decimal amount,
        Guid idempotencyKey)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction(
            IsolationLevel.Serializable);

        try
        {
            const string findSql = """
                SELECT
                    Id,
                    SavingsGroupId,
                    SavingsGroupMemberId,
                    Amount,
                    ContributedAtUtc,
                    IdempotencyKey
                FROM dbo.Contributions WITH (UPDLOCK, HOLDLOCK)
                WHERE SavingsGroupId = @GroupId
                  AND SavingsGroupMemberId = @MemberId
                  AND IdempotencyKey = @IdempotencyKey;
                """;

            var existing = connection.QuerySingleOrDefault<Contribution>(
                findSql,
                new
                {
                    GroupId = groupId,
                    MemberId = memberId,
                    IdempotencyKey = idempotencyKey
                },
                transaction);

            if (existing is not null)
            {
                if (existing.Amount != amount)
                {
                    transaction.Commit();

                    return new ContributionCreateResult(
                        ContributionCreateStatus.KeyReusedWithDifferentAmount,
                        existing);
                }

                EnsureOutboxEntry(
                    connection,
                    transaction,
                    existing.Id);

                transaction.Commit();

                return new ContributionCreateResult(
                    ContributionCreateStatus.AlreadyExists,
                    existing);
            }

            const string insertSql = """
                INSERT INTO dbo.Contributions
                (
                    SavingsGroupId,
                    SavingsGroupMemberId,
                    Amount,
                    IdempotencyKey
                )
                OUTPUT
                    INSERTED.Id,
                    INSERTED.SavingsGroupId,
                    INSERTED.SavingsGroupMemberId,
                    INSERTED.Amount,
                    INSERTED.ContributedAtUtc,
                    INSERTED.IdempotencyKey
                VALUES
                (
                    @GroupId,
                    @MemberId,
                    @Amount,
                    @IdempotencyKey
                );
                """;

            var contribution = connection.QuerySingle<Contribution>(
                insertSql,
                new
                {
                    GroupId = groupId,
                    MemberId = memberId,
                    Amount = amount,
                    IdempotencyKey = idempotencyKey
                },
                transaction);

            EnsureOutboxEntry(
                connection,
                transaction,
                contribution.Id);

            transaction.Commit();

            return new ContributionCreateResult(
                ContributionCreateStatus.Created,
                contribution);
        }
        catch
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Preserve the original exception.
            }

            throw;
        }
    }

    private static void EnsureOutboxEntry(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid contributionId)
    {
        const string sql = """
            IF NOT EXISTS
            (
                SELECT 1
                FROM dbo.ContributionLedgerOutbox
                    WITH (UPDLOCK, HOLDLOCK)
                WHERE ContributionId = @ContributionId
            )
            BEGIN
                INSERT INTO dbo.ContributionLedgerOutbox
                (
                    ContributionId
                )
                VALUES
                (
                    @ContributionId
                );
            END;
            """;

        connection.Execute(
            sql,
            new { ContributionId = contributionId },
            transaction);
    }

    public IEnumerable<Contribution> GetByMember(
        Guid groupId,
        Guid memberId)
    {
        using var connection = new SqlConnection(_connectionString);

        const string sql = """
            SELECT
                Id,
                SavingsGroupId,
                SavingsGroupMemberId,
                Amount,
                ContributedAtUtc,
                IdempotencyKey
            FROM dbo.Contributions
            WHERE SavingsGroupId = @GroupId
              AND SavingsGroupMemberId = @MemberId
            ORDER BY ContributedAtUtc DESC;
            """;

        return connection.Query<Contribution>(
            sql,
            new
            {
                GroupId = groupId,
                MemberId = memberId
            });
    }
}
