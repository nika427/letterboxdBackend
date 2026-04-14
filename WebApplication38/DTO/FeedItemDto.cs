namespace WebApplication38.DTO;

public class FeedItemDto
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string MoviePoster { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}