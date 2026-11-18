using System.Security.Claims;
using Cinema.Api.Dtos;
using Cinema.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Authorize(Roles = "User")]
[Route("api/reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly ReservationService _reservations;

    public ReservationsController(ReservationService reservations)
    {
        _reservations = reservations;
    }

    [HttpPost]
    public async Task<ActionResult<ReservationResultDto>> Create(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue("sub"), out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _reservations.CreateAsync(
                userId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
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
}
