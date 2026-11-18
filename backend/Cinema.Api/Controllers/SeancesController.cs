
using Cinema.Api.Data;
using Cinema.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/seances")]
public class SeancesController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public SeancesController(CinemaDbContext db)
    {
        _db = db;
    }

    // GET /api/seances?filmId=1
    [HttpGet]
    public async Task<ActionResult<List<SeanceDto>>> GetSeances(
        [FromQuery] int? filmId)
    {
        var query = _db.Seances
            .AsNoTracking()
            .Where(s => s.StartsAtUtc >= DateTime.UtcNow);

        if (filmId.HasValue)
        {
            query = query.Where(s => s.FilmId == filmId.Value);
        }

        var seances = await query
            .OrderBy(s => s.StartsAtUtc)
            .Select(s => new SeanceDto(
                s.Id,
                s.FilmId,
                s.Film.Title,
                s.RoomId,
                s.Room.Name,
                s.StartsAtUtc
            ))
            .ToListAsync();

        return Ok(seances);
    }

    // GET /api/seances/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SeanceDto>> GetSeance(int id)
    {
        var seance = await _db.Seances
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SeanceDto(
                s.Id,
                s.FilmId,
                s.Film.Title,
                s.RoomId,
                s.Room.Name,
                s.StartsAtUtc
            ))
            .FirstOrDefaultAsync();

        if (seance == null)
            return NotFound();

        return Ok(seance);
    }
}
