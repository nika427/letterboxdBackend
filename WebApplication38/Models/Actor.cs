namespace WebApplication38.Models;

public class Actor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? Biography { get; set; }
    public string? PhotoUrl { get; set; }
    public List<MovieActor> MovieActors { get; set; } = new();
}
