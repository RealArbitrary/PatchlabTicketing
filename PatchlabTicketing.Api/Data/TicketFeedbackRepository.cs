using Dapper;
using Microsoft.Data.SqlClient;
using PatchlabTicketing.Api.Models;

namespace PatchlabTicketing.Api.Data;

public class TicketFeedbackRepository
{
    private readonly string _connectionString;

    public TicketFeedbackRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Patchlab")
            ?? throw new InvalidOperationException("Patchlab connection string missing");
    }

    public async Task<IEnumerable<TicketFeedback>> GetByTicketNumberAsync(string ticketNumber)
    {
        using var conn = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT f.Id, f.Status, f.Reason, f.CreatedAt
            FROM TicketFeedback f
            INNER JOIN Tickets t ON t.Id = f.TicketId
            WHERE t.TicketNumber = @TicketNumber
            ORDER BY f.CreatedAt DESC";
        return await conn.QueryAsync<TicketFeedback>(sql, new { TicketNumber = ticketNumber });
    }
}