
namespace Cinema.Api.Models;

public class ReservationSeat
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int SeanceId { get; set; }
    public int SeatId { get; set; }

    public Reservation Reservation { get; set; } = null!;
    public Seance Seance { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
}
