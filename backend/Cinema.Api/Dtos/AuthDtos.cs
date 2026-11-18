
using System.ComponentModel.DataAnnotations;

namespace Cinema.Api.Dtos;

public record RegisterRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required, MinLength(8)] string Password);

public record LoginRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required] string Password);

public record AuthResponse(
    int UserId,
    string Email,
    string Role,
    string Token);
