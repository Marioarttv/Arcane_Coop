namespace Arcane_Coop.Models;

/// <summary>
/// Word-Forge (Affix Workshop) models for cooperative word formation puzzle
/// Player A (Piltover) has root words to drag and forge
/// Player B (Zaunite) has affixes to drag and combine with roots
/// Both players drag elements to central anvil to create target words
/// </summary>

public class WordElement
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string Description { get; set; } = "";
    public WordElementType Type { get; set; }
    public bool IsUsed { get; set; } = false;
}

public enum WordElementType
{
    Root,       // Base word (rely, help, care, use, read)
    Prefix,     // Beginning affix (un-, re-)
    Suffix      // Ending affix (-able, -er, -ment)
}

public enum GameMode
{
    Assisted,   // Shows target definitions for guidance
    Freeform    // No hints - discover combinations independently
}

public class WordCombination
{
    public string Id { get; set; } = "";
    public string RootId { get; set; } = "";
    public string AffixId { get; set; } = "";
    public string ResultWord { get; set; } = "";
    public string Definition { get; set; } = "";
    public bool IsCompleted { get; set; } = false;
    public int Order { get; set; } // For progression tracking
}

public class AnvilSlot
{
    public WordElement? RootElement { get; set; }
    public WordElement? AffixElement { get; set; }
    public bool IsComplete => RootElement != null && AffixElement != null;
}

public class WordForgePlayerView
{
    public string Role { get; set; } = ""; // "Piltover" or "Zaunite"
    public string DisplayName { get; set; } = "";
    public string Instruction { get; set; } = "";
    
    // Player-specific word elements
    public WordElement[]? AvailableElements { get; set; }
    
    // Shared anvil state
    public AnvilSlot? AnvilState { get; set; }
    
    // Game progress
    public WordCombination[]? TargetCombinations { get; set; }
    public WordCombination[]? CompletedCombinations { get; set; }
    public int ElementsRemaining { get; set; }
    
    // Game state
    public bool IsCompleted { get; set; }
    public GameMode Mode { get; set; }
    public string? CompletionMessage { get; set; }
    public string? ErrorMessage { get; set; }
}

public class WordForgeGameState
{
    public int CompletedCombinations { get; set; }
    public int TotalCombinations { get; set; }
    public bool IsCompleted { get; set; }
    public int PlayerCount { get; set; }
    public int PlayersNeeded { get; set; } = 2;
    public bool HasStarted { get; set; }
    public GameMode Mode { get; set; }
}

public class WordForgeAction
{
    public string ActionType { get; set; } = ""; // "place_element", "forge_combination", "restart"
    public string ElementId { get; set; } = "";
    public string SlotType { get; set; } = ""; // "root" or "affix"
}

public class ForgeAttempt
{
    public string RootId { get; set; } = "";
    public string AffixId { get; set; } = "";
    public string ExpectedResult { get; set; } = "";
    public bool IsSuccess { get; set; }
    public string ResultMessage { get; set; } = "";
}