using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cinema.Api.Data;
using Cinema.Api.Dtos;
using Cinema.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly CinemaDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public AuthController(CinemaDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            return Conflict(new { message = "Email already registered." });
        }

        var user = new AppUser { Email = email, Role = "User" };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            return Conflict(new { message = "Email already registered." });
        }

        return StatusCode(StatusCodes.Status201Created,
            new { user.Id, user.Email, user.Role });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null || _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var token = new JwtSecurityToken(
            issuer: "Cinema.Api",
            audience: "Cinema.Web",
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.Role)
            },
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));

        return Ok(new AuthResponse(
            user.Id,
            user.Email,
            user.Role,
            new JwtSecurityTokenHandler().WriteToken(token)));
    }
}
