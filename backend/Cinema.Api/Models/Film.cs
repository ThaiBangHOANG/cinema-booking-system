
namespace Cinema.Api.Models;

public class Film
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string? PosterUrl { get; set; }

    public ICollection<Seance> Seances { get; set; }
        = new List<Seance>();
}
