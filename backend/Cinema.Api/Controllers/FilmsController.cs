
using Cinema.Api.Data;
using Cinema.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/films")]
public class FilmsController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public FilmsController(CinemaDbContext db)
    {
        _db = db;
    }

    // GET /api/films
    [HttpGet]
    public async Task<ActionResult<List<FilmDto>>> GetFilms()
    {
        var films = await _db.Films
            .AsNoTracking()
            .OrderBy(f => f.Title)
            .Select(f => new FilmDto(
                f.Id,
                f.Title,
                f.Description,
                f.DurationMinutes,
                f.PosterUrl
            ))
            .ToListAsync();

        return Ok(films);
    }

    // GET /api/films/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FilmDto>> GetFilm(int id)
    {
        var film = await _db.Films
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FilmDto(
                f.Id,
                f.Title,
                f.Description,
                f.DurationMinutes,
                f.PosterUrl
            ))
            .FirstOrDefaultAsync();

        if (film == null)
            return NotFound();

        return Ok(film);
    }
}
