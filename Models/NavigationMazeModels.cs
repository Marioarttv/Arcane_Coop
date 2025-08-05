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
    
    // Piltover player (map view)
    public NavigationMapData? MapData { get; set; }
    
    // Shared
    public string? GameOverMessage { get; set; }
    public bool IsGameOver { get; set; }
}

public class NavigationMapData
{
    public int CurrentLocationId { get; set; }
    public NavigationMapLocation[] Locations { get; set; } = Array.Empty<NavigationMapLocation>();
    public NavigationMapPath[] CorrectPath { get; set; } = Array.Empty<NavigationMapPath>();
}

public class NavigationMapLocation
{
    public int LocationId { get; set; }
    public string Name { get; set; } = "";
    public float X { get; set; } // Position on map (percentage)
    public float Y { get; set; } // Position on map (percentage)
    public bool IsCurrentLocation { get; set; }
    public bool IsCompleted { get; set; }
}

public class NavigationMapPath
{
    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }
    public string Direction { get; set; } = "";
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