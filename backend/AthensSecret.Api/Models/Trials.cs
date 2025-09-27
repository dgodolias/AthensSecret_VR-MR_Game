namespace AthensSecret.Api.Models;

public class MirrorsTrial
{
    public int GameSessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalGainedWisdom { get; set; }
    
    // Navigation property
    public virtual GameSession? GameSession { get; set; }
}

public class OliveTreeTrial
{
    public int GameSessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalGainedWisdom { get; set; }
    public DateTime? InvestmentStartTime { get; set; }
    
    // Navigation property
    public virtual GameSession? GameSession { get; set; }
}

public class PathTrial
{
    public int GameSessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalGainedWisdom { get; set; }
    public bool SafePath { get; set; }
    
    // Navigation property
    public virtual GameSession? GameSession { get; set; }
}