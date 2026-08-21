public class TicketComment
{
    public int Id { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public record AddCommentRequest(string Comment);