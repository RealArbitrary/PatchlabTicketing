namespace PatchlabTicketing.Api.Models;

public class Ticket
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string CellphoneNumber { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string? Area { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TicketType { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}