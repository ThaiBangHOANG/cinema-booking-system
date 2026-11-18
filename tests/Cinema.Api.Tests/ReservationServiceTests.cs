using Cinema.Api.Data;
using Cinema.Api.Dtos;
using Cinema.Api.Models;
using Cinema.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Cinema.Api.Tests;

public sealed class ReservationServiceTests
{
    [Fact]
    public void Reservation_DefaultTicketCode_IsUniqueAndHasExpectedLength()
    {
        var first = new Reservation();
        var second = new Reservation();

        Assert.Equal(32, first.TicketCode.Length);
        Assert.Equal(32, second.TicketCode.Length);
        Assert.NotEqual(first.TicketCode, second.TicketCode);
    }

    [Fact]
    public async Task CreateAsync_ValidSeats_CreatesReservationAndTicket()
    {
        await using var data = await TestData.CreateAsync();
        var service = new ReservationService(data.Db);

        var result = await service.CreateAsync(
            data.UserId,
            new CreateReservationRequest(data.SeanceId, [data.Seat1Id, data.Seat2Id]));

        Assert.True(result.ReservationId > 0);
        Assert.Equal(data.SeanceId, result.SeanceId);
        Assert.Equal(32, result.TicketCode.Length);
        Assert.Equal(2, result.SeatIds.Count);
        Assert.Equal(2, await data.Db.ReservationSeats.CountAsync(
            rs => rs.ReservationId == result.ReservationId));
    }

    [Fact]
    public async Task CreateAsync_AlreadyBookedSeat_ThrowsSeatAlreadyBooked()
    {
        await using var data = await TestData.CreateAsync();
        var service = new ReservationService(data.Db);
        var request = new CreateReservationRequest(data.SeanceId, [data.Seat1Id]);

        await service.CreateAsync(data.UserId, request);

        await Assert.ThrowsAsync<SeatAlreadyBookedException>(
            () => service.CreateAsync(data.UserId, request));
    }

    [Fact]
    public async Task CreateAsync_DuplicateSeatIds_ThrowsArgumentException()
    {
        await using var data = await TestData.CreateAsync();
        var service = new ReservationService(data.Db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(
            data.UserId,
            new CreateReservationRequest(data.SeanceId, [data.Seat1Id, data.Seat1Id])));
    }

    [Fact]
    public async Task CreateAsync_EmptySelection_ThrowsArgumentException()
    {
        await using var data = await TestData.CreateAsync();
        var service = new ReservationService(data.Db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(
            data.UserId,
            new CreateReservationRequest(data.SeanceId, [])));
    }

    [Fact]
    public async Task CreateAsync_SeatFromAnotherRoom_ThrowsArgumentException()
    {
        await using var data = await TestData.CreateAsync();
        var service = new ReservationService(data.Db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(
            data.UserId,
            new CreateReservationRequest(data.SeanceId, [data.OtherRoomSeatId])));
    }

    [Fact]
    public async Task CreateAsync_UnknownSeance_ThrowsKeyNotFoundException()
    {
        await using var data = await TestData.CreateAsync();
        var service = new ReservationService(data.Db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(
            data.UserId,
            new CreateReservationRequest(999999, [data.Seat1Id])));
    }

    [Fact]
    public async Task CreateAsync_StartedSeance_ThrowsArgumentException()
    {
        await using var data = await TestData.CreateAsync();
        var service = new ReservationService(data.Db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(
            data.UserId,
            new CreateReservationRequest(data.PastSeanceId, [data.Seat1Id])));
    }

    [Fact]
    public async Task CreateAsync_SameSeatInDifferentSeances_IsAllowed()
    {
        await using var data = await TestData.CreateAsync();
        var service = new ReservationService(data.Db);

        var first = await service.CreateAsync(data.UserId,
            new CreateReservationRequest(data.SeanceId, [data.Seat1Id]));
        var second = await service.CreateAsync(data.UserId,
            new CreateReservationRequest(data.OtherSeanceId, [data.Seat1Id]));

        Assert.NotEqual(first.ReservationId, second.ReservationId);
        Assert.NotEqual(first.TicketCode, second.TicketCode);
        Assert.Equal(2, await data.Db.ReservationSeats.CountAsync());
    }

    [Fact]
    public async Task Database_UniqueIndex_RejectsDuplicateSeatForSameSeance()
    {
        await using var data = await TestData.CreateAsync();

        data.Db.Reservations.Add(new Reservation
        {
            UserId = data.UserId,
            SeanceId = data.SeanceId,
            ReservationSeats = [new ReservationSeat
            {
                SeatId = data.Seat1Id,
                SeanceId = data.SeanceId
            }]
        });
        await data.Db.SaveChangesAsync();

        data.Db.Reservations.Add(new Reservation
        {
            UserId = data.UserId,
            SeanceId = data.SeanceId,
            ReservationSeats = [new ReservationSeat
            {
                SeatId = data.Seat1Id,
                SeanceId = data.SeanceId
            }]
        });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => data.Db.SaveChangesAsync());
    }

    private sealed class TestData : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public CinemaDbContext Db { get; }
        public int UserId { get; }
        public int SeanceId { get; }
        public int OtherSeanceId { get; }
        public int PastSeanceId { get; }
        public int Seat1Id { get; }
        public int Seat2Id { get; }
        public int OtherRoomSeatId { get; }

        private TestData(
            SqliteConnection connection,
            CinemaDbContext db,
            int userId,
            int seanceId,
            int otherSeanceId,
            int pastSeanceId,
            int seat1Id,
            int seat2Id,
            int otherRoomSeatId)
        {
            _connection = connection;
            Db = db;
            UserId = userId;
            SeanceId = seanceId;
            OtherSeanceId = otherSeanceId;
            PastSeanceId = pastSeanceId;
            Seat1Id = seat1Id;
            Seat2Id = seat2Id;
            OtherRoomSeatId = otherRoomSeatId;
        }

        public static async Task<TestData> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<CinemaDbContext>()
                .UseSqlite(connection)
                .Options;
            var db = new CinemaDbContext(options);
            await db.Database.EnsureCreatedAsync();

            var user = new AppUser
            {
                Email = "test.user@example.test",
                PasswordHash = "test-hash-only",
                Role = "User"
            };
            var film = new Film
            {
                Title = "Film Test",
                Description = "Fixture for xUnit tests",
                DurationMinutes = 100
            };
            var room = new Room
            {
                Name = "Salle Test",
                Capacity = 2,
                Seats =
                [
                    new Seat { SeatNumber = "A01" },
                    new Seat { SeatNumber = "A02" }
                ]
            };
            var otherRoom = new Room
            {
                Name = "Autre Salle",
                Capacity = 1,
                Seats = [new Seat { SeatNumber = "B01" }]
            };

            var firstSeance = new Seance
            {
                Film = film,
                Room = room,
                StartsAtUtc = DateTime.UtcNow.AddDays(1)
            };
            var secondSeance = new Seance
            {
                Film = film,
                Room = room,
                StartsAtUtc = DateTime.UtcNow.AddDays(2)
            };
            var pastSeance = new Seance
            {
                Film = film,
                Room = room,
                StartsAtUtc = DateTime.UtcNow.AddDays(-1)
            };

            db.AddRange(user, film, room, otherRoom, firstSeance, secondSeance, pastSeance);
            await db.SaveChangesAsync();

            return new TestData(
                connection,
                db,
                user.Id,
                firstSeance.Id,
                secondSeance.Id,
                pastSeance.Id,
                room.Seats.Single(s => s.SeatNumber == "A01").Id,
                room.Seats.Single(s => s.SeatNumber == "A02").Id,
                otherRoom.Seats.Single().Id);
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
