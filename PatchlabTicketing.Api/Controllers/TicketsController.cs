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
}