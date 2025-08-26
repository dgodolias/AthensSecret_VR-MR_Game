namespace AthensSecret.Api.Models;

public class GameConfig
{
    public EnergySettings Energy { get; set; } = new();
    public TrialSettings Trials { get; set; } = new();
    public TimingSettings Timing { get; set; } = new();
    public ScoreSettings Scoring { get; set; } = new();
    public InvestmentSettings Investment { get; set; } = new();
}

public class EnergySettings
{
    public int StartingEnergy { get; set; } = 50;
    public int MinEnergy { get; set; } = 0;
    public int MaxEnergy { get; set; } = 1000;
    public int EnergyCapWarningThreshold { get; set; } = 950;
}

public class TrialSettings
{
    public PatienceTrialSettings Patience { get; set; } = new();
    public ResourceTrialSettings Resource { get; set; } = new();
    public RiskTrialSettings Risk { get; set; } = new();
}

public class PatienceTrialSettings
{
    public int TimerDurationSeconds { get; set; } = 60;
    public int BonusThresholdSeconds { get; set; } = 30;
    public int BonusEnergyAmount { get; set; } = 50;
    public int BonusScoreAmount { get; set; } = 50;
    public int NumberOfMirrors { get; set; } = 3;
    public string CorrectMirrorLogic { get; set; } = "random"; // "random", "fixed", "sequence"
}

public class ResourceTrialSettings
{
    public int PatienceGridSize { get; set; } = 25; // 5x5 grid
    public int ImmediateBonusEnergy { get; set; } = 50;
    public OliveInvestmentSettings OliveInvestment { get; set; } = new();
}

public class OliveInvestmentSettings
{
    public bool EnableInvestment { get; set; } = true;
    public string Formula { get; set; } = "sqrt"; // "linear", "sqrt", "log", "quadratic"
    public double FormulaMultiplier { get; set; } = 1.0;
    public int MaxBonusCap { get; set; } = 100;
    public int UpdateIntervalMs { get; set; } = 1000;
    public int MaxInvestmentDurationSeconds { get; set; } = 300; // 5 minutes max
}

public class RiskTrialSettings
{
    public int NumberOfPaths { get; set; } = 2;
    public PathSettings SafePath { get; set; } = new() { EnergyModifier = 10, Description = "Safe Path" };
    public PathSettings RiskyPath { get; set; } = new() { EnergyModifier = 25, Description = "Risky Path", SuccessRate = 0.7 };
}

public class PathSettings
{
    public string Description { get; set; } = "";
    public int EnergyModifier { get; set; }
    public double SuccessRate { get; set; } = 1.0; // 1.0 = 100% success, 0.7 = 70% success
    public int FailurePenalty { get; set; } = -15;
}

public class TimingSettings
{
    public int GameSessionTimeoutMinutes { get; set; } = 30;
    public int InactivityTimeoutMinutes { get; set; } = 10;
    public int AutoSaveIntervalSeconds { get; set; } = 30;
}

public class ScoreSettings
{
    public ScoreWeights Weights { get; set; } = new();
    public ScoreBonuses Bonuses { get; set; } = new();
}

public class ScoreWeights
{
    public double EnergyToScoreRatio { get; set; } = 1.0; // 1 energy = 1 score point
    public double TimeCompletionBonus { get; set; } = 0.5; // bonus for fast completion
    public double PerfectTrialMultiplier { get; set; } = 1.5; // bonus for perfect trials
}

public class ScoreBonuses
{
    public int FirstTimeCompletionBonus { get; set; } = 100;
    public int AllTrialsCompletedBonus { get; set; } = 200;
    public int HighEnergyFinishBonus { get; set; } = 150; // bonus for finishing with high energy
    public int HighEnergyThreshold { get; set; } = 200; // energy threshold for bonus
}

public class InvestmentSettings
{
    public Dictionary<string, InvestmentFormula> AvailableFormulas { get; set; } = new()
    {
        { "linear", new InvestmentFormula { Name = "Linear", Description = "1 energy per second", Formula = "x" } },
        { "sqrt", new InvestmentFormula { Name = "Square Root", Description = "Square root of seconds", Formula = "√x" } },
        { "log", new InvestmentFormula { Name = "Logarithmic", Description = "Log base 2 of (x+1)", Formula = "log₂(x+1)" } },
        { "quadratic", new InvestmentFormula { Name = "Quadratic", Description = "x² ÷ 100 (slow start, exponential)", Formula = "x²/100" } }
    };
}

public class InvestmentFormula
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Formula { get; set; } = "";
}
