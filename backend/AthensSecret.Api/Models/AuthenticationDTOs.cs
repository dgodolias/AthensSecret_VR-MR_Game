namespace AthensSecret.Api.Models;

// New simple registration - no password needed
public class PlayerRegistrationRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Q1 { get; set; }
    public required string Q2 { get; set; }
    public required string Q3 { get; set; }
}

public class PlayerRegistrationResponse
{
    public int PlayerId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string Message { get; set; } = "Registration successful";
}

// Simple player verification - just check if ID exists
public class PlayerVerificationRequest
{
    public int PlayerId { get; set; }
}

public class PlayerVerificationResponse
{
    public bool Exists { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string Message { get; set; } = "";
}
