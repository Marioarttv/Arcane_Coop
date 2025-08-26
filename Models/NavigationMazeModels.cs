namespace Arcane_Coop.Models;

/// <summary>
/// Navigation Maze models for cooperative pathfinding puzzle
/// </summary>
public class NavigationLocation
{
    public int LocationId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public NavigationChoice[] Choices { get; set; } = Array.Empty<NavigationChoice>();
    public int CorrectChoiceIndex { get; set; }
    public string SuccessMessage { get; set; } = "";
    public string[] Tags { get; set; } = Array.Empty<string>();
}

public class NavigationChoice
{
    public string Direction { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsCorrect { get; set; }
    public string GameOverMessage { get; set; } = "";
}

public class NavigationPlayerView
{
    public string Role { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Instruction { get; set; } = "";
    
    // Zaun player (first-person view)
    public string? LocationName { get; set; }
    public string? LocationDescription { get; set; }
    public string? LocationImage { get; set; }
    public NavigationChoice[]? AvailableChoices { get; set; }
    
    // Piltover player (notes view)
    public NavigationNotePublic[]? Notes { get; set; }
    
    // Shared
    public string? GameOverMessage { get; set; }
    public bool IsGameOver { get; set; }
}

public class NavigationGameState
{
    public int CurrentLocationId { get; set; }
    public int TotalLocations { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsGameOver { get; set; }
    public int PlayerCount { get; set; }
    public int PlayersNeeded { get; set; }
    public string? GameOverMessage { get; set; }
}

/// <summary>
/// Internal note definition with matching tags and intended direction.
/// Only Text and Id are sent to Piltover client.
/// </summary>
public class NavigationNote
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public string[] AppliesWhenTags { get; set; } = Array.Empty<string>();
    public string Direction { get; set; } = ""; // Human-readable (e.g., LEFT, FORWARD, AROUND RIGHT)
}

public class NavigationNotePublic
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
}