using AthensSecret.Api.Data;
using AthensSecret.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly ApiDbContext _context;

    public GameController(ApiDbContext context)
    {
        _context = context;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartGame([FromBody] StartGameRequest request)
    {
        // Check if player exists, create if not
        var player = await _context.Players.FirstOrDefaultAsync(p => p.Username == request.Username);
        if (player == null)
        {
            player = new Player { Username = request.Username };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
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
}

public class StartGameRequest
{
    public required string Username { get; set; }
}
