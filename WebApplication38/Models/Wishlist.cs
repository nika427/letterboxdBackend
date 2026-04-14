namespace WebApplication38.Models;

public class Wishlist
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int MovieId { get; set; }
    public Movie? Movie { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}