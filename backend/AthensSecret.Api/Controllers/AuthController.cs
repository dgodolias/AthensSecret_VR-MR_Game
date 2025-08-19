using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AthensSecret.Api.Data;
using AthensSecret.Api.Models;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApiDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Check if username already exists
            if (await _context.Players.AnyAsync(p => p.Username == request.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new player
            var player = new Player
            {
                Username = request.Username,
                PasswordHash = passwordHash
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(player);

            return Ok(new AuthResponse
            {
                Token = token,
                Username = player.Username,
                PlayerId = player.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Find player by username
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Username == request.Username);
            if (player == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, player.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Generate JWT token
            var token = GenerateJwtToken(player);

            return Ok(new AuthResponse
            {
                Token = token,
                Username = player.Username,
                PlayerId = player.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // With JWT tokens, logout is handled client-side by removing the token
        // Server-side logout would require token blacklisting which is more complex
        return Ok(new { message = "Logged out successfully" });
    }

    private string GenerateJwtToken(Player player)
    {
        var jwtKey = _configuration["JwtSettings:Key"] ?? "your-super-secret-jwt-key-that-is-at-least-32-characters-long";
        var key = Encoding.ASCII.GetBytes(jwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                new Claim(ClaimTypes.Name, player.Username),
                new Claim("PlayerId", player.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
