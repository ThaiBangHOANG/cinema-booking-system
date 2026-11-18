
namespace Cinema.Api.Dtos;

public record TicketDto(
    int ReservationId,
    string TicketCode,
    int SeanceId,
    string FilmTitle,
    string RoomName,
    DateTime StartsAtUtc,
    DateTime ReservedAtUtc,
    List<string> SeatNumbers
);
