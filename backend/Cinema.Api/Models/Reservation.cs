
namespace Cinema.Api.Models;

public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SeanceId { get; set; }

    public DateTime ReservedAtUtc { get; set; } = DateTime.UtcNow;

    public string TicketCode { get; set; }
        = Guid.NewGuid().ToString("N");

    public AppUser User { get; set; } = null!;
    public Seance Seance { get; set; } = null!;

    public ICollection<ReservationSeat> ReservationSeats { get; set; }
        = new List<ReservationSeat>();
}
