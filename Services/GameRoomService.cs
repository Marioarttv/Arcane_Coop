using Arcane_Coop.Data;
using Arcane_Coop.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Arcane_Coop.Services;

public interface IGameRoomService
{
    Task<GameRoom?> CreateRoomAsync(string roomName, string creatorName);
    Task<GameRoom?> GetRoomAsync(string roomId);
    Task<Player?> JoinRoomAsync(string roomId, string playerName, string character, string city, string connectionId);
    Task<bool> LeaveRoomAsync(string roomId, string connectionId);
    Task<bool> UpdateGameStateAsync(string roomId, GameState gameState);
    Task<List<Player>> GetPlayersInRoomAsync(string roomId);
    Task<bool> IsRoomFullAsync(string roomId);
    Task<bool> UpdatePlayerConnectionAsync(string connectionId, bool isConnected);
    Task<bool> UpdatePlayerConnectionIdAsync(int playerId, string newConnectionId);
    Task<string> GenerateUniqueRoomIdAsync();
}

public class GameRoomService : IGameRoomService
{
    private readonly GameDbContext _context;
    private readonly ILogger<GameRoomService> _logger;

    public GameRoomService(GameDbContext context, ILogger<GameRoomService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GameRoom?> CreateRoomAsync(string roomName, string creatorName)
    {
        try
        {
            var roomId = await GenerateUniqueRoomIdAsync();
            var initialGameState = new GameState
            {
                SharedState = new Dictionary<string, object>
                {
                    ["current_phase"] = 1,
                    ["completed_puzzles"] = new List<string>()
                },
                PlayerSpecificState = new Dictionary<string, Dictionary<string, object>>(),
                CompletedPuzzles = new List<string>(),
                CurrentPhase = 1,
                LastUpdated = DateTime.UtcNow
            };

            var room = new GameRoom
            {
                RoomId = roomId,
                RoomName = roomName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                MaxPlayers = 2,
                GameStateJson = JsonSerializer.Serialize(initialGameState),
                Players = new List<Player>()
            };

            _context.GameRooms.Add(room);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created room {RoomId} with name {RoomName}", roomId, roomName);
            return room;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room {RoomName}", roomName);
            return null;
        }
    }

    public async Task<GameRoom?> GetRoomAsync(string roomId)
    {
        try
        {
            return await _context.GameRooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room {RoomId}", roomId);
            return null;
        }
    }

    public async Task<Player?> JoinRoomAsync(string roomId, string playerName, string character, string city, string connectionId)
    {
        try
        {
            var room = await GetRoomAsync(roomId);
            if (room == null)
            {
                _logger.LogWarning("Attempted to join non-existent room {RoomId}", roomId);
                return null;
            }

            if (room.Players.Count >= room.MaxPlayers)
            {
                _logger.LogWarning("Room {RoomId} is full", roomId);
                return null;
            }

            // Check if player with this connection is already in room
            var existingPlayer = room.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (existingPlayer != null)
            {
                existingPlayer.IsConnected = true;
                await _context.SaveChangesAsync();
                return existingPlayer;
            }

            // Validate city selection - ensure one Zaun and one Piltover player
            var zaunPlayers = room.Players.Count(p => p.City.Equals("Zaun", StringComparison.OrdinalIgnoreCase));
            var piltoverPlayers = room.Players.Count(p => p.City.Equals("Piltover", StringComparison.OrdinalIgnoreCase));

            if (city.Equals("Zaun", StringComparison.OrdinalIgnoreCase) && zaunPlayers >= 1)
            {
                _logger.LogWarning("Room {RoomId} already has a Zaun player", roomId);
                return null;
            }

            if (city.Equals("Piltover", StringComparison.OrdinalIgnoreCase) && piltoverPlayers >= 1)
            {
                _logger.LogWarning("Room {RoomId} already has a Piltover player", roomId);
                return null;
            }

            var player = new Player
            {
                ConnectionId = connectionId,
                PlayerName = playerName,
                Character = character,
                City = city,
                JoinedAt = DateTime.UtcNow,
                IsConnected = true,
                RoomId = roomId
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Player {PlayerName} joined room {RoomId} as {Character} from {City}", 
                playerName, roomId, character, city);
            
            return player;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding player {PlayerName} to room {RoomId}", playerName, roomId);
            return null;
        }
    }

    public async Task<bool> LeaveRoomAsync(string roomId, string connectionId)
    {
        try
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.ConnectionId == connectionId);

            if (player != null)
            {
                player.IsConnected = false;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Player {PlayerName} left room {RoomId}", player.PlayerName, roomId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing player from room {RoomId}", roomId);
            return false;
        }
    }

    public async Task<bool> UpdateGameStateAsync(string roomId, GameState gameState)
    {
        try
        {
            var room = await _context.GameRooms.FirstOrDefaultAsync(r => r.RoomId == roomId);
            if (room == null) return false;

            gameState.LastUpdated = DateTime.UtcNow;
            room.GameStateJson = JsonSerializer.Serialize(gameState);
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating game state for room {RoomId}", roomId);
            return false;
        }
    }

    public async Task<List<Player>> GetPlayersInRoomAsync(string roomId)
    {
        try
        {
            return await _context.Players
                .Where(p => p.RoomId == roomId && p.IsConnected)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting players for room {RoomId}", roomId);
            return new List<Player>();
        }
    }

    public async Task<bool> IsRoomFullAsync(string roomId)
    {
        try
        {
            var room = await GetRoomAsync(roomId);
            return room != null && room.Players.Count(p => p.IsConnected) >= room.MaxPlayers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if room {RoomId} is full", roomId);
            return true; // Assume full on error to prevent issues
        }
    }

    public async Task<bool> UpdatePlayerConnectionAsync(string connectionId, bool isConnected)
    {
        try
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.ConnectionId == connectionId);

            if (player != null)
            {
                player.IsConnected = isConnected;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating connection status for {ConnectionId}", connectionId);
            return false;
        }
    }

    public async Task<bool> UpdatePlayerConnectionIdAsync(int playerId, string newConnectionId)
    {
        try
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.PlayerId == playerId);

            if (player != null)
            {
                player.ConnectionId = newConnectionId;
                player.IsConnected = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated connection ID for player {PlayerId} to {ConnectionId}", playerId, newConnectionId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating connection ID for player {PlayerId}", playerId);
            return false;
        }
    }

    public async Task<string> GenerateUniqueRoomIdAsync()
    {
        string roomId;
        bool exists;

        do
        {
            // Generate a 6-character room code
            roomId = GenerateRoomCode();
            exists = await _context.GameRooms.AnyAsync(r => r.RoomId == roomId);
        } while (exists);

        return roomId;
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}