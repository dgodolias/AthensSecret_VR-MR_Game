using AthensSecret.Api.Data;
using AthensSecret.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<GameController> _logger;

    public GameController(ApiDbContext context, ILogger<GameController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Start a new game session
    [HttpPost("start")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> StartGame([FromQuery] int playerId)
    {
        try
        {
            _logger.LogInformation("Game start attempt for PlayerId: {PlayerId} from IP: {ClientIP}", 
                playerId, HttpContext.Connection.RemoteIpAddress?.ToString());

            // Validate player ID
            if (playerId <= 0)
            {
                _logger.LogWarning("Invalid PlayerId provided: {PlayerId}", playerId);
                return BadRequest(new { message = "Invalid Player ID" });
            }

            // Check if player exists
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                _logger.LogWarning("Player not found: {PlayerId}", playerId);
                return NotFound(new { message = "Player not found" });
            }

        // Check if player already has an active session (EndedAt is null)
        var activeSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.PlayerId == playerId && gs.EndedAt == null);

        if (activeSession != null)
        {
            return BadRequest(new { message = "Player already has an active game session", sessionId = activeSession.Id });
        }

        // Create new game session
        var gameSession = new GameSession
        {
            PlayerId = playerId,
            StartedAt = DateTime.UtcNow,
            EndedAt = null
        };

            _context.GameSessions.Add(gameSession);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Game session started successfully: SessionId={SessionId}, PlayerId={PlayerId}", 
                gameSession.Id, playerId);

            return Ok(new
            {
                sessionId = gameSession.Id,
                playerId = gameSession.PlayerId,
                startedAt = gameSession.StartedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game session for PlayerId: {PlayerId}", playerId);
            return StatusCode(500, new { message = "Failed to start game session" });
        }
    }

    // End a game session
    [HttpPost("end")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> EndGame([FromQuery] int sessionId, [FromQuery] int playerId)
    {
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        gameSession.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = gameSession.Id,
            playerId = gameSession.PlayerId,
            startedAt = gameSession.StartedAt,
            endedAt = gameSession.EndedAt
        });
    }

    // Get game session info
    [HttpGet("session/{sessionId}")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> GetGameSession(int sessionId, [FromQuery] int playerId)
    {
        var gameSession = await _context.GameSessions
            .Include(gs => gs.Player)
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId);

        if (gameSession == null)
        {
            return NotFound("Game session not found");
        }

        return Ok(new
        {
            sessionId = gameSession.Id,
            playerId = gameSession.PlayerId,
            playerName = gameSession.Player != null ? $"{gameSession.Player.FirstName} {gameSession.Player.LastName}" : "Unknown Player",
            startedAt = gameSession.StartedAt,
            endedAt = gameSession.EndedAt,
            isActive = gameSession.EndedAt == null
        });
    }

    // MIRRORS TRIAL endpoints
    [HttpPost("mirrors/start")]
    public async Task<IActionResult> StartMirrorsTrial([FromQuery] int sessionId, [FromQuery] int playerId)
    {
        // Verify session belongs to player and is active
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        // Check if mirrors trial already exists
        var existingTrial = await _context.MirrorsTrials
            .FirstOrDefaultAsync(mt => mt.GameSessionId == sessionId);

        if (existingTrial != null)
        {
            return BadRequest("Mirrors trial already exists for this session");
        }

        var mirrorsTrial = new MirrorsTrial
        {
            GameSessionId = sessionId,
            StartTime = DateTime.UtcNow,
            EndTime = null,
            TotalGainedWisdom = 0
        };

        _context.MirrorsTrials.Add(mirrorsTrial);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = sessionId,
            startTime = mirrorsTrial.StartTime
        });
    }

    [HttpPost("mirrors/end")]
    public async Task<IActionResult> EndMirrorsTrial([FromQuery] int sessionId, [FromQuery] int playerId, [FromBody] MirrorsTrialEndRequest request)
    {
        // Verify session belongs to player and is active
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        var mirrorsTrial = await _context.MirrorsTrials
            .FirstOrDefaultAsync(mt => mt.GameSessionId == sessionId);

        if (mirrorsTrial == null)
        {
            return NotFound("Mirrors trial not found");
        }

        mirrorsTrial.EndTime = DateTime.UtcNow;
        mirrorsTrial.TotalGainedWisdom = request.TotalGainedWisdom;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = sessionId,
            startTime = mirrorsTrial.StartTime,
            endTime = mirrorsTrial.EndTime,
            totalGainedWisdom = mirrorsTrial.TotalGainedWisdom
        });
    }

    // OLIVE TREE TRIAL endpoints
    [HttpPost("olivetree/start")]
    public async Task<IActionResult> StartOliveTreeTrial([FromQuery] int sessionId, [FromQuery] int playerId)
    {
        // Verify session belongs to player and is active
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        // Check if olive tree trial already exists
        var existingTrial = await _context.OliveTreeTrials
            .FirstOrDefaultAsync(ot => ot.GameSessionId == sessionId);

        if (existingTrial != null)
        {
            return BadRequest("Olive tree trial already exists for this session");
        }

        var oliveTreeTrial = new OliveTreeTrial
        {
            GameSessionId = sessionId,
            StartTime = DateTime.UtcNow,
            EndTime = null,
            TotalGainedWisdom = 0,
            InvestmentStartTime = null // Will be set if player chooses to invest
        };

        _context.OliveTreeTrials.Add(oliveTreeTrial);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = sessionId,
            startTime = oliveTreeTrial.StartTime
        });
    }

    [HttpPost("olivetree/invest")]
    public async Task<IActionResult> SetOliveTreeInvestment([FromQuery] int sessionId, [FromQuery] int playerId)
    {
        // Verify session belongs to player and is active
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        var oliveTreeTrial = await _context.OliveTreeTrials
            .FirstOrDefaultAsync(ot => ot.GameSessionId == sessionId);

        if (oliveTreeTrial == null)
        {
            return NotFound("Olive tree trial not found");
        }

        oliveTreeTrial.InvestmentStartTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = sessionId,
            investmentStartTime = oliveTreeTrial.InvestmentStartTime
        });
    }

    [HttpPost("olivetree/end")]
    public async Task<IActionResult> EndOliveTreeTrial([FromQuery] int sessionId, [FromQuery] int playerId, [FromBody] OliveTreeTrialEndRequest request)
    {
        // Verify session belongs to player and is active
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        var oliveTreeTrial = await _context.OliveTreeTrials
            .FirstOrDefaultAsync(ot => ot.GameSessionId == sessionId);

        if (oliveTreeTrial == null)
        {
            return NotFound("Olive tree trial not found");
        }

        oliveTreeTrial.EndTime = DateTime.UtcNow;
        oliveTreeTrial.TotalGainedWisdom = request.TotalGainedWisdom;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = sessionId,
            startTime = oliveTreeTrial.StartTime,
            endTime = oliveTreeTrial.EndTime,
            investmentStartTime = oliveTreeTrial.InvestmentStartTime,
            totalGainedWisdom = oliveTreeTrial.TotalGainedWisdom
        });
    }

    // PATH TRIAL endpoints
    [HttpPost("path/start")]
    public async Task<IActionResult> StartPathTrial([FromQuery] int sessionId, [FromQuery] int playerId)
    {
        // Verify session belongs to player and is active
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        // Check if path trial already exists
        var existingTrial = await _context.PathTrials
            .FirstOrDefaultAsync(pt => pt.GameSessionId == sessionId);

        if (existingTrial != null)
        {
            return BadRequest("Path trial already exists for this session");
        }

        var pathTrial = new PathTrial
        {
            GameSessionId = sessionId,
            StartTime = DateTime.UtcNow,
            EndTime = null,
            TotalGainedWisdom = 0,
            SafePath = false // Default to unsafe path
        };

        _context.PathTrials.Add(pathTrial);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = sessionId,
            startTime = pathTrial.StartTime
        });
    }

    [HttpPost("path/end")]
    public async Task<IActionResult> EndPathTrial([FromQuery] int sessionId, [FromQuery] int playerId, [FromBody] PathTrialEndRequest request)
    {
        // Verify session belongs to player and is active
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        var pathTrial = await _context.PathTrials
            .FirstOrDefaultAsync(pt => pt.GameSessionId == sessionId);

        if (pathTrial == null)
        {
            return NotFound("Path trial not found");
        }

        pathTrial.EndTime = DateTime.UtcNow;
        pathTrial.TotalGainedWisdom = request.TotalGainedWisdom;
        pathTrial.SafePath = request.SafePath;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = sessionId,
            startTime = pathTrial.StartTime,
            endTime = pathTrial.EndTime,
            totalGainedWisdom = pathTrial.TotalGainedWisdom,
            safePath = pathTrial.SafePath
        });
    }

    // Get all trials for a session
    [HttpGet("session/{sessionId}/trials")]
    public async Task<IActionResult> GetTrials(int sessionId, [FromQuery] int playerId)
    {
        // Verify session belongs to player
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId);

        if (gameSession == null)
        {
            return NotFound("Game session not found");
        }

        var mirrors = await _context.MirrorsTrials
            .FirstOrDefaultAsync(mt => mt.GameSessionId == sessionId);

        var oliveTree = await _context.OliveTreeTrials
            .FirstOrDefaultAsync(ot => ot.GameSessionId == sessionId);

        var path = await _context.PathTrials
            .FirstOrDefaultAsync(pt => pt.GameSessionId == sessionId);

        return Ok(new
        {
            sessionId = sessionId,
            mirrors = mirrors != null ? new
            {
                startTime = mirrors.StartTime,
                endTime = mirrors.EndTime,
                totalGainedWisdom = mirrors.TotalGainedWisdom
            } : null,
            oliveTree = oliveTree != null ? new
            {
                startTime = oliveTree.StartTime,
                endTime = oliveTree.EndTime,
                totalGainedWisdom = oliveTree.TotalGainedWisdom,
                investmentStartTime = oliveTree.InvestmentStartTime
            } : null,
            path = path != null ? new
            {
                startTime = path.StartTime,
                endTime = path.EndTime,
                totalGainedWisdom = path.TotalGainedWisdom,
                safePath = path.SafePath
            } : null
        });
    }

    // Get player statistics by comparing responses with age-based expected values
    [HttpGet("statistics")]
    public async Task<IActionResult> GetPlayerStatistics([FromQuery] int sessionId, [FromQuery] int playerId)
    {
        // Verify session belongs to player and is ended
        var gameSession = await _context.GameSessions
            .Include(gs => gs.Player)
            .ThenInclude(p => p!.Response)
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt != null);

        if (gameSession == null)
        {
            return NotFound("Completed game session not found");
        }

        if (gameSession.Player?.Response == null)
        {
            return NotFound("Player responses not found");
        }

        // Get age-based statistics for this player
        var ageStats = await _context.ResponsesStatistics
            .FirstOrDefaultAsync(rs => rs.Age == gameSession.Player.Age);

        if (ageStats == null)
        {
            return NotFound($"Statistics not available for age {gameSession.Player.Age}");
        }

        var player = gameSession.Player;
        var response = player.Response;

        // Compare Q1 (patience) with expected patience for age
        string q1Comparison = response.Q1 > ageStats.Patience ? "Higher" : 
                             response.Q1 < ageStats.Patience ? "Lower" : "Equal";

        // Compare Q2 (risk) with expected risk for age  
        string q2Comparison = response.Q2 > ageStats.Risk ? "Higher" :
                             response.Q2 < ageStats.Risk ? "Lower" : "Equal";

        return Ok(new
        {
            sessionId = sessionId,
            playerId = playerId,
            playerAge = player.Age,
            playerResponses = new
            {
                Q1_Patience = response.Q1,
                Q2_Risk = response.Q2
            },
            expectedForAge = new
            {
                Patience = ageStats.Patience,
                Risk = ageStats.Risk
            },
            comparison = new
            {
                Q1_vs_Expected_Patience = new
                {
                    PlayerValue = response.Q1,
                    ExpectedValue = ageStats.Patience,
                    Comparison = q1Comparison,
                    Difference = response.Q1 - ageStats.Patience
                },
                Q2_vs_Expected_Risk = new
                {
                    PlayerValue = response.Q2,
                    ExpectedValue = ageStats.Risk,
                    Comparison = q2Comparison,
                    Difference = response.Q2 - ageStats.Risk
                }
            }
        });
    }
}

// Request models
public class MirrorsTrialEndRequest
{
    public int TotalGainedWisdom { get; set; }
}

public class OliveTreeTrialEndRequest
{
    public int TotalGainedWisdom { get; set; }
}

public class PathTrialEndRequest
{
    public int TotalGainedWisdom { get; set; }
    public bool SafePath { get; set; }
}
