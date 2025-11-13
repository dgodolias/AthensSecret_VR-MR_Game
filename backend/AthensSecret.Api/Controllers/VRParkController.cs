using AthensSecret.Api.Data;
using AthensSecret.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AthensSecret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VRParkController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<VRParkController> _logger;

    public VRParkController(ApiDbContext context, ILogger<VRParkController> logger)
    {
        _context = context;
        _logger = logger;
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
                    _logger.LogWarning("VR Park signup failed: Email already exists - {Email}", request.Email);
                    return BadRequest(new { message = "Το email υπάρχει ήδη" });
                }
            }

            // Create new VR Park user
            var random = new Random();
            var randomVideo = random.Next(1, 5); // Random number between 1 and 4

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
            _logger.LogError(ex, "VR Park signup failed: {ErrorMessage}", ex.Message);
            return StatusCode(500, new
            {
                message = "Αποτυχία εγγραφής",
                error = ex.Message
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
            _logger.LogError(ex, "VR Park session start failed: {ErrorMessage}", ex.Message);
            return StatusCode(500, new
            {
                message = "Αποτυχία εκκίνησης session",
                error = ex.Message
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
            _logger.LogError(ex, "VR Park session end failed: {ErrorMessage}", ex.Message);
            return StatusCode(500, new
            {
                message = "Αποτυχία ολοκλήρωσης session",
                error = ex.Message
            });
        }
    }
}
