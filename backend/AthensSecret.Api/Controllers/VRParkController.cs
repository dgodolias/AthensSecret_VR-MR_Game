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
            var vrParkUser = new VRParkUser
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                Age = request.Age,
                CreatedAt = DateTime.UtcNow
            };

            _context.VRParkUsers.Add(vrParkUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("VR Park user created successfully: UserId={UserId}, Name={FirstName} {LastName}", 
                vrParkUser.Id, vrParkUser.FirstName, vrParkUser.LastName);

            return Ok(new VRParkSignupResponse
            {
                UserId = vrParkUser.Id,
                FirstName = vrParkUser.FirstName,
                LastName = vrParkUser.LastName,
                Email = vrParkUser.Email,
                Age = vrParkUser.Age,
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

    // Get all VR Park users (optional - για admin purposes)
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _context.VRParkUsers
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Age,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve VR Park users: {ErrorMessage}", ex.Message);
            return StatusCode(500, new { message = "Αποτυχία ανάκτησης χρηστών" });
        }
    }

    // Get user count
    [HttpGet("count")]
    public async Task<IActionResult> GetUserCount()
    {
        try
        {
            var count = await _context.VRParkUsers.CountAsync();
            return Ok(new { totalUsers = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get VR Park user count: {ErrorMessage}", ex.Message);
            return StatusCode(500, new { message = "Αποτυχία" });
        }
    }
}
