namespace AthensSecret.Api.Models;

public class Response
{
    public int PlayerId { get; set; }
    public int Q1 { get; set; }
    public int Q2 { get; set; }
    public int Q3 { get; set; }
    
    // Navigation property
    public virtual Player? Player { get; set; }
}