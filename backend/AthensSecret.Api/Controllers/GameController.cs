using AthensSecret.Api.Data;
using AthensSecret.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints in this controller
public class GameController : ControllerBase
{
    private readonly ApiDbContext _context;

    public GameController(ApiDbContext context)
    {
        _context = context;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartGame()
    {
        // Get player ID from JWT token
        var playerIdClaim = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(playerIdClaim) || !int.TryParse(playerIdClaim, out int playerId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        // Get player from database
        var player = await _context.Players.FindAsync(playerId);
        if (player == null)
        {
            return NotFound(new { message = "Player not found" });
        }

        // Create new game session
        var gameSession = new GameSession
        {
            PlayerId = player.Id,
            WisdomEnergy = 50, // Starting energy
            StartTime = DateTime.UtcNow,
            CurrentTrial = "start",
            Score = 0,
            IsActive = true
        };

        _context.GameSessions.Add(gameSession);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            SessionId = gameSession.Id,
            PlayerId = player.Id,
            Username = player.Username,
            WisdomEnergy = gameSession.WisdomEnergy,
            CurrentTrial = gameSession.CurrentTrial,
            Score = gameSession.Score
        });
    }

    [HttpGet("{sessionId}/state")]
    public async Task<IActionResult> GetGameState(int sessionId)
    {
        var gameSession = await _context.GameSessions
            .Include(gs => gs.Player)
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.IsActive);

        if (gameSession == null)
        {
            return NotFound("Game session not found or inactive");
        }

        return Ok(new
        {
            SessionId = gameSession.Id,
            PlayerId = gameSession.PlayerId,
            Username = gameSession.Player?.Username,
            WisdomEnergy = gameSession.WisdomEnergy,
            CurrentTrial = gameSession.CurrentTrial,
            Score = gameSession.Score,
            StartTime = gameSession.StartTime,
            IsActive = gameSession.IsActive
        });
    }

    [HttpPost("{sessionId}/end")]
    public async Task<IActionResult> EndGame(int sessionId)
    {
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.IsActive);

        if (gameSession == null)
        {
            return NotFound("Game session not found or already ended");
        }

        gameSession.IsActive = false;
        gameSession.EndTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Game ended successfully", Score = gameSession.Score });
    }

    [HttpGet("player/{username}/sessions")]
    public async Task<IActionResult> GetPlayerSessions(string username)
    {
        var player = await _context.Players
            .Include(p => p.GameSessions)
            .FirstOrDefaultAsync(p => p.Username == username);

        if (player == null)
        {
            return NotFound("Player not found");
        }

        var sessions = player.GameSessions
            .OrderByDescending(gs => gs.StartTime)
            .Select(gs => new
            {
                SessionId = gs.Id,
                WisdomEnergy = gs.WisdomEnergy,
                Score = gs.Score,
                StartTime = gs.StartTime,
                EndTime = gs.EndTime,
                IsActive = gs.IsActive,
                CurrentTrial = gs.CurrentTrial
            })
            .ToList();

        return Ok(new
        {
            Username = player.Username,
            TotalSessions = sessions.Count,
            Sessions = sessions
        });
    }
}

public class StartGameRequest
{
    public required string Username { get; set; }
}
