using Dapper;
using Microsoft.Data.SqlClient;
using PatchlabTicketing.Api.Models;

namespace PatchlabTicketing.Api.Data;

public class TicketRepository
{
    private readonly string _connectionString;

    public TicketRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Patchlab")
            ?? throw new InvalidOperationException("Patchlab connection string missing");
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        const string sql = @"
        SELECT t.Id, t.TicketNumber, t.CellphoneNumber, t.Issue, t.Area, t.CreatedAt, t.ResolvedAt, t.Status,
               c.FirstName, c.LastName
        FROM Tickets t
        LEFT JOIN Customers c ON c.CellphoneNumber = t.CellphoneNumber
        ORDER BY t.Id DESC";
        return await conn.QueryAsync<Ticket>(sql);
    }

    public async Task<bool> CloseTicketAsync(string ticketNumber)
    {
        using var conn = new SqlConnection(_connectionString);
        const string sql = @"
            UPDATE Tickets
            SET Status = 'Closed',
                ResolvedAt = CASE WHEN Status <> 'Closed' THEN GETUTCDATE() ELSE ResolvedAt END
            WHERE TicketNumber = @TicketNumber";
        var rowsAffected = await conn.ExecuteAsync(sql, new { TicketNumber = ticketNumber });
        return rowsAffected > 0;
    }

    
}