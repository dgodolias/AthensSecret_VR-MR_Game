namespace AthensSecret.Api.Models;

public class GameSession
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int WisdomEnergy { get; set; } = 50; // Default starting energy
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public string CurrentTrial { get; set; } = "start";
    public int Score { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual Player? Player { get; set; }
}
