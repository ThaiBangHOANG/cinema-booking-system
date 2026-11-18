
using System.ComponentModel.DataAnnotations;

namespace Cinema.Api.Dtos;

public record CreateReservationRequest(
    int SeanceId,
    [param: Required] List<int> SeatIds);

public record ReservationResultDto(
    int ReservationId,
    int SeanceId,
    string TicketCode,
    DateTime ReservedAtUtc,
    List<int> SeatIds);
