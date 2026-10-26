
using AjoLedger.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AjoLedger.Api.Repositories;

public class DapperContributionLedgerOutboxRepository
    : IContributionLedgerOutboxRepository
{
    private readonly string _connectionString;

    public DapperContributionLedgerOutboxRepository(
        IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AjoLedger")
            ?? throw new InvalidOperationException(
                "Connection string 'AjoLedger' was not found.");
    }

    public async Task<IReadOnlyList<ContributionLedgerOutboxItem>>
        GetPendingBatch(int batchSize)
    {
        if (batchSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                "Batch size must be between 1 and 100.");
        }

        const string sql = """
            SELECT TOP (@BatchSize)
                o.Id AS OutboxId,
                c.Id AS ContributionId,
                c.SavingsGroupId,
                c.SavingsGroupMemberId,
                c.Amount,
                c.ContributedAtUtc,
                c.IdempotencyKey
            FROM dbo.ContributionLedgerOutbox AS o
            INNER JOIN dbo.Contributions AS c
                ON c.Id = o.ContributionId
            WHERE o.Status = 'Pending'
              AND o.NextAttemptAtUtc <= SYSUTCDATETIME()
            ORDER BY o.CreatedAtUtc, o.Id;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        var items = await connection.QueryAsync<
            ContributionLedgerOutboxItem>(
                sql,
                new { BatchSize = batchSize });

        return items.AsList();
    }

    public async Task MarkProcessed(Guid outboxId)
    {
        const string sql = """
            UPDATE dbo.ContributionLedgerOutbox
            SET
                Status = 'Processed',
                ProcessedAtUtc = SYSUTCDATETIME(),
                LastError = NULL
            WHERE Id = @OutboxId
              AND Status = 'Pending';
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            sql,
            new { OutboxId = outboxId });
    }

    public async Task RecordFailure(Guid outboxId, string error)
    {
        const string sql = """
            UPDATE dbo.ContributionLedgerOutbox
            SET
                AttemptCount = AttemptCount + 1,
                LastError = LEFT(@Error, 2000),
                NextAttemptAtUtc = DATEADD(
                    SECOND,
                    CASE
                        WHEN AttemptCount >= 10 THEN 3600
                        WHEN AttemptCount >= 8 THEN 1800
                        WHEN AttemptCount >= 6 THEN 600
                        WHEN AttemptCount >= 4 THEN 300
                        WHEN AttemptCount >= 2 THEN 60
                        ELSE 15
                    END,
                    SYSUTCDATETIME()
                )
            WHERE Id = @OutboxId
              AND Status = 'Pending';
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            sql,
            new
            {
                OutboxId = outboxId,
                Error = error
            });
    }

    public async Task MarkProcessedByContributionId(
    Guid contributionId)
{
    const string sql = """
        UPDATE dbo.ContributionLedgerOutbox
        SET
            Status = 'Processed',
            ProcessedAtUtc = SYSUTCDATETIME(),
            LastError = NULL
        WHERE ContributionId = @ContributionId
          AND Status = 'Pending';
        """;

    await using var connection =
        new SqlConnection(_connectionString);

    await connection.ExecuteAsync(
        sql,
        new { ContributionId = contributionId });
}
}
