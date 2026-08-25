using Dapper;
using Microsoft.Data.SqlClient;
using PatchlabTicketing.Api.Models;

namespace PatchlabTicketing.Api.Data;

public class TicketPhotoRepository
{
    private readonly string _connectionString;

    public TicketPhotoRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Patchlab")
            ?? throw new InvalidOperationException("Patchlab connection string missing");
    }

    public async Task<IEnumerable<TicketPhoto>> GetByTicketNumberAsync(string ticketNumber)
    {
        using var conn = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT p.Id, p.FilePath, p.CreatedAt
            FROM TicketPhotos p
            INNER JOIN Tickets t ON t.Id = p.TicketId
            WHERE t.TicketNumber = @TicketNumber
            ORDER BY p.CreatedAt DESC";
        return await conn.QueryAsync<TicketPhoto>(sql, new { TicketNumber = ticketNumber });
    }
}
