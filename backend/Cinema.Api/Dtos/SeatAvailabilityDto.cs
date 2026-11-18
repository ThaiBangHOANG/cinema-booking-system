
namespace Cinema.Api.Dtos;

public record SeatAvailabilityDto(
    int SeatId,
    string SeatNumber,
    bool IsAvailable
);

public record SeanceSeatsDto(
    int SeanceId,
    int RoomId,
    string RoomName,
    int TotalSeats,
    int AvailableSeats,
    List<SeatAvailabilityDto> Seats
);
