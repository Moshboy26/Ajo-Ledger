
using AjoLedger.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AjoLedger.Api.Repositories;

public class DapperLedgerAccountRepository : ILedgerAccountRepository
{
    private readonly string _connectionString;

    public DapperLedgerAccountRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AjoLedger")
            ?? throw new InvalidOperationException(
                "AjoLedger connection string is missing.");
    }

    public async Task<LedgerAccount> Create(LedgerAccount account)
    {
        const string sql = """
            INSERT INTO dbo.LedgerAccounts
            (
                SavingsGroupId,
                AccountCode,
                Name,
                AccountType,
                IsActive
            )
            OUTPUT
                INSERTED.Id,
                INSERTED.SavingsGroupId,
                INSERTED.AccountCode,
                INSERTED.Name,
                INSERTED.AccountType,
                INSERTED.IsActive,
                INSERTED.CreatedAtUtc
            VALUES
            (
                @SavingsGroupId,
                @AccountCode,
                @Name,
                @AccountType,
                @IsActive
            );
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        return await connection.QuerySingleAsync<LedgerAccount>(
            sql, account);
    }

    public async Task<LedgerAccount?> GetById(
        Guid savingsGroupId,
        Guid accountId)
    {
        const string sql = """
            SELECT
                Id,
                SavingsGroupId,
                AccountCode,
                Name,
                AccountType,
                IsActive,
                CreatedAtUtc
            FROM dbo.LedgerAccounts
            WHERE SavingsGroupId = @SavingsGroupId
              AND Id = @AccountId;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        return await connection.QuerySingleOrDefaultAsync<LedgerAccount>(
            sql,
            new { SavingsGroupId = savingsGroupId, AccountId = accountId });
    }

    public async Task<IEnumerable<LedgerAccount>> GetByGroup(
        Guid savingsGroupId)
    {
        const string sql = """
            SELECT
                Id,
                SavingsGroupId,
                AccountCode,
                Name,
                AccountType,
                IsActive,
                CreatedAtUtc
            FROM dbo.LedgerAccounts
            WHERE SavingsGroupId = @SavingsGroupId
            ORDER BY CreatedAtUtc, AccountCode;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        return await connection.QueryAsync<LedgerAccount>(
            sql, new { SavingsGroupId = savingsGroupId });
    }
}
