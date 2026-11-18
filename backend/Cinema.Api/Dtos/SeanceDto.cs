
namespace Cinema.Api.Dtos;

public record SeanceDto(
    int Id,
    int FilmId,
    string FilmTitle,
    int RoomId,
    string RoomName,
    DateTime StartsAtUtc
);
