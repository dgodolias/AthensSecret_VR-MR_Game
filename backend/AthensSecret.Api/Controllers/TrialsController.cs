using AthensSecret.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrialsController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly Random _random;

    public TrialsController(ApiDbContext context)
    {
        _context = context;
        _random = new Random();
    }

    [HttpPost("patience")]
    public async Task<IActionResult> PatienceChoice([FromBody] PatienceChoiceRequest request)
    {
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == request.SessionId && gs.IsActive);

        if (gameSession == null)
        {
            return NotFound("Game session not found");
        }

        bool isCorrect = false;
        string result = "";
        int energyChange = 0;

        // Logic for patience trial (mirrors)
        if (request.Choice == 1) // Correct mirror
        {
            isCorrect = true;
            energyChange = 25;
            result = "Σωστή επιλογή! Η αντανάκλασή σας είναι καθαρή.";
        }
        else if (request.Choice == 2) // Wrong mirror
        {
            isCorrect = false;
            energyChange = -25;
            result = "Λάθος επιλογή. Η αντανάκλαση παραμορφώθηκε.";
        }
        else if (request.Choice == 3) // Wait option
        {
            isCorrect = true;
            energyChange = 50; // Bonus for patience
            result = "Εξαιρετική επιλογή! Η υπομονή σας ανταμείφθηκε.";
        }

        gameSession.WisdomEnergy += energyChange;
        gameSession.Score += isCorrect ? 100 : 0;
        gameSession.CurrentTrial = "resource";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            IsCorrect = isCorrect,
            Result = result,
            EnergyChange = energyChange,
            NewWisdomEnergy = gameSession.WisdomEnergy,
            NewScore = gameSession.Score,
            NextTrial = gameSession.CurrentTrial
        });
    }

    [HttpPost("resource")]
    public async Task<IActionResult> ResourceChoice([FromBody] ResourceChoiceRequest request)
    {
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == request.SessionId && gs.IsActive);

        if (gameSession == null)
        {
            return NotFound("Game session not found");
        }

        string result = "";
        int energyChange = 0;

        if (request.PlantOlive)
        {
            // Investment choice - lose energy now, gain more later
            energyChange = -50;
            result = "Φυτέψατε την ελιά. Θα αποδώσει καρπούς στο μέλλον.";
            // TODO: Implement passive energy gain over time
        }
        else
        {
            // Immediate gain choice
            energyChange = 25;
            result = "Πήρατε άμεσο κέρδος ενέργειας.";
        }

        gameSession.WisdomEnergy += energyChange;
        gameSession.Score += 50; // Points for making any choice
        gameSession.CurrentTrial = "risk";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Result = result,
            EnergyChange = energyChange,
            NewWisdomEnergy = gameSession.WisdomEnergy,
            NewScore = gameSession.Score,
            NextTrial = gameSession.CurrentTrial
        });
    }

    [HttpPost("risk")]
    public async Task<IActionResult> RiskChoice([FromBody] RiskChoiceRequest request)
    {
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == request.SessionId && gs.IsActive);

        if (gameSession == null)
        {
            return NotFound("Game session not found");
        }

        string result = "";
        int energyChange = 0;
        bool isSuccess = false;

        if (request.ChooseSafePath)
        {
            // Safe path - guaranteed small gain
            energyChange = 25;
            result = "Επιλέξατε το ασφαλές μονοπάτι. Μικρό αλλά σίγουρο κέρδος.";
            isSuccess = true;
        }
        else
        {
            // Risky path - 50% chance for big gain or big loss
            if (_random.NextDouble() > 0.5) // 50% success rate
            {
                energyChange = 100;
                result = "Το ρίσκο σας αποδόθηκε! Μεγάλο κέρδος ενέργειας.";
                isSuccess = true;
            }
            else
            {
                energyChange = -100;
                result = "Το αβέβαιο μονοπάτι οδήγησε σε απώλεια ενέργειας.";
                isSuccess = false;
            }
        }

        gameSession.WisdomEnergy += energyChange;
        gameSession.Score += isSuccess ? 150 : 25;
        gameSession.CurrentTrial = "completed";

        // Ensure energy doesn't go below 0
        if (gameSession.WisdomEnergy < 0)
        {
            gameSession.WisdomEnergy = 0;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            IsSuccess = isSuccess,
            Result = result,
            EnergyChange = energyChange,
            NewWisdomEnergy = gameSession.WisdomEnergy,
            NewScore = gameSession.Score,
            NextTrial = gameSession.CurrentTrial,
            GameCompleted = gameSession.CurrentTrial == "completed"
        });
    }
}

public class PatienceChoiceRequest
{
    public int SessionId { get; set; }
    public int Choice { get; set; } // 1 = Mirror 1, 2 = Mirror 2, 3 = Wait
}

public class ResourceChoiceRequest
{
    public int SessionId { get; set; }
    public bool PlantOlive { get; set; }
}

public class RiskChoiceRequest
{
    public int SessionId { get; set; }
    public bool ChooseSafePath { get; set; }
}
