using AthensSecret.Api.Data;
using AthensSecret.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly ApiDbContext _context;

    public PlayerController(ApiDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Register a new player with webform data (no password needed)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterPlayer([FromBody] PlayerRegistrationRequest request)
    {
        try
        {
            // Check if email already exists
            if (await _context.Players.AnyAsync(p => p.Email == request.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Create new player
            var player = new Player
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Age = request.Age
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Create response record
            var response = new Response
            {
                PlayerId = player.Id,
                Q1 = request.Q1,
                Q2 = request.Q2,
                Q3 = request.Q3
            };

            _context.Responses.Add(response);
            await _context.SaveChangesAsync();

            return Ok(new PlayerRegistrationResponse
            {
                PlayerId = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                Email = player.Email,
                Age = player.Age,
                Message = $"Registration successful! Your Player ID is: {player.Id}. Please save this ID to access the game."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Verify if a player ID exists (simple authentication replacement)
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPlayer([FromBody] PlayerVerificationRequest request)
    {
        try
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == request.PlayerId);
            
            if (player == null)
            {
                return Ok(new PlayerVerificationResponse
                {
                    Exists = false,
                    Message = "Player ID not found. Please check your ID or register first."
                });
            }

            return Ok(new PlayerVerificationResponse
            {
                Exists = true,
                FirstName = player.FirstName,
                LastName = player.LastName,
                Email = player.Email,
                Age = player.Age,
                Message = $"Welcome back, {player.FirstName}!"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get player details by ID
    /// </summary>
    [HttpGet("{playerId}")]
    public async Task<IActionResult> GetPlayer(int playerId)
    {
        try
        {
            var player = await _context.Players
                .Include(p => p.Response)
                .Include(p => p.GameSessions)
                .FirstOrDefaultAsync(p => p.Id == playerId);

            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            return Ok(new
            {
                PlayerId = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                Email = player.Email,
                Age = player.Age,
                TotalSessions = player.GameSessions.Count,
                Responses = player.Response != null ? new
                {
                    Q1 = player.Response.Q1,
                    Q2 = player.Response.Q2,
                    Q3 = player.Response.Q3
                } : null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}