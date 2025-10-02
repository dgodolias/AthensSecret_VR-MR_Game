using AthensSecret.Api.Data;
using AthensSecret.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly ApiDbContext _context;

    public ConfigController(ApiDbContext context)
    {
        _context = context;
    }

    // Get game configuration (for game logic)
    [HttpGet]
    public async Task<IActionResult> GetConfiguration()
    {
        var config = await _context.GameConfigurations.OrderBy(c => c.Id).FirstOrDefaultAsync();

        if (config == null)
        {
            return NotFound("Game configuration not found");
        }

        return Ok(new
        {
            id = config.Id,
            startingWisdom = config.StartingWisdom,
            mirrorWisdomIfWaits = config.MirrorWisdomIfWaits,
            mirrorWisdomIfRisksCorrectly = config.MirrorWisdomIfRisksCorrectly,
            mirrorWisdomIfRisksFalsely = config.MirrorWisdomIfRisksFalsely,
            oliveTreeWisdomNotInvestment = config.OliveTreeWisdomNotInvestment,
            oliveTreeWisdomInvestmentFunction = config.OliveTreeWisdomInvestmentFunction,
            safePathWisdom = config.SafePathWisdom,
            uncertainPathWisdom = config.UncertainPathWisdom,
            uncertainPathWisdomSmallPlank = config.UncertainPathWisdomSmallPlank,
            uncertainPathWisdomMediumPlank = config.UncertainPathWisdomMediumPlank,
            uncertainPathWisdomBigPlank = config.UncertainPathWisdomBigPlank,
            unlockWisdomHiddenRoom = config.UnlockWisdomHiddenRoom
        });
    }

    // Get configuration by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConfiguration(int id)
    {
        var config = await _context.GameConfigurations.FindAsync(id);

        if (config == null)
        {
            return NotFound("Game configuration not found");
        }

        return Ok(new
        {
            id = config.Id,
            startingWisdom = config.StartingWisdom,
            mirrorWisdomIfWaits = config.MirrorWisdomIfWaits,
            mirrorWisdomIfRisksCorrectly = config.MirrorWisdomIfRisksCorrectly,
            mirrorWisdomIfRisksFalsely = config.MirrorWisdomIfRisksFalsely,
            oliveTreeWisdomNotInvestment = config.OliveTreeWisdomNotInvestment,
            oliveTreeWisdomInvestmentFunction = config.OliveTreeWisdomInvestmentFunction,
            safePathWisdom = config.SafePathWisdom,
            uncertainPathWisdom = config.UncertainPathWisdom,
            uncertainPathWisdomSmallPlank = config.UncertainPathWisdomSmallPlank,
            uncertainPathWisdomMediumPlank = config.UncertainPathWisdomMediumPlank,
            uncertainPathWisdomBigPlank = config.UncertainPathWisdomBigPlank,
            unlockWisdomHiddenRoom = config.UnlockWisdomHiddenRoom
        });
    }

    // Create new configuration (admin only)
    [HttpPost]
    public async Task<IActionResult> CreateConfiguration([FromBody] GameConfigurationRequest request)
    {
        var config = new GameConfiguration
        {
            StartingWisdom = request.StartingWisdom,
            MirrorWisdomIfWaits = request.MirrorWisdomIfWaits,
            MirrorWisdomIfRisksCorrectly = request.MirrorWisdomIfRisksCorrectly,
            MirrorWisdomIfRisksFalsely = request.MirrorWisdomIfRisksFalsely,
            OliveTreeWisdomNotInvestment = request.OliveTreeWisdomNotInvestment,
            OliveTreeWisdomInvestmentFunction = request.OliveTreeWisdomInvestmentFunction,
            SafePathWisdom = request.SafePathWisdom,
            UncertainPathWisdom = request.UncertainPathWisdom,
            UncertainPathWisdomSmallPlank = request.UncertainPathWisdomSmallPlank,
            UncertainPathWisdomMediumPlank = request.UncertainPathWisdomMediumPlank,
            UncertainPathWisdomBigPlank = request.UncertainPathWisdomBigPlank,
            UnlockWisdomHiddenRoom = request.UnlockWisdomHiddenRoom
        };

        _context.GameConfigurations.Add(config);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetConfiguration), new { id = config.Id }, new
        {
            id = config.Id,
            startingWisdom = config.StartingWisdom,
            mirrorWisdomIfWaits = config.MirrorWisdomIfWaits,
            mirrorWisdomIfRisksCorrectly = config.MirrorWisdomIfRisksCorrectly,
            mirrorWisdomIfRisksFalsely = config.MirrorWisdomIfRisksFalsely,
            oliveTreeWisdomNotInvestment = config.OliveTreeWisdomNotInvestment,
            oliveTreeWisdomInvestmentFunction = config.OliveTreeWisdomInvestmentFunction,
            safePathWisdom = config.SafePathWisdom,
            uncertainPathWisdom = config.UncertainPathWisdom,
            uncertainPathWisdomSmallPlank = config.UncertainPathWisdomSmallPlank,
            uncertainPathWisdomMediumPlank = config.UncertainPathWisdomMediumPlank,
            uncertainPathWisdomBigPlank = config.UncertainPathWisdomBigPlank,
            unlockWisdomHiddenRoom = config.UnlockWisdomHiddenRoom
        });
    }

    // Update configuration (admin only)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConfiguration(int id, [FromBody] GameConfigurationRequest request)
    {
        var config = await _context.GameConfigurations.FindAsync(id);

        if (config == null)
        {
            return NotFound("Game configuration not found");
        }

        config.StartingWisdom = request.StartingWisdom;
        config.MirrorWisdomIfWaits = request.MirrorWisdomIfWaits;
        config.MirrorWisdomIfRisksCorrectly = request.MirrorWisdomIfRisksCorrectly;
        config.MirrorWisdomIfRisksFalsely = request.MirrorWisdomIfRisksFalsely;
        config.OliveTreeWisdomNotInvestment = request.OliveTreeWisdomNotInvestment;
        config.OliveTreeWisdomInvestmentFunction = request.OliveTreeWisdomInvestmentFunction;
        config.SafePathWisdom = request.SafePathWisdom;
        config.UncertainPathWisdom = request.UncertainPathWisdom;
        config.UncertainPathWisdomSmallPlank = request.UncertainPathWisdomSmallPlank;
        config.UncertainPathWisdomMediumPlank = request.UncertainPathWisdomMediumPlank;
        config.UncertainPathWisdomBigPlank = request.UncertainPathWisdomBigPlank;
        config.UnlockWisdomHiddenRoom = request.UnlockWisdomHiddenRoom;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = config.Id,
            startingWisdom = config.StartingWisdom,
            mirrorWisdomIfWaits = config.MirrorWisdomIfWaits,
            mirrorWisdomIfRisksCorrectly = config.MirrorWisdomIfRisksCorrectly,
            mirrorWisdomIfRisksFalsely = config.MirrorWisdomIfRisksFalsely,
            oliveTreeWisdomNotInvestment = config.OliveTreeWisdomNotInvestment,
            oliveTreeWisdomInvestmentFunction = config.OliveTreeWisdomInvestmentFunction,
            safePathWisdom = config.SafePathWisdom,
            uncertainPathWisdom = config.UncertainPathWisdom,
            uncertainPathWisdomSmallPlank = config.UncertainPathWisdomSmallPlank,
            uncertainPathWisdomMediumPlank = config.UncertainPathWisdomMediumPlank,
            uncertainPathWisdomBigPlank = config.UncertainPathWisdomBigPlank,
            unlockWisdomHiddenRoom = config.UnlockWisdomHiddenRoom
        });
    }

    // Delete configuration (admin only)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConfiguration(int id)
    {
        var config = await _context.GameConfigurations.FindAsync(id);

        if (config == null)
        {
            return NotFound("Game configuration not found");
        }

        _context.GameConfigurations.Remove(config);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Get all configurations (admin only)
    [HttpGet("all")]
    public async Task<IActionResult> GetAllConfigurations()
    {
        var configs = await _context.GameConfigurations
            .Select(c => new
            {
                id = c.Id,
                startingWisdom = c.StartingWisdom,
                mirrorWisdomIfWaits = c.MirrorWisdomIfWaits,
                mirrorWisdomIfRisksCorrectly = c.MirrorWisdomIfRisksCorrectly,
                mirrorWisdomIfRisksFalsely = c.MirrorWisdomIfRisksFalsely,
                oliveTreeWisdomNotInvestment = c.OliveTreeWisdomNotInvestment,
                oliveTreeWisdomInvestmentFunction = c.OliveTreeWisdomInvestmentFunction,
                safePathWisdom = c.SafePathWisdom,
                uncertainPathWisdom = c.UncertainPathWisdom,
                uncertainPathWisdomSmallPlank = c.UncertainPathWisdomSmallPlank,
                uncertainPathWisdomMediumPlank = c.UncertainPathWisdomMediumPlank,
                uncertainPathWisdomBigPlank = c.UncertainPathWisdomBigPlank,
                unlockWisdomHiddenRoom = c.UnlockWisdomHiddenRoom
            })
            .ToListAsync();

        return Ok(configs);
    }
}

// Request model for configuration
public class GameConfigurationRequest
{
    public int StartingWisdom { get; set; }
    public int MirrorWisdomIfWaits { get; set; }
    public int MirrorWisdomIfRisksCorrectly { get; set; }
    public int MirrorWisdomIfRisksFalsely { get; set; }
    public int OliveTreeWisdomNotInvestment { get; set; }
    public string OliveTreeWisdomInvestmentFunction { get; set; } = string.Empty;
    public int SafePathWisdom { get; set; }
    public int UncertainPathWisdom { get; set; }
    public int UncertainPathWisdomSmallPlank { get; set; }
    public int UncertainPathWisdomMediumPlank { get; set; }
    public int UncertainPathWisdomBigPlank { get; set; }
    public int UnlockWisdomHiddenRoom { get; set; }
}
