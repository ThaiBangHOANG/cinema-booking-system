
namespace Cinema.Api.Models;

public class Seat
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;

    public Room Room { get; set; } = null!;

    public ICollection<ReservationSeat> ReservationSeats { get; set; }
        = new List<ReservationSeat>();
}
