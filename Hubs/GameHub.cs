using Microsoft.AspNetCore.SignalR;
using Arcane_Coop.Services;

namespace Arcane_Coop.Hubs;

public class GameHub : Hub
{
    private readonly IGameRoomService _gameRoomService;
    private readonly IPuzzleEngine _puzzleEngine;
    private readonly IStateManager _stateManager;
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        IGameRoomService gameRoomService, 
        IPuzzleEngine puzzleEngine, 
        IStateManager stateManager,
        ILogger<GameHub> logger)
    {
        _gameRoomService = gameRoomService;
        _puzzleEngine = puzzleEngine;
        _stateManager = stateManager;
        _logger = logger;
    }

    public async Task CreateRoom(string roomName, string playerName, string city)
    {
        _logger.LogInformation("CreateRoom called with: RoomName={RoomName}, PlayerName={PlayerName}, City={City}", 
            roomName, playerName, city);
        
        try
        {
            _logger.LogInformation("Attempting to create room...");
            var room = await _gameRoomService.CreateRoomAsync(roomName, playerName);
            if (room == null)
            {
                _logger.LogWarning("Room creation returned null");
                await Clients.Caller.SendAsync("RoomCreationFailed", "Failed to create room.");
                return;
            }

            _logger.LogInformation("Room created successfully with ID: {RoomId}", room.RoomId);

            _logger.LogInformation("Attempting to add player to room...");
            // First player gets their chosen city, no character needed
            var player = await _gameRoomService.JoinRoomAsync(room.RoomId, playerName, "", city, Context.ConnectionId);
            if (player == null)
            {
                _logger.LogWarning("Failed to add player to room");
                await Clients.Caller.SendAsync("JoinRoomFailed", "Failed to join created room.");
                return;
            }

            _logger.LogInformation("Player added to room successfully");

            await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomId);
            _logger.LogInformation("Added player to SignalR group");

            // Send room state instead of just creation confirmation
            var availablePuzzles = await _puzzleEngine.GetAvailablePuzzlesAsync(room.RoomId);
            await Clients.Caller.SendAsync("JoinedRoom", room, availablePuzzles);
            _logger.LogInformation("Sent room state to creator");
            
            _logger.LogInformation("Player {PlayerName} created and joined room {RoomId}", playerName, room.RoomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room for player {PlayerName}", playerName);
            await Clients.Caller.SendAsync("Error", "An error occurred while creating the room: " + ex.Message);
        }
    }

    public async Task JoinRoom(string roomId, string playerName)
    {
        try
        {
            // First check if room exists and get current players
            var room = await _gameRoomService.GetRoomAsync(roomId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("JoinRoomFailed", "Room not found.");
                return;
            }

            if (room.Players.Count >= room.MaxPlayers)
            {
                await Clients.Caller.SendAsync("JoinRoomFailed", "Room is full.");
                return;
            }

            // Automatically assign the opposite city from existing player
            string assignedCity = "";
            if (room.Players.Count == 1)
            {
                var existingPlayer = room.Players.First();
                assignedCity = existingPlayer.City.Equals("Zaun", StringComparison.OrdinalIgnoreCase) ? "Piltover" : "Zaun";
            }
            else
            {
                // First player case (shouldn't happen in join, but fallback)
                assignedCity = "Zaun";
            }

            var player = await _gameRoomService.JoinRoomAsync(roomId, playerName, "", assignedCity, Context.ConnectionId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("JoinRoomFailed", "Unable to join room.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            // FIRST: Send room state to the joining player so they have context
            var updatedRoom = await _gameRoomService.GetRoomAsync(roomId);
            var availablePuzzles = await _puzzleEngine.GetAvailablePuzzlesAsync(roomId);
            
            await Clients.Caller.SendAsync("JoinedRoom", updatedRoom, availablePuzzles);
            
            // THEN: Notify all players (including the joiner) about the new player
            await Clients.Group(roomId).SendAsync("PlayerJoined", player);
            
            _logger.LogInformation("Player {PlayerName} joined room {RoomId} from {City}", 
                playerName, roomId, assignedCity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining room {RoomId} for player {PlayerName}", roomId, playerName);
            await Clients.Caller.SendAsync("Error", "An error occurred while joining the room.");
        }
    }

    public async Task LeaveRoom(string roomId)
    {
        try
        {
            var success = await _gameRoomService.LeaveRoomAsync(roomId, Context.ConnectionId);
            if (success)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                await Clients.OthersInGroup(roomId).SendAsync("PlayerLeft", Context.ConnectionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving room {RoomId}", roomId);
        }
    }

    public async Task SendMessageToRoom(string roomId, string playerName, string message)
    {
        await Clients.Group(roomId).SendAsync("ReceiveMessage", playerName, message);
    }

    public async Task RequestPuzzle(string roomId, int puzzleId)
    {
        try
        {
            var room = await _gameRoomService.GetRoomAsync(roomId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found.");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Player not found in room.");
                return;
            }

            var puzzleData = await _puzzleEngine.GetPuzzleForPlayerAsync(roomId, player.PlayerId.ToString(), puzzleId);
            if (puzzleData == null)
            {
                await Clients.Caller.SendAsync("Error", "Puzzle not found.");
                return;
            }

            await Clients.Caller.SendAsync("PuzzleData", puzzleData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting puzzle {PuzzleId} for room {RoomId}", puzzleId, roomId);
            await Clients.Caller.SendAsync("Error", "An error occurred while loading the puzzle.");
        }
    }

    public async Task SubmitAnswer(string roomId, int puzzleId, string answer)
    {
        try
        {
            var room = await _gameRoomService.GetRoomAsync(roomId);
            if (room == null) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            var result = await _puzzleEngine.ValidateAnswerAsync(roomId, puzzleId, player.PlayerId.ToString(), answer);
            
            // Send result to the player who submitted
            await Clients.Caller.SendAsync("AnswerResult", puzzleId, result);

            // If puzzle was completed, notify all players in the room
            if (result.PuzzleCompleted)
            {
                var availablePuzzles = await _puzzleEngine.GetAvailablePuzzlesAsync(roomId);
                await Clients.Group(roomId).SendAsync("PuzzleCompleted", puzzleId, availablePuzzles);
            }
            else if (result.IsCorrect == false && !string.IsNullOrEmpty(result.Message))
            {
                // For cooperative puzzles, notify the other player about progress
                var progress = await _puzzleEngine.GetPuzzleProgressAsync(roomId, puzzleId);
                await Clients.OthersInGroup(roomId).SendAsync("PuzzleProgress", puzzleId, progress);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting answer for puzzle {PuzzleId} in room {RoomId}", puzzleId, roomId);
            await Clients.Caller.SendAsync("Error", "An error occurred while submitting your answer.");
        }
    }

    public async Task JoinRoomByConnectionId(string roomId, string playerName)
    {
        try
        {
            var room = await _gameRoomService.GetRoomAsync(roomId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found.");
                return;
            }

            // Find player by name (proper identification)
            var player = room.Players.FirstOrDefault(p => p.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Player not found in room.");
                return;
            }

            // Update connection ID and ensure connected status
            await _gameRoomService.UpdatePlayerConnectionIdAsync(player.PlayerId, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            _logger.LogInformation("Player {PlayerName} reconnected to room {RoomId} with connection {ConnectionId}", 
                playerName, roomId, Context.ConnectionId);

            // FIRST: Send updated room state to the reconnecting player
            var updatedRoom = await _gameRoomService.GetRoomAsync(roomId);
            var availablePuzzles = await _puzzleEngine.GetAvailablePuzzlesAsync(roomId);
            var gameState = await _stateManager.GetFullGameStateAsync(roomId);

            await Clients.Caller.SendAsync("JoinedRoom", updatedRoom, availablePuzzles);
            
            // THEN: Notify other players that this player reconnected
            await Clients.OthersInGroup(roomId).SendAsync("PlayerReconnected", player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining room by connection for {PlayerName} in {RoomId}", playerName, roomId);
            await Clients.Caller.SendAsync("Error", "An error occurred while joining the room.");
        }
    }

    public async Task RequestRoomState(string roomId)
    {
        try
        {
            var room = await _gameRoomService.GetRoomAsync(roomId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found.");
                return;
            }

            // Simple approach: just send room state without trying to modify connections
            var availablePuzzles = await _puzzleEngine.GetAvailablePuzzlesAsync(roomId);
            var gameState = await _stateManager.GetFullGameStateAsync(roomId);

            await Clients.Caller.SendAsync("RoomState", room, availablePuzzles, gameState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting room state for {RoomId}", roomId);
            await Clients.Caller.SendAsync("Error", "An error occurred while loading room state.");
        }
    }

    public async Task SendPlayerAction(string roomId, string action, object data)
    {
        await Clients.OthersInGroup(roomId).SendAsync("PlayerActionReceived", Context.ConnectionId, action, data);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            // Update player connection status
            await _gameRoomService.UpdatePlayerConnectionAsync(Context.ConnectionId, false);
            
            // Note: We don't remove from groups here as the player might reconnect
            // The LeaveRoom method should be called explicitly when a player intentionally leaves
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling disconnection for {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}