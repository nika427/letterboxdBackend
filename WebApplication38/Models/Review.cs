namespace WebApplication38.Models;

public class Review
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int UserId { get; set; }
    public User? User { get; set; }
    public int MovieId { get; set; }
    public Movie? Movie { get; set; }
}
