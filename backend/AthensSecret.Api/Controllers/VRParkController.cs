using AthensSecret.Api.Data;
using AthensSecret.Api.Models;
using AthensSecret.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AthensSecret.Api.Middleware;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VRParkController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<VRParkController> _logger;
    private readonly AdminSecurityOptions _adminOptions;
    private readonly AdminAuthenticationTracker _authTracker;
    private readonly IWebHostEnvironment _environment;

    public VRParkController(
        ApiDbContext context, 
        ILogger<VRParkController> logger, 
        IOptions<AdminSecurityOptions> adminOptions,
        AdminAuthenticationTracker authTracker,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _adminOptions = adminOptions.Value;
        _authTracker = authTracker;
        _environment = environment;
    }

    // Helper: Convert UTC to Greece time (UTC+2 standard, UTC+3 daylight saving)
    private static DateTime ConvertToGreeceTime(DateTime utcTime)
    {
        var greeceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time"); // Greece, Turkey, Bulgaria
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, greeceTimeZone);
    }

    // Signup endpoint for VR Park users
    [HttpPost("signup")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> SignupUser([FromBody] VRParkSignupRequest request)
    {
        try
        {
            _logger.LogInformation("VR Park signup attempt: {FirstName} {LastName}, Age: {Age}, Email: {Email}", 
                request.FirstName, request.LastName, request.Age, request.Email ?? "N/A");

            // Validate model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                _logger.LogWarning("VR Park signup validation failed: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "Μη έγκυρα δεδομένα", errors });
            }

            // Check if email already exists (if provided)
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existingUser = await _context.VRParkUsers
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    _logger.LogWarning("VR Park signup failed: Email already exists - {Email}, UserId: {UserId}", 
                        request.Email, existingUser.Id);
                    
                    // Return user ID to handle re-verification
                    return BadRequest(new { 
                        message = $"Το email υπάρχει ήδη και αντιστοιχεί στον παίκτη με ID {existingUser.Id}",
                        userId = existingUser.Id 
                    });
                }
            }

            // Create new VR Park user
            // Use Random.Shared for thread-safe random number generation
            var randomVideo = Random.Shared.Next(1, 5); // Random number between 1 and 4

            var vrParkUser = new VRParkUser
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                Age = request.Age,
                Video = randomVideo,
                CreatedAt = DateTime.UtcNow
            };

            _context.VRParkUsers.Add(vrParkUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("VR Park user created successfully: UserId={UserId}, Name={FirstName} {LastName}, Video={Video}", 
                vrParkUser.Id, vrParkUser.FirstName, vrParkUser.LastName, vrParkUser.Video);

            return Ok(new VRParkSignupResponse
            {
                UserId = vrParkUser.Id,
                FirstName = vrParkUser.FirstName,
                LastName = vrParkUser.LastName,
                Email = vrParkUser.Email,
                Age = vrParkUser.Age,
                Video = vrParkUser.Video,
                Message = "Η εγγραφή σας ολοκληρώθηκε επιτυχώς!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VR Park signup failed");
            
            // Never expose exception details in production
            return StatusCode(500, new
            {
                message = "Αποτυχία εγγραφής. Παρακαλώ δοκιμάστε ξανά."
            });
        }
    }

    // Verify user exists by ID
    [HttpGet("verify/{userId}")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> VerifyUser(int userId)
    {
        try
        {
            _logger.LogInformation("VR Park user verification attempt: UserId={UserId}", userId);

            if (userId <= 0)
            {
                _logger.LogWarning("Invalid UserId provided: {UserId}", userId);
                return BadRequest(new { message = "Μη έγκυρο User ID" });
            }

            var user = await _context.VRParkUsers.FindAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("VR Park user not found: UserId={UserId}", userId);
                return NotFound(new { message = "Ο χρήστης δεν βρέθηκε" });
            }

            _logger.LogInformation("VR Park user verified successfully: UserId={UserId}, Name={FirstName} {LastName}, Video={Video}", 
                user.Id, user.FirstName, user.LastName, user.Video);

            return Ok(new
            {
                userId = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                age = user.Age,
                video = user.Video,
                createdAt = user.CreatedAt,
                message = $"Καλώς ήρθες, {user.FirstName}!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VR Park user verification failed: {ErrorMessage}", ex.Message);
            return StatusCode(500, new { message = "Σφάλμα επαλήθευσης χρήστη" });
        }
    }

    // Start a new game session
    [HttpPost("session/start")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> StartGameSession([FromBody] VRParkStartSessionRequest request)
    {
        try
        {
            _logger.LogInformation("VR Park session start attempt: UserId={UserId}", request.UserId);

            // Validate model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                _logger.LogWarning("VR Park session start validation failed: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "Μη έγκυρα δεδομένα", errors });
            }

            // Verify user exists
            var userExists = await _context.VRParkUsers.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
            {
                _logger.LogWarning("VR Park session start failed: User not found - UserId={UserId}", request.UserId);
                return NotFound(new { message = "Ο χρήστης δεν βρέθηκε" });
            }

            // Create new game session
            var gameSession = new VRParkGameSession
            {
                UserId = request.UserId,
                StartedAt = DateTime.UtcNow,
                EyetrackingSequence = null,
                EndedAt = null
            };

            _context.VRParkGameSessions.Add(gameSession);
            await _context.SaveChangesAsync();

            _logger.LogInformation("VR Park session started successfully: SessionId={SessionId}, UserId={UserId}", 
                gameSession.Id, gameSession.UserId);

            return Ok(new VRParkStartSessionResponse
            {
                SessionId = gameSession.Id,
                UserId = gameSession.UserId,
                StartedAt = gameSession.StartedAt,
                Message = "Το session ξεκίνησε επιτυχώς!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VR Park session start failed");
            
            return StatusCode(500, new
            {
                message = "Αποτυχία εκκίνησης session. Παρακαλώ δοκιμάστε ξανά."
            });
        }
    }

    // End a game session and update with eye tracking data
    [HttpPost("session/end")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> EndGameSession([FromBody] VRParkEndSessionRequest request)
    {
        try
        {
            _logger.LogInformation("VR Park session end attempt: SessionId={SessionId}, UserId={UserId}", 
                request.SessionId, request.UserId);

            // Validate model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                _logger.LogWarning("VR Park session end validation failed: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "Μη έγκυρα δεδομένα", errors });
            }

            // Find the session and verify it belongs to the user
            var gameSession = await _context.VRParkGameSessions
                .FirstOrDefaultAsync(gs => gs.Id == request.SessionId && gs.UserId == request.UserId);

            if (gameSession == null)
            {
                _logger.LogWarning("VR Park session not found or user mismatch: SessionId={SessionId}, UserId={UserId}", 
                    request.SessionId, request.UserId);
                return NotFound(new { message = "Το session δεν βρέθηκε ή δεν ανήκει στον χρήστη" });
            }

            // Check if session is already ended
            if (gameSession.EndedAt != null)
            {
                _logger.LogWarning("VR Park session already ended: SessionId={SessionId}", request.SessionId);
                return BadRequest(new { message = "Το session έχει ήδη ολοκληρωθεί" });
            }

            // Update session
            gameSession.EndedAt = DateTime.UtcNow;
            gameSession.EyetrackingSequence = request.EyetrackingSequence;

            await _context.SaveChangesAsync();

            var duration = gameSession.EndedAt.Value - gameSession.StartedAt;

            _logger.LogInformation("VR Park session ended successfully: SessionId={SessionId}, Duration={Duration}s", 
                gameSession.Id, duration.TotalSeconds);

            return Ok(new VRParkEndSessionResponse
            {
                SessionId = gameSession.Id,
                UserId = gameSession.UserId,
                EyetrackingSequence = gameSession.EyetrackingSequence,
                StartedAt = gameSession.StartedAt,
                EndedAt = gameSession.EndedAt.Value,
                Duration = duration,
                Message = "Το session ολοκληρώθηκε επιτυχώς!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VR Park session end failed");
            
            return StatusCode(500, new
            {
                message = "Αποτυχία ολοκλήρωσης session. Παρακαλώ δοκιμάστε ξανά."
            });
        }
    }

    // Database viewer endpoint - Get all VR Park users
    [HttpGet("database/users")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Check if IP is locked out due to failed attempts
            if (_authTracker.IsIpLocked(ipAddress))
            {
                var remainingTime = _authTracker.GetRemainingLockoutTime(ipAddress);
                _logger.LogWarning(
                    "VR Park database users access denied: IP {IpAddress} is locked out. Remaining time: {RemainingMinutes} minutes",
                    ipAddress, remainingTime?.TotalMinutes ?? 0);
                
                return StatusCode(429, new 
                { 
                    message = "Too many failed attempts. Please try again later.",
                    retryAfterMinutes = (int)Math.Ceiling(remainingTime?.TotalMinutes ?? 0)
                });
            }

            // Check admin key from header
            if (!Request.Headers.TryGetValue("X-Admin-Key", out var adminKey) || string.IsNullOrWhiteSpace(adminKey))
            {
                _authTracker.RecordFailedAttempt(ipAddress);
                _logger.LogWarning(
                    "VR Park database users access denied: Missing admin key from IP {IpAddress}. Failed attempts: {Attempts}",
                    ipAddress, _authTracker.GetFailedAttemptCount(ipAddress));
                return Unauthorized(new { message = "Admin key is required" });
            }

            // Validate admin key from AdminSettings
            if (adminKey != _adminOptions.ApiKey)
            {
                _authTracker.RecordFailedAttempt(ipAddress);
                var failedAttempts = _authTracker.GetFailedAttemptCount(ipAddress);
                
                _logger.LogWarning(
                    "VR Park database users access denied: Invalid admin key from IP {IpAddress}. Failed attempts: {Attempts}",
                    ipAddress, failedAttempts);
                
                return StatusCode(403, new { message = "Invalid admin key" });
            }

            // Successful authentication - reset failed attempts
            _authTracker.ResetFailedAttempts(ipAddress);

            _logger.LogInformation("VR Park database users fetch: Authorized");

            var users = await _context.VRParkUsers
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    UserId = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Age = u.Age,
                    Video = u.Video,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            // Convert UTC times to Greece time for display
            var usersWithGreeceTime = users.Select(u => new
            {
                u.UserId,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Age,
                u.Video,
                CreatedAt = ConvertToGreeceTime(u.CreatedAt)
            }).ToList();

            _logger.LogInformation("VR Park database users fetched: Count={Count}", users.Count);

            return Ok(usersWithGreeceTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VR Park database users fetch failed");
            
            return StatusCode(500, new { message = "Σφάλμα ανάκτησης δεδομένων" });
        }
    }

    // Database viewer endpoint - Get all VR Park game sessions
    [HttpGet("database/sessions")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> GetAllSessions()
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Check if IP is locked out due to failed attempts
            if (_authTracker.IsIpLocked(ipAddress))
            {
                var remainingTime = _authTracker.GetRemainingLockoutTime(ipAddress);
                _logger.LogWarning(
                    "VR Park database sessions access denied: IP {IpAddress} is locked out. Remaining time: {RemainingMinutes} minutes",
                    ipAddress, remainingTime?.TotalMinutes ?? 0);
                
                return StatusCode(429, new 
                { 
                    message = "Too many failed attempts. Please try again later.",
                    retryAfterMinutes = (int)Math.Ceiling(remainingTime?.TotalMinutes ?? 0)
                });
            }

            // Check admin key from header
            if (!Request.Headers.TryGetValue("X-Admin-Key", out var adminKey) || string.IsNullOrWhiteSpace(adminKey))
            {
                _authTracker.RecordFailedAttempt(ipAddress);
                _logger.LogWarning(
                    "VR Park database sessions access denied: Missing admin key from IP {IpAddress}. Failed attempts: {Attempts}",
                    ipAddress, _authTracker.GetFailedAttemptCount(ipAddress));
                return Unauthorized(new { message = "Admin key is required" });
            }

            // Validate admin key from AdminSettings
            if (adminKey != _adminOptions.ApiKey)
            {
                _authTracker.RecordFailedAttempt(ipAddress);
                var failedAttempts = _authTracker.GetFailedAttemptCount(ipAddress);
                
                _logger.LogWarning(
                    "VR Park database sessions access denied: Invalid admin key from IP {IpAddress}. Failed attempts: {Attempts}",
                    ipAddress, failedAttempts);
                
                return StatusCode(403, new { message = "Invalid admin key" });
            }

            // Successful authentication - reset failed attempts
            _authTracker.ResetFailedAttempts(ipAddress);

            _logger.LogInformation("VR Park database sessions fetch: Authorized");

            var sessions = await _context.VRParkGameSessions
                .OrderByDescending(s => s.StartedAt)
                .Select(s => new
                {
                    SessionId = s.Id,
                    UserId = s.UserId,
                    StartedAt = s.StartedAt,
                    EndedAt = s.EndedAt,
                    EyetrackingSequence = s.EyetrackingSequence
                })
                .ToListAsync();

            // Convert UTC times to Greece time for display
            var sessionsWithGreeceTime = sessions.Select(s => new
            {
                s.SessionId,
                s.UserId,
                StartedAt = ConvertToGreeceTime(s.StartedAt),
                EndedAt = s.EndedAt.HasValue ? ConvertToGreeceTime(s.EndedAt.Value) : (DateTime?)null,
                s.EyetrackingSequence
            }).ToList();

            _logger.LogInformation("VR Park database sessions fetched: Count={Count}", sessions.Count);

            return Ok(sessionsWithGreeceTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VR Park database sessions fetch failed");
            
            return StatusCode(500, new { message = "Σφάλμα ανάκτησης δεδομένων" });
        }
    }
}
