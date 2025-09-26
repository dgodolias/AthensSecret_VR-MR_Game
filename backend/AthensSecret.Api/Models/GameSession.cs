namespace AthensSecret.Api.Models;

public class GameSession
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    // Navigation properties - exactly as per relationships.md
    public virtual Player? Player { get; set; }
    public virtual MirrorsTrial? MirrorsTrial { get; set; }
    public virtual OilTreeTrial? OilTreeTrial { get; set; }
    public virtual PathTrial? PathTrial { get; set; }
}
