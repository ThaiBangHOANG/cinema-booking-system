using System.ComponentModel.DataAnnotations;

namespace Cinema.Api.Dtos;

public record AdminSeanceRequest(
    [param: Range(1, int.MaxValue)] int FilmId,
    [param: Range(1, int.MaxValue)] int RoomId,
    DateTime StartsAtUtc
);
