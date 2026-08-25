using System.Text;
using Microsoft.AspNetCore.Mvc;
using PatchlabTicketing.Api.Data;

namespace PatchlabTicketing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly TicketRepository _repo;

    public TicketsController(TicketRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tickets = await _repo.GetAllAsync();
        return Ok(tickets);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var tickets = await _repo.GetAllAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Ticket No,Cell No,Name & Surname,Location of issue,Issue description,Date ticket logged,Date ticket resolved");

        foreach (var t in tickets)
        {
            var nameAndSurname = $"{t.FirstName} {t.LastName}".Trim();
            var row = new[]
            {
                t.TicketNumber,
                t.CellphoneNumber,
                nameAndSurname,
                t.Area ?? string.Empty,
                t.Issue,
                t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                t.ResolvedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
            };
            sb.AppendLine(string.Join(",", row.Select(CsvEscape)));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "tickets-export.csv");
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    [HttpPut("{ticketNumber}/close")]
    public async Task<IActionResult> Close(string ticketNumber)
    {
        var success = await _repo.CloseTicketAsync(ticketNumber);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{ticketNumber}/feedback")]
    public async Task<IActionResult> GetFeedback(string ticketNumber, [FromServices] TicketFeedbackRepository feedbackRepo)
    {
        var feedback = await feedbackRepo.GetByTicketNumberAsync(ticketNumber);
        return Ok(feedback);
    }

    [HttpGet("{ticketNumber}/comments")]
    public async Task<IActionResult> GetComments(string ticketNumber, [FromServices] TicketCommentRepository repo)
    {
        var comments = await repo.GetByTicketNumberAsync(ticketNumber);
        return Ok(comments);
    }

    [HttpPost("{ticketNumber}/comments")]
    public async Task<IActionResult> AddComment(string ticketNumber, [FromBody] AddCommentRequest request, [FromServices] TicketCommentRepository repo)
    {
        if (string.IsNullOrWhiteSpace(request.Comment)) return BadRequest();
        await repo.AddAsync(ticketNumber, request.Comment);
        return NoContent();
    }

    [HttpDelete("{ticketNumber}/comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(string ticketNumber, int commentId, [FromServices] TicketCommentRepository repo)
    {
        var success = await repo.DeleteAsync(commentId);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

}