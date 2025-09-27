namespace AthensSecret.Api.Models;

public class GameConfiguration
{
    public int Id { get; set; }
    
    // Core wisdom values - exactly as per relationships.md
    public int StartingWisdom { get; set; }
    public int MirrorWisdomIfWaits { get; set; }
    public int MirrorWisdomIfRisksCorrectly { get; set; }
    public int MirrorWisdomIfRisksFalsely { get; set; }
    public int OliveTreeWisdomNotInvestment { get; set; }
    public required string OliveTreeWisdomInvestmentFunction { get; set; }
    public int SafePathWisdom { get; set; }
    public int UncertainPathWisdom { get; set; }
    public int UncertainPathWisdomSmallPlank { get; set; }
    public int UncertainPathWisdomMediumPlank { get; set; }
    public int UncertainPathWisdomBigPlank { get; set; }
}