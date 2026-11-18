
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cinema.Api.Data;

public class CinemaDbContextFactory
    : IDesignTimeDbContextFactory<CinemaDbContext>
{
    public CinemaDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<CinemaDbContext>()
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is not configured.");

        var optionsBuilder =
            new DbContextOptionsBuilder<CinemaDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new CinemaDbContext(optionsBuilder.Options);
    }
}
