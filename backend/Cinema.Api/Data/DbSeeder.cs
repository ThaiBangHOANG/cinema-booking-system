
using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<CinemaDbContext>();

        // 1. Create 10 rooms and 1700 seats
        if (!await db.Rooms.AnyAsync())
        {
            int[] capacities =
            {
                240, 220, 200, 190, 180,
                170, 150, 140, 120, 90
            };

            for (int i = 0; i < capacities.Length; i++)
            {
                var room = new Room
                {
                    Name = $"Salle {i + 1}",
                    Capacity = capacities[i]
                };

                for (int n = 1; n <= capacities[i]; n++)
                {
                    char row = (char)('A' + (n - 1) / 20);
                    int number = (n - 1) % 20 + 1;

                    room.Seats.Add(new Seat
                    {
                        SeatNumber = $"{row}{number:00}"
                    });
                }

                db.Rooms.Add(room);
            }

            await db.SaveChangesAsync();
        }

        // 2. Create demo films
        if (!await db.Films.AnyAsync())
        {
            db.Films.AddRange(
                new Film
                {
                    Title = "La Nuit du Jazz",
                    Description = "Un voyage musical inoubliable.",
                    DurationMinutes = 110
                },
                new Film
                {
                    Title = "Les Etoiles de Paris",
                    Description = "Une aventure romantique.",
                    DurationMinutes = 120
                },
                new Film
                {
                    Title = "Le Dernier Train",
                    Description = "Un thriller mystérieux.",
                    DurationMinutes = 105
                },
                new Film
                {
                    Title = "Voyage vers Demain",
                    Description = "Une aventure de science-fiction.",
                    DurationMinutes = 130
                },
                new Film
                {
                    Title = "Horizons",
                    Description = "Un drame familial émouvant.",
                    DurationMinutes = 115
                }
            );

            await db.SaveChangesAsync();
        }

        // 3. Create screenings for the next 14 days
        var rooms = await db.Rooms
            .OrderBy(r => r.Id)
            .ToListAsync();

        var films = await db.Films
            .OrderBy(f => f.Id)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var endDate = today.AddDays(14);

        var existing = await db.Seances
            .Where(s => s.StartsAtUtc >= today
                     && s.StartsAtUtc < endDate)
            .Select(s => new { s.RoomId, s.StartsAtUtc })
            .ToListAsync();

        var existingKeys = existing
            .Select(s => (s.RoomId, s.StartsAtUtc))
            .ToHashSet();

        int[] hours = { 14, 19 };

        for (int day = 0; day < 14; day++)
        {
            foreach (var room in rooms)
            {
                for (int slot = 0; slot < hours.Length; slot++)
                {
                    var start = today
                        .AddDays(day)
                        .AddHours(hours[slot]);

                    if (!existingKeys.Add((room.Id, start)))
                        continue;

                    db.Seances.Add(new Seance
                    {
                        RoomId = room.Id,
                        FilmId = films[
                            (room.Id + day + slot) % films.Count
                        ].Id,
                        StartsAtUtc = start
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }
}
