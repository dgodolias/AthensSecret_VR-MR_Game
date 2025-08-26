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
            Console.WriteLine("🔧 [CONFIG API] GetGameConfig() called");
            Console.WriteLine($"🌍 Environment: {_environment.EnvironmentName}");
            
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

            // DETAILED CONSOLE LOGGING για validation
            Console.WriteLine("📊 [CONFIG] Energy Settings:");
            Console.WriteLine($"   StartingEnergy: {config.Energy.StartingEnergy}");
            Console.WriteLine($"   MaxEnergy: {config.Energy.MaxEnergy}");
            Console.WriteLine($"   EnergyCapWarningThreshold: {config.Energy.EnergyCapWarningThreshold}");
            
            Console.WriteLine("⏳ [CONFIG] Patience Trial Settings:");
            Console.WriteLine($"   TimerDurationSeconds: {config.Trials.Patience.TimerDurationSeconds}");
            Console.WriteLine($"   BonusThresholdSeconds: {config.Trials.Patience.BonusThresholdSeconds}");
            Console.WriteLine($"   BonusEnergyAmount: {config.Trials.Patience.BonusEnergyAmount}");
            Console.WriteLine($"   NumberOfMirrors: {config.Trials.Patience.NumberOfMirrors}");
            Console.WriteLine($"   CorrectMirrorLogic: {config.Trials.Patience.CorrectMirrorLogic}");
            
            Console.WriteLine("🌱 [CONFIG] Resource Trial Settings:");
            Console.WriteLine($"   PatienceGridSize: {config.Trials.Resource.PatienceGridSize}");
            Console.WriteLine($"   ImmediateBonusEnergy: {config.Trials.Resource.ImmediateBonusEnergy}");
            Console.WriteLine($"   OliveInvestment.EnableInvestment: {config.Trials.Resource.OliveInvestment.EnableInvestment}");
            Console.WriteLine($"   OliveInvestment.Formula: {config.Trials.Resource.OliveInvestment.Formula}");
            Console.WriteLine($"   OliveInvestment.MaxBonusCap: {config.Trials.Resource.OliveInvestment.MaxBonusCap}");
            Console.WriteLine($"   OliveInvestment.UpdateIntervalMs: {config.Trials.Resource.OliveInvestment.UpdateIntervalMs}");
            Console.WriteLine($"   OliveInvestment.MaxInvestmentDurationSeconds: {config.Trials.Resource.OliveInvestment.MaxInvestmentDurationSeconds}");
            
            Console.WriteLine("🛤️ [CONFIG] Risk Trial Settings:");
            Console.WriteLine($"   NumberOfPaths: {config.Trials.Risk.NumberOfPaths}");
            Console.WriteLine($"   SafePath.EnergyModifier: {config.Trials.Risk.SafePath.EnergyModifier}");
            Console.WriteLine($"   SafePath.SuccessRate: {config.Trials.Risk.SafePath.SuccessRate}");
            Console.WriteLine($"   SafePath.FailurePenalty: {config.Trials.Risk.SafePath.FailurePenalty}");
            Console.WriteLine($"   RiskyPath.EnergyModifier: {config.Trials.Risk.RiskyPath.EnergyModifier}");
            Console.WriteLine($"   RiskyPath.SuccessRate: {config.Trials.Risk.RiskyPath.SuccessRate}");
            Console.WriteLine($"   RiskyPath.FailurePenalty: {config.Trials.Risk.RiskyPath.FailurePenalty}");
            
            Console.WriteLine("🎯 [CONFIG] Scoring Settings:");
            Console.WriteLine($"   EnergyToScoreRatio: {config.Scoring.Weights.EnergyToScoreRatio}");
            Console.WriteLine($"   TimeCompletionBonus: {config.Scoring.Weights.TimeCompletionBonus}");
            Console.WriteLine($"   PerfectTrialMultiplier: {config.Scoring.Weights.PerfectTrialMultiplier}");
            Console.WriteLine($"   FirstTimeCompletionBonus: {config.Scoring.Bonuses.FirstTimeCompletionBonus}");
            Console.WriteLine($"   AllTrialsCompletedBonus: {config.Scoring.Bonuses.AllTrialsCompletedBonus}");
            Console.WriteLine($"   HighEnergyFinishBonus: {config.Scoring.Bonuses.HighEnergyFinishBonus}");
            Console.WriteLine($"   HighEnergyThreshold: {config.Scoring.Bonuses.HighEnergyThreshold}");

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

            Console.WriteLine($"✅ [CONFIG] Response created successfully at {response.Timestamp}");
            Console.WriteLine($"📦 [CONFIG] Response.Success: {response.Success}");
            Console.WriteLine($"📦 [CONFIG] Response.Message: {response.Message}");
            Console.WriteLine($"📦 [CONFIG] Response.StatusCode: {response.StatusCode}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [CONFIG ERROR] Exception in GetGameConfig(): {ex.Message}");
            Console.WriteLine($"❌ [CONFIG ERROR] Stack Trace: {ex.StackTrace}");
            
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
            Console.WriteLine("🌐 [SERVER INFO API] GetServerInfo() called");
            Console.WriteLine($"🌍 Environment: {_environment.EnvironmentName}");
            Console.WriteLine($"🖥️ Machine Name: {Environment.MachineName}");
            
            // Get active sessions count
            var activeSessions = await _context.GameSessions.CountAsync(gs => gs.IsActive);
            Console.WriteLine($"🎮 [SERVER INFO] Active Sessions: {activeSessions}");

            // Get database info (sanitized)
            var databaseConnected = await _context.Database.CanConnectAsync();
            Console.WriteLine($"🗄️ [SERVER INFO] Database Connected: {databaseConnected}");
            
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

            // DETAILED CONSOLE LOGGING για validation
            Console.WriteLine("📊 [SERVER INFO] Server Details:");
            Console.WriteLine($"   ServerVersion: {serverInfo.ServerVersion}");
            Console.WriteLine($"   ApiVersion: {serverInfo.ApiVersion}");
            Console.WriteLine($"   Environment: {serverInfo.Environment}");
            Console.WriteLine($"   ServerTime: {serverInfo.ServerTime}");
            
            Console.WriteLine("💚 [SERVER INFO] Status:");
            Console.WriteLine($"   IsHealthy: {serverInfo.Status.IsHealthy}");
            Console.WriteLine($"   HealthMessage: {serverInfo.Status.HealthMessage}");
            Console.WriteLine($"   ActiveSessions: {serverInfo.Status.ActiveSessions}");
            Console.WriteLine($"   LastHealthCheck: {serverInfo.Status.LastHealthCheck}");
            
            Console.WriteLine("🔗 [SERVER INFO] Endpoints:");
            Console.WriteLine($"   BaseUrl: {serverInfo.Endpoints.BaseUrl}");
            Console.WriteLine($"   Auth.Login: {serverInfo.Endpoints.Auth.Login}");
            Console.WriteLine($"   Auth.Register: {serverInfo.Endpoints.Auth.Register}");
            Console.WriteLine($"   Game.Start: {serverInfo.Endpoints.Game.Start}");
            Console.WriteLine($"   Game.GetState: {serverInfo.Endpoints.Game.GetState}");
            Console.WriteLine($"   Config.GameConfig: {serverInfo.Endpoints.Config.GameConfig}");
            Console.WriteLine($"   Config.ServerInfo: {serverInfo.Endpoints.Config.ServerInfo}");
            
            Console.WriteLine("🔐 [SERVER INFO] Security Settings:");
            Console.WriteLine($"   JWT.ExpirationHours: {serverInfo.Security.Jwt.ExpirationHours}");
            Console.WriteLine($"   JWT.Algorithm: {serverInfo.Security.Jwt.Algorithm}");
            Console.WriteLine($"   JWT.RequireHttps: {serverInfo.Security.Jwt.RequireHttps}");
            Console.WriteLine($"   CORS.AllowCredentials: {serverInfo.Security.Cors.AllowCredentials}");
            Console.WriteLine($"   CORS.AllowedOrigins: [{string.Join(", ", serverInfo.Security.Cors.AllowedOrigins)}]");
            Console.WriteLine($"   RateLimit.Enabled: {serverInfo.Security.RateLimit.Enabled}");
            Console.WriteLine($"   RateLimit.RequestsPerMinute: {serverInfo.Security.RateLimit.RequestsPerMinute}");
            
            Console.WriteLine("🗄️ [SERVER INFO] Database:");
            Console.WriteLine($"   Provider: {serverInfo.Database.Provider}");
            Console.WriteLine($"   IsConnected: {serverInfo.Database.IsConnected}");
            Console.WriteLine($"   ConnectionString: {serverInfo.Database.ConnectionString}");

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

            Console.WriteLine($"✅ [SERVER INFO] Response created successfully at {response.Timestamp}");
            Console.WriteLine($"📦 [SERVER INFO] Response.Success: {response.Success}");
            Console.WriteLine($"📦 [SERVER INFO] Response.Message: {response.Message}");
            Console.WriteLine($"📦 [SERVER INFO] Response.StatusCode: {response.StatusCode}");
            Console.WriteLine($"📦 [SERVER INFO] Metadata RequestId: {response.Metadata["requestId"]}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SERVER INFO ERROR] Exception in GetServerInfo(): {ex.Message}");
            Console.WriteLine($"❌ [SERVER INFO ERROR] Stack Trace: {ex.StackTrace}");
            
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
