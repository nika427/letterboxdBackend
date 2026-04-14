namespace WebApplication38.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string? Genre { get; set; }
    public string? PosterUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<MovieActor> MovieActors { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
}
