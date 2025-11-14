using Microsoft.Extensions.Options;
using AthensSecret.Api.Services;

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
    private readonly AdminAuthenticationTracker _authTracker;

    public AdminSecurityMiddleware(
        RequestDelegate next,
        IOptions<AdminSecurityOptions> options,
        ILogger<AdminSecurityMiddleware> logger,
        AdminAuthenticationTracker authTracker)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;
        _authTracker = authTracker;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if this is an admin API endpoint
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Check if IP is locked out
            if (_authTracker.IsIpLocked(ipAddress))
            {
                var remainingTime = _authTracker.GetRemainingLockoutTime(ipAddress);
                _logger.LogWarning(
                    "Admin access denied: IP {IpAddress} is locked out. Remaining time: {RemainingMinutes} minutes. Path: {Path}",
                    ipAddress, remainingTime?.TotalMinutes ?? 0, context.Request.Path);
                
                context.Response.StatusCode = 429;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new 
                { 
                    message = "Too many failed attempts. Please try again later.",
                    retryAfterMinutes = (int)Math.Ceiling(remainingTime?.TotalMinutes ?? 0)
                });
                return;
            }

            // Check for admin key in header
            if (!context.Request.Headers.TryGetValue("X-Admin-Key", out var providedKey) ||
                string.IsNullOrEmpty(providedKey))
            {
                _authTracker.RecordFailedAttempt(ipAddress);
                _logger.LogWarning(
                    "Unauthorized admin access attempt from IP {IpAddress} to {Path}: Missing admin key. Failed attempts: {Attempts}",
                    ipAddress, context.Request.Path, _authTracker.GetFailedAttemptCount(ipAddress));
                
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid or missing admin key");
                return;
            }

            if (providedKey != _options.ApiKey)
            {
                _authTracker.RecordFailedAttempt(ipAddress);
                var failedAttempts = _authTracker.GetFailedAttemptCount(ipAddress);
                
                _logger.LogWarning(
                    "Unauthorized admin access attempt from IP {IpAddress} to {Path}: Invalid admin key. Failed attempts: {Attempts}",
                    ipAddress, context.Request.Path, failedAttempts);
                
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid or missing admin key");
                return;
            }

            // Successful authentication - reset failed attempts
            _authTracker.ResetFailedAttempts(ipAddress);

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