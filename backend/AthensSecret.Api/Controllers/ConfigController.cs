using AthensSecret.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AthensSecret.Api.Data;
using System.Diagnostics;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApiDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ConfigController(IConfiguration configuration, ApiDbContext context, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _context = context;
        _environment = environment;
    }

    /// <summary>
    /// Get game configuration for Unity client
    /// </summary>
    /// <returns>Complete game configuration including trials, energy settings, and mechanics</returns>
    [HttpGet("game")]
    public ActionResult<ApiResponse<GameConfig>> GetGameConfig()
    {
        try
        {
            var config = new GameConfig
            {
                Energy = new EnergySettings
                {
                    StartingEnergy = _configuration.GetValue<int>("GameSettings:StartingEnergy", 50),
                    MinEnergy = _configuration.GetValue<int>("GameSettings:MinEnergy", 0),
                    MaxEnergy = _configuration.GetValue<int>("GameSettings:MaxEnergy", 1000),
                    EnergyCapWarningThreshold = _configuration.GetValue<int>("GameSettings:EnergyCapWarningThreshold", 950)
                },
                Trials = new TrialSettings
                {
                    Patience = new PatienceTrialSettings
                    {
                        TimerDurationSeconds = _configuration.GetValue<int>("GameSettings:Trials:Patience:TimerDurationSeconds", 60),
                        BonusThresholdSeconds = _configuration.GetValue<int>("GameSettings:Trials:Patience:BonusThresholdSeconds", 30),
                        BonusEnergyAmount = _configuration.GetValue<int>("GameSettings:Trials:Patience:BonusEnergyAmount", 50),
                        BonusScoreAmount = _configuration.GetValue<int>("GameSettings:Trials:Patience:BonusScoreAmount", 50),
                        NumberOfMirrors = _configuration.GetValue<int>("GameSettings:Trials:Patience:NumberOfMirrors", 3),
                        CorrectMirrorLogic = _configuration.GetValue<string>("GameSettings:Trials:Patience:CorrectMirrorLogic", "random") ?? "random"
                    },
                    Resource = new ResourceTrialSettings
                    {
                        PatienceGridSize = _configuration.GetValue<int>("GameSettings:Trials:Resource:PatienceGridSize", 25),
                        ImmediateBonusEnergy = _configuration.GetValue<int>("GameSettings:Trials:Resource:ImmediateBonusEnergy", 50),
                        OliveInvestment = new OliveInvestmentSettings
                        {
                            EnableInvestment = _configuration.GetValue<bool>("GameSettings:Trials:Resource:OliveInvestment:EnableInvestment", true),
                            Formula = _configuration.GetValue<string>("GameSettings:Trials:Resource:OliveInvestment:Formula", "sqrt") ?? "sqrt",
                            FormulaMultiplier = _configuration.GetValue<double>("GameSettings:Trials:Resource:OliveInvestment:FormulaMultiplier", 1.0),
                            MaxBonusCap = _configuration.GetValue<int>("GameSettings:Trials:Resource:OliveInvestment:MaxBonusCap", 100),
                            UpdateIntervalMs = _configuration.GetValue<int>("GameSettings:Trials:Resource:OliveInvestment:UpdateIntervalMs", 1000),
                            MaxInvestmentDurationSeconds = _configuration.GetValue<int>("GameSettings:Trials:Resource:OliveInvestment:MaxInvestmentDurationSeconds", 300)
                        }
                    },
                    Risk = new RiskTrialSettings
                    {
                        NumberOfPaths = _configuration.GetValue<int>("GameSettings:Trials:Risk:NumberOfPaths", 2),
                        SafePath = new PathSettings
                        {
                            Description = "Safe Path",
                            EnergyModifier = _configuration.GetValue<int>("GameSettings:Trials:Risk:SafePath:EnergyModifier", 10),
                            SuccessRate = _configuration.GetValue<double>("GameSettings:Trials:Risk:SafePath:SuccessRate", 1.0),
                            FailurePenalty = _configuration.GetValue<int>("GameSettings:Trials:Risk:SafePath:FailurePenalty", 0)
                        },
                        RiskyPath = new PathSettings
                        {
                            Description = "Risky Path",
                            EnergyModifier = _configuration.GetValue<int>("GameSettings:Trials:Risk:RiskyPath:EnergyModifier", 25),
                            SuccessRate = _configuration.GetValue<double>("GameSettings:Trials:Risk:RiskyPath:SuccessRate", 0.7),
                            FailurePenalty = _configuration.GetValue<int>("GameSettings:Trials:Risk:RiskyPath:FailurePenalty", -15)
                        }
                    }
                },
                Timing = new TimingSettings
                {
                    GameSessionTimeoutMinutes = _configuration.GetValue<int>("GameSettings:Timing:GameSessionTimeoutMinutes", 30),
                    InactivityTimeoutMinutes = _configuration.GetValue<int>("GameSettings:Timing:InactivityTimeoutMinutes", 10),
                    AutoSaveIntervalSeconds = _configuration.GetValue<int>("GameSettings:Timing:AutoSaveIntervalSeconds", 30)
                },
                Scoring = new ScoreSettings
                {
                    Weights = new ScoreWeights
                    {
                        EnergyToScoreRatio = _configuration.GetValue<double>("GameSettings:Scoring:Weights:EnergyToScoreRatio", 1.0),
                        TimeCompletionBonus = _configuration.GetValue<double>("GameSettings:Scoring:Weights:TimeCompletionBonus", 0.5),
                        PerfectTrialMultiplier = _configuration.GetValue<double>("GameSettings:Scoring:Weights:PerfectTrialMultiplier", 1.5)
                    },
                    Bonuses = new ScoreBonuses
                    {
                        FirstTimeCompletionBonus = _configuration.GetValue<int>("GameSettings:Scoring:Bonuses:FirstTimeCompletionBonus", 100),
                        AllTrialsCompletedBonus = _configuration.GetValue<int>("GameSettings:Scoring:Bonuses:AllTrialsCompletedBonus", 200),
                        HighEnergyFinishBonus = _configuration.GetValue<int>("GameSettings:Scoring:Bonuses:HighEnergyFinishBonus", 150),
                        HighEnergyThreshold = _configuration.GetValue<int>("GameSettings:Scoring:Bonuses:HighEnergyThreshold", 200)
                    }
                },
                Investment = new InvestmentSettings()
            };

            var response = new ApiResponse<GameConfig>
            {
                Success = true,
                Data = config,
                Message = "Game configuration retrieved successfully",
                StatusCode = 200,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "configVersion", "1.0.0" },
                    { "environment", _environment.EnvironmentName },
                    { "lastUpdated", DateTime.UtcNow }
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse<GameConfig>
            {
                Success = false,
                Data = null,
                Message = "Failed to retrieve game configuration",
                StatusCode = 500,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "environment", _environment.EnvironmentName }
                }
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Get server information for Unity client
    /// </summary>
    /// <returns>Server status, API endpoints, and connection information</returns>
    [HttpGet("server")]
    public async Task<ActionResult<ApiResponse<ServerInfo>>> GetServerInfo()
    {
        try
        {
            // Get active sessions count
            var activeSessions = await _context.GameSessions.CountAsync(gs => gs.IsActive);

            // Get database info (sanitized)
            var databaseConnected = await _context.Database.CanConnectAsync();
            
            var serverInfo = new ServerInfo
            {
                ServerVersion = _configuration.GetValue<string>("ServerInfo:Version", "1.0.0") ?? "1.0.0",
                ApiVersion = _configuration.GetValue<string>("ServerInfo:ApiVersion", "v1") ?? "v1",
                ServerTime = DateTime.UtcNow,
                Environment = _environment.EnvironmentName,
                Status = new ServerStatus
                {
                    IsHealthy = databaseConnected,
                    HealthMessage = databaseConnected ? "Server is running normally" : "Database connection issues",
                    ActiveSessions = activeSessions,
                    LastHealthCheck = DateTime.UtcNow
                },
                Endpoints = new ApiEndpoints
                {
                    BaseUrl = $"{Request.Scheme}://{Request.Host}/api",
                    Auth = new AuthEndpoints(),
                    Game = new GameEndpoints(),
                    Config = new ConfigEndpoints(),
                    Unity = new UnityEndpoints()
                },
                Security = new SecuritySettings
                {
                    Jwt = new JwtSettings
                    {
                        ExpirationHours = _configuration.GetValue<int>("JwtSettings:ExpirationHours", 24),
                        Algorithm = "HS256",
                        RequireHttps = _environment.IsProduction(),
                        ValidateLifetime = true
                    },
                    Cors = new CorsSettings
                    {
                        AllowCredentials = true,
                        AllowedOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>(),
                        AllowedMethods = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" },
                        AllowedHeaders = new[] { "Authorization", "Content-Type" }
                    },
                    RateLimit = new RateLimitSettings
                    {
                        Enabled = _configuration.GetValue<bool>("RateLimit:Enabled", false),
                        RequestsPerMinute = _configuration.GetValue<int>("RateLimit:RequestsPerMinute", 60),
                        RequestsPerHour = _configuration.GetValue<int>("RateLimit:RequestsPerHour", 1000)
                    }
                },
                Database = new DatabaseInfo
                {
                    Provider = "PostgreSQL",
                    IsConnected = databaseConnected,
                    ConnectionString = "[SANITIZED FOR SECURITY]", // Never expose real connection string
                    Migration = new MigrationInfo
                    {
                        LatestMigration = "Latest migration applied",
                        PendingMigrations = false,
                        LastMigrationDate = DateTime.UtcNow // This would be retrieved from migrations table in real implementation
                    }
                }
            };

            var response = new ApiResponse<ServerInfo>
            {
                Success = true,
                Data = serverInfo,
                Message = "Server information retrieved successfully",
                StatusCode = 200,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "requestId", Guid.NewGuid().ToString() },
                    { "serverNode", Environment.MachineName },
                    { "uptime", DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime() }
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse<ServerInfo>
            {
                Success = false,
                Data = null,
                Message = "Failed to retrieve server information",
                StatusCode = 500,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "environment", _environment.EnvironmentName }
                }
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Get API version information
    /// </summary>
    /// <returns>API version and compatibility information</returns>
    [HttpGet("version")]
    public ActionResult<ApiResponse<object>> GetVersion()
    {
        var versionInfo = new
        {
            ApiVersion = "v1",
            ServerVersion = _configuration.GetValue<string>("ServerInfo:Version", "1.0.0"),
            MinSupportedClientVersion = _configuration.GetValue<string>("ServerInfo:MinSupportedClientVersion", "1.0.0"),
            MaxSupportedClientVersion = _configuration.GetValue<string>("ServerInfo:MaxSupportedClientVersion", "1.99.99"),
            Environment = _environment.EnvironmentName,
            BuildDate = _configuration.GetValue<string>("ServerInfo:BuildDate", DateTime.UtcNow.ToString("yyyy-MM-dd")),
            Endpoints = new
            {
                GameConfig = "/api/config/game",
                ServerInfo = "/api/config/server",
                Health = "/api/config/version"
            }
        };

        var response = new ApiResponse<object>
        {
            Success = true,
            Data = versionInfo,
            Message = "API version information retrieved successfully",
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }
}
