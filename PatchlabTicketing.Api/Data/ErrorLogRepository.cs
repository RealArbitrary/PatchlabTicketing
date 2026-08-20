using Dapper;
using Microsoft.Data.SqlClient;
using PatchlabTicketing.Api.Models;

namespace PatchlabTicketing.Api.Data;

public class ErrorLogRepository
{
    private readonly string _connectionString;

    public ErrorLogRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Patchlab")
            ?? throw new InvalidOperationException("Patchlab connection string missing");
    }

    public async Task<IEnumerable<ErrorLog>> GetRecentAsync(int take = 200)
    {
        using var conn = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT TOP (@Take) Id, Severity, Source, Message, StackTrace, CreatedAt
            FROM ErrorLogs
            ORDER BY CreatedAt DESC";
        return await conn.QueryAsync<ErrorLog>(sql, new { Take = take });
    }
}