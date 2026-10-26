
using System.Data;
using AjoLedger.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AjoLedger.Api.Repositories;

public class DapperLedgerTransactionRepository
    : ILedgerTransactionRepository
{
    private readonly string _connectionString;

    public DapperLedgerTransactionRepository(
        IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AjoLedger")
            ?? throw new InvalidOperationException(
                "AjoLedger connection string is missing.");
    }

    public async Task<LedgerTransactionCreateResult> Create(
        LedgerTransaction transaction)
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var dbTransaction =
            (SqlTransaction)await connection.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var existing = await connection.QuerySingleOrDefaultAsync<LedgerTransaction>(
                """
                SELECT Id, SavingsGroupId, Reference, TransactionType,
                       Description, IdempotencyKey, CreatedAtUtc
                FROM dbo.LedgerTransactions WITH (UPDLOCK, HOLDLOCK)
                WHERE SavingsGroupId = @SavingsGroupId
                  AND IdempotencyKey = @IdempotencyKey;
                """,
                new
                {
                    transaction.SavingsGroupId,
                    transaction.IdempotencyKey
                },
                dbTransaction);

            if (existing is not null)
            {
                var existingEntries =
                    (await connection.QueryAsync<LedgerEntry>(
                        """
                        SELECT Id, SavingsGroupId, LedgerTransactionId,
                               LedgerAccountId, Debit, Credit, CreatedAtUtc
                        FROM dbo.LedgerEntries
                        WHERE SavingsGroupId = @SavingsGroupId
                          AND LedgerTransactionId = @TransactionId
                        ORDER BY LedgerAccountId, Debit, Credit;
                        """,
                        new
                        {
                            transaction.SavingsGroupId,
                            TransactionId = existing.Id
                        },
                        dbTransaction)).ToList();

                bool sameHeader =
                    existing.Reference == transaction.Reference &&
                    existing.TransactionType == transaction.TransactionType &&
                    existing.Description == transaction.Description;

                bool sameEntries =
                    existingEntries.Count == transaction.Entries.Count &&
                    existingEntries
                        .Select(e => new
                        {
                            e.LedgerAccountId,
                            e.Debit,
                            e.Credit
                        })
                        .OrderBy(e => e.LedgerAccountId)
                        .ThenBy(e => e.Debit)
                        .ThenBy(e => e.Credit)
                        .SequenceEqual(
                            transaction.Entries
                                .Select(e => new
                                {
                                    e.LedgerAccountId,
                                    e.Debit,
                                    e.Credit
                                })
                                .OrderBy(e => e.LedgerAccountId)
                                .ThenBy(e => e.Debit)
                                .ThenBy(e => e.Credit));

                await dbTransaction.CommitAsync();

                existing.Entries = existingEntries;

                return new LedgerTransactionCreateResult(
                    sameHeader && sameEntries
                        ? LedgerTransactionCreateStatus.AlreadyExists
                        : LedgerTransactionCreateStatus
                            .IdempotencyKeyReusedWithDifferentPayload,
                    existing);
            }

            var referenceExists =
                await connection.ExecuteScalarAsync<int>(
                    """
                    SELECT COUNT(1)
                    FROM dbo.LedgerTransactions WITH (UPDLOCK, HOLDLOCK)
                    WHERE SavingsGroupId = @SavingsGroupId
                      AND Reference = @Reference;
                    """,
                    new
                    {
                        transaction.SavingsGroupId,
                        transaction.Reference
                    },
                    dbTransaction);

            if (referenceExists > 0)
            {
                await dbTransaction.RollbackAsync();

                return new LedgerTransactionCreateResult(
                    LedgerTransactionCreateStatus.ReferenceAlreadyExists,
                    transaction);
            }

            transaction.Id = Guid.NewGuid();

            var created = await connection.QuerySingleAsync<LedgerTransaction>(
                """
                INSERT INTO dbo.LedgerTransactions
                (
                    Id, SavingsGroupId, Reference, TransactionType,
                    Description, IdempotencyKey
                )
                OUTPUT
                    INSERTED.Id,
                    INSERTED.SavingsGroupId,
                    INSERTED.Reference,
                    INSERTED.TransactionType,
                    INSERTED.Description,
                    INSERTED.IdempotencyKey,
                    INSERTED.CreatedAtUtc
                VALUES
                (
                    @Id, @SavingsGroupId, @Reference, @TransactionType,
                    @Description, @IdempotencyKey
                );
                """,
                transaction,
                dbTransaction);

            const string insertEntrySql = """
                INSERT INTO dbo.LedgerEntries
                (
                    SavingsGroupId,
                    LedgerTransactionId,
                    LedgerAccountId,
                    Debit,
                    Credit
                )
                OUTPUT
                    INSERTED.Id,
                    INSERTED.SavingsGroupId,
                    INSERTED.LedgerTransactionId,
                    INSERTED.LedgerAccountId,
                    INSERTED.Debit,
                    INSERTED.Credit,
                    INSERTED.CreatedAtUtc
                VALUES
                (
                    @SavingsGroupId,
                    @LedgerTransactionId,
                    @LedgerAccountId,
                    @Debit,
                    @Credit
                );
                """;

            created.Entries = [];

            foreach (var entry in transaction.Entries)
            {
                entry.Id = Guid.NewGuid();
                entry.SavingsGroupId = transaction.SavingsGroupId;
                entry.LedgerTransactionId = created.Id;

                var savedEntry =
                    await connection.QuerySingleAsync<LedgerEntry>(
                        insertEntrySql,
                        entry,
                        dbTransaction);

                created.Entries.Add(savedEntry);
            }

            await dbTransaction.CommitAsync();

            return new LedgerTransactionCreateResult(
                LedgerTransactionCreateStatus.Created,
                created);
        }
        catch
        {
            try
            {
                await dbTransaction.RollbackAsync();
            }
            catch
            {
                // Preserve the original exception.
            }

            throw;
        }
    }

    public async Task<LedgerTransaction?> GetById(
        Guid savingsGroupId,
        Guid transactionId)
    {
        const string sql = """
            SELECT Id, SavingsGroupId, Reference, TransactionType,
                   Description, IdempotencyKey, CreatedAtUtc
            FROM dbo.LedgerTransactions
            WHERE SavingsGroupId = @SavingsGroupId
              AND Id = @TransactionId;

            SELECT Id, SavingsGroupId, LedgerTransactionId,
                   LedgerAccountId, Debit, Credit, CreatedAtUtc
            FROM dbo.LedgerEntries
            WHERE SavingsGroupId = @SavingsGroupId
              AND LedgerTransactionId = @TransactionId
            ORDER BY LedgerAccountId;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        using var multi = await connection.QueryMultipleAsync(
            sql,
            new { SavingsGroupId = savingsGroupId, TransactionId = transactionId });

        var result = await multi.ReadSingleOrDefaultAsync<LedgerTransaction>();

        if (result is null)
        {
            return null;
        }

        result.Entries = (await multi.ReadAsync<LedgerEntry>()).ToList();

        return result;
    }

    public async Task<IEnumerable<LedgerTransaction>> GetByGroup(
        Guid savingsGroupId)
    {
        const string sql = """
            SELECT Id, SavingsGroupId, Reference, TransactionType,
                   Description, IdempotencyKey, CreatedAtUtc
            FROM dbo.LedgerTransactions
            WHERE SavingsGroupId = @SavingsGroupId
            ORDER BY CreatedAtUtc DESC;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        var transactions =
            (await connection.QueryAsync<LedgerTransaction>(
                sql, new { SavingsGroupId = savingsGroupId })).ToList();

        if (transactions.Count == 0)
        {
            return transactions;
        }

        var entries = await connection.QueryAsync<LedgerEntry>(
            """
            SELECT Id, SavingsGroupId, LedgerTransactionId,
                   LedgerAccountId, Debit, Credit, CreatedAtUtc
            FROM dbo.LedgerEntries
            WHERE SavingsGroupId = @SavingsGroupId
            ORDER BY LedgerTransactionId, LedgerAccountId;
            """,
            new { SavingsGroupId = savingsGroupId });

        var entriesByTransaction = entries
            .GroupBy(entry => entry.LedgerTransactionId)
            .ToDictionary(
                group => group.Key,
                group => group.ToList());

        foreach (var item in transactions)
        {
            item.Entries = entriesByTransaction.TryGetValue(
                item.Id, out var transactionEntries)
                    ? transactionEntries
                    : [];
        }

        return transactions;
    }
}
