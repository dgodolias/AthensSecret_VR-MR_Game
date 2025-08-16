namespace AthensSecret.Api.Models;

public class Player
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
}
