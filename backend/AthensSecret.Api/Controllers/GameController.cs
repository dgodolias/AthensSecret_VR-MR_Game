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

    // Start a new game session
    [HttpPost("start")]
    public async Task<IActionResult> StartGame([FromQuery] int playerId)
    {
        // Check if player exists
        var player = await _context.Players.FindAsync(playerId);
        if (player == null)
        {
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

        return Ok(new
        {
            sessionId = gameSession.Id,
            playerId = gameSession.PlayerId,
            startedAt = gameSession.StartedAt
        });
    }

    // End a game session
    [HttpPost("end")]
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

    // OIL TREE TRIAL endpoints
    [HttpPost("oiltree/start")]
    public async Task<IActionResult> StartOilTreeTrial([FromQuery] int sessionId, [FromQuery] int playerId)
    {
        // Verify session belongs to player and is active
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        // Check if oil tree trial already exists
        var existingTrial = await _context.OilTreeTrials
            .FirstOrDefaultAsync(ot => ot.GameSessionId == sessionId);

        if (existingTrial != null)
        {
            return BadRequest("Oil tree trial already exists for this session");
        }

        var oilTreeTrial = new OilTreeTrial
        {
            GameSessionId = sessionId,
            StartTime = DateTime.UtcNow,
            EndTime = null,
            TotalGainedWisdom = 0,
            InvestmentStartTime = null // Will be set if player chooses to invest
        };

        _context.OilTreeTrials.Add(oilTreeTrial);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = sessionId,
            startTime = oilTreeTrial.StartTime
        });
    }

    [HttpPost("oiltree/invest")]
    public async Task<IActionResult> SetOilTreeInvestment([FromQuery] int sessionId, [FromQuery] int playerId)
    {
        // Verify session belongs to player and is active
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        var oilTreeTrial = await _context.OilTreeTrials
            .FirstOrDefaultAsync(ot => ot.GameSessionId == sessionId);

        if (oilTreeTrial == null)
        {
            return NotFound("Oil tree trial not found");
        }

        oilTreeTrial.InvestmentStartTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = sessionId,
            investmentStartTime = oilTreeTrial.InvestmentStartTime
        });
    }

    [HttpPost("oiltree/end")]
    public async Task<IActionResult> EndOilTreeTrial([FromQuery] int sessionId, [FromQuery] int playerId, [FromBody] OilTreeTrialEndRequest request)
    {
        // Verify session belongs to player and is active
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.PlayerId == playerId && gs.EndedAt == null);

        if (gameSession == null)
        {
            return NotFound("Active game session not found");
        }

        var oilTreeTrial = await _context.OilTreeTrials
            .FirstOrDefaultAsync(ot => ot.GameSessionId == sessionId);

        if (oilTreeTrial == null)
        {
            return NotFound("Oil tree trial not found");
        }

        oilTreeTrial.EndTime = DateTime.UtcNow;
        oilTreeTrial.TotalGainedWisdom = request.TotalGainedWisdom;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            sessionId = sessionId,
            startTime = oilTreeTrial.StartTime,
            endTime = oilTreeTrial.EndTime,
            investmentStartTime = oilTreeTrial.InvestmentStartTime,
            totalGainedWisdom = oilTreeTrial.TotalGainedWisdom
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

        var oilTree = await _context.OilTreeTrials
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
            oilTree = oilTree != null ? new
            {
                startTime = oilTree.StartTime,
                endTime = oilTree.EndTime,
                totalGainedWisdom = oilTree.TotalGainedWisdom,
                investmentStartTime = oilTree.InvestmentStartTime
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
                Q2_Risk = response.Q2,
                Q3 = response.Q3
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

public class OilTreeTrialEndRequest
{
    public int TotalGainedWisdom { get; set; }
}

public class PathTrialEndRequest
{
    public int TotalGainedWisdom { get; set; }
    public bool SafePath { get; set; }
}
