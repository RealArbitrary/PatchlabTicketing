using System.Text.Json;
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
        SELECT t.Id, t.TicketNumber, t.CellphoneNumber, t.Issue, t.Area, t.CreatedAt, t.Status,
               c.FirstName, c.LastName
        FROM Tickets t
        LEFT JOIN Customers c ON c.CellphoneNumber = t.CellphoneNumber
        ORDER BY t.Id DESC";
        return await conn.QueryAsync<Ticket>(sql);
    }

    public async Task<bool> CloseTicketAsync(string ticketNumber)
    {
        using var conn = new SqlConnection(_connectionString);
        const string sql = "UPDATE Tickets SET Status = 'Closed' WHERE TicketNumber = @TicketNumber";
        var rowsAffected = await conn.ExecuteAsync(sql, new { TicketNumber = ticketNumber });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteTicketAsync(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            var ticket = await conn.QuerySingleOrDefaultAsync<Ticket>(
                "SELECT Id, TicketNumber, CellphoneNumber, Issue, Area, CreatedAt, ResolvedAt, Status FROM Tickets WHERE Id = @Id",
                new { Id = id }, transaction);

            if (ticket is null)
            {
                transaction.Rollback();
                return false;
            }

            var comments = await conn.QueryAsync<TicketComment>(
                "SELECT Id, Comment, CreatedAt FROM TicketComments WHERE TicketId = @Id",
                new { Id = id }, transaction);

            var feedback = await conn.QueryAsync<TicketFeedback>(
                "SELECT Id, Status, Reason, CreatedAt FROM TicketFeedback WHERE TicketId = @Id",
                new { Id = id }, transaction);

            const string insertArchiveSql = @"
                INSERT INTO DeletedTickets
                    (TicketId, TicketNumber, CellphoneNumber, Issue, Area, CreatedAt, ResolvedAt, Status, CommentsJson, FeedbackJson, DeletedAt)
                VALUES
                    (@Id, @TicketNumber, @CellphoneNumber, @Issue, @Area, @CreatedAt, @ResolvedAt, @Status, @CommentsJson, @FeedbackJson, GETUTCDATE())";

            await conn.ExecuteAsync(insertArchiveSql, new
            {
                Id = ticket.Id,
                ticket.TicketNumber,
                ticket.CellphoneNumber,
                ticket.Issue,
                ticket.Area,
                ticket.CreatedAt,
                ticket.ResolvedAt,
                ticket.Status,
                CommentsJson = JsonSerializer.Serialize(comments),
                FeedbackJson = JsonSerializer.Serialize(feedback),
            }, transaction);

            await conn.ExecuteAsync("DELETE FROM TicketFeedback WHERE TicketId = @Id", new { Id = id }, transaction);
            await conn.ExecuteAsync("DELETE FROM TicketComments WHERE TicketId = @Id", new { Id = id }, transaction);
            await conn.ExecuteAsync("DELETE FROM Tickets WHERE Id = @Id", new { Id = id }, transaction);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}