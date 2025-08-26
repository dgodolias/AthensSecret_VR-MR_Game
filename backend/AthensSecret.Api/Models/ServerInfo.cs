namespace AthensSecret.Api.Models;

public class ServerInfo
{
    public string ServerVersion { get; set; } = "1.0.0";
    public string ApiVersion { get; set; } = "v1";
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
    public string Environment { get; set; } = "Development";
    public ServerStatus Status { get; set; } = new();
    public ApiEndpoints Endpoints { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public DatabaseInfo Database { get; set; } = new();
}

public class ServerStatus
{
    public bool IsHealthy { get; set; } = true;
    public string HealthMessage { get; set; } = "Server is running normally";
    public TimeSpan Uptime { get; set; }
    public int ActiveSessions { get; set; } = 0;
    public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;
}

public class ApiEndpoints
{
    public string BaseUrl { get; set; } = "";
    public AuthEndpoints Auth { get; set; } = new();
    public GameEndpoints Game { get; set; } = new();
    public ConfigEndpoints Config { get; set; } = new();
    public UnityEndpoints Unity { get; set; } = new();
}

public class AuthEndpoints
{
    public string Register { get; set; } = "/api/auth/register";
    public string Login { get; set; } = "/api/auth/login";
    public string Logout { get; set; } = "/api/auth/logout";
    public string RefreshToken { get; set; } = "/api/auth/refresh";
}

public class GameEndpoints
{
    public string Start { get; set; } = "/api/game/start";
    public string GetState { get; set; } = "/api/game/{sessionId}/state";
    public string EndGame { get; set; } = "/api/game/{sessionId}/end";
    public string SubmitComplete { get; set; } = "/api/game/submit-complete";
    public string PlayerSessions { get; set; } = "/api/game/player/{username}/sessions";
}

public class ConfigEndpoints
{
    public string GameConfig { get; set; } = "/api/config/game";
    public string ServerInfo { get; set; } = "/api/config/server";
    public string Version { get; set; } = "/api/config/version";
}

public class UnityEndpoints
{
    public string UnityStart { get; set; } = "/api/unity/start";
    public string UnityHealth { get; set; } = "/api/unity/health";
    public string UnityVersion { get; set; } = "/api/unity/version";
    public string UnityEvents { get; set; } = "/api/unity/events";
}

public class SecuritySettings
{
    public JwtSettings Jwt { get; set; } = new();
    public CorsSettings Cors { get; set; } = new();
    public RateLimitSettings RateLimit { get; set; } = new();
}

public class JwtSettings
{
    public int ExpirationHours { get; set; } = 24;
    public string Algorithm { get; set; } = "HS256";
    public bool RequireHttps { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
}

public class CorsSettings
{
    public bool AllowCredentials { get; set; } = true;
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
    public string[] AllowedHeaders { get; set; } = { "Authorization", "Content-Type" };
}

public class RateLimitSettings
{
    public bool Enabled { get; set; } = false;
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerHour { get; set; } = 1000;
}

public class DatabaseInfo
{
    public string Provider { get; set; } = "PostgreSQL";
    public string Version { get; set; } = "";
    public bool IsConnected { get; set; } = true;
    public string ConnectionString { get; set; } = ""; // Will be sanitized before sending
    public MigrationInfo Migration { get; set; } = new();
}

public class MigrationInfo
{
    public string LatestMigration { get; set; } = "";
    public bool PendingMigrations { get; set; } = false;
    public DateTime LastMigrationDate { get; set; }
}

// Response wrapper for standardized API responses
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public string Message { get; set; } = "";
    public int StatusCode { get; set; } = 200;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = "";
    public string Detail { get; set; } = "";
    public string TraceId { get; set; } = "";
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
