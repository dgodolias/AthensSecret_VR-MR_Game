using AthensSecret.Api.Data;
using AthensSecret.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("ApiPolicy")]
public class PlayerController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<PlayerController> _logger;

    public PlayerController(ApiDbContext context, ILogger<PlayerController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Register a new player with webform data (no password needed)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterPlayer([FromBody] PlayerRegistrationRequest request)
    {
        try
        {
            _logger.LogInformation("Player registration attempt from IP: {ClientIP}", 
                HttpContext.Connection.RemoteIpAddress?.ToString());

            // Input validation
            if (request == null)
            {
                _logger.LogWarning("Invalid registration request: null request data");
                return BadRequest(new { message = "Invalid request data" });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.FirstName))
                return BadRequest(new { message = "First name is required" });
            
            if (string.IsNullOrWhiteSpace(request.LastName))
                return BadRequest(new { message = "Last name is required" });
            
            // Athens Secret game requires age 18+ (adult content); VR Park uses 1-120
            if (request.Age < 18 || request.Age > 120)
                return BadRequest(new { message = "Age must be between 18 and 120" });
            
            if (request.Q1 < 1 || request.Q1 > 10)
                return BadRequest(new { message = "Q1 must be between 1 and 10" });
            
            if (request.Q2 < 1 || request.Q2 > 10)
                return BadRequest(new { message = "Q2 must be between 1 and 10" });

            // Validate string lengths
            if (request.FirstName.Length > 100)
                return BadRequest(new { message = "First name too long" });
            
            if (request.LastName.Length > 100)
                return BadRequest(new { message = "Last name too long" });

            // Validate email format if provided
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
                if (!emailRegex.IsMatch(request.Email))
                    return BadRequest(new { message = "Invalid email format" });
                
                if (request.Email.Length > 200)
                    return BadRequest(new { message = "Email too long" });
            }

            // Check if email already exists (only if email is provided)
            if (!string.IsNullOrWhiteSpace(request.Email) && 
                await _context.Players.AnyAsync(p => p.Email == request.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Sanitize input to prevent injection attacks
            var sanitizedFirstName = request.FirstName.Trim();
            var sanitizedLastName = request.LastName.Trim();
            var sanitizedEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();

            // Create new player
            var player = new Player
            {
                FirstName = sanitizedFirstName,
                LastName = sanitizedLastName,
                Email = sanitizedEmail,
                Age = request.Age
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Create response record
            var response = new Response
            {
                PlayerId = player.Id,
                Q1 = request.Q1,  // Patience
                Q2 = request.Q2   // Risk tolerance
            };

            _context.Responses.Add(response);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Player registration successful: PlayerId={PlayerId}, Email={Email}", 
                player.Id, sanitizedEmail ?? "none");

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
            _logger.LogError(ex, "Player registration failed for IP: {ClientIP}", 
                HttpContext.Connection.RemoteIpAddress?.ToString());

            // Log the error but don't expose internal details in production
            if (HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
            {
                return StatusCode(500, new { message = $"Registration failed: {ex.Message}" });
            }
            
            return StatusCode(500, new { message = "Registration failed. Please try again later." });
        }
    }

    /// <summary>
    /// Verify if a player ID exists (simple authentication replacement)
    /// </summary>
    [HttpPost("verify")]
    [EnableRateLimiting("StrictPolicy")]
    public async Task<IActionResult> VerifyPlayer([FromBody] PlayerVerificationRequest request)
    {
        try
        {
            if (request.PlayerId <= 0)
                return BadRequest(new { message = "Invalid Player ID" });

            var player = await _context.Players.OrderBy(p => p.Id).FirstOrDefaultAsync(p => p.Id == request.PlayerId);

            if (player == null)
            {
                // Add random delay to prevent timing-based enumeration attacks
                await Task.Delay(Random.Shared.Next(100, 300));
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
            _logger.LogError(ex, "Player verification error");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get player details by ID
    /// </summary>
    [HttpGet("{playerId}")]
    [EnableRateLimiting("StrictPolicy")]
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
                    Q1_Patience = player.Response.Q1,
                    Q2_Risk = player.Response.Q2
                } : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPlayer error for PlayerId: {PlayerId}", playerId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}