
using Cinema.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/reservations")]
public sealed class AdminReservationsController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public AdminReservationsController(CinemaDbContext db)
    {
        _db = db;
    }

    // GET /api/admin/reservations
    // GET /api/admin/reservations?seanceId=11
    [HttpGet]
    public async Task<IActionResult> GetReservations(
        [FromQuery] int? seanceId,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (limit < 1 || limit > 200)
        {
            return BadRequest(new
            {
                message = "Limit must be between 1 and 200."
            });
        }

        if (seanceId.HasValue && seanceId.Value <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid screening ID."
            });
        }

        var query = _db.Reservations.AsNoTracking();

        if (seanceId.HasValue)
        {
            query = query.Where(r =>
                r.SeanceId == seanceId.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var reservations = await query
            .OrderByDescending(r => r.ReservedAtUtc)
            .ThenByDescending(r => r.Id)
            .Take(limit)
            .Select(r => new
            {
                r.Id,
                r.TicketCode,
                r.UserId,
                UserEmail = r.User.Email,
                r.SeanceId,
                FilmTitle = r.Seance.Film.Title,
                RoomName = r.Seance.Room.Name,
                r.Seance.StartsAtUtc,
                r.ReservedAtUtc,

                SeatNumbers = r.ReservationSeats
                    .OrderBy(rs => rs.Seat.SeatNumber)
                    .Select(rs => rs.Seat.SeatNumber)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            Total = total,
            Items = reservations
        });
    }
}
