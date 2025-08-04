namespace Arcane_Coop.Models;

/// <summary>
/// Simple Signal Decoder models for single audio/sentence approach
/// </summary>
public class SimpleSignalData
{
    public string FullSentence { get; set; } = "";
    public string SentenceWithBlanks { get; set; } = "";
    public string[] MissingWords { get; set; } = Array.Empty<string>();
    public string AudioFile { get; set; } = "";
}

public class SimplePlayerView
{
    public string Role { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Instruction { get; set; } = "";
    public string? SentenceWithBlanks { get; set; }
    public string? AudioFile { get; set; }
    public List<string> GuessedWords { get; set; } = new();
    public List<string> AttemptHistory { get; set; } = new();
}

public class SimpleGameState
{
    public int Score { get; set; }
    public int HintsUsed { get; set; }
    public int PlayerCount { get; set; }
    public int PlayersNeeded { get; set; }
    public bool IsCompleted { get; set; }
    public int RemainingWords { get; set; }
    public int CurrentSignal { get; set; }
    public int TotalSignals { get; set; }
    public int SignalsCompleted { get; set; }
}