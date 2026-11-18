
namespace Cinema.Api.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }

    public ICollection<Seat> Seats { get; set; }
        = new List<Seat>();

    public ICollection<Seance> Seances { get; set; }
        = new List<Seance>();
}
