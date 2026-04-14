namespace WebApplication38.Requests;

public class AddReviewRequest
{
    public int MovieId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
