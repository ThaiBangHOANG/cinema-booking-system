using Cinema.Api.Data;
using Cinema.Api.Dtos;
using Cinema.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Cinema.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/seances")]
public sealed class AdminSeancesController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public AdminSeancesController(CinemaDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var seances = await _db.Seances.AsNoTracking()
            .OrderBy(s => s.StartsAtUtc)
            .Select(s => new
            {
                s.Id,
                s.FilmId,
                FilmTitle = s.Film.Title,
                s.RoomId,
                RoomName = s.Room.Name,
                s.StartsAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(seances);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var seance = await _db.Seances.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.FilmId,
                FilmTitle = s.Film.Title,
                s.RoomId,
                RoomName = s.Room.Name,
                s.StartsAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (seance is null) return NotFound();
        return Ok(seance);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] AdminSeanceRequest request,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateScheduleAsync(request, null, cancellationToken);
        if (invalid is not null) return invalid;

        var seance = new Seance
        {
            FilmId = request.FilmId,
            RoomId = request.RoomId,
            StartsAtUtc = request.StartsAtUtc
        };

        _db.Seances.Add(seance);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { message = "Une seance existe deja dans cette salle a cet horaire." });
        }

        return CreatedAtAction(nameof(GetById), new { id = seance.Id }, new
        {
            seance.Id,
            seance.FilmId,
            seance.RoomId,
            seance.StartsAtUtc
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] AdminSeanceRequest request,
        CancellationToken cancellationToken)
    {
        var seance = await _db.Seances.FirstOrDefaultAsync(s => s.Id == id,
            cancellationToken);
        if (seance is null) return NotFound();

        if (await _db.Reservations.AnyAsync(r => r.SeanceId == id, cancellationToken))
        {
            return Conflict(new { message = "Impossible de modifier une seance ayant des reservations." });
        }

        var invalid = await ValidateScheduleAsync(request, id, cancellationToken);
        if (invalid is not null) return invalid;

        seance.FilmId = request.FilmId;
        seance.RoomId = request.RoomId;
        seance.StartsAtUtc = request.StartsAtUtc;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { message = "Une seance existe deja dans cette salle a cet horaire." });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var seance = await _db.Seances.FirstOrDefaultAsync(s => s.Id == id,
            cancellationToken);
        if (seance is null) return NotFound();

        if (await _db.Reservations.AnyAsync(r => r.SeanceId == id, cancellationToken))
        {
            return Conflict(new { message = "Impossible de supprimer une seance ayant des reservations." });
        }

        _db.Seances.Remove(seance);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            return Conflict(new { message = "Cette seance est referencee par une reservation." });
        }

        return NoContent();
    }

    // The sample database is small. For a production-scale system,
    // replace this check with a database-level overlap constraint.
    private async Task<IActionResult?> ValidateScheduleAsync(
        AdminSeanceRequest request,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        if (request.StartsAtUtc.Kind != DateTimeKind.Utc ||
            request.StartsAtUtc <= DateTime.UtcNow)
        {
            return BadRequest(new { message = "StartsAtUtc doit etre une date UTC future (suffixe Z)." });
        }

        var film = await _db.Films.AsNoTracking()
            .Where(f => f.Id == request.FilmId)
            .Select(f => new { f.DurationMinutes })
            .FirstOrDefaultAsync(cancellationToken);
        if (film is null || film.DurationMinutes <= 0)
        {
            return BadRequest(new { message = "Film introuvable ou duree invalide." });
        }

        if (!await _db.Rooms.AnyAsync(r => r.Id == request.RoomId, cancellationToken))
        {
            return BadRequest(new { message = "Salle introuvable." });
        }

        // Keep 15 minutes between screenings for cleaning and room turnover.
        var newEnd = request.StartsAtUtc.AddMinutes(film.DurationMinutes + 15);
        var existing = await _db.Seances.AsNoTracking()
            .Where(s => s.RoomId == request.RoomId &&
                        (!excludeId.HasValue || s.Id != excludeId.Value))
            .Select(s => new { s.StartsAtUtc, s.Film.DurationMinutes })
            .ToListAsync(cancellationToken);

        var overlaps = existing.Any(s =>
            s.StartsAtUtc < newEnd &&
            s.StartsAtUtc.AddMinutes(s.DurationMinutes + 15) > request.StartsAtUtc);

        if (overlaps)
        {
            return Conflict(new { message = "Chevauchement avec une autre seance dans cette salle." });
        }

        return null;
    }
}
