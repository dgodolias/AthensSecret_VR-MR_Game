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
        var config = await _context.GameConfigurations.FirstOrDefaultAsync();

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
            oilTreeWisdomNotInvestment = config.OilTreeWisdomNotInvestment,
            oilTreeWisdomInvestmentFunction = config.OilTreeWisdomInvestmentFunction,
            safePathWisdom = config.SafePathWisdom,
            uncertainPathWisdom = config.UncertainPathWisdom,
            uncertainPathWisdomSmallPlank = config.UncertainPathWisdomSmallPlank,
            uncertainPathWisdomMediumPlank = config.UncertainPathWisdomMediumPlank,
            uncertainPathWisdomBigPlank = config.UncertainPathWisdomBigPlank
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
            oilTreeWisdomNotInvestment = config.OilTreeWisdomNotInvestment,
            oilTreeWisdomInvestmentFunction = config.OilTreeWisdomInvestmentFunction,
            safePathWisdom = config.SafePathWisdom,
            uncertainPathWisdom = config.UncertainPathWisdom,
            uncertainPathWisdomSmallPlank = config.UncertainPathWisdomSmallPlank,
            uncertainPathWisdomMediumPlank = config.UncertainPathWisdomMediumPlank,
            uncertainPathWisdomBigPlank = config.UncertainPathWisdomBigPlank
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
            OilTreeWisdomNotInvestment = request.OilTreeWisdomNotInvestment,
            OilTreeWisdomInvestmentFunction = request.OilTreeWisdomInvestmentFunction,
            SafePathWisdom = request.SafePathWisdom,
            UncertainPathWisdom = request.UncertainPathWisdom,
            UncertainPathWisdomSmallPlank = request.UncertainPathWisdomSmallPlank,
            UncertainPathWisdomMediumPlank = request.UncertainPathWisdomMediumPlank,
            UncertainPathWisdomBigPlank = request.UncertainPathWisdomBigPlank
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
            oilTreeWisdomNotInvestment = config.OilTreeWisdomNotInvestment,
            oilTreeWisdomInvestmentFunction = config.OilTreeWisdomInvestmentFunction,
            safePathWisdom = config.SafePathWisdom,
            uncertainPathWisdom = config.UncertainPathWisdom,
            uncertainPathWisdomSmallPlank = config.UncertainPathWisdomSmallPlank,
            uncertainPathWisdomMediumPlank = config.UncertainPathWisdomMediumPlank,
            uncertainPathWisdomBigPlank = config.UncertainPathWisdomBigPlank
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
        config.OilTreeWisdomNotInvestment = request.OilTreeWisdomNotInvestment;
        config.OilTreeWisdomInvestmentFunction = request.OilTreeWisdomInvestmentFunction;
        config.SafePathWisdom = request.SafePathWisdom;
        config.UncertainPathWisdom = request.UncertainPathWisdom;
        config.UncertainPathWisdomSmallPlank = request.UncertainPathWisdomSmallPlank;
        config.UncertainPathWisdomMediumPlank = request.UncertainPathWisdomMediumPlank;
        config.UncertainPathWisdomBigPlank = request.UncertainPathWisdomBigPlank;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = config.Id,
            startingWisdom = config.StartingWisdom,
            mirrorWisdomIfWaits = config.MirrorWisdomIfWaits,
            mirrorWisdomIfRisksCorrectly = config.MirrorWisdomIfRisksCorrectly,
            mirrorWisdomIfRisksFalsely = config.MirrorWisdomIfRisksFalsely,
            oilTreeWisdomNotInvestment = config.OilTreeWisdomNotInvestment,
            oilTreeWisdomInvestmentFunction = config.OilTreeWisdomInvestmentFunction,
            safePathWisdom = config.SafePathWisdom,
            uncertainPathWisdom = config.UncertainPathWisdom,
            uncertainPathWisdomSmallPlank = config.UncertainPathWisdomSmallPlank,
            uncertainPathWisdomMediumPlank = config.UncertainPathWisdomMediumPlank,
            uncertainPathWisdomBigPlank = config.UncertainPathWisdomBigPlank
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
                oilTreeWisdomNotInvestment = c.OilTreeWisdomNotInvestment,
                oilTreeWisdomInvestmentFunction = c.OilTreeWisdomInvestmentFunction,
                safePathWisdom = c.SafePathWisdom,
                uncertainPathWisdom = c.UncertainPathWisdom,
                uncertainPathWisdomSmallPlank = c.UncertainPathWisdomSmallPlank,
                uncertainPathWisdomMediumPlank = c.UncertainPathWisdomMediumPlank,
                uncertainPathWisdomBigPlank = c.UncertainPathWisdomBigPlank
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
    public int OilTreeWisdomNotInvestment { get; set; }
    public string OilTreeWisdomInvestmentFunction { get; set; } = string.Empty;
    public int SafePathWisdom { get; set; }
    public int UncertainPathWisdom { get; set; }
    public int UncertainPathWisdomSmallPlank { get; set; }
    public int UncertainPathWisdomMediumPlank { get; set; }
    public int UncertainPathWisdomBigPlank { get; set; }
}
