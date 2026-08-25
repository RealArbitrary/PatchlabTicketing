namespace PatchlabTicketing.Api.Models;

public class TicketPhoto
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
