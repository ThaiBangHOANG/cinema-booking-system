
namespace Cinema.Api.Models;

public class Seance
{
    public int Id { get; set; }
    public int FilmId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartsAtUtc { get; set; }

    public Film Film { get; set; } = null!;
    public Room Room { get; set; } = null!;

    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();

    public ICollection<ReservationSeat> ReservationSeats { get; set; }
        = new List<ReservationSeat>();
}
