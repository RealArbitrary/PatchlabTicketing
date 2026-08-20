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
        const string sql = "SELECT Id, TicketNumber, CellphoneNumber, Issue, CreatedAt, Status FROM Tickets ORDER BY Id DESC";
        return await conn.QueryAsync<Ticket>(sql);
    }

    public async Task<bool> CloseTicketAsync(string ticketNumber)
    {
        using var conn = new SqlConnection(_connectionString);
        const string sql = "UPDATE Tickets SET Status = 'Closed' WHERE TicketNumber = @TicketNumber";
        var rowsAffected = await conn.ExecuteAsync(sql, new { TicketNumber = ticketNumber });
        return rowsAffected > 0;
    }
}