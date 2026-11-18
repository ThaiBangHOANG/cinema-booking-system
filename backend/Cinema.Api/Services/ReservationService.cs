using Cinema.Api.Data;
using Cinema.Api.Dtos;
using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Cinema.Api.Services;

public sealed class SeatAlreadyBookedException : Exception
{
    public SeatAlreadyBookedException()
        : base("At least one selected seat is already reserved for this screening.")
    {
    }
}

public sealed class ReservationService
{
    private readonly CinemaDbContext _db;

    public ReservationService(CinemaDbContext db)
    {
        _db = db;
    }

    public async Task<ReservationResultDto> CreateAsync(
        int userId,
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.SeatIds is null || request.SeatIds.Count == 0 ||
            request.SeatIds.Any(id => id <= 0) ||
            request.SeatIds.Distinct().Count() != request.SeatIds.Count)
        {
            throw new ArgumentException("Select at least one valid seat without duplicates.");
        }

        var seance = await _db.Seances.AsNoTracking()
            .Where(s => s.Id == request.SeanceId)
            .Select(s => new { s.RoomId, s.StartsAtUtc })
            .FirstOrDefaultAsync(cancellationToken);

        if (seance is null)
        {
            throw new KeyNotFoundException("Screening not found.");
        }

        if (seance.StartsAtUtc <= DateTime.UtcNow)
        {
            throw new ArgumentException("Cannot book a screening that has already started.");
        }

        var seatIds = request.SeatIds.ToArray();

        var validSeatCount = await _db.Seats.AsNoTracking()
            .CountAsync(s => s.RoomId == seance.RoomId && seatIds.Contains(s.Id),
                cancellationToken);

        if (validSeatCount != seatIds.Length)
        {
            throw new ArgumentException("One or more seats do not belong to this screening room.");
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var alreadyReserved = await _db.ReservationSeats.AsNoTracking()
            .AnyAsync(rs => rs.SeanceId == request.SeanceId &&
                            seatIds.Contains(rs.SeatId), cancellationToken);

        if (alreadyReserved)
        {
            throw new SeatAlreadyBookedException();
        }

        var reservation = new Reservation
        {
            UserId = userId,
            SeanceId = request.SeanceId,
            ReservationSeats = seatIds.Select(seatId => new ReservationSeat
            {
                SeanceId = request.SeanceId,
                SeatId = seatId
            }).ToList()
        };

        _db.Reservations.Add(reservation);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_ReservationSeats_SeanceId_SeatId"
            })
        {
            // The database unique index is the final guard against racing requests.
            throw new SeatAlreadyBookedException();
        }

        return new ReservationResultDto(
            reservation.Id,
            reservation.SeanceId,
            reservation.TicketCode,
            reservation.ReservedAtUtc,
            seatIds.ToList());
    }
}
