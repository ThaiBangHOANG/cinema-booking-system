
using Cinema.Api.Data;
using Cinema.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/statistics")]
public sealed class AdminStatisticsController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public AdminStatisticsController(CinemaDbContext db)
    {
        _db = db;
    }

    // GET /api/admin/statistics/occupancy
    [HttpGet("occupancy")]
    public async Task<ActionResult<OccupancySummaryDto>>
        GetOccupancy(CancellationToken cancellationToken)
    {
        // Upcoming screenings for the next 14 days
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(14);

        var data = await _db.Seances
            .AsNoTracking()
            .Where(s => s.StartsAtUtc >= start &&
                        s.StartsAtUtc < end)
            .OrderBy(s => s.StartsAtUtc)
            .Select(s => new
            {
                s.Id,
                FilmTitle = s.Film.Title,
                RoomName = s.Room.Name,
                s.StartsAtUtc,

                TotalSeats = s.Room.Seats.Count(),

                ReservedSeats = s.ReservationSeats.Count()
            })
            .ToListAsync(cancellationToken);

        var seances = data.Select(s =>
        {
            var rate = s.TotalSeats == 0
                ? 0
                : Math.Round(
                    100.0 * s.ReservedSeats / s.TotalSeats, 2);

            return new SeanceOccupancyDto(
                s.Id,
                s.FilmTitle,
                s.RoomName,
                s.StartsAtUtc,
                s.TotalSeats,
                s.ReservedSeats,
                s.TotalSeats - s.ReservedSeats,
                rate
            );
        }).ToList();

        int totalSeats = seances.Sum(s => s.TotalSeats);
        int reservedSeats = seances.Sum(s => s.ReservedSeats);

        double occupancyRate = totalSeats == 0
            ? 0
            : Math.Round(
                100.0 * reservedSeats / totalSeats, 2);

        return Ok(new OccupancySummaryDto(
            seances.Count,
            totalSeats,
            reservedSeats,
            totalSeats - reservedSeats,
            occupancyRate,
            seances
        ));
    }
}
