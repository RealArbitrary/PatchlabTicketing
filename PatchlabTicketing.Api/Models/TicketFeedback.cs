namespace PatchlabTicketing.Api.Models;

public class TicketFeedback
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty; // "Satisfied" or "Unhappy"
    public string? Reason { get; set; }                 // populated only for "Unhappy"
    public DateTime CreatedAt { get; set; }
}