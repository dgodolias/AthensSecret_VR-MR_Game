namespace AthensSecret.Api.Models;

public class Player
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public int Age { get; set; }
    
    // Navigation properties
    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    public virtual Response? Response { get; set; }
}
