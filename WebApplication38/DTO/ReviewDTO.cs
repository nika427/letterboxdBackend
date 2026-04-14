namespace WebApplication38.DTO;

public class ReviewDTO
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
}
