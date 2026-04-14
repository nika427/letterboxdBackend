namespace WebApplication38.Requests;

public class UpdateMovieRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string? Genre { get; set; }
    public string? PosterUrl { get; set; }
}
