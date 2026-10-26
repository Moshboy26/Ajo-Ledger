
using AjoLedger.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AjoLedger.Api.Repositories;

public class DapperSavingsGroupRepository
    : ISavingsGroupRepository
{
    private readonly string _connectionString;

    public DapperSavingsGroupRepository(
        IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AjoLedger")
            ?? throw new InvalidOperationException(
                "AjoLedger connection string is missing.");
    }

    public SavingsGroup Create(SavingsGroup group)
    {
        const string sql = """
            INSERT INTO dbo.SavingsGroups
                (Id, Name, ContributionAmount, MaxMembers,
                 CreatedAtUtc, IsActive)
            VALUES
                (@Id, @Name, @ContributionAmount, @MaxMembers,
                 @CreatedAtUtc, @IsActive);
            """;

        using var connection =
            new SqlConnection(_connectionString);

        connection.Execute(sql, group);

        return group;
    }

    public IEnumerable<SavingsGroup> GetAll()
    {
        const string sql = """
            SELECT Id, Name, ContributionAmount, MaxMembers,
                   CreatedAtUtc, IsActive
            FROM dbo.SavingsGroups;
            """;

        using var connection =
            new SqlConnection(_connectionString);

        return connection.Query<SavingsGroup>(sql).ToList();
    }

    public SavingsGroup? GetById(Guid id)
    {
        const string sql = """
            SELECT Id, Name, ContributionAmount, MaxMembers,
                   CreatedAtUtc, IsActive
            FROM dbo.SavingsGroups
            WHERE Id = @Id;
            """;

        using var connection =
            new SqlConnection(_connectionString);

        return connection.QuerySingleOrDefault<SavingsGroup>(
            sql, new { Id = id });
    }

    public bool Update(SavingsGroup group)
    {
        const string sql = """
            UPDATE dbo.SavingsGroups
            SET Name = @Name,
                ContributionAmount = @ContributionAmount,
                MaxMembers = @MaxMembers
            WHERE Id = @Id;
            """;

        using var connection =
            new SqlConnection(_connectionString);

        return connection.Execute(sql, group) == 1;
    }

    public bool Deactivate(Guid id)
    {
        const string sql = """
            UPDATE dbo.SavingsGroups
            SET IsActive = 0
            WHERE Id = @Id;
            """;

        using var connection =
            new SqlConnection(_connectionString);

        return connection.Execute(sql, new { Id = id }) == 1;
    }
}
