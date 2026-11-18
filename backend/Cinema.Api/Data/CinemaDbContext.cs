
using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Data;

public class CinemaDbContext : DbContext
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Film> Films => Set<Film>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Seance> Seances => Set<Seance>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationSeat> ReservationSeats => Set<ReservationSeat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USERS
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(20);
        });

        // FILMS
        modelBuilder.Entity<Film>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Description)
                .HasMaxLength(2000);

            entity.Property(x => x.PosterUrl)
                .HasMaxLength(1000);
        });

        // ROOMS
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.Name).IsUnique();
        });

        // SEATS
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SeatNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(x => x.Room)
                .WithMany(x => x.Seats)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.RoomId,
                x.SeatNumber
            }).IsUnique();
        });

        // SEANCES
        modelBuilder.Entity<Seance>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Film)
                .WithMany(x => x.Seances)
                .HasForeignKey(x => x.FilmId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Room)
                .WithMany(x => x.Seances)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.RoomId,
                x.StartsAtUtc
            }).IsUnique();
        });

        // RESERVATIONS
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TicketCode)
                .IsRequired()
                .HasMaxLength(32);

            entity.HasIndex(x => x.TicketCode).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Seance)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.SeanceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RESERVATION SEATS
        modelBuilder.Entity<ReservationSeat>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Reservation)
                .WithMany(x => x.ReservationSeats)
                .HasForeignKey(x => x.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Seance)
                .WithMany(x => x.ReservationSeats)
                .HasForeignKey(x => x.SeanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Seat)
                .WithMany(x => x.ReservationSeats)
                .HasForeignKey(x => x.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent double booking
            entity.HasIndex(x => new
            {
                x.SeanceId,
                x.SeatId
            }).IsUnique();
        });
    }
}
