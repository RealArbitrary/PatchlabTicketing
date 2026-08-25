using Dapper;
using Microsoft.Data.SqlClient;

namespace PatchlabTicketing.Api.Data;

public class TicketCommentRepository
{
    private readonly string _connectionString;
    public TicketCommentRepository(IConfiguration config) =>
        _connectionString = config.GetConnectionString("Patchlab")
            ?? throw new InvalidOperationException("Patchlab connection string missing");

    public async Task<IEnumerable<TicketComment>> GetByTicketNumberAsync(string ticketNumber)
    {
        using var conn = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT tc.Id, tc.Comment, tc.CreatedAt
            FROM TicketComments tc
            INNER JOIN Tickets t ON t.Id = tc.TicketId
            WHERE t.TicketNumber = @TicketNumber
            ORDER BY tc.CreatedAt DESC";
        return await conn.QueryAsync<TicketComment>(sql, new { TicketNumber = ticketNumber });
    }

    public async Task AddAsync(string ticketNumber, string comment)
    {
        using var conn = new SqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO TicketComments (TicketId, Comment, CreatedAt)
            SELECT Id, @Comment, GETUTCDATE() FROM Tickets WHERE TicketNumber = @TicketNumber";
        await conn.ExecuteAsync(sql, new { TicketNumber = ticketNumber, Comment = comment });
    }

    public async Task<bool> DeleteAsync(int commentId)
    {
        using var conn = new SqlConnection(_connectionString);
        const string sql = "DELETE FROM TicketComments WHERE Id = @Id";
        var rowsAffected = await conn.ExecuteAsync(sql, new { Id = commentId });
        return rowsAffected > 0;
    }
}