namespace AthensSecret.Api.Models;

// New simple registration - no password needed
public class PlayerRegistrationRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }  // Email is now optional
    public int Age { get; set; }
    public int Q1 { get; set; }  // Patience (1-10)
    public int Q2 { get; set; }  // Risk tolerance (1-10)
}

public class PlayerRegistrationResponse
{
    public int PlayerId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }  // Email is now optional
    public int Age { get; set; }
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
    public int Age { get; set; }
    public string Message { get; set; } = "";
}
