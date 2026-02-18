using System.Collections.Concurrent;

namespace AthensSecret.Api.Services;

/// <summary>
/// Service for tracking failed admin authentication attempts to prevent brute force attacks
/// </summary>
public class AdminAuthenticationTracker
{
    private readonly ConcurrentDictionary<string, FailedAttemptRecord> _failedAttempts = new();
    private readonly ILogger<AdminAuthenticationTracker> _logger;

    // Configuration
    private const int MaxFailedAttempts = 5;
    private const int LockoutDurationMinutes = 15;
    private const int CleanupIntervalMinutes = 30;

    public AdminAuthenticationTracker(ILogger<AdminAuthenticationTracker> logger)
    {
        _logger = logger;
        _logger.LogWarning("AdminAuthenticationTracker initialized with in-memory storage. Failed attempt records will be lost on application restart.");

        // Start background cleanup task
        _ = Task.Run(async () => await CleanupExpiredRecordsAsync());
    }

    /// <summary>
    /// Records a failed authentication attempt
    /// </summary>
    public void RecordFailedAttempt(string ipAddress)
    {
        var record = _failedAttempts.AddOrUpdate(
            ipAddress,
            // Add new record
            _ => new FailedAttemptRecord
            {
                FailedAttempts = 1,
                FirstAttemptTime = DateTime.UtcNow,
                LastAttemptTime = DateTime.UtcNow,
                IsLocked = false
            },
            // Update existing record
            (_, existing) =>
            {
                existing.FailedAttempts++;
                existing.LastAttemptTime = DateTime.UtcNow;
                
                // Lock if max attempts reached
                if (existing.FailedAttempts >= MaxFailedAttempts && !existing.IsLocked)
                {
                    existing.IsLocked = true;
                    existing.LockoutEndTime = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                    
                    _logger.LogWarning(
                        "IP {IpAddress} locked out after {Attempts} failed admin authentication attempts. Lockout until {LockoutEnd}",
                        ipAddress, existing.FailedAttempts, existing.LockoutEndTime);
                }
                
                return existing;
            });

        _logger.LogWarning(
            "Failed admin authentication attempt from IP {IpAddress}. Total attempts: {TotalAttempts}",
            ipAddress, record.FailedAttempts);
    }

    /// <summary>
    /// Resets failed attempts for an IP (called on successful authentication)
    /// </summary>
    public void ResetFailedAttempts(string ipAddress)
    {
        if (_failedAttempts.TryRemove(ipAddress, out var removed))
        {
            _logger.LogInformation(
                "Cleared failed authentication records for IP {IpAddress} after successful authentication",
                ipAddress);
        }
    }

    /// <summary>
    /// Checks if an IP is currently locked out
    /// </summary>
    public bool IsIpLocked(string ipAddress)
    {
        if (!_failedAttempts.TryGetValue(ipAddress, out var record))
        {
            return false;
        }

        // Check if lockout has expired
        if (record.IsLocked && record.LockoutEndTime.HasValue)
        {
            if (DateTime.UtcNow >= record.LockoutEndTime.Value)
            {
                // Lockout expired, remove record
                _failedAttempts.TryRemove(ipAddress, out _);
                _logger.LogInformation(
                    "Lockout expired for IP {IpAddress}. Record removed.",
                    ipAddress);
                return false;
            }
        }

        return record.IsLocked;
    }

    /// <summary>
    /// Gets remaining lockout time for an IP
    /// </summary>
    public TimeSpan? GetRemainingLockoutTime(string ipAddress)
    {
        if (!_failedAttempts.TryGetValue(ipAddress, out var record))
        {
            return null;
        }

        if (record.IsLocked && record.LockoutEndTime.HasValue)
        {
            var remaining = record.LockoutEndTime.Value - DateTime.UtcNow;
            return remaining.TotalSeconds > 0 ? remaining : null;
        }

        return null;
    }

    /// <summary>
    /// Gets failed attempt count for an IP
    /// </summary>
    public int GetFailedAttemptCount(string ipAddress)
    {
        return _failedAttempts.TryGetValue(ipAddress, out var record) 
            ? record.FailedAttempts 
            : 0;
    }

    /// <summary>
    /// Background task to cleanup expired records
    /// </summary>
    private async Task CleanupExpiredRecordsAsync()
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(CleanupIntervalMinutes));

                var now = DateTime.UtcNow;
                var expiredKeys = _failedAttempts
                    .Where(kvp => 
                        !kvp.Value.IsLocked && 
                        (now - kvp.Value.LastAttemptTime).TotalMinutes > CleanupIntervalMinutes)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _failedAttempts.TryRemove(key, out _);
                }

                if (expiredKeys.Count > 0)
                {
                    _logger.LogInformation(
                        "Cleaned up {Count} expired admin authentication records",
                        expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin authentication tracker cleanup");
            }
        }
    }

    /// <summary>
    /// Gets statistics about current tracking state
    /// </summary>
    public AdminAuthStats GetStatistics()
    {
        var now = DateTime.UtcNow;
        return new AdminAuthStats
        {
            TotalTrackedIps = _failedAttempts.Count,
            LockedIps = _failedAttempts.Count(kvp => kvp.Value.IsLocked),
            TotalFailedAttempts = _failedAttempts.Values.Sum(v => v.FailedAttempts),
            RecentAttempts = _failedAttempts.Count(kvp => 
                (now - kvp.Value.LastAttemptTime).TotalMinutes < 5)
        };
    }
}

/// <summary>
/// Record of failed authentication attempts for a specific IP
/// </summary>
public class FailedAttemptRecord
{
    public int FailedAttempts { get; set; }
    public DateTime FirstAttemptTime { get; set; }
    public DateTime LastAttemptTime { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockoutEndTime { get; set; }
}

/// <summary>
/// Statistics about admin authentication tracking
/// </summary>
public class AdminAuthStats
{
    public int TotalTrackedIps { get; set; }
    public int LockedIps { get; set; }
    public int TotalFailedAttempts { get; set; }
    public int RecentAttempts { get; set; }
}
