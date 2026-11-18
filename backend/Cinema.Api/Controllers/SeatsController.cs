
using Cinema.Api.Data;
using Cinema.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/seances/{seanceId:int}/seats")]
public class SeatsController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public SeatsController(CinemaDbContext db)
    {
        _db = db;
    }

    // GET /api/seances/{seanceId}/seats
    [HttpGet]
    public async Task<ActionResult<SeanceSeatsDto>> GetSeats(
        int seanceId)
    {
        // 1. Find the screening and its room
        var seance = await _db.Seances
            .AsNoTracking()
            .Where(s => s.Id == seanceId)
            .Select(s => new
            {
                s.Id,
                s.RoomId,
                RoomName = s.Room.Name
            })
            .FirstOrDefaultAsync();

        if (seance == null)
        {
            return NotFound(new
            {
                Message = "Séance introuvable."
            });
        }

        // 2. Find all seats in the room
        // and check their availability
        var seats = await _db.Seats
            .AsNoTracking()
            .Where(s => s.RoomId == seance.RoomId)
            .OrderBy(s => s.SeatNumber)
            .Select(s => new SeatAvailabilityDto(
                s.Id,
                s.SeatNumber,
                !_db.ReservationSeats.Any(rs =>
                    rs.SeanceId == seanceId &&
                    rs.SeatId == s.Id)
            ))
            .ToListAsync();

        // 3. Count available seats
        var availableSeats = seats.Count(s => s.IsAvailable);

        return Ok(new SeanceSeatsDto(
            seance.Id,
            seance.RoomId,
            seance.RoomName,
            seats.Count,
            availableSeats,
            seats
        ));
    }
}
