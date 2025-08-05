namespace Arcane_Coop.Models;

/// <summary>
/// Alchemy Lab models for cooperative potion brewing puzzle
/// Player A (Piltover) reads recipe instructions
/// Player B (Zaunite) performs alchemy using drag-drop interface
/// </summary>

public class AlchemyIngredient
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public IngredientState State { get; set; } = IngredientState.Raw;
    public bool IsUsed { get; set; } = false;
}

public enum IngredientState
{
    Raw,        // Original state
    Ground,     // After mortar & pestle
    Heated,     // After heating station
    Chopped,    // After cutting board
    Filtered,   // After filtering station
    Processed   // Generic processed state
}

public enum ProcessingStation
{
    MortarPestle,
    HeatingStation,
    CuttingBoard,
    FilteringStation,
    Cauldron
}

public class RecipeStep
{
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = "";
    public string IngredientId { get; set; } = "";
    public ProcessingStation RequiredStation { get; set; }
    public IngredientState RequiredState { get; set; }
    public string DetailedDescription { get; set; } = "";
}

public class AlchemyRecipe
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public RecipeStep[] Steps { get; set; } = Array.Empty<RecipeStep>();
    public string[] RequiredIngredients { get; set; } = Array.Empty<string>();
}

public class AlchemyPlayerView
{
    public string Role { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Instruction { get; set; } = "";
    
    // Piltover player (Recipe Reader)
    public AlchemyRecipe? Recipe { get; set; }
    public int CurrentStepIndex { get; set; }
    
    // Zaunite player (Lab Worker)
    public AlchemyIngredient[]? AvailableIngredients { get; set; }
    public AlchemyIngredient[]? CauldronContents { get; set; }
    public ProcessingStation[]? AvailableStations { get; set; }
    
    // Shared
    public bool IsCompleted { get; set; }
    public int Score { get; set; }
    public string? CompletionMessage { get; set; }
    public string[]? Mistakes { get; set; }
}

public class AlchemyGameState
{
    public int CurrentStepIndex { get; set; }
    public int TotalSteps { get; set; }
    public bool IsCompleted { get; set; }
    public int Score { get; set; }
    public int PlayerCount { get; set; }
    public int PlayersNeeded { get; set; }
    public bool HasStarted { get; set; }
}

public class AlchemyAction
{
    public string ActionType { get; set; } = ""; // "process", "add_to_cauldron", "submit"
    public string IngredientId { get; set; } = "";
    public ProcessingStation Station { get; set; }
    public int Position { get; set; } = -1; // For cauldron ordering
}

public class AlchemySubmission
{
    public AlchemyIngredient[] CauldronContents { get; set; } = Array.Empty<AlchemyIngredient>();
    public int Attempts { get; set; }
}