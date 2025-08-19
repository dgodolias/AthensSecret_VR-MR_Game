namespace AthensSecret.Api.Models;

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class RegisterRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class AuthResponse
{
    public required string Token { get; set; }
    public required string Username { get; set; }
    public int PlayerId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
