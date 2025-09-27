namespace AthensSecret.Api.Models;

public class Response
{
    public int PlayerId { get; set; }
    public int Q1 { get; set; }  // Patience (1-10)
    public int Q2 { get; set; }  // Risk tolerance (1-10)
    
    // Navigation property
    public virtual Player? Player { get; set; }
}