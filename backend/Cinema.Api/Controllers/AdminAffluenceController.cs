
using Cinema.Api.Data;
using Cinema.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/statistics/affluence")]
public sealed class AdminAffluenceController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public AdminAffluenceController(CinemaDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<AffluenceReportDto>> Get(
        [FromQuery] int days = 14,
        [FromQuery] DateOnly? festivalStart = null,
        [FromQuery] DateOnly? festivalEnd = null,
        CancellationToken cancellationToken = default)
    {
        if (days < 1 || days > 30)
            return BadRequest(new
            {
                message = "Days must be between 1 and 30."
            });

        if (festivalStart.HasValue != festivalEnd.HasValue ||
            (festivalStart.HasValue &&
             festivalStart.Value > festivalEnd!.Value))
            return BadRequest(new
            {
                message = "Invalid festival period."
            });

        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(days);

        var screenings = await _db.Seances
            .AsNoTracking()
            .Where(s => s.StartsAtUtc >= start &&
                        s.StartsAtUtc < end)
            .Select(s => new
            {
                s.StartsAtUtc,
                TotalSeats = s.Room.Seats.Count(),
                ReservedSeats = s.ReservationSeats.Count()
            })
            .ToListAsync(cancellationToken);

        static double CalculateRate(int reserved, int total) =>
            total == 0
                ? 0
                : Math.Round(100.0 * reserved / total, 2);

        // Daily trends
        var dailyTrends = new List<DailyAffluenceDto>();

        for (int i = 0; i < days; i++)
        {
            var date = DateOnly.FromDateTime(start.AddDays(i));

            var dayData = screenings
                .Where(s => DateOnly.FromDateTime(
                    s.StartsAtUtc) == date)
                .ToList();

            int total = dayData.Sum(s => s.TotalSeats);
            int reserved = dayData.Sum(s => s.ReservedSeats);

            double rate = CalculateRate(reserved, total);

            bool weekend =
                date.DayOfWeek is DayOfWeek.Saturday
                    or DayOfWeek.Sunday;

            bool festival =
                festivalStart.HasValue &&
                date >= festivalStart.Value &&
                date <= festivalEnd!.Value;

            string alert = total == 0
                ? "Aucune séance"
                : rate >= 80
                    ? "Élevée"
                    : weekend || festival || rate >= 50
                        ? "À surveiller"
                        : "Normale";

            dailyTrends.Add(new DailyAffluenceDto(
                date,
                weekend,
                festival,
                dayData.Count,
                total,
                reserved,
                rate,
                alert
            ));
        }

        // Hourly trends (UTC)
        var hourlyTrends = screenings
            .GroupBy(s => s.StartsAtUtc.Hour)
            .OrderBy(g => g.Key)
            .Select(g => new HourlyAffluenceDto(
                g.Key,
                g.Count(),
                g.Sum(s => s.ReservedSeats),
                CalculateRate(
                    g.Sum(s => s.ReservedSeats),
                    g.Sum(s => s.TotalSeats))
            ))
            .ToList();

        int totalSeats = screenings.Sum(s => s.TotalSeats);
        int reservedSeats = screenings.Sum(s => s.ReservedSeats);

        return Ok(new AffluenceReportDto(
            DateOnly.FromDateTime(start),
            DateOnly.FromDateTime(end.AddDays(-1)),
            reservedSeats,
            CalculateRate(reservedSeats, totalSeats),
            dailyTrends,
            hourlyTrends
        ));
    }
}
