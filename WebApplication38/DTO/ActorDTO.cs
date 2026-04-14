namespace WebApplication38.DTO;

public class ActorDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? Biography { get; set; }
    public string? PhotoUrl { get; set; }
    public int MovieCount { get; set; }
}
