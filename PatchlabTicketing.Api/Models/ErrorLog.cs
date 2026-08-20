namespace PatchlabTicketing.Api.Models;

public class ErrorLog
{
    public int Id { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public DateTime CreatedAt { get; set; }
}