
namespace Cinema.Api.Dtos;

public record FilmDto(
    int Id,
    string Title,
    string Description,
    int DurationMinutes,
    string? PosterUrl
);
