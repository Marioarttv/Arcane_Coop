using System.ComponentModel.DataAnnotations;

namespace Arcane_Coop.Models;

public class GameRoom
{
    [Key]
    public string RoomId { get; set; } = string.Empty;
    
    public string RoomName { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public int MaxPlayers { get; set; } = 2;
    
    public string GameStateJson { get; set; } = "{}";
    
    public List<Player> Players { get; set; } = new();
}

public class Player
{
    [Key]
    public int PlayerId { get; set; }
    
    public string ConnectionId { get; set; } = string.Empty;
    
    public string PlayerName { get; set; } = string.Empty;
    
    public string Character { get; set; } = string.Empty; // Vi, Caitlyn, Jayce, Viktor
    
    public string City { get; set; } = string.Empty; // Zaun, Piltover
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsConnected { get; set; } = true;
    
    // Foreign key
    public string RoomId { get; set; } = string.Empty;
    public GameRoom? Room { get; set; }
}

public class GameState
{
    public Dictionary<string, object> SharedState { get; set; } = new();
    public Dictionary<string, Dictionary<string, object>> PlayerSpecificState { get; set; } = new();
    public List<string> CompletedPuzzles { get; set; } = new();
    public int CurrentPhase { get; set; } = 1;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}