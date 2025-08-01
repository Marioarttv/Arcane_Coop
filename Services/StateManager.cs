using Arcane_Coop.Models;
using System.Text.Json;

namespace Arcane_Coop.Services;

public interface IStateManager
{
    Task<T?> GetSharedStateAsync<T>(string roomId, string key);
    Task<bool> SetSharedStateAsync<T>(string roomId, string key, T value);
    Task<T?> GetPlayerStateAsync<T>(string roomId, string playerId, string key);
    Task<bool> SetPlayerStateAsync<T>(string roomId, string playerId, string key, T value);
    Task<bool> ClearPlayerStateAsync(string roomId, string playerId, string key);
    Task<bool> ClearSharedStateAsync(string roomId, string key);
    Task<GameState?> GetFullGameStateAsync(string roomId);
    Task<Dictionary<string, object>> GetPlayerSpecificStateAsync(string roomId, string playerId);
}

public class StateManager : IStateManager
{
    private readonly IGameRoomService _gameRoomService;
    private readonly ILogger<StateManager> _logger;

    public StateManager(IGameRoomService gameRoomService, ILogger<StateManager> logger)
    {
        _gameRoomService = gameRoomService;
        _logger = logger;
    }

    public async Task<T?> GetSharedStateAsync<T>(string roomId, string key)
    {
        try
        {
            var gameState = await GetGameStateAsync(roomId);
            if (gameState?.SharedState.ContainsKey(key) == true)
            {
                var value = gameState.SharedState[key];
                return DeserializeValue<T>(value);
            }
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shared state {Key} for room {RoomId}", key, roomId);
            return default(T);
        }
    }

    public async Task<bool> SetSharedStateAsync<T>(string roomId, string key, T value)
    {
        try
        {
            var gameState = await GetGameStateAsync(roomId) ?? new GameState();
            
            gameState.SharedState[key] = SerializeValue(value);
            gameState.LastUpdated = DateTime.UtcNow;

            return await _gameRoomService.UpdateGameStateAsync(roomId, gameState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting shared state {Key} for room {RoomId}", key, roomId);
            return false;
        }
    }

    public async Task<T?> GetPlayerStateAsync<T>(string roomId, string playerId, string key)
    {
        try
        {
            var gameState = await GetGameStateAsync(roomId);
            if (gameState?.PlayerSpecificState.ContainsKey(playerId) == true &&
                gameState.PlayerSpecificState[playerId].ContainsKey(key))
            {
                var value = gameState.PlayerSpecificState[playerId][key];
                return DeserializeValue<T>(value);
            }
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player state {Key} for player {PlayerId} in room {RoomId}", key, playerId, roomId);
            return default(T);
        }
    }

    public async Task<bool> SetPlayerStateAsync<T>(string roomId, string playerId, string key, T value)
    {
        try
        {
            var gameState = await GetGameStateAsync(roomId) ?? new GameState();
            
            if (!gameState.PlayerSpecificState.ContainsKey(playerId))
            {
                gameState.PlayerSpecificState[playerId] = new Dictionary<string, object>();
            }

            gameState.PlayerSpecificState[playerId][key] = SerializeValue(value);
            gameState.LastUpdated = DateTime.UtcNow;

            return await _gameRoomService.UpdateGameStateAsync(roomId, gameState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting player state {Key} for player {PlayerId} in room {RoomId}", key, playerId, roomId);
            return false;
        }
    }

    public async Task<bool> ClearPlayerStateAsync(string roomId, string playerId, string key)
    {
        try
        {
            var gameState = await GetGameStateAsync(roomId);
            if (gameState?.PlayerSpecificState.ContainsKey(playerId) == true &&
                gameState.PlayerSpecificState[playerId].ContainsKey(key))
            {
                gameState.PlayerSpecificState[playerId].Remove(key);
                gameState.LastUpdated = DateTime.UtcNow;

                return await _gameRoomService.UpdateGameStateAsync(roomId, gameState);
            }
            return true; // Already cleared
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing player state {Key} for player {PlayerId} in room {RoomId}", key, playerId, roomId);
            return false;
        }
    }

    public async Task<bool> ClearSharedStateAsync(string roomId, string key)
    {
        try
        {
            var gameState = await GetGameStateAsync(roomId);
            if (gameState?.SharedState.ContainsKey(key) == true)
            {
                gameState.SharedState.Remove(key);
                gameState.LastUpdated = DateTime.UtcNow;

                return await _gameRoomService.UpdateGameStateAsync(roomId, gameState);
            }
            return true; // Already cleared
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing shared state {Key} for room {RoomId}", key, roomId);
            return false;
        }
    }

    public async Task<GameState?> GetFullGameStateAsync(string roomId)
    {
        return await GetGameStateAsync(roomId);
    }

    public async Task<Dictionary<string, object>> GetPlayerSpecificStateAsync(string roomId, string playerId)
    {
        try
        {
            var gameState = await GetGameStateAsync(roomId);
            if (gameState?.PlayerSpecificState.ContainsKey(playerId) == true)
            {
                return gameState.PlayerSpecificState[playerId];
            }
            return new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player specific state for player {PlayerId} in room {RoomId}", playerId, roomId);
            return new Dictionary<string, object>();
        }
    }

    private async Task<GameState?> GetGameStateAsync(string roomId)
    {
        try
        {
            var room = await _gameRoomService.GetRoomAsync(roomId);
            if (room == null) return null;

            if (string.IsNullOrEmpty(room.GameStateJson) || room.GameStateJson == "{}")
            {
                return new GameState();
            }

            return JsonSerializer.Deserialize<GameState>(room.GameStateJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing game state for room {RoomId}", roomId);
            return null;
        }
    }

    private static object SerializeValue<T>(T value)
    {
        if (value == null) return null!;
        
        // For primitive types, store directly
        if (value is string || value is int || value is bool || value is double || value is float)
        {
            return value;
        }

        // For complex types, serialize to JSON
        return JsonSerializer.Serialize(value);
    }

    private static T? DeserializeValue<T>(object value)
    {
        if (value == null) return default(T);

        // If T is the same type as the stored value, return directly
        if (value is T directValue)
        {
            return directValue;
        }

        // If the stored value is a string and T is not string, try to deserialize from JSON
        if (value is string jsonString && typeof(T) != typeof(string))
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonString);
            }
            catch
            {
                // If deserialization fails, try to convert the string to T
                return (T)Convert.ChangeType(jsonString, typeof(T));
            }
        }

        // Try to convert the value to T
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default(T);
        }
    }
}