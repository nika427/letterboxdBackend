namespace WebApplication38.DTO;

public class MovieDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string? Genre { get; set; }
    public string? PosterUrl { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int ActorCount { get; set; }
}
