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

    private static readonly int[] ValidRangeDays = { 30, 60, 90 };

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string range = "all")
    {
        int? rangeDays = null;
        if (!string.IsNullOrWhiteSpace(range) && !range.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(range, out var days) || !ValidRangeDays.Contains(days))
            {
                return BadRequest("range must be one of: 30, 60, 90, all");
            }
            rangeDays = days;
        }

        var tickets = await _repo.GetAllAsync();

        if (rangeDays.HasValue)
        {
            var cutoff = DateTime.UtcNow.AddDays(-rangeDays.Value);
            tickets = tickets.Where(t => t.CreatedAt >= cutoff);
        }

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
        var fileName = $"tickets-export-{DateTime.UtcNow:yyyy-MM-dd}.csv";
        return File(bytes, "text/csv", fileName);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var success = await _repo.DeleteTicketAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id}/type")]
    public async Task<IActionResult> UpdateTicketType(int id, [FromBody] UpdateTicketTypeRequest request)
    {
        int ticketTypeValue;
        if (request.TicketType == "IT")
        {
            ticketTypeValue = 0;
        }
        else if (request.TicketType == "Herstelwerk")
        {
            ticketTypeValue = 1;
        }
        else
        {
            return BadRequest("TicketType must be one of: IT, Herstelwerk");
        }

        var success = await _repo.UpdateTicketTypeAsync(id, ticketTypeValue);
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

    [HttpGet("{ticketNumber}/photos")]
    public async Task<IActionResult> GetPhotos(string ticketNumber, [FromServices] TicketPhotoRepository repo)
    {
        var photos = await repo.GetByTicketNumberAsync(ticketNumber);
        return Ok(photos);
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

public record UpdateTicketTypeRequest(string TicketType);