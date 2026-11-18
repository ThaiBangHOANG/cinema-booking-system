
namespace Cinema.Api.Dtos;

public record DailyAffluenceDto(
    DateOnly Date,
    bool IsWeekend,
    bool IsFestival,
    int TotalSeances,
    int TotalSeats,
    int ReservedSeats,
    double OccupancyRate,
    string AlertLevel
);

public record HourlyAffluenceDto(
    int HourUtc,
    int TotalSeances,
    int ReservedSeats,
    double OccupancyRate
);

public record AffluenceReportDto(
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalReservedSeats,
    double GlobalOccupancyRate,
    List<DailyAffluenceDto> DailyTrends,
    List<HourlyAffluenceDto> HourlyTrends
);
