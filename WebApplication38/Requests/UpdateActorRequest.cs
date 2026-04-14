namespace WebApplication38.Requests;

public class UpdateActorRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? Biography { get; set; }
    public string? PhotoUrl { get; set; }
}
