
using System.Security.Claims;
using Cinema.Api.Data;
using Cinema.Api.Dtos;
using Cinema.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly ReservationService _reservations;
    private readonly CinemaDbContext _db;

    public ReservationsController(
        ReservationService reservations,
        CinemaDbContext db)
    {
        _reservations = reservations;
        _db = db;
    }

    // POST /api/reservations
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ReservationResultDto>> Create(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(
            User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _reservations.CreateAsync(
                userId, request, cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (SeatAlreadyBookedException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/reservations/{id}/ticket
    [HttpGet("{id:int}/ticket")]
    [Authorize(Roles = "User,Admin")]
    public async Task<ActionResult<TicketDto>> GetTicket(
        int id,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(
            User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized();
        }

        var reservation = await _db.Reservations
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.UserId,
                r.SeanceId,
                r.TicketCode,
                r.ReservedAtUtc,
                FilmTitle = r.Seance.Film.Title,
                RoomName = r.Seance.Room.Name,
                r.Seance.StartsAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Only the owner or an admin can view the ticket
        if (reservation == null ||
            (!User.IsInRole("Admin") &&
             reservation.UserId != userId))
        {
            return NotFound(new
            {
                message = "Ticket not found."
            });
        }

        var seatNumbers = await _db.ReservationSeats
            .AsNoTracking()
            .Where(rs => rs.ReservationId == id)
            .OrderBy(rs => rs.Seat.SeatNumber)
            .Select(rs => rs.Seat.SeatNumber)
            .ToListAsync(cancellationToken);

        var ticket = new TicketDto(
            id,
            reservation.TicketCode,
            reservation.SeanceId,
            reservation.FilmTitle,
            reservation.RoomName,
            reservation.StartsAtUtc,
            reservation.ReservedAtUtc,
            seatNumbers
        );

        return Ok(ticket);
    }
}
