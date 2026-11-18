
namespace Cinema.Api.Dtos;

public record SeanceOccupancyDto(
    int SeanceId,
    string FilmTitle,
    string RoomName,
    DateTime StartsAtUtc,
    int TotalSeats,
    int ReservedSeats,
    int AvailableSeats,
    double OccupancyRate
);

public record OccupancySummaryDto(
    int TotalSeances,
    int TotalSeats,
    int ReservedSeats,
    int AvailableSeats,
    double OccupancyRate,
    List<SeanceOccupancyDto> Seances
);
