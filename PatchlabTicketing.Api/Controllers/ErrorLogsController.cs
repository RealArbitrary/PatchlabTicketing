using Microsoft.AspNetCore.Mvc;
using PatchlabTicketing.Api.Data;

namespace PatchlabTicketing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErrorLogsController : ControllerBase
{
    private readonly ErrorLogRepository _repo;

    public ErrorLogsController(ErrorLogRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent()
    {
        var logs = await _repo.GetRecentAsync();
        return Ok(logs);
    }
}