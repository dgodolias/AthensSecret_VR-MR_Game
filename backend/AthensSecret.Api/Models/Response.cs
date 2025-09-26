namespace AthensSecret.Api.Models;

public class Response
{
    public int PlayerId { get; set; }
    public required string Q1 { get; set; }
    public required string Q2 { get; set; }
    public required string Q3 { get; set; }
    
    // Navigation property
    public virtual Player? Player { get; set; }
}