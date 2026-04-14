namespace WebApplication38.DTO;

public class ActivityItemDto
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string MoviePoster { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}