using Microsoft.Extensions.Options;

namespace AthensSecret.Api.Middleware;

public class AdminSecurityOptions
{
    public const string SectionName = "AdminSettings";
    public string ApiKey { get; set; } = string.Empty;
}

public class AdminSecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AdminSecurityOptions _options;
    private readonly ILogger<AdminSecurityMiddleware> _logger;

    public AdminSecurityMiddleware(
        RequestDelegate next,
        IOptions<AdminSecurityOptions> options,
        ILogger<AdminSecurityMiddleware> logger)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if this is an admin API endpoint
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        {
            // Check for admin key in header
            if (!context.Request.Headers.TryGetValue("X-Admin-Key", out var providedKey) ||
                string.IsNullOrEmpty(providedKey) ||
                providedKey != _options.ApiKey)
            {
                _logger.LogWarning("Unauthorized admin access attempt from {IP} to {Path}", 
                    context.Connection.RemoteIpAddress, context.Request.Path);
                
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid or missing admin key");
                return;
            }

            _logger.LogInformation("Authorized admin access from {IP} to {Path}", 
                context.Connection.RemoteIpAddress, context.Request.Path);
        }

        await _next(context);
    }
}

public static class AdminSecurityMiddlewareExtensions
{
    public static IApplicationBuilder UseAdminSecurity(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AdminSecurityMiddleware>();
    }
}