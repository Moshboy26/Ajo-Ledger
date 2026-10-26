
using AjoLedger.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AjoLedger.Api.Repositories;

public class DapperLedgerReportRepository : ILedgerReportRepository
{
    private readonly string _connectionString;

    public DapperLedgerReportRepository(
        IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AjoLedger")
            ?? throw new InvalidOperationException(
                "AjoLedger connection string is missing.");
    }

    public async Task<IEnumerable<LedgerAccountBalance>> GetAccountBalances(
        Guid savingsGroupId)
    {
        const string sql = """
            SELECT
                a.Id AS LedgerAccountId,
                a.SavingsGroupId,
                a.AccountCode,
                a.Name AS AccountName,
                a.AccountType,
                COALESCE(SUM(e.Debit), 0) AS TotalDebits,
                COALESCE(SUM(e.Credit), 0) AS TotalCredits,
                CASE
                    WHEN a.AccountType IN ('Asset', 'Expense')
                        THEN COALESCE(SUM(e.Debit), 0)
                           - COALESCE(SUM(e.Credit), 0)
                    ELSE COALESCE(SUM(e.Credit), 0)
                       - COALESCE(SUM(e.Debit), 0)
                END AS Balance
            FROM dbo.LedgerAccounts AS a
            LEFT JOIN dbo.LedgerEntries AS e
                ON e.LedgerAccountId = a.Id
                AND e.SavingsGroupId = a.SavingsGroupId
            WHERE a.SavingsGroupId = @SavingsGroupId
            GROUP BY
                a.Id,
                a.SavingsGroupId,
                a.AccountCode,
                a.Name,
                a.AccountType
            ORDER BY a.AccountCode;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        return await connection.QueryAsync<LedgerAccountBalance>(
            sql,
            new { SavingsGroupId = savingsGroupId });
    }
    
public async Task<LedgerAccountStatement?> GetAccountStatement(
    Guid savingsGroupId,
    Guid accountId)
{
    const string sql = """
        SELECT
            a.SavingsGroupId,
            a.Id AS LedgerAccountId,
            a.AccountCode,
            a.Name AS AccountName,
            a.AccountType,
            COALESCE(SUM(e.Debit), 0) AS TotalDebits,
            COALESCE(SUM(e.Credit), 0) AS TotalCredits,
            CASE
                WHEN a.AccountType IN ('Asset', 'Expense')
                    THEN COALESCE(SUM(e.Debit), 0)
                       - COALESCE(SUM(e.Credit), 0)
                ELSE COALESCE(SUM(e.Credit), 0)
                   - COALESCE(SUM(e.Debit), 0)
            END AS Balance
        FROM dbo.LedgerAccounts AS a
        LEFT JOIN dbo.LedgerEntries AS e
            ON e.LedgerAccountId = a.Id
            AND e.SavingsGroupId = a.SavingsGroupId
        WHERE a.SavingsGroupId = @SavingsGroupId
          AND a.Id = @AccountId
        GROUP BY
            a.SavingsGroupId,
            a.Id,
            a.AccountCode,
            a.Name,
            a.AccountType;

        SELECT
            e.Id AS LedgerEntryId,
            e.LedgerTransactionId,
            t.Reference,
            t.TransactionType,
            t.Description,
            e.Debit,
            e.Credit,
            e.CreatedAtUtc
        FROM dbo.LedgerEntries AS e
        INNER JOIN dbo.LedgerTransactions AS t
            ON t.SavingsGroupId = e.SavingsGroupId
            AND t.Id = e.LedgerTransactionId
        WHERE e.SavingsGroupId = @SavingsGroupId
          AND e.LedgerAccountId = @AccountId
        ORDER BY e.CreatedAtUtc, e.Id;
        """;

    await using var connection =
        new SqlConnection(_connectionString);

    using var multi = await connection.QueryMultipleAsync(
        sql,
        new { SavingsGroupId = savingsGroupId, AccountId = accountId });

    var statement =
        await multi.ReadSingleOrDefaultAsync<LedgerAccountStatement>();

    if (statement is null)
    {
        return null;
    }

    statement.Entries =
        (await multi.ReadAsync<LedgerAccountStatementEntry>()).ToList();

    return statement;
}

}
