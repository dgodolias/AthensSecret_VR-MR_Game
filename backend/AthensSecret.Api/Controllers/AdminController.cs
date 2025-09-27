using AthensSecret.Api.Data;
using AthensSecret.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApiDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get the current game configuration
    /// </summary>
    [HttpGet("configuration")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> GetConfiguration()
    {
        try
        {
            _logger.LogInformation("Configuration GET request from IP: {ClientIP}", 
                HttpContext.Connection.RemoteIpAddress?.ToString());

            var config = await _context.GameConfigurations.OrderBy(c => c.Id).FirstOrDefaultAsync();
            
            if (config == null)
            {
                _logger.LogWarning("No game configuration found in database, creating default configuration");
                
                // Create default configuration
                var defaultConfig = new GameConfiguration
                {
                    StartingWisdom = 50,
                    MirrorWisdomIfWaits = 50,
                    MirrorWisdomIfRisksCorrectly = 75,
                    MirrorWisdomIfRisksFalsely = 25,
                    OliveTreeWisdomNotInvestment = 50,
                    OliveTreeWisdomInvestmentFunction = "sqrt",
                    SafePathWisdom = 10,
                    UncertainPathWisdom = 25,
                    UncertainPathWisdomSmallPlank = 15,
                    UncertainPathWisdomMediumPlank = 25,
                    UncertainPathWisdomBigPlank = 40
                };
                
                _context.GameConfigurations.Add(defaultConfig);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Default game configuration created with Id={ConfigId}", defaultConfig.Id);
                return Ok(defaultConfig);
            }

            _logger.LogInformation("Configuration retrieved successfully: Id={ConfigId}", config.Id);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve game configuration");
            return StatusCode(500, new { message = "Failed to retrieve configuration" });
        }
    }

    /// <summary>
    /// Update the game configuration
    /// </summary>
    [HttpPut("configuration")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> UpdateConfiguration([FromBody] GameConfiguration request)
    {
        try
        {
            _logger.LogInformation("Configuration UPDATE request from IP: {ClientIP}", 
                HttpContext.Connection.RemoteIpAddress?.ToString());

            // Input validation
            if (request == null)
            {
                _logger.LogWarning("Invalid configuration update request: null request data");
                return BadRequest(new { message = "Invalid request data" });
            }

            // Validate wisdom values are positive
            if (request.StartingWisdom < 0 || request.MirrorWisdomIfWaits < 0 || 
                request.MirrorWisdomIfRisksCorrectly < 0 || request.MirrorWisdomIfRisksFalsely < 0 ||
                request.OliveTreeWisdomNotInvestment < 0 || request.SafePathWisdom < 0 ||
                request.UncertainPathWisdom < 0 || request.UncertainPathWisdomSmallPlank < 0 ||
                request.UncertainPathWisdomMediumPlank < 0 || request.UncertainPathWisdomBigPlank < 0)
            {
                return BadRequest(new { message = "Wisdom values cannot be negative" });
            }

            // Validate investment function
            var validFunctions = new[] { "sqrt", "linear", "log" };
            if (string.IsNullOrWhiteSpace(request.OliveTreeWisdomInvestmentFunction) || 
                !validFunctions.Contains(request.OliveTreeWisdomInvestmentFunction.ToLower()))
            {
                return BadRequest(new { message = "Invalid investment function. Must be: sqrt, linear, or log" });
            }

            // Find existing configuration
            var existingConfig = await _context.GameConfigurations.OrderBy(c => c.Id).FirstOrDefaultAsync();
            
            if (existingConfig == null)
            {
                // Create new configuration if none exists
                var newConfig = new GameConfiguration
                {
                    StartingWisdom = request.StartingWisdom,
                    MirrorWisdomIfWaits = request.MirrorWisdomIfWaits,
                    MirrorWisdomIfRisksCorrectly = request.MirrorWisdomIfRisksCorrectly,
                    MirrorWisdomIfRisksFalsely = request.MirrorWisdomIfRisksFalsely,
                    OliveTreeWisdomNotInvestment = request.OliveTreeWisdomNotInvestment,
                    OliveTreeWisdomInvestmentFunction = request.OliveTreeWisdomInvestmentFunction?.ToLower() ?? "sqrt",
                    SafePathWisdom = request.SafePathWisdom,
                    UncertainPathWisdom = request.UncertainPathWisdom,
                    UncertainPathWisdomSmallPlank = request.UncertainPathWisdomSmallPlank,
                    UncertainPathWisdomMediumPlank = request.UncertainPathWisdomMediumPlank,
                    UncertainPathWisdomBigPlank = request.UncertainPathWisdomBigPlank
                };

                _context.GameConfigurations.Add(newConfig);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("New game configuration created: Id={ConfigId}", newConfig.Id);
                return Ok(newConfig);
            }
            else
            {
                // Update existing configuration
                existingConfig.StartingWisdom = request.StartingWisdom;
                existingConfig.MirrorWisdomIfWaits = request.MirrorWisdomIfWaits;
                existingConfig.MirrorWisdomIfRisksCorrectly = request.MirrorWisdomIfRisksCorrectly;
                existingConfig.MirrorWisdomIfRisksFalsely = request.MirrorWisdomIfRisksFalsely;
                existingConfig.OliveTreeWisdomNotInvestment = request.OliveTreeWisdomNotInvestment;
                existingConfig.OliveTreeWisdomInvestmentFunction = request.OliveTreeWisdomInvestmentFunction?.ToLower() ?? "sqrt";
                existingConfig.SafePathWisdom = request.SafePathWisdom;
                existingConfig.UncertainPathWisdom = request.UncertainPathWisdom;
                existingConfig.UncertainPathWisdomSmallPlank = request.UncertainPathWisdomSmallPlank;
                existingConfig.UncertainPathWisdomMediumPlank = request.UncertainPathWisdomMediumPlank;
                existingConfig.UncertainPathWisdomBigPlank = request.UncertainPathWisdomBigPlank;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Game configuration updated successfully: Id={ConfigId}", existingConfig.Id);
                return Ok(existingConfig);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update game configuration");
            
            // Log the error but don't expose internal details in production
            if (HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
            {
                return StatusCode(500, new { message = $"Failed to update configuration: {ex.Message}" });
            }
            
            return StatusCode(500, new { message = "Failed to update configuration. Please try again later." });
        }
    }

    /// <summary>
    /// Reset configuration to default values
    /// </summary>
    [HttpPost("configuration/reset")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> ResetConfiguration()
    {
        try
        {
            _logger.LogInformation("Configuration RESET request from IP: {ClientIP}", 
                HttpContext.Connection.RemoteIpAddress?.ToString());

            var existingConfig = await _context.GameConfigurations.OrderBy(c => c.Id).FirstOrDefaultAsync();
            
            if (existingConfig == null)
            {
                return NotFound(new { message = "No configuration found to reset" });
            }

            // Reset to default values from schema
            existingConfig.StartingWisdom = 50;
            existingConfig.MirrorWisdomIfWaits = 50;
            existingConfig.MirrorWisdomIfRisksCorrectly = 75;
            existingConfig.MirrorWisdomIfRisksFalsely = 25;
            existingConfig.OliveTreeWisdomNotInvestment = 50;
            existingConfig.OliveTreeWisdomInvestmentFunction = "sqrt";
            existingConfig.SafePathWisdom = 10;
            existingConfig.UncertainPathWisdom = 25;
            existingConfig.UncertainPathWisdomSmallPlank = 15;
            existingConfig.UncertainPathWisdomMediumPlank = 25;
            existingConfig.UncertainPathWisdomBigPlank = 40;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Game configuration reset to defaults: Id={ConfigId}", existingConfig.Id);
            return Ok(existingConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset game configuration");
            return StatusCode(500, new { message = "Failed to reset configuration" });
        }
    }
}