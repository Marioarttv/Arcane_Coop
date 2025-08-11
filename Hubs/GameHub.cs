using Microsoft.AspNetCore.SignalR;
using Arcane_Coop.Services;
using System.Collections.Concurrent;
using System.Linq;
using Arcane_Coop.Models;

namespace Arcane_Coop.Hubs;

public class GameHub : Hub
{
    private readonly IAct1StoryEngine _act1StoryEngine;

    public GameHub(IAct1StoryEngine act1StoryEngine)
    {
        _act1StoryEngine = act1StoryEngine;
    }
    private static readonly ConcurrentDictionary<string, TicTacToeGame> _games = new();
    private static readonly ConcurrentDictionary<string, CodeCrackerGame> _codeCrackerGames = new();
    private static readonly ConcurrentDictionary<string, SimpleSignalDecoderGame> _signalDecoderGames = new();
    private static readonly ConcurrentDictionary<string, NavigationMazeGame> _navigationMazeGames = new();
    private static readonly ConcurrentDictionary<string, AlchemyGame> _alchemyGames = new();
    private static readonly ConcurrentDictionary<string, RuneProtocolGame> _runeProtocolGames = new();
    private static readonly ConcurrentDictionary<string, PictureExplanationGame> _pictureExplanationGames = new();
    private static readonly ConcurrentDictionary<string, WordForgeGame> _wordForgeGames = new();
    private static readonly ConcurrentDictionary<string, VisualNovelMultiplayerGame> _visualNovelGames = new();
    private static readonly ConcurrentDictionary<string, Act1MultiplayerGame> _act1Games = new();
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _roomPlayers = new();
    // Track richer lobby player metadata per connection within a room (used for personalized redirects)
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, LobbyPlayerInfo>> _lobbyPlayers = new();

    private sealed class LobbyPlayerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "piltover" or "zaun"
        public string Avatar { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }

    private sealed class LobbyRolesSnapshot
    {
        public string? PiltoverBy { get; set; }
        public string? ZaunBy { get; set; }
    }

    public async Task JoinRoom(string roomId, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        // Add player to room tracking
        var roomPlayerDict = _roomPlayers.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, string>());
        roomPlayerDict.TryAdd(Context.ConnectionId, playerName);

        // Initialize lobby player metadata with at least the name; role/avatar can be updated later
        var lobbyDict = _lobbyPlayers.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, LobbyPlayerInfo>());
        lobbyDict.AddOrUpdate(Context.ConnectionId,
            _ => new LobbyPlayerInfo { Name = playerName, JoinedAt = DateTime.UtcNow },
            (_, existing) => { existing.Name = playerName; return existing; });
        
        // Send room state to the joining player
        var allPlayerNames = roomPlayerDict.Values.ToList();
        await Clients.Caller.SendAsync("RoomState", allPlayerNames);
        
        // Notify others in the room
        await Clients.Group(roomId).SendAsync("PlayerJoined", playerName, Context.ConnectionId);
    }

    public async Task LeaveRoom(string roomId, string playerName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        
        // Remove player from room tracking
        if (_roomPlayers.TryGetValue(roomId, out var roomPlayerDict))
        {
            roomPlayerDict.TryRemove(Context.ConnectionId, out _);
            
            // Clean up empty rooms
            if (roomPlayerDict.IsEmpty)
            {
                _roomPlayers.TryRemove(roomId, out _);
            }
        }

        // Remove from lobby metadata tracking
        if (_lobbyPlayers.TryGetValue(roomId, out var lobbyDict))
        {
            lobbyDict.TryRemove(Context.ConnectionId, out _);
            if (lobbyDict.IsEmpty)
            {
                _lobbyPlayers.TryRemove(roomId, out _);
            }
        }
        
        await Clients.Group(roomId).SendAsync("PlayerLeft", playerName, Context.ConnectionId);
        await BroadcastLobbyRoles(roomId);
    }

    public async Task SendMessageToRoom(string roomId, string playerName, string message)
    {
        await Clients.Group(roomId).SendAsync("ReceiveMessage", playerName, message);
    }

    public async Task RedirectPlayersToAct1(string originalRoomId, string storyLobbyName, string role, string avatar, string playerName)
    {
        // Prefer personalized redirects with enforced distinct roles when metadata is available
        if (_lobbyPlayers.TryGetValue(originalRoomId, out var lobbyDict) && !lobbyDict.IsEmpty)
        {
            // Build ordered list of players by JoinedAt to break ties deterministically
            var players = lobbyDict
                .Select(kvp => (ConnectionId: kvp.Key, Info: kvp.Value))
                .OrderBy(p => p.Info.JoinedAt)
                .ToList();

            // Determine final roles ensuring distinct assignment when two players are present
            string? assignedPiltoverConn = null;
            string? assignedZaunConn = null;

            foreach (var p in players)
            {
                var desired = (p.Info.Role ?? string.Empty).Trim().ToLowerInvariant();
                if (desired == "piltover" && assignedPiltoverConn == null)
                {
                    assignedPiltoverConn = p.ConnectionId;
                }
                else if ((desired == "zaun" || desired == "zaunite") && assignedZaunConn == null)
                {
                    assignedZaunConn = p.ConnectionId;
                }
            }

            // If duplicates or missing, fill remaining slots by join order
            foreach (var p in players)
            {
                if (assignedPiltoverConn == null && p.ConnectionId != assignedZaunConn)
                {
                    assignedPiltoverConn = p.ConnectionId;
                }
                else if (assignedZaunConn == null && p.ConnectionId != assignedPiltoverConn)
                {
                    assignedZaunConn = p.ConnectionId;
                }
            }

            // Send per-client redirects with final roles
            foreach (var p in players)
            {
                var finalRole = p.ConnectionId == assignedPiltoverConn ? "piltover" : "zaun";
                var effectiveAvatar = string.IsNullOrWhiteSpace(p.Info.Avatar) ? avatar : p.Info.Avatar;
                var effectiveName = string.IsNullOrWhiteSpace(p.Info.Name) ? playerName : p.Info.Name;
                var parameters = $"roomId={Uri.EscapeDataString(storyLobbyName)}&role={finalRole}&avatar={effectiveAvatar}&name={Uri.EscapeDataString(effectiveName)}&squad={Uri.EscapeDataString(originalRoomId)}";
                await Clients.Client(p.ConnectionId).SendAsync("RedirectToAct1", $"/act1-multiplayer?{parameters}");
            }
            return;
        }

        // Fallback: broadcast using caller's params (legacy behavior)
        var fallback = $"roomId={Uri.EscapeDataString(storyLobbyName)}&role={role}&avatar={avatar}&name={Uri.EscapeDataString(playerName)}&squad={Uri.EscapeDataString(originalRoomId)}";
        await Clients.Group(originalRoomId).SendAsync("RedirectToAct1", $"/act1-multiplayer?{fallback}");
    }
    
    public async Task RedirectPlayersToAct1WithScene(string originalRoomId, string storyLobbyName, string role, string avatar, string playerName, int sceneIndex)
    {
        // Similar to RedirectPlayersToAct1 but with sceneIndex parameter
        if (_lobbyPlayers.TryGetValue(originalRoomId, out var lobbyDict) && !lobbyDict.IsEmpty)
        {
            var players = lobbyDict
                .Select(kvp => (ConnectionId: kvp.Key, Info: kvp.Value))
                .OrderBy(p => p.Info.JoinedAt)
                .ToList();

            string? assignedPiltoverConn = null;
            string? assignedZaunConn = null;

            foreach (var p in players)
            {
                var desired = (p.Info.Role ?? string.Empty).Trim().ToLowerInvariant();
                if (desired == "piltover" && assignedPiltoverConn == null)
                {
                    assignedPiltoverConn = p.ConnectionId;
                }
                else if ((desired == "zaun" || desired == "zaunite") && assignedZaunConn == null)
                {
                    assignedZaunConn = p.ConnectionId;
                }
            }

            foreach (var p in players)
            {
                if (assignedPiltoverConn == null && p.ConnectionId != assignedZaunConn)
                {
                    assignedPiltoverConn = p.ConnectionId;
                }
                else if (assignedZaunConn == null && p.ConnectionId != assignedPiltoverConn)
                {
                    assignedZaunConn = p.ConnectionId;
                }
            }

            foreach (var p in players)
            {
                var finalRole = p.ConnectionId == assignedPiltoverConn ? "piltover" : "zaun";
                var effectiveAvatar = string.IsNullOrWhiteSpace(p.Info.Avatar) ? avatar : p.Info.Avatar;
                var effectiveName = string.IsNullOrWhiteSpace(p.Info.Name) ? playerName : p.Info.Name;
                var parameters = $"roomId={Uri.EscapeDataString(storyLobbyName)}&role={finalRole}&avatar={effectiveAvatar}&name={Uri.EscapeDataString(effectiveName)}&squad={Uri.EscapeDataString(originalRoomId)}&sceneIndex={sceneIndex}";
                await Clients.Client(p.ConnectionId).SendAsync("RedirectToAct1", $"/act1-multiplayer?{parameters}");
            }
            return;
        }

        var fallback = $"roomId={Uri.EscapeDataString(storyLobbyName)}&role={role}&avatar={avatar}&name={Uri.EscapeDataString(playerName)}&squad={Uri.EscapeDataString(originalRoomId)}&sceneIndex={sceneIndex}";
        await Clients.Group(originalRoomId).SendAsync("RedirectToAct1", $"/act1-multiplayer?{fallback}");
    }
    
    public async Task RedirectPlayersToPuzzle(string originalRoomId, string puzzleRoomName, string role, string avatar, string playerName, string puzzleName)
    {
        // Redirect both players to a puzzle with story mode enabled
        if (_lobbyPlayers.TryGetValue(originalRoomId, out var lobbyDict) && !lobbyDict.IsEmpty)
        {
            var players = lobbyDict
                .Select(kvp => (ConnectionId: kvp.Key, Info: kvp.Value))
                .OrderBy(p => p.Info.JoinedAt)
                .ToList();

            string? assignedPiltoverConn = null;
            string? assignedZaunConn = null;

            foreach (var p in players)
            {
                var desired = (p.Info.Role ?? string.Empty).Trim().ToLowerInvariant();
                if (desired == "piltover" && assignedPiltoverConn == null)
                {
                    assignedPiltoverConn = p.ConnectionId;
                }
                else if ((desired == "zaun" || desired == "zaunite") && assignedZaunConn == null)
                {
                    assignedZaunConn = p.ConnectionId;
                }
            }

            foreach (var p in players)
            {
                if (assignedPiltoverConn == null && p.ConnectionId != assignedZaunConn)
                {
                    assignedPiltoverConn = p.ConnectionId;
                }
                else if (assignedZaunConn == null && p.ConnectionId != assignedPiltoverConn)
                {
                    assignedZaunConn = p.ConnectionId;
                }
            }

            foreach (var p in players)
            {
                var finalRole = p.ConnectionId == assignedPiltoverConn ? "piltover" : "zaun";
                var effectiveAvatar = string.IsNullOrWhiteSpace(p.Info.Avatar) ? avatar : p.Info.Avatar;
                var effectiveName = string.IsNullOrWhiteSpace(p.Info.Name) ? playerName : p.Info.Name;
                var parameters = $"role={finalRole}&avatar={effectiveAvatar}&name={Uri.EscapeDataString(effectiveName)}&squad={Uri.EscapeDataString(puzzleRoomName)}&story=true";
                await Clients.Client(p.ConnectionId).SendAsync("RedirectToAct1", $"/{puzzleName}?{parameters}");
            }
            return;
        }

        var fallback = $"role={role}&avatar={avatar}&name={Uri.EscapeDataString(playerName)}&squad={Uri.EscapeDataString(puzzleRoomName)}&story=true";
        await Clients.Group(originalRoomId).SendAsync("RedirectToAct1", $"/{puzzleName}?{fallback}");
    }

    // Update or set per-connection lobby metadata for personalized redirects
    public Task UpdateLobbyPlayerInfo(string roomId, string role, string avatar, string playerName)
    {
        var lobbyDict = _lobbyPlayers.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, LobbyPlayerInfo>());
        lobbyDict.AddOrUpdate(Context.ConnectionId,
            _ => new LobbyPlayerInfo { Name = playerName, Role = role, Avatar = avatar },
            (_, existing) =>
            {
                existing.Name = playerName;
                existing.Role = role;
                existing.Avatar = avatar;
                return existing;
            });
        _ = BroadcastLobbyRoles(roomId);
        return Task.CompletedTask;
    }

    public Task RequestLobbyRoles(string roomId)
    {
        return BroadcastLobbyRoles(roomId, Context.ConnectionId);
    }

    private Task BroadcastLobbyRoles(string roomId, string? onlyToConnectionId = null)
    {
        if (!_lobbyPlayers.TryGetValue(roomId, out var lobbyDict))
        {
            return Task.CompletedTask;
        }

        string? piltoverBy = null;
        string? zaunBy = null;
        foreach (var kvp in lobbyDict)
        {
            var role = (kvp.Value.Role ?? string.Empty).Trim().ToLowerInvariant();
            if (role == "piltover" && string.IsNullOrEmpty(piltoverBy)) piltoverBy = kvp.Value.Name;
            if ((role == "zaun" || role == "zaunite") && string.IsNullOrEmpty(zaunBy)) zaunBy = kvp.Value.Name;
        }

        if (onlyToConnectionId != null)
        {
            return Clients.Client(onlyToConnectionId).SendAsync("LobbyRolesUpdated", piltoverBy, zaunBy);
        }
        return Clients.Group(roomId).SendAsync("LobbyRolesUpdated", piltoverBy, zaunBy);
    }

    public async Task SendPlayerAction(string roomId, string action, object data)
    {
        await Clients.OthersInGroup(roomId).SendAsync("PlayerActionReceived", Context.ConnectionId, action, data);
    }

    // Tic-Tac-Toe specific methods
    public async Task JoinGame(string roomId, string playerName)
    {
        var game = _games.GetOrAdd(roomId, _ => new TicTacToeGame());
        
        var playerSymbol = game.AddPlayer(Context.ConnectionId, playerName);
        if (playerSymbol != null)
        {
            await Clients.Caller.SendAsync("GameJoined", playerSymbol, game);
            await Clients.Group(roomId).SendAsync("GameStateUpdated", game);
        }
        else
        {
            await Clients.Caller.SendAsync("GameFull");
        }
    }

    public async Task MakeMove(string roomId, int position)
    {
        if (_games.TryGetValue(roomId, out var game))
        {
            var result = game.MakeMove(Context.ConnectionId, position);
            if (result.Success)
            {
                await Clients.Group(roomId).SendAsync("GameStateUpdated", game);
                
                if (game.GameOver)
                {
                    await Clients.Group(roomId).SendAsync("GameEnded", game.Winner, game.WinningLine);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("InvalidMove", result.Message);
            }
        }
    }

    public async Task RestartGame(string roomId)
    {
        if (_games.TryGetValue(roomId, out var game))
        {
            game.Reset();
            await Clients.Group(roomId).SendAsync("GameStateUpdated", game);
        }
    }

    // Code Cracker specific methods
    public async Task JoinCodeCrackerGame(string roomId, string playerName)
    {
        // Ensure player is in the SignalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var game = _codeCrackerGames.GetOrAdd(roomId, _ => new CodeCrackerGame());
        
        var playerRole = game.AddPlayer(Context.ConnectionId, playerName);
        if (playerRole != null)
        {
            await Clients.Caller.SendAsync("CodeCrackerGameJoined", playerRole.ToString(), game.GetPlayerView(Context.ConnectionId));
            await Clients.Group(roomId).SendAsync("CodeCrackerGameStateUpdated", game.GetGameState());
        }
        else
        {
            await Clients.Caller.SendAsync("CodeCrackerGameFull");
        }
    }
    
    public async Task JoinCodeCrackerGameWithRole(string roomId, string playerName, string requestedRole)
    {
        // Ensure player is in the SignalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var game = _codeCrackerGames.GetOrAdd(roomId, _ => new CodeCrackerGame());
        
        // Try to add player with requested role
        var playerRole = game.AddPlayerWithRole(Context.ConnectionId, playerName, requestedRole);
        if (playerRole != null)
        {
            await Clients.Caller.SendAsync("CodeCrackerGameJoined", playerRole.ToString(), game.GetPlayerView(Context.ConnectionId));
            await Clients.Group(roomId).SendAsync("CodeCrackerGameStateUpdated", game.GetGameState());
        }
        else
        {
            await Clients.Caller.SendAsync("CodeCrackerGameFull");
        }
    }

    public async Task SubmitCodeCrackerGuess(string roomId, string guess)
    {
        if (_codeCrackerGames.TryGetValue(roomId, out var game))
        {
            var result = game.SubmitGuess(Context.ConnectionId, guess);
            if (result.Success)
            {
                // Send success animation to all players
                await Clients.Group(roomId).SendAsync("CodeCrackerCorrectGuess", result.Message);
                
                await Clients.Group(roomId).SendAsync("CodeCrackerGameStateUpdated", game.GetGameState());
                
                // Update player views with new word data after correct guess (with delay for animation)
                await Task.Delay(2500); // Wait for success animation
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("CodeCrackerPlayerViewUpdated", game.GetPlayerView(player));
                }
                
                if (game.IsCompleted)
                {
                    await Clients.Group(roomId).SendAsync("CodeCrackerGameCompleted", result.Message, game.Score, game.CurrentWordIndex);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("CodeCrackerInvalidGuess", result.Message);
            }
        }
    }

    public async Task RequestCodeCrackerHint(string roomId)
    {
        if (_codeCrackerGames.TryGetValue(roomId, out var game))
        {
            var hint = game.GetHint(Context.ConnectionId);
            if (hint != null)
            {
                await Clients.Caller.SendAsync("CodeCrackerHintReceived", hint);
                await Clients.Group(roomId).SendAsync("CodeCrackerGameStateUpdated", game.GetGameState());
            }
        }
    }

    public async Task RestartCodeCrackerGame(string roomId)
    {
        if (_codeCrackerGames.TryGetValue(roomId, out var game))
        {
            game.Reset();
            await Clients.Group(roomId).SendAsync("CodeCrackerGameStateUpdated", game.GetGameState());
            
            // Send updated player views
            foreach (var player in game.GetConnectedPlayers())
            {
                await Clients.Client(player).SendAsync("CodeCrackerPlayerViewUpdated", game.GetPlayerView(player));
            }
        }
    }

    // Signal Decoder specific methods - SIMPLIFIED
    public async Task JoinSignalDecoderGame(string roomId, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var game = _signalDecoderGames.GetOrAdd(roomId, _ => new SimpleSignalDecoderGame());
        
        var playerRole = game.AddPlayer(Context.ConnectionId, playerName);
        if (playerRole != null)
        {
            await Clients.Caller.SendAsync("SignalDecoderGameJoined", playerRole.ToString(), game.GetPlayerView(Context.ConnectionId));
            await Clients.Group(roomId).SendAsync("SignalDecoderGameStateUpdated", game.GetGameState());
            
            // Start game if both players are connected
            if (game.PlayerCount == 2)
            {
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("SignalDecoderPlayerViewUpdated", game.GetPlayerView(player));
                }
            }
        }
        else
        {
            await Clients.Caller.SendAsync("SignalDecoderGameFull");
        }
    }
    
    public async Task JoinSignalDecoderGameWithRole(string roomId, string playerName, string requestedRole)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var game = _signalDecoderGames.GetOrAdd(roomId, _ => new SimpleSignalDecoderGame());
        
        var playerRole = game.AddPlayerWithRole(Context.ConnectionId, playerName, requestedRole);
        if (playerRole != null)
        {
            await Clients.Caller.SendAsync("SignalDecoderGameJoined", playerRole.ToString(), game.GetPlayerView(Context.ConnectionId));
            await Clients.Group(roomId).SendAsync("SignalDecoderGameStateUpdated", game.GetGameState());
            
            // Start game if both players are connected
            if (game.PlayerCount == 2)
            {
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("SignalDecoderPlayerViewUpdated", game.GetPlayerView(player));
                }
            }
        }
        else
        {
            await Clients.Caller.SendAsync("SignalDecoderGameFull");
        }
    }

    public async Task SubmitSignalDecoderGuess(string roomId, string guess)
    {
        if (_signalDecoderGames.TryGetValue(roomId, out var game))
        {
            var result = game.SubmitGuess(Context.ConnectionId, guess);
            if (result.Success)
            {
                await Clients.Group(roomId).SendAsync("SignalDecoderCorrectGuess", result.Message);
                await Clients.Group(roomId).SendAsync("SignalDecoderGameStateUpdated", game.GetGameState());
                
                // Update player views
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("SignalDecoderPlayerViewUpdated", game.GetPlayerView(player));
                }
                
                if (game.IsCompleted)
                {
                    await Clients.Group(roomId).SendAsync("SignalDecoderGameCompleted", "All words decoded!", game.Score, 1);
                }
                else if (result.Message.Contains("New signal incoming"))
                {
                    // Give time for celebration, then send new signal data
                    await Task.Delay(2000);
                    await Clients.Group(roomId).SendAsync("SignalDecoderGameStateUpdated", game.GetGameState());
                    
                    // Send updated player views with new signal
                    foreach (var player in game.GetConnectedPlayers())
                    {
                        await Clients.Client(player).SendAsync("SignalDecoderPlayerViewUpdated", game.GetPlayerView(player));
                    }
                }
            }
            else
            {
                await Clients.Caller.SendAsync("SignalDecoderInvalidGuess", result.Message);
            }
        }
    }

    public async Task RequestSignalDecoderHint(string roomId)
    {
        if (_signalDecoderGames.TryGetValue(roomId, out var game))
        {
            var hint = game.GetHint(Context.ConnectionId);
            if (hint != null)
            {
                await Clients.Caller.SendAsync("SignalDecoderHintReceived", hint);
                await Clients.Group(roomId).SendAsync("SignalDecoderGameStateUpdated", game.GetGameState());
            }
        }
    }

    public async Task RestartSignalDecoderGame(string roomId)
    {
        if (_signalDecoderGames.TryGetValue(roomId, out var game))
        {
            game.Reset();
            await Clients.Group(roomId).SendAsync("SignalDecoderGameStateUpdated", game.GetGameState());
            
            // Send updated player views
            foreach (var player in game.GetConnectedPlayers())
            {
                await Clients.Client(player).SendAsync("SignalDecoderPlayerViewUpdated", game.GetPlayerView(player));
            }
        }
    }

    // Navigation Maze specific methods
    public async Task JoinNavigationMazeGame(string roomId, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var game = _navigationMazeGames.GetOrAdd(roomId, _ => new NavigationMazeGame());
        
        var playerRole = game.AddPlayer(Context.ConnectionId, playerName);
        if (playerRole != null)
        {
            await Clients.Caller.SendAsync("NavigationMazeGameJoined", playerRole.ToString(), game.GetPlayerView(Context.ConnectionId));
            await Clients.Group(roomId).SendAsync("NavigationMazeGameStateUpdated", game.GetGameState());
            
            // Start game if both players are connected
            if (game.PlayerCount == 2)
            {
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("NavigationMazePlayerViewUpdated", game.GetPlayerView(player));
                }
            }
        }
        else
        {
            await Clients.Caller.SendAsync("NavigationMazeGameFull");
        }
    }

    public async Task MakeNavigationChoice(string roomId, string choice)
    {
        if (_navigationMazeGames.TryGetValue(roomId, out var game))
        {
            var result = game.MakeChoice(Context.ConnectionId, choice);
            if (result.Success)
            {
                if (game.IsGameOver)
                {
                    // Game over - wrong choice
                    await Clients.Group(roomId).SendAsync("NavigationMazeGameOver", result.Message);
                    await Clients.Group(roomId).SendAsync("NavigationMazeGameStateUpdated", game.GetGameState());
                    
                    // Update player views with game over state
                    foreach (var player in game.GetConnectedPlayers())
                    {
                        await Clients.Client(player).SendAsync("NavigationMazePlayerViewUpdated", game.GetPlayerView(player));
                    }
                }
                else if (game.IsCompleted)
                {
                    // Victory!
                    await Clients.Group(roomId).SendAsync("NavigationMazeGameCompleted", result.Message);
                    await Clients.Group(roomId).SendAsync("NavigationMazeGameStateUpdated", game.GetGameState());
                    
                    // Update player views with victory state
                    foreach (var player in game.GetConnectedPlayers())
                    {
                        await Clients.Client(player).SendAsync("NavigationMazePlayerViewUpdated", game.GetPlayerView(player));
                    }
                }
                else
                {
                    // Correct choice - advance to next location
                    await Clients.Group(roomId).SendAsync("NavigationMazeCorrectChoice", result.Message);
                    await Clients.Group(roomId).SendAsync("NavigationMazeGameStateUpdated", game.GetGameState());
                    
                    // Small delay for success feedback, then update views
                    await Task.Delay(1500);
                    foreach (var player in game.GetConnectedPlayers())
                    {
                        await Clients.Client(player).SendAsync("NavigationMazePlayerViewUpdated", game.GetPlayerView(player));
                    }
                }
            }
            else
            {
                await Clients.Caller.SendAsync("NavigationMazeInvalidChoice", result.Message);
            }
        }
    }

    public async Task RestartNavigationMazeGame(string roomId)
    {
        if (_navigationMazeGames.TryGetValue(roomId, out var game))
        {
            game.Reset();
            await Clients.Group(roomId).SendAsync("NavigationMazeGameStateUpdated", game.GetGameState());
            
            // Send updated player views
            foreach (var player in game.GetConnectedPlayers())
            {
                await Clients.Client(player).SendAsync("NavigationMazePlayerViewUpdated", game.GetPlayerView(player));
            }
        }
    }

    // Alchemy Lab specific methods
    public async Task JoinAlchemyGame(string roomId, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var game = _alchemyGames.GetOrAdd(roomId, _ => new AlchemyGame());
        
        var playerRole = game.AddPlayer(Context.ConnectionId, playerName);
        if (playerRole != null)
        {
            await Clients.Caller.SendAsync("AlchemyGameJoined", playerRole.ToString(), game.GetPlayerView(Context.ConnectionId));
            await Clients.Group(roomId).SendAsync("AlchemyGameStateUpdated", game.GetGameState());
            
            // Start game if both players are connected
            if (game.PlayerCount == 2)
            {
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("AlchemyPlayerViewUpdated", game.GetPlayerView(player));
                }
            }
        }
        else
        {
            await Clients.Caller.SendAsync("AlchemyGameFull");
        }
    }
    
    public async Task JoinAlchemyGameWithRole(string roomId, string playerName, string requestedRole)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var game = _alchemyGames.GetOrAdd(roomId, _ => new AlchemyGame());
        
        // Try to add player with requested role
        var playerRole = game.AddPlayerWithRole(Context.ConnectionId, playerName, requestedRole);
        if (playerRole != null)
        {
            await Clients.Caller.SendAsync("AlchemyGameJoined", playerRole.ToString(), game.GetPlayerView(Context.ConnectionId));
            await Clients.Group(roomId).SendAsync("AlchemyGameStateUpdated", game.GetGameState());
            
            // Start game if both players are connected
            if (game.PlayerCount == 2)
            {
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("AlchemyPlayerViewUpdated", game.GetPlayerView(player));
                }
            }
        }
        else
        {
            await Clients.Caller.SendAsync("AlchemyGameFull");
        }
    }

    public async Task ProcessIngredient(string roomId, string ingredientId, string stationName)
    {
        if (_alchemyGames.TryGetValue(roomId, out var game))
        {
            var result = game.ProcessIngredient(Context.ConnectionId, ingredientId, stationName);
            if (result.Success)
            {
                // Update player views with new ingredient state
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("AlchemyPlayerViewUpdated", game.GetPlayerView(player));
                }
            }
            else
            {
                await Clients.Caller.SendAsync("AlchemyInvalidAction", result.Message);
            }
        }
    }
    
    public async Task CombineIngredients(string roomId, string ingredient1Id, string ingredient2Id)
    {
        if (_alchemyGames.TryGetValue(roomId, out var game))
        {
            var result = game.CombineIngredients(Context.ConnectionId, ingredient1Id, ingredient2Id);
            if (result.Success)
            {
                // Update player views with new ingredient state
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("AlchemyPlayerViewUpdated", game.GetPlayerView(player));
                }
                
                // Send success message
                await Clients.Caller.SendAsync("AlchemyInvalidAction", result.Message);
            }
            else
            {
                await Clients.Caller.SendAsync("AlchemyInvalidAction", result.Message);
            }
        }
    }

    public async Task AddToCauldron(string roomId, string ingredientId, int position)
    {
        if (_alchemyGames.TryGetValue(roomId, out var game))
        {
            var result = game.AddToCauldron(Context.ConnectionId, ingredientId, position);
            if (result.Success)
            {
                // Update player views with new cauldron state
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("AlchemyPlayerViewUpdated", game.GetPlayerView(player));
                }
            }
            else
            {
                await Clients.Caller.SendAsync("AlchemyInvalidAction", result.Message);
            }
        }
    }

    public async Task SubmitPotion(string roomId)
    {
        if (_alchemyGames.TryGetValue(roomId, out var game))
        {
            var result = game.SubmitPotion(Context.ConnectionId);
            if (result.Success)
            {
                if (game.IsCompleted)
                {
                    // Success!
                    await Clients.Group(roomId).SendAsync("AlchemyGameCompleted", result.Message, game.Score);
                    await Clients.Group(roomId).SendAsync("AlchemyGameStateUpdated", game.GetGameState());
                    
                    foreach (var player in game.GetConnectedPlayers())
                    {
                        await Clients.Client(player).SendAsync("AlchemyPlayerViewUpdated", game.GetPlayerView(player));
                    }
                }
                else
                {
                    // Failed attempt - give feedback on mistakes
                    await Clients.Group(roomId).SendAsync("AlchemyPotionIncorrect", result.Message, game.GetMistakes());
                    game.ResetCauldron(); // Clear cauldron for retry
                    
                    foreach (var player in game.GetConnectedPlayers())
                    {
                        await Clients.Client(player).SendAsync("AlchemyPlayerViewUpdated", game.GetPlayerView(player));
                    }
                }
            }
            else
            {
                await Clients.Caller.SendAsync("AlchemyInvalidAction", result.Message);
            }
        }
    }

    public async Task RestartAlchemyGame(string roomId)
    {
        if (_alchemyGames.TryGetValue(roomId, out var game))
        {
            game.Reset();
            await Clients.Group(roomId).SendAsync("AlchemyGameStateUpdated", game.GetGameState());
            
            // Send updated player views
            foreach (var player in game.GetConnectedPlayers())
            {
                await Clients.Client(player).SendAsync("AlchemyPlayerViewUpdated", game.GetPlayerView(player));
            }
        }
    }

    // Rune Protocol specific methods
    public async Task JoinRuneProtocolGame(string roomId, string playerName)
    {
        Console.WriteLine($"[DEBUG] JoinRuneProtocolGame - RoomId: {roomId}, PlayerName: {playerName}, ConnectionId: {Context.ConnectionId}");
        
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var game = _runeProtocolGames.GetOrAdd(roomId, _ => new RuneProtocolGame());
        Console.WriteLine($"[DEBUG] Game retrieved/created for room {roomId}. Current players: {game.PlayerCount}");
        
        var playerRole = game.AddPlayer(Context.ConnectionId, playerName);
        Console.WriteLine($"[DEBUG] AddPlayer result - Role: {playerRole}, Total players now: {game.PlayerCount}");
        
        if (playerRole != null)
        {
            var playerView = game.GetPlayerView(Context.ConnectionId);
            Console.WriteLine($"[DEBUG] PlayerView created - Role: {playerView.Role}, Controllable runes: [{string.Join(", ", playerView.ControllableRunes)}]");
            
            await Clients.Caller.SendAsync("RuneProtocolGameJoined", playerRole.ToString(), playerView);
            await Clients.Group(roomId).SendAsync("RuneProtocolGameStateUpdated", game.GetGameState());
            
            // Start game if both players are connected
            if (game.PlayerCount == 2)
            {
                Console.WriteLine("[DEBUG] Both players connected, sending updated views");
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("RuneProtocolPlayerViewUpdated", game.GetPlayerView(player));
                }
            }
        }
        else
        {
            Console.WriteLine("[DEBUG] Game is full, sending GameFull message");
            await Clients.Caller.SendAsync("RuneProtocolGameFull");
        }
    }

    public async Task ToggleRune(string roomId, int runeIndex)
    {
        Console.WriteLine($"[DEBUG] ToggleRune called - RoomId: {roomId}, RuneIndex: {runeIndex}, ConnectionId: {Context.ConnectionId}");
        
        if (_runeProtocolGames.TryGetValue(roomId, out var game))
        {
            Console.WriteLine($"[DEBUG] Game found. Players in game: {game.Players.Count}");
            Console.WriteLine($"[DEBUG] Player roles: {string.Join(", ", game.Players.Select(p => $"{p.Key}={p.Value}"))}");
            
            var result = game.ToggleRune(Context.ConnectionId, runeIndex);
            Console.WriteLine($"[DEBUG] ToggleRune result - Success: {result.Success}, Message: {result.Message}");
            
            if (result.Success)
            {
                Console.WriteLine($"[DEBUG] Rune toggled successfully. New states: [{string.Join(", ", game.RuneStates.Select((state, i) => $"R{i+1}={state}"))}]");
                
                // Update all players with new rune state
                await Clients.Group(roomId).SendAsync("RuneProtocolGameStateUpdated", game.GetGameState());
                
                // Update individual player views
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("RuneProtocolPlayerViewUpdated", game.GetPlayerView(player));
                }
                
                if (game.IsCompleted)
                {
                    Console.WriteLine("[DEBUG] Level completed!");
                    await Clients.Group(roomId).SendAsync("RuneProtocolGameCompleted", result.Message, game.Score, game.CurrentLevel);
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG] Sending invalid action message: {result.Message}");
                await Clients.Caller.SendAsync("RuneProtocolInvalidAction", result.Message);
            }
        }
        else
        {
            Console.WriteLine($"[DEBUG] Game not found for roomId: {roomId}");
            await Clients.Caller.SendAsync("RuneProtocolInvalidAction", "Game not found");
        }
    }


    public async Task AdvanceRuneProtocolLevel(string roomId)
    {
        if (_runeProtocolGames.TryGetValue(roomId, out var game))
        {
            var result = game.AdvanceLevel(Context.ConnectionId);
            if (result.Success)
            {
                await Clients.Group(roomId).SendAsync("RuneProtocolLevelAdvanced", result.Message);
                await Clients.Group(roomId).SendAsync("RuneProtocolGameStateUpdated", game.GetGameState());
                
                // Update player views with new level data
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("RuneProtocolPlayerViewUpdated", game.GetPlayerView(player));
                }
            }
            else
            {
                await Clients.Caller.SendAsync("RuneProtocolInvalidAction", result.Message);
            }
        }
    }

    public async Task ToggleRuneProtocolValidationHints(string roomId)
    {
        if (_runeProtocolGames.TryGetValue(roomId, out var game))
        {
            var result = game.ToggleValidationHints(Context.ConnectionId);
            if (result.Success)
            {
                // Update all players with new hint state
                await Clients.Group(roomId).SendAsync("RuneProtocolGameStateUpdated", game.GetGameState());
                
                foreach (var player in game.GetConnectedPlayers())
                {
                    await Clients.Client(player).SendAsync("RuneProtocolPlayerViewUpdated", game.GetPlayerView(player));
                }
                
                // Send status message to all players
                await Clients.Group(roomId).SendAsync("RuneProtocolValidationToggled", result.Message);
            }
            else
            {
                await Clients.Caller.SendAsync("RuneProtocolInvalidAction", result.Message);
            }
        }
    }

    public async Task RestartRuneProtocolGame(string roomId)
    {
        if (_runeProtocolGames.TryGetValue(roomId, out var game))
        {
            game.Reset();
            await Clients.Group(roomId).SendAsync("RuneProtocolGameStateUpdated", game.GetGameState());
            
            // Send updated player views
            foreach (var player in game.GetConnectedPlayers())
            {
                await Clients.Client(player).SendAsync("RuneProtocolPlayerViewUpdated", game.GetPlayerView(player));
            }
        }
    }

    // Picture Explanation Game methods
    public async Task JoinPictureExplanationGame(string roomId, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var game = _pictureExplanationGames.GetOrAdd(roomId, _ => new PictureExplanationGame());
        
        try
        {
            var role = game.AddPlayer(Context.ConnectionId, playerName);
            if (!string.IsNullOrEmpty(role))
            {
                var playerView = game.GetPlayerView(Context.ConnectionId);
                var gameState = game.GetGameState();
                
                await Clients.Caller.SendAsync("PictureExplanationGameJoined", role, playerView);
                await Clients.Group(roomId).SendAsync("PictureExplanationGameStateUpdated", gameState);
                
                // Update all player views
                foreach (var playerId in game.GetConnectedPlayers())
                {
                    await Clients.Client(playerId).SendAsync("PictureExplanationPlayerViewUpdated", game.GetPlayerView(playerId));
                }
            }
            else
            {
                await Clients.Caller.SendAsync("PictureExplanationGameFull");
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("PictureExplanationInvalidAction", ex.Message);
        }
    }

    public async Task JoinPictureExplanationGameWithRole(string roomId, string playerName, string requestedRole)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        var game = _pictureExplanationGames.GetOrAdd(roomId, _ => new PictureExplanationGame());
        try
        {
            var role = game.AddPlayer(Context.ConnectionId, playerName, requestedRole);
            if (!string.IsNullOrEmpty(role))
            {
                var playerView = game.GetPlayerView(Context.ConnectionId);
                var gameState = game.GetGameState();
                await Clients.Caller.SendAsync("PictureExplanationGameJoined", role, playerView);
                await Clients.Group(roomId).SendAsync("PictureExplanationGameStateUpdated", gameState);
                foreach (var playerId in game.GetConnectedPlayers())
                {
                    await Clients.Client(playerId).SendAsync("PictureExplanationPlayerViewUpdated", game.GetPlayerView(playerId));
                }
            }
            else
            {
                await Clients.Caller.SendAsync("PictureExplanationGameFull");
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("PictureExplanationInvalidAction", ex.Message);
        }
    }

    public async Task FinishDescribing(string roomId)
    {
        if (_pictureExplanationGames.TryGetValue(roomId, out var game))
        {
            try
            {
                var result = game.FinishDescribing(Context.ConnectionId);
                if (result.Success)
                {
                    var gameState = game.GetGameState();
                    await Clients.Group(roomId).SendAsync("PictureExplanationGameStateUpdated", gameState);
                    
                    // Update player views
                    foreach (var playerId in game.GetConnectedPlayers())
                    {
                        await Clients.Client(playerId).SendAsync("PictureExplanationPlayerViewUpdated", game.GetPlayerView(playerId));
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("PictureExplanationInvalidAction", result.Message);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("PictureExplanationInvalidAction", ex.Message);
            }
        }
    }

    public async Task SubmitPictureChoice(string roomId, int choiceIndex)
    {
        if (_pictureExplanationGames.TryGetValue(roomId, out var game))
        {
            try
            {
                var result = game.SubmitChoice(Context.ConnectionId, choiceIndex);
                if (result.Success)
                {
                    var gameState = game.GetGameState();
                    await Clients.Group(roomId).SendAsync("PictureExplanationGameStateUpdated", gameState);
                    
                    // If round is complete, show results
                    if (result.RoundComplete)
                    {
                        var roundResult = game.GetLastRoundResult();
                        await Clients.Group(roomId).SendAsync("PictureExplanationRoundCompleted", roundResult);
                    }
                    
                    // Update player views
                    foreach (var playerId in game.GetConnectedPlayers())
                    {
                        await Clients.Client(playerId).SendAsync("PictureExplanationPlayerViewUpdated", game.GetPlayerView(playerId));
                    }
                    
                    // Check if game is complete
                    if (game.IsCompleted())
                    {
                        await Clients.Group(roomId).SendAsync("PictureExplanationGameCompleted", 
                            $"Game completed! Final score: {game.GetScore()}/{game.TotalRounds * 10}", game.GetScore());
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("PictureExplanationInvalidAction", result.Message);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("PictureExplanationInvalidAction", ex.Message);
            }
        }
    }

    public async Task NextPictureRound(string roomId)
    {
        if (_pictureExplanationGames.TryGetValue(roomId, out var game))
        {
            try
            {
                var result = game.NextRound();
                if (result.Success)
                {
                    var gameState = game.GetGameState();
                    await Clients.Group(roomId).SendAsync("PictureExplanationGameStateUpdated", gameState);
                    
                    // Update player views
                    foreach (var playerId in game.GetConnectedPlayers())
                    {
                        await Clients.Client(playerId).SendAsync("PictureExplanationPlayerViewUpdated", game.GetPlayerView(playerId));
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("PictureExplanationInvalidAction", result.Message);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("PictureExplanationInvalidAction", ex.Message);
            }
        }
    }

    public async Task RestartPictureExplanationGame(string roomId)
    {
        if (_pictureExplanationGames.TryGetValue(roomId, out var game))
        {
            game.Reset();
            var gameState = game.GetGameState();
            await Clients.Group(roomId).SendAsync("PictureExplanationGameStateUpdated", gameState);
            
            // Update player views
            foreach (var playerId in game.GetConnectedPlayers())
            {
                await Clients.Client(playerId).SendAsync("PictureExplanationPlayerViewUpdated", game.GetPlayerView(playerId));
            }
        }
    }

    // Word-Forge (Affix Workshop) specific methods
    public async Task JoinWordForgeGame(string roomId, string playerName, string gameMode = "Assisted")
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var game = _wordForgeGames.GetOrAdd(roomId, _ => new WordForgeGame());
        var mode = Enum.Parse<GameMode>(gameMode, true);
        
        var playerRole = game.AddPlayer(Context.ConnectionId, playerName, mode);
        if (playerRole != null)
        {
            await Clients.Caller.SendAsync("WordForgeGameJoined", playerRole.ToString(), game.GetPlayerView(Context.ConnectionId));
            await Clients.Group(roomId).SendAsync("WordForgeGameStateUpdated", game.GetGameState());
        }
        else
        {
            await Clients.Caller.SendAsync("WordForgeGameFull");
        }
    }

    public async Task PlaceElementOnAnvil(string roomId, string elementId, string slotType)
    {
        if (_wordForgeGames.TryGetValue(roomId, out var game))
        {
            var result = game.PlaceElement(Context.ConnectionId, elementId, slotType);
            if (result.Success)
            {
                // Update player views
                foreach (var playerId in game.GetConnectedPlayers())
                {
                    await Clients.Client(playerId).SendAsync("WordForgePlayerViewUpdated", game.GetPlayerView(playerId));
                }
                
                await Clients.Group(roomId).SendAsync("WordForgeGameStateUpdated", game.GetGameState());
            }
            else
            {
                await Clients.Caller.SendAsync("WordForgeInvalidAction", result.Message);
            }
        }
    }

    public async Task RemoveElementFromAnvil(string roomId, string elementId, string slotType)
    {
        if (_wordForgeGames.TryGetValue(roomId, out var game))
        {
            var result = game.RemoveElement(Context.ConnectionId, elementId, slotType);
            if (result.Success)
            {
                // Update player views
                foreach (var playerId in game.GetConnectedPlayers())
                {
                    await Clients.Client(playerId).SendAsync("WordForgePlayerViewUpdated", game.GetPlayerView(playerId));
                }
                
                await Clients.Group(roomId).SendAsync("WordForgeGameStateUpdated", game.GetGameState());
            }
            else
            {
                await Clients.Caller.SendAsync("WordForgeInvalidAction", result.Message);
            }
        }
    }

    public async Task ForgeWordCombination(string roomId)
    {
        if (_wordForgeGames.TryGetValue(roomId, out var game))
        {
            var result = game.ForgeAttempt(Context.ConnectionId);
            if (result.IsSuccess)
            {
                // Update all players
                foreach (var playerId in game.GetConnectedPlayers())
                {
                    await Clients.Client(playerId).SendAsync("WordForgePlayerViewUpdated", game.GetPlayerView(playerId));
                }
                
                await Clients.Group(roomId).SendAsync("WordForgeGameStateUpdated", game.GetGameState());
                await Clients.Group(roomId).SendAsync("WordForgeCombinationSuccess", result.ResultMessage);
                
                // Check if game completed
                if (game.IsCompleted())
                {
                    await Clients.Group(roomId).SendAsync("WordForgeGameCompleted", "🎉 All words forged! The workshop is complete!");
                }
            }
            else
            {
                await Clients.Group(roomId).SendAsync("WordForgeCombinationFailed", result.ResultMessage);
            }
        }
    }

    public async Task RestartWordForgeGame(string roomId)
    {
        if (_wordForgeGames.TryGetValue(roomId, out var game))
        {
            game.Reset();
            
            // Update all players
            foreach (var playerId in game.GetConnectedPlayers())
            {
                await Clients.Client(playerId).SendAsync("WordForgePlayerViewUpdated", game.GetPlayerView(playerId));
            }
            
            await Clients.Group(roomId).SendAsync("WordForgeGameStateUpdated", game.GetGameState());
        }
    }

    // ==============================================
    // Visual Novel System - Multiplayer Methods
    // ==============================================

    public async Task JoinVisualNovelGame(string roomId, string playerName)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            var game = _visualNovelGames.GetOrAdd(roomId, _ => new VisualNovelMultiplayerGame { RoomId = roomId });
            
            // Check if room is full (max 2 players)
            if (game.Players.Count >= 2)
            {
                await Clients.Caller.SendAsync("VisualNovelGameFull");
                return;
            }
            
            // Assign player role: first player = Piltover, second = Zaunite
            var playerRole = game.Players.Count == 0 ? VisualNovelPlayerRole.Piltover : VisualNovelPlayerRole.Zaunite;
            var playerId = Context.ConnectionId;
            
            var player = new VisualNovelPlayer
            {
                PlayerId = playerId,
                PlayerName = playerName,
                PlayerRole = playerRole,
                IsConnected = true,
                JoinedAt = DateTime.UtcNow
            };
            
            game.Players.Add(player);
            
            // Initialize game scene if first player
            if (game.Players.Count == 1)
            {
                // Create default scene based on first player's role
                game.CurrentScene = CreateDefaultVisualNovelScene(playerRole == VisualNovelPlayerRole.Piltover ? NovelTheme.Piltover : NovelTheme.Zaun);
                game.GameState = new VisualNovelState 
                { 
                    CurrentSceneId = game.CurrentScene.Id,
                    CurrentDialogueIndex = 0,
                    IsTextFullyDisplayed = false
                };
            }
            
            // Start game when 2 players joined
            if (game.Players.Count == 2)
            {
                game.Status = VisualNovelGameStatus.InProgress;
            }
            
            // Send game state to all players
            await BroadcastVisualNovelGameState(roomId);
            
            await Clients.Caller.SendAsync("VisualNovelGameJoined", CreatePlayerView(game, playerId));
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("VisualNovelError", $"Failed to join game: {ex.Message}");
        }
    }

    public async Task SkipVisualNovelText(string roomId)
    {
        try
        {
            if (!_visualNovelGames.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("VisualNovelError", "Game not found");
                return;
            }
            
            var playerId = Context.ConnectionId;
            if (game.GetPlayer(playerId) == null)
            {
                await Clients.Caller.SendAsync("VisualNovelError", "Player not in game");
                return;
            }
            
            // Debouncing: check if enough time has passed since last action
            if (!game.CanPerformAction())
            {
                return; // Silently ignore if within debounce period
            }
            
            // Only allow skip if text is currently animating
            if (!game.IsTextAnimating)
            {
                return;
            }
            
            game.RecordAction();
            game.IsTextAnimating = false;
            game.GameState.IsTextFullyDisplayed = true;
            
            // Broadcast text skip to all players
            await Clients.Group(roomId).SendAsync("VisualNovelTextSkipped");
            await BroadcastVisualNovelGameState(roomId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("VisualNovelError", $"Failed to skip text: {ex.Message}");
        }
    }

    public async Task ContinueVisualNovel(string roomId)
    {
        try
        {
            if (!_visualNovelGames.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("VisualNovelError", "Game not found");
                return;
            }
            
            var playerId = Context.ConnectionId;
            if (game.GetPlayer(playerId) == null)
            {
                await Clients.Caller.SendAsync("VisualNovelError", "Player not in game");
                return;
            }
            
            // Debouncing: check if enough time has passed since last action
            if (!game.CanPerformAction())
            {
                return; // Silently ignore if within debounce period
            }
            
            // Only allow continue if text is fully displayed and not animating
            if (!game.GameState.IsTextFullyDisplayed || game.IsTextAnimating)
            {
                return;
            }
            
            // Check if this is the last dialogue in the scene
            var isLastDialogue = game.GameState.CurrentDialogueIndex >= game.CurrentScene.DialogueLines.Count - 1;
            if (isLastDialogue)
            {
                game.Status = VisualNovelGameStatus.Completed;
                await Clients.Group(roomId).SendAsync("VisualNovelGameCompleted");
                await BroadcastVisualNovelGameState(roomId);
                return;
            }
            
            game.RecordAction();
            game.GameState.CurrentDialogueIndex++;
            game.GameState.IsTextFullyDisplayed = false;
            game.IsTextAnimating = true;
            game.TextAnimationStartTime = DateTime.UtcNow;
            
            // Broadcast dialogue progression to all players
            await Clients.Group(roomId).SendAsync("VisualNovelDialogueContinued", game.GameState.CurrentDialogueIndex);
            await BroadcastVisualNovelGameState(roomId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("VisualNovelError", $"Failed to continue: {ex.Message}");
        }
    }

    public async Task RestartVisualNovel(string roomId)
    {
        try
        {
            if (!_visualNovelGames.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("VisualNovelError", "Game not found");
                return;
            }
            
            var playerId = Context.ConnectionId;
            if (game.GetPlayer(playerId) == null)
            {
                await Clients.Caller.SendAsync("VisualNovelError", "Player not in game");
                return;
            }
            
            // Reset game state
            game.Status = VisualNovelGameStatus.InProgress;
            game.GameState.CurrentDialogueIndex = 0;
            game.GameState.IsTextFullyDisplayed = false;
            game.IsTextAnimating = true;
            game.TextAnimationStartTime = DateTime.UtcNow;
            game.RecordAction();
            
            await Clients.Group(roomId).SendAsync("VisualNovelGameRestarted");
            await BroadcastVisualNovelGameState(roomId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("VisualNovelError", $"Failed to restart: {ex.Message}");
        }
    }

    private async Task BroadcastVisualNovelGameState(string roomId)
    {
        if (!_visualNovelGames.TryGetValue(roomId, out var game)) return;
        
        foreach (var player in game.Players)
        {
            var playerView = CreatePlayerView(game, player.PlayerId);
            await Clients.Client(player.PlayerId).SendAsync("VisualNovelPlayerViewUpdated", playerView);
        }
    }

    private VisualNovelPlayerView CreatePlayerView(VisualNovelMultiplayerGame game, string playerId)
    {
        var player = game.GetPlayer(playerId);
        if (player == null) return new VisualNovelPlayerView();
        
        var connectedPlayers = game.Players.Where(p => p.IsConnected).Select(p => p.PlayerName).ToList();
        
        return new VisualNovelPlayerView
        {
            RoomId = game.RoomId,
            PlayerId = playerId,
            PlayerRole = player.PlayerRole,
            GameStatus = game.Status,
            CurrentScene = game.CurrentScene,
            GameState = game.GameState,
            ConnectedPlayers = connectedPlayers,
            CanSkip = game.IsTextAnimating && !game.GameState.IsTextFullyDisplayed,
            CanContinue = game.GameState.IsTextFullyDisplayed && !game.IsTextAnimating && 
                         game.GameState.CurrentDialogueIndex < game.CurrentScene.DialogueLines.Count - 1,
            IsTextAnimating = game.IsTextAnimating,
            StatusMessage = game.Status switch
            {
                VisualNovelGameStatus.WaitingForPlayers => $"Waiting for players... ({game.Players.Count}/2)",
                VisualNovelGameStatus.InProgress => "Story in progress",
                VisualNovelGameStatus.Completed => "Story completed",
                _ => ""
            }
        };
    }

    private VisualNovelScene CreateDefaultVisualNovelScene(NovelTheme theme)
    {
        var scene = new VisualNovelScene
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{theme} Multiplayer Story",
            Layout = SceneLayout.DualCharacters,
            Theme = theme
        };

        if (theme == NovelTheme.Piltover)
        {
            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter
                {
                    Id = "jayce",
                    Name = "Jayce",
                    DisplayName = "Jayce Talis",
                    ImagePath = "/images/Jayce.jpeg",
                    Position = CharacterPosition.Left,
                    ThemeColor = "#c8aa6e"
                },
                new VisualNovelCharacter
                {
                    Id = "viktor",
                    Name = "Viktor",
                    DisplayName = "Viktor",
                    ImagePath = "/images/Viktor.jpeg",
                    Position = CharacterPosition.Right,
                    ThemeColor = "#0596aa"
                }
            });

            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine
                {
                    CharacterId = "jayce",
                    Text = "Welcome to our multiplayer investigation. Together, we'll uncover the truth behind this conspiracy.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40
                },
                new DialogueLine
                {
                    CharacterId = "viktor",
                    Text = "The synchronized nature of our discovery suggests collaboration will be essential. We must work as one.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 45
                },
                new DialogueLine
                {
                    CharacterId = "jayce",
                    Text = "Any action taken by one of us affects us both. Communication and coordination are key to our success.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40
                }
            });
        }
        else
        {
            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter
                {
                    Id = "vi",
                    Name = "Vi",
                    DisplayName = "Vi",
                    ImagePath = "/images/Vi.jpeg",
                    Position = CharacterPosition.Left,
                    ThemeColor = "#00d4aa"
                },
                new VisualNovelCharacter
                {
                    Id = "caitlyn",
                    Name = "Caitlyn",
                    DisplayName = "Sheriff Caitlyn",
                    ImagePath = "/images/Cait.jpeg",
                    Position = CharacterPosition.Right,
                    ThemeColor = "#ff007f"
                }
            });

            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Alright partner, this is a team operation. When one of us moves, we both move. Got it?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 35
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Understood. Our actions are synchronized - every choice we make affects both our perspectives.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 50
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Stay sharp and stay together. In Zaun, we watch each other's backs, and that's exactly what we're gonna do.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 35
                }
            });
        }

        return scene;
    }

    // ==============================================
    // Act 1 Story Campaign - Multiplayer Methods
    // ==============================================

    public async Task JoinAct1Game(string roomId, string playerName, string originalSquadName, string role, string avatar)
    {
        await JoinAct1GameAtScene(roomId, playerName, originalSquadName, role, avatar, null);
    }

    public async Task JoinAct1GameAtScene(string roomId, string playerName, string originalSquadName, string role, string avatar, int? startAtSceneIndex)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            var game = _act1Games.GetOrAdd(roomId, _ => new Act1MultiplayerGame { RoomId = roomId });
            
            // Check if room is full (max 2 players)
            if (game.Players.Count >= 2)
            {
                await Clients.Caller.SendAsync("Act1GameFull");
                return;
            }
            
            var playerId = Context.ConnectionId;
            
            var player = new Act1Player
            {
                PlayerId = playerId,
                PlayerName = playerName,
                OriginalSquadName = originalSquadName,
                SquadName = roomId, // Full room ID with modifiers
                PlayerRole = role,
                PlayerAvatar = avatar,
                IsConnected = true,
                JoinedAt = DateTime.UtcNow
            };
            
            game.Players.Add(player);
            
            // Initialize game scene - handle both first player and scene updates
            if (game.Players.Count == 1)
            {
                // Handle direct scene navigation (for continuing from puzzles)
                if (startAtSceneIndex.HasValue && startAtSceneIndex.Value >= 0)
                {
                    Console.WriteLine($"[GameHub] Setting scene index to {startAtSceneIndex.Value} for first player");
                    game.CurrentSceneIndex = startAtSceneIndex.Value;
                    var currentPhase = game.CurrentSceneIndex < game.StoryProgression.Count 
                        ? game.StoryProgression[game.CurrentSceneIndex] 
                        : "emergency_briefing";
                    Console.WriteLine($"[GameHub] Current phase at index {game.CurrentSceneIndex}: '{currentPhase}'");
                    
                    if (currentPhase == "database_revelation")
                    {
                        game.CurrentScene = _act1StoryEngine.CreateDatabaseRevelationScene(originalSquadName, game);
                    }
                    else
                    {
                        game.CurrentScene = _act1StoryEngine.CreateEmergencyBriefingScene(originalSquadName, game);
                    }
                }
                else
                {
                    game.CurrentScene = _act1StoryEngine.CreateEmergencyBriefingScene(originalSquadName, game);
                }
                
                game.GameState = new VisualNovelState 
                { 
                    CurrentSceneId = game.CurrentScene.Id,
                    CurrentDialogueIndex = 0,
                    IsTextFullyDisplayed = false
                };
            }
            // For second player, check if we need to update the scene based on startAtSceneIndex
            else if (startAtSceneIndex.HasValue && startAtSceneIndex.Value >= 0 && startAtSceneIndex.Value != game.CurrentSceneIndex)
            {
                Console.WriteLine($"[GameHub] Second player requesting scene index {startAtSceneIndex.Value}, updating from {game.CurrentSceneIndex}");
                game.CurrentSceneIndex = startAtSceneIndex.Value;
                var currentPhase = game.CurrentSceneIndex < game.StoryProgression.Count 
                    ? game.StoryProgression[game.CurrentSceneIndex] 
                    : "emergency_briefing";
                
                if (currentPhase == "database_revelation")
                {
                    game.CurrentScene = _act1StoryEngine.CreateDatabaseRevelationScene(originalSquadName, game);
                }
                else
                {
                    game.CurrentScene = _act1StoryEngine.CreateEmergencyBriefingScene(originalSquadName, game);
                }
                
                game.GameState = new VisualNovelState 
                { 
                    CurrentSceneId = game.CurrentScene.Id,
                    CurrentDialogueIndex = 0,
                    IsTextFullyDisplayed = false
                };
            }
            // Ensure scene is always set even for second player (fallback)
            else if (game.CurrentScene == null)
            {
                game.CurrentScene = _act1StoryEngine.CreateEmergencyBriefingScene(originalSquadName, game);
                game.GameState = new VisualNovelState 
                { 
                    CurrentSceneId = game.CurrentScene.Id,
                    CurrentDialogueIndex = 0,
                    IsTextFullyDisplayed = false
                };
            }
            
            // Start game when 2 players joined
            if (game.Players.Count == 2)
            {
                game.Status = Act1GameStatus.InProgress;
                game.IsTextAnimating = true;
                game.TextAnimationStartTime = DateTime.UtcNow;
                Console.WriteLine($"[GameHub] Act1 game started with 2 players at scene index {game.CurrentSceneIndex} ({(game.CurrentSceneIndex < game.StoryProgression.Count ? game.StoryProgression[game.CurrentSceneIndex] : "unknown")})");
            }
            
            // Send game state to all players
            await BroadcastAct1GameState(roomId);
            
            await Clients.Caller.SendAsync("Act1GameJoined", CreateAct1PlayerView(game, playerId));
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Act1Error", $"Failed to join game: {ex.Message}");
        }
    }

    public async Task SkipAct1Text(string roomId)
    {
        try
        {
            if (!_act1Games.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("Act1Error", "Game not found");
                return;
            }
            
            var playerId = Context.ConnectionId;
            if (game.GetPlayer(playerId) == null)
            {
                await Clients.Caller.SendAsync("Act1Error", "Player not in game");
                return;
            }
            
            // Debouncing: check if enough time has passed since last action
            if (!game.CanPerformAction())
            {
                return; // Silently ignore if within debounce period
            }
            
            // Only allow skip if text is currently animating
            if (!game.IsTextAnimating)
            {
                return;
            }
            
            game.RecordAction();
            game.IsTextAnimating = false;
            game.GameState.IsTextFullyDisplayed = true;
            
            // Broadcast text skip to all players
            await Clients.Group(roomId).SendAsync("Act1TextSkipped");
            await BroadcastAct1GameState(roomId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Act1Error", $"Failed to skip text: {ex.Message}");
        }
    }

    public async Task ContinueAct1(string roomId)
    {
        try
        {
            if (!_act1Games.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("Act1Error", "Game not found");
                return;
            }
            
            var playerId = Context.ConnectionId;
            if (game.GetPlayer(playerId) == null)
            {
                await Clients.Caller.SendAsync("Act1Error", "Player not in game");
                return;
            }
            
            // Debouncing: check if enough time has passed since last action
            if (!game.CanPerformAction())
            {
                return; // Silently ignore if within debounce period
            }
            
            // Only allow continue if text is fully displayed and not animating
            if (!game.GameState.IsTextFullyDisplayed || game.IsTextAnimating)
            {
                return;
            }
            
            // Check if this is the last dialogue in the main content (before branches)
            var mainContentEndIndex = game.CurrentScene.MainContentEndIndex ?? (game.CurrentScene.DialogueLines.Count - 1);
            var currentIndex = game.GameState.CurrentDialogueIndex;
            
            // Only check for scene end if we're in the main content range (0 to mainContentEndIndex)
            // Branch dialogues (after mainContentEndIndex) should never trigger scene end
            var isInMainContent = currentIndex <= mainContentEndIndex;
            var isLastDialogueInScene = isInMainContent && currentIndex == mainContentEndIndex;
            
            if (isLastDialogueInScene)
            {
                // Move to next scene or complete game
                await ProgressToNextAct1Scene(roomId, game);
                return;
            }
            
            game.RecordAction();
            var oldIndex = game.GameState.CurrentDialogueIndex;
            var oldDialogue = game.CurrentScene.DialogueLines[oldIndex];
            
            // Check if the current dialogue has a NextDialogueId for branching
            if (!string.IsNullOrEmpty(oldDialogue.NextDialogueId))
            {
                // Find the dialogue with the specified ID
                var nextIndex = game.CurrentScene.DialogueLines.FindIndex(d => d.Id == oldDialogue.NextDialogueId);
                if (nextIndex >= 0)
                {
                    var targetDialogue = game.CurrentScene.DialogueLines[nextIndex];
                    Console.WriteLine($"[Act1GameHub] BRANCH JUMP: Dialogue #{oldIndex} (ID: {oldDialogue.Id}) → jumping to dialogue ID '{oldDialogue.NextDialogueId}' (index: {nextIndex})");
                    Console.WriteLine($"[Act1GameHub] Target dialogue: Speaker={targetDialogue.CharacterId}, ID={targetDialogue.Id ?? "null"}, Text=\"{targetDialogue.Text.Substring(0, Math.Min(50, targetDialogue.Text.Length))}...\"");
                    game.GameState.CurrentDialogueIndex = nextIndex;
                }
                else
                {
                    Console.WriteLine($"[Act1GameHub] BRANCH ERROR: Dialogue #{oldIndex} (ID: {oldDialogue.Id}) references missing dialogue ID '{oldDialogue.NextDialogueId}', falling back to next sequential dialogue");
                    game.GameState.CurrentDialogueIndex++;
                }
            }
            else
            {
                // Normal linear progression
                var nextDialogue = game.GameState.CurrentDialogueIndex + 1 < game.CurrentScene.DialogueLines.Count ? game.CurrentScene.DialogueLines[game.GameState.CurrentDialogueIndex + 1] : null;
                Console.WriteLine($"[Act1GameHub] DIALOGUE CONTINUE: Moving from index {oldIndex} to {oldIndex + 1}");
                if (nextDialogue != null)
                {
                    Console.WriteLine($"[Act1GameHub] Next dialogue: Speaker={nextDialogue.CharacterId}, ID={nextDialogue.Id ?? "null"}, Text=\"{nextDialogue.Text.Substring(0, Math.Min(50, nextDialogue.Text.Length))}...\"");
                }
                game.GameState.CurrentDialogueIndex++;
            }
            
            // Log dialogue progression for debugging
            var currentDialogue = game.CurrentScene.DialogueLines[game.GameState.CurrentDialogueIndex];
            var speaker = currentDialogue.CharacterId ?? "Narrator";
            var choiceInfo = currentDialogue.IsPlayerChoice ? " [CHOICE POINT]" : "";
            var dialogueId = !string.IsNullOrEmpty(currentDialogue.Id) ? $" (ID: {currentDialogue.Id})" : "";
            Console.WriteLine($"[Act1GameHub] CONTINUE: Dialogue #{oldIndex} → #{game.GameState.CurrentDialogueIndex}: {speaker}{dialogueId}{choiceInfo} - \"{currentDialogue.Text}\"");
            
            game.GameState.IsTextFullyDisplayed = false;
            game.IsTextAnimating = true;
            game.TextAnimationStartTime = DateTime.UtcNow;
            
            // Broadcast dialogue progression to all players
            await Clients.Group(roomId).SendAsync("Act1DialogueContinued", game.GameState.CurrentDialogueIndex);
            await BroadcastAct1GameState(roomId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Act1Error", $"Failed to continue: {ex.Message}");
        }
    }

    // Called by clients when the current dialogue line has fully finished animating on their side
    public async Task Act1TypingCompleted(string roomId)
    {
        try
        {
            if (!_act1Games.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("Act1Error", "Game not found");
                return;
            }

            var playerId = Context.ConnectionId;
            if (game.GetPlayer(playerId) == null)
            {
                await Clients.Caller.SendAsync("Act1Error", "Player not in game");
                return;
            }

            // Mark text as fully displayed
            game.IsTextAnimating = false;
            game.GameState.IsTextFullyDisplayed = true;
            game.RecordAction();

            await BroadcastAct1GameState(roomId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Act1Error", $"Failed to complete typing: {ex.Message}");
        }
    }

    private async Task ProgressToNextAct1Scene(string roomId, Act1MultiplayerGame game)
    {
        var result = _act1StoryEngine.ProgressToNextScene(game);

        if (result.TransitionStarted)
        {
            await Clients.Group(roomId).SendAsync("Act1SceneTransition", result.NextGameName ?? "");
            await BroadcastAct1GameState(roomId);

            _ = Task.Delay(3000).ContinueWith(async _ =>
            {
                if (_act1Games.TryGetValue(roomId, out var currentGame) && currentGame.Status == Act1GameStatus.SceneTransition)
                {
                    if (result.RedirectUrlsByPlayerId != null)
                    {
                        foreach (var kvp in result.RedirectUrlsByPlayerId)
                        {
                            var playerId = kvp.Key;
                            var url = kvp.Value;
                            await Clients.Client(playerId).SendAsync("Act1RedirectToNextGame", url);
                        }
                    }
                }
            });
        }
        else if (result.StoryCompleted)
        {
            await Clients.Group(roomId).SendAsync("Act1GameCompleted");
            await BroadcastAct1GameState(roomId);
        }
    }

    public async Task RestartAct1(string roomId)
    {
        try
        {
            if (!_act1Games.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("Act1Error", "Game not found");
                return;
            }
            
            var playerId = Context.ConnectionId;
            if (game.GetPlayer(playerId) == null)
            {
                await Clients.Caller.SendAsync("Act1Error", "Player not in game");
                return;
            }
            
            // Reset game state
            var originalSquadName = game.Players.FirstOrDefault()?.OriginalSquadName ?? "";
            game.Status = Act1GameStatus.InProgress;
            game.CurrentSceneIndex = 0;
            game.CurrentScene = _act1StoryEngine.CreateEmergencyBriefingScene(originalSquadName, game);
            game.GameState.CurrentDialogueIndex = 0;
            game.GameState.IsTextFullyDisplayed = false;
            game.IsTextAnimating = true;
            game.TextAnimationStartTime = DateTime.UtcNow;
            game.ShowTransition = false;
            game.NextGameName = "";
            game.RecordAction();
            
            await Clients.Group(roomId).SendAsync("Act1GameRestarted");
            await BroadcastAct1GameState(roomId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Act1Error", $"Failed to restart: {ex.Message}");
        }
    }

    public async Task MakeAct1Choice(string roomId, string choiceId)
    {
        try
        {
            if (!_act1Games.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("Act1Error", "Game not found");
                return;
            }
            
            var playerId = Context.ConnectionId;
            var player = game.GetPlayer(playerId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Act1Error", "Player not in game");
                return;
            }
            
            // Validate that there's a pending choice
            if (game.GameState.CurrentDialogueIndex >= game.CurrentScene.DialogueLines.Count)
            {
                await Clients.Caller.SendAsync("Act1Error", "No dialogue available");
                return;
            }
            
            var currentDialogue = game.CurrentScene.DialogueLines[game.GameState.CurrentDialogueIndex];
            if (!currentDialogue.IsPlayerChoice || currentDialogue.Choices.Count == 0)
            {
                await Clients.Caller.SendAsync("Act1Error", "No choice available");
                return;
            }
            
            // Validate that this player can make the choice
            if (!string.IsNullOrEmpty(currentDialogue.ChoiceOwnerRole) && 
                !player.PlayerRole.Equals(currentDialogue.ChoiceOwnerRole, StringComparison.OrdinalIgnoreCase))
            {
                await Clients.Caller.SendAsync("Act1Error", "You cannot make this choice");
                return;
            }
            
            // Find the selected choice
            var selectedChoice = currentDialogue.Choices.FirstOrDefault(c => c.Id == choiceId);
            if (selectedChoice == null)
            {
                await Clients.Caller.SendAsync("Act1Error", "Invalid choice");
                return;
            }
            
            // Validate role requirement for the specific choice
            if (!string.IsNullOrEmpty(selectedChoice.RequiredRole) &&
                !player.PlayerRole.Equals(selectedChoice.RequiredRole, StringComparison.OrdinalIgnoreCase))
            {
                await Clients.Caller.SendAsync("Act1Error", "You don't meet the requirements for this choice");
                return;
            }
            
            // Process the choice
            currentDialogue.SelectedChoiceId = choiceId;
            
            Console.WriteLine($"[Act1GameHub] Processing choice at dialogue index {game.GameState.CurrentDialogueIndex}");
            Console.WriteLine($"[Act1GameHub] Current dialogue ID: {currentDialogue.Id ?? "null"}");
            Console.WriteLine($"[Act1GameHub] Selected choice ID: {choiceId}");
            Console.WriteLine($"[Act1GameHub] Choice NextDialogueId: {selectedChoice.NextDialogueId ?? "null"}");
            
            // Add to choice history
            game.ChoiceHistory.Add($"{player.PlayerName} ({player.PlayerRole}): {selectedChoice.Text}");
            
            // Apply any consequences
            if (selectedChoice.Consequences != null && selectedChoice.Consequences.Count > 0)
            {
                Console.WriteLine($"[Act1GameHub] CONSEQUENCES: Applying {selectedChoice.Consequences.Count} game state changes:");
                foreach (var consequence in selectedChoice.Consequences)
                {
                    game.GameState.GameState[consequence.Key] = consequence.Value;
                    Console.WriteLine($"  - {consequence.Key} = {consequence.Value}");
                }
            }
            
            // Apply expression changes if specified
            if (selectedChoice.ResultExpression.HasValue && !string.IsNullOrEmpty(currentDialogue.CharacterId))
            {
                var character = game.CurrentScene.Characters.FirstOrDefault(c => c.Id == currentDialogue.CharacterId);
                if (character != null)
                {
                    var oldExpression = character.CurrentExpression;
                    character.CurrentExpression = selectedChoice.ResultExpression.Value;
                    Console.WriteLine($"[Act1GameHub] EXPRESSION: {character.DisplayName} expression changed {oldExpression} → {selectedChoice.ResultExpression.Value}");
                }
            }
            
            // Move to the next dialogue (could be based on NextDialogueId for branching)
            if (!string.IsNullOrEmpty(selectedChoice.NextDialogueId))
            {
                // Find the dialogue with matching ID for branching
                var nextIndex = game.CurrentScene.DialogueLines.FindIndex(d => d.Id == selectedChoice.NextDialogueId);
                if (nextIndex >= 0)
                {
                    var targetDialogue = game.CurrentScene.DialogueLines[nextIndex];
                    Console.WriteLine($"[Act1GameHub] BRANCH TAKEN: Choice '{selectedChoice.Text}' (ID: {choiceId}) → branching to dialogue ID '{selectedChoice.NextDialogueId}' (index: {nextIndex})");
                    Console.WriteLine($"[Act1GameHub] Target dialogue: Speaker={targetDialogue.CharacterId}, Text=\"{targetDialogue.Text.Substring(0, Math.Min(50, targetDialogue.Text.Length))}...\"");
                    game.GameState.CurrentDialogueIndex = nextIndex;
                }
                else
                {
                    Console.WriteLine($"[Act1GameHub] BRANCH ERROR: Choice '{selectedChoice.Text}' references missing dialogue ID '{selectedChoice.NextDialogueId}', falling back to next sequential dialogue");
                    // Default to next dialogue
                    game.GameState.CurrentDialogueIndex++;
                }
            }
            else
            {
                Console.WriteLine($"[Act1GameHub] SEQUENTIAL FLOW: Choice '{selectedChoice.Text}' (ID: {choiceId}) → continuing to next dialogue in sequence");
                // Move to the next dialogue in sequence
                game.GameState.CurrentDialogueIndex++;
            }
            
            // Reset text animation state for the new dialogue
            game.GameState.IsTextFullyDisplayed = false;
            game.IsTextAnimating = true;
            game.TextAnimationStartTime = DateTime.UtcNow;
            game.RecordAction();
            
            // Broadcast the choice made to all players
            await Clients.Group(roomId).SendAsync("Act1ChoiceMade", new
            {
                PlayerId = playerId,
                PlayerName = player.PlayerName,
                PlayerRole = player.PlayerRole,
                ChoiceId = choiceId,
                ChoiceText = selectedChoice.Text
            });
            
            // After choice is made, start animating the new dialogue
            game.IsTextAnimating = true;
            game.GameState.IsTextFullyDisplayed = false;
            
            // Update game state for all players - they should now be able to continue
            await BroadcastAct1GameState(roomId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Act1Error", $"Failed to make choice: {ex.Message}");
        }
    }
    

    private async Task BroadcastAct1GameState(string roomId)
    {
        if (!_act1Games.TryGetValue(roomId, out var game)) return;
        
        foreach (var player in game.Players)
        {
            var playerView = CreateAct1PlayerView(game, player.PlayerId);
            await Clients.Client(player.PlayerId).SendAsync("Act1PlayerViewUpdated", playerView);
        }
    }

    private Act1PlayerView CreateAct1PlayerView(Act1MultiplayerGame game, string playerId)
    {
        return _act1StoryEngine.CreatePlayerView(game, playerId);
    }

    public async Task DebugSkipToDialogue(string roomId, int targetDialogueIndex)
    {
        try
        {
            if (!_act1Games.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("Act1Error", "Game not found");
                return;
            }
            
            var playerId = Context.ConnectionId;
            if (game.GetPlayer(playerId) == null)
            {
                await Clients.Caller.SendAsync("Act1Error", "Player not in game");
                return;
            }
            
            // Validate target index
            if (targetDialogueIndex < 0 || targetDialogueIndex >= game.CurrentScene.DialogueLines.Count)
            {
                await Clients.Caller.SendAsync("Act1Error", $"Invalid dialogue index: {targetDialogueIndex}");
                return;
            }
            
            // Jump to the target dialogue
            game.GameState.CurrentDialogueIndex = targetDialogueIndex;
            game.GameState.IsTextFullyDisplayed = false;
            game.IsTextAnimating = true;
            game.TextAnimationStartTime = DateTime.UtcNow;
            game.RecordAction();
            
            Console.WriteLine($"[Act1GameHub] DEBUG: Jumped to dialogue index {targetDialogueIndex}");
            
            // Broadcast the state change to all players
            await Clients.Group(roomId).SendAsync("Act1DialogueContinued", targetDialogueIndex);
            await BroadcastAct1GameState(roomId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Act1Error", $"Debug skip failed: {ex.Message}");
        }
    }

    // Content moved to IAct1StoryEngine

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove player from room tracking
        foreach (var roomKvp in _roomPlayers)
        {
            var roomId = roomKvp.Key;
            var roomPlayerDict = roomKvp.Value;
            
            if (roomPlayerDict.TryRemove(Context.ConnectionId, out var playerName))
            {
                await Clients.Group(roomId).SendAsync("PlayerLeft", playerName, Context.ConnectionId);
                
                // Clean up empty rooms
                if (roomPlayerDict.IsEmpty)
                {
                    _roomPlayers.TryRemove(roomId, out _);
                }
            }
        }
        
        // Remove player from any games
        foreach (var kvp in _games)
        {
            if (kvp.Value.RemovePlayer(Context.ConnectionId))
            {
                await Clients.Group(kvp.Key).SendAsync("GameStateUpdated", kvp.Value);
            }
        }
        
        // Remove player from code cracker games
        foreach (var kvp in _codeCrackerGames)
        {
            if (kvp.Value.RemovePlayer(Context.ConnectionId))
            {
                await Clients.Group(kvp.Key).SendAsync("CodeCrackerGameStateUpdated", kvp.Value.GetGameState());
            }
        }
        
        // Remove player from signal decoder games
        foreach (var kvp in _signalDecoderGames)
        {
            if (kvp.Value.RemovePlayer(Context.ConnectionId))
            {
                await Clients.Group(kvp.Key).SendAsync("SignalDecoderGameStateUpdated", kvp.Value.GetGameState());
            }
        }
        
        // Remove player from navigation maze games
        foreach (var kvp in _navigationMazeGames)
        {
            if (kvp.Value.RemovePlayer(Context.ConnectionId))
            {
                await Clients.Group(kvp.Key).SendAsync("NavigationMazeGameStateUpdated", kvp.Value.GetGameState());
            }
        }
        
        // Remove player from alchemy games
        foreach (var kvp in _alchemyGames)
        {
            if (kvp.Value.RemovePlayer(Context.ConnectionId))
            {
                await Clients.Group(kvp.Key).SendAsync("AlchemyGameStateUpdated", kvp.Value.GetGameState());
            }
        }
        
        // Remove player from rune protocol games
        foreach (var kvp in _runeProtocolGames)
        {
            if (kvp.Value.RemovePlayer(Context.ConnectionId))
            {
                await Clients.Group(kvp.Key).SendAsync("RuneProtocolGameStateUpdated", kvp.Value.GetGameState());
            }
        }
        
        // Remove player from picture explanation games
        foreach (var kvp in _pictureExplanationGames)
        {
            if (kvp.Value.RemovePlayer(Context.ConnectionId))
            {
                await Clients.Group(kvp.Key).SendAsync("PictureExplanationGameStateUpdated", kvp.Value.GetGameState());
            }
        }
        
        // Remove player from word forge games
        foreach (var kvp in _wordForgeGames)
        {
            if (kvp.Value.RemovePlayer(Context.ConnectionId))
            {
                await Clients.Group(kvp.Key).SendAsync("WordForgeGameStateUpdated", kvp.Value.GetGameState());
            }
        }
        
        // Remove player from visual novel games
        foreach (var kvp in _visualNovelGames)
        {
            var game = kvp.Value;
            var disconnectedPlayer = game.GetPlayer(Context.ConnectionId);
            
            if (disconnectedPlayer != null)
            {
                // Mark player as disconnected
                disconnectedPlayer.IsConnected = false;
                
                // Remove player completely from the game
                game.Players.RemoveAll(p => p.PlayerId == Context.ConnectionId);
                
                // Clean up empty games or notify remaining players
                if (game.Players.Count == 0)
                {
                    _visualNovelGames.TryRemove(kvp.Key, out _);
                }
                else
                {
                    await Clients.Group(kvp.Key).SendAsync("VisualNovelPlayerDisconnected", disconnectedPlayer.PlayerName);
                    await BroadcastVisualNovelGameState(kvp.Key);
                }
            }
        }
        
        // Remove player from Act 1 games
        foreach (var kvp in _act1Games)
        {
            var game = kvp.Value;
            var disconnectedPlayer = game.GetPlayer(Context.ConnectionId);
            
            if (disconnectedPlayer != null)
            {
                // Mark player as disconnected
                disconnectedPlayer.IsConnected = false;
                
                // Remove player completely from the game
                game.Players.RemoveAll(p => p.PlayerId == Context.ConnectionId);
                
                // Clean up empty games or notify remaining players
                if (game.Players.Count == 0)
                {
                    _act1Games.TryRemove(kvp.Key, out _);
                }
                else
                {
                    await Clients.Group(kvp.Key).SendAsync("Act1PlayerDisconnected", disconnectedPlayer.PlayerName);
                    await BroadcastAct1GameState(kvp.Key);
                }
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task ContinueStoryToScene3(string roomId)
    {
        try
        {
            // Get players from Picture Explanation game
            if (!_pictureExplanationGames.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("PictureExplanationInvalidAction", "Game not found for story continuation");
                return;
            }

            var connectedPlayers = game.GetConnectedPlayers();
            if (connectedPlayers.Count != 2)
            {
                await Clients.Caller.SendAsync("PictureExplanationInvalidAction", "Both players needed for story continuation");
                return;
            }
            
            // Build redirect URLs for Act1 Scene 3 (database_revelation)
            var redirectUrls = new Dictionary<string, string>();
            foreach (var connectionId in connectedPlayers)
            {
                var playerView = game.GetPlayerView(connectionId);
                var roleName = playerView.Role.ToLower();
                var playerName = playerView.DisplayName;
                
                var parameters = $"role={roleName}&avatar=1&name={Uri.EscapeDataString(playerName)}&roomId={Uri.EscapeDataString(roomId)}&squad={Uri.EscapeDataString(roomId)}&sceneIndex=2";
                redirectUrls[connectionId] = $"/act1-multiplayer?{parameters}";
            }

            // Send redirect to all players
            foreach (var connectionId in connectedPlayers)
            {
                if (redirectUrls.TryGetValue(connectionId, out var url))
                {
                    Console.WriteLine($"[GameHub] Redirecting player {connectionId} to: {url}");
                    await Clients.Client(connectionId).SendAsync("RedirectToStoryScene3", url);
                }
            }

            Console.WriteLine($"[GameHub] Continuing story to Scene 3 for room {roomId} with {connectedPlayers.Count} players");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameHub] Error continuing story to Scene 3: {ex.Message}");
            await Clients.Caller.SendAsync("PictureExplanationInvalidAction", "Error continuing story - please try again");
        }
    }

    public async Task ContinueStoryAfterSignalDecoder(string roomId)
    {
        try
        {
            // Get players from Signal Decoder game  
            if (!_signalDecoderGames.TryGetValue(roomId, out var game))
            {
                await Clients.Caller.SendAsync("SignalDecoderInvalidGuess", "Game not found for story continuation");
                return;
            }

            var connectedPlayers = game.GetConnectedPlayers();
            if (connectedPlayers.Count != 2)
            {
                await Clients.Caller.SendAsync("SignalDecoderInvalidGuess", "Both players needed for story continuation");
                return;
            }

            // TODO: Implement the next scene in the story progression
            // For now, send a placeholder message indicating where the next scene would go
            foreach (var connectionId in connectedPlayers)
            {
                await Clients.Client(connectionId).SendAsync("RedirectToNextStoryScene", "/");
            }

            Console.WriteLine($"[GameHub] Signal Decoder completed - story continuation ready for room {roomId} with {connectedPlayers.Count} players");
            Console.WriteLine($"[GameHub] TODO: Implement next scene after Signal Decoder in story progression");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameHub] Error continuing story after Signal Decoder: {ex.Message}");
            await Clients.Caller.SendAsync("SignalDecoderInvalidGuess", "Error continuing story - please try again");
        }
    }
}

public class AlchemyGame
{
    public enum PlayerRole { Piltover, Zaunite }
    
    // Healing Potion Recipe for Vi - Advanced 4-Ingredient Version with Combination
    private static readonly AlchemyRecipe HealingPotionRecipe = new AlchemyRecipe
    {
        Name = "Vi's Healing Potion",
        Description = "A powerful healing elixir requiring precise combination techniques. The Shimmer Crystal and Hex Berries must be combined first, then further processed for maximum potency.",
        RequiredIngredients = new[] { "shimmer_crystal", "hex_berries", "zaun_grey", "vial_of_tears" },
        Steps = new[]
        {
            new RecipeStep
            {
                StepNumber = 1,
                Instruction = "Step 1: Combine magical components",
                IngredientId = "shimmer_essence", // This will be created by combining shimmer_crystal + hex_berries
                RequiredStation = ProcessingStation.MixingStation,
                RequiredState = IngredientState.Mixed,
                DetailedDescription = "Place both Shimmer Crystal and Hex Berries into the mixing station. The combination will create a volatile Shimmer Essence that requires further processing."
            },
            new RecipeStep
            {
                StepNumber = 2,
                Instruction = "Step 2: Stabilize the essence",
                IngredientId = "shimmer_essence",
                RequiredStation = ProcessingStation.HeatingStation,
                RequiredState = IngredientState.Heated,
                DetailedDescription = "Heat the unstable Shimmer Essence until it glows with steady blue light. This stabilizes the magical properties."
            },
            new RecipeStep
            {
                StepNumber = 3,
                Instruction = "Step 3: Prepare the mushroom",
                IngredientId = "zaun_grey",
                RequiredStation = ProcessingStation.CuttingBoard,
                RequiredState = IngredientState.Chopped,
                DetailedDescription = "Carefully chop the Zaun Grey mushroom into small, uniform pieces. Precision is crucial for proper dissolution."
            },
            new RecipeStep
            {
                StepNumber = 4,
                Instruction = "Step 4: Final catalyst",
                IngredientId = "vial_of_tears",
                RequiredStation = ProcessingStation.Cauldron,
                RequiredState = IngredientState.Raw,
                DetailedDescription = "Add ingredients to cauldron in this order: Heated Shimmer Essence, Chopped Zaun Grey, and finally the Vial of Tears (unprocessed) as a catalyst."
            }
        }
    };
    
    private static readonly AlchemyIngredient[] IngredientBank = new[]
    {
        new AlchemyIngredient
        {
            Id = "shimmer_crystal",
            Name = "Shimmer Crystal",
            Description = "A crystalline fragment that glows with inner light",
            ImagePath = "images/alchemy/shimmer_crystal_raw.png"
        },
        new AlchemyIngredient
        {
            Id = "hex_berries",
            Name = "Hex Berries",
            Description = "Magical berries from Piltover's hextech gardens",
            ImagePath = "images/alchemy/hex_berries_raw.png"
        },
        new AlchemyIngredient
        {
            Id = "zaun_grey",
            Name = "Zaun Grey Mushroom",
            Description = "A hardy mushroom that grows in the undercity's toxic soil",
            ImagePath = "images/alchemy/zaun_grey_raw.png"
        },
        new AlchemyIngredient
        {
            Id = "vial_of_tears",
            Name = "Vial of Tears",
            Description = "Precious tears collected from the Grey - adds emotional potency",
            ImagePath = "images/alchemy/vial_of_tears_raw.png"
        },
        // Combined ingredients (created by mixing station)
        new AlchemyIngredient
        {
            Id = "shimmer_essence",
            Name = "Shimmer Essence",
            Description = "A volatile combination of crystal and berries - requires further processing",
            ImagePath = "images/alchemy/shimmer_essence_raw.png"
        }
    };
    
    public Dictionary<string, PlayerRole> Players { get; set; } = new();
    public Dictionary<string, string> PlayerNames { get; set; } = new();
    public List<AlchemyIngredient> AvailableIngredients { get; set; } = new();
    public List<AlchemyIngredient> CauldronContents { get; set; } = new();
    public bool IsCompleted { get; set; } = false;
    public int Score { get; set; } = 100;
    public int Attempts { get; set; } = 0;
    public List<string> ProcessingErrors { get; set; } = new();
    
    public int PlayerCount => Players.Count;
    
    public AlchemyGame()
    {
        // Initialize available ingredients (exclude combined ingredients like shimmer_essence)
        AvailableIngredients = IngredientBank.Where(ingredient => ingredient.Id != "shimmer_essence")
            .Select(ingredient => new AlchemyIngredient
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Description = ingredient.Description,
                ImagePath = ingredient.ImagePath,
                State = IngredientState.Raw,
                IsUsed = false
            }).ToList();
    }
    
    public PlayerRole? AddPlayer(string connectionId, string playerName)
    {
        if (Players.TryGetValue(connectionId, out var existingRole))
        {
            return existingRole;
        }
        
        if (Players.Count >= 2) return null;
        
        var role = Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite;
        Players[connectionId] = role;
        PlayerNames[connectionId] = playerName;
        return role;
    }
    
    public PlayerRole? AddPlayerWithRole(string connectionId, string playerName, string requestedRole)
    {
        if (Players.TryGetValue(connectionId, out var existingRole))
        {
            return existingRole;
        }
        
        if (Players.Count >= 2) return null;
        
        // Check if requested role is available
        var requestedEnum = requestedRole.ToLower() switch
        {
            "piltover" => PlayerRole.Piltover,
            "zaun" => PlayerRole.Zaunite,
            "zaunite" => PlayerRole.Zaunite,
            _ => Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite
        };
        
        // If role is already taken, assign the other one
        if (Players.Values.Contains(requestedEnum))
        {
            requestedEnum = requestedEnum == PlayerRole.Piltover ? PlayerRole.Zaunite : PlayerRole.Piltover;
        }
        
        Players[connectionId] = requestedEnum;
        PlayerNames[connectionId] = playerName;
        return requestedEnum;
    }
    
    public bool RemovePlayer(string connectionId)
    {
        var removed = Players.Remove(connectionId);
        PlayerNames.Remove(connectionId);
        return removed;
    }
    
    public List<string> GetConnectedPlayers()
    {
        return Players.Keys.ToList();
    }
    
    public (bool Success, string Message) ProcessIngredient(string connectionId, string ingredientId, string stationName)
    {
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");
            
        if (Players[connectionId] != PlayerRole.Zaunite)
            return (false, "Only the Zaunite player can process ingredients");
            
        var ingredient = AvailableIngredients.FirstOrDefault(i => i.Id == ingredientId);
        if (ingredient == null || ingredient.IsUsed)
            return (false, "Ingredient not available");
            
        // Parse station enum
        if (!Enum.TryParse<ProcessingStation>(stationName, out var station))
            return (false, "Invalid processing station");
            
        // Handle mixing station differently (requires 2 ingredients)
        if (station == ProcessingStation.MixingStation)
        {
            return (false, "Use CombineIngredients method for mixing station");
        }
        
        // Process the ingredient based on station
        var newState = station switch
        {
            ProcessingStation.MortarPestle => IngredientState.Ground,
            ProcessingStation.HeatingStation => IngredientState.Heated,
            ProcessingStation.CuttingBoard => IngredientState.Chopped,
            ProcessingStation.FilteringStation => IngredientState.Filtered,
            _ => ingredient.State
        };
        
        ingredient.State = newState;
        ingredient.ImagePath = GetProcessedImagePath(ingredient.Id, newState);
        
        return (true, $"{ingredient.Name} processed successfully");
    }
    
    public (bool Success, string Message) CombineIngredients(string connectionId, string ingredient1Id, string ingredient2Id)
    {
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");
            
        if (Players[connectionId] != PlayerRole.Zaunite)
            return (false, "Only the Zaunite player can combine ingredients");
            
        var ingredient1 = AvailableIngredients.FirstOrDefault(i => i.Id == ingredient1Id && !i.IsUsed);
        var ingredient2 = AvailableIngredients.FirstOrDefault(i => i.Id == ingredient2Id && !i.IsUsed);
        
        if (ingredient1 == null || ingredient2 == null)
            return (false, "One or both ingredients not available");
            
        // Check for valid combination: Shimmer Crystal + Hex Berries = Shimmer Essence
        var validCombination = (ingredient1Id == "shimmer_crystal" && ingredient2Id == "hex_berries") ||
                              (ingredient1Id == "hex_berries" && ingredient2Id == "shimmer_crystal");
                              
        if (!validCombination)
            return (false, "These ingredients cannot be combined. Try Shimmer Crystal + Hex Berries.");
            
        // Mark original ingredients as used
        ingredient1.IsUsed = true;
        ingredient2.IsUsed = true;
        
        // Create the combined ingredient
        var shimmerEssence = IngredientBank.First(i => i.Id == "shimmer_essence");
        var combinedIngredient = new AlchemyIngredient
        {
            Id = shimmerEssence.Id,
            Name = shimmerEssence.Name,
            Description = shimmerEssence.Description,
            ImagePath = shimmerEssence.ImagePath,
            State = IngredientState.Mixed,
            IsUsed = false
        };
        
        AvailableIngredients.Add(combinedIngredient);
        
        return (true, $"Successfully combined {ingredient1.Name} and {ingredient2.Name} into {combinedIngredient.Name}!");
    }
    
    public (bool Success, string Message) AddToCauldron(string connectionId, string ingredientId, int position)
    {
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");
            
        if (Players[connectionId] != PlayerRole.Zaunite)
            return (false, "Only the Zaunite player can add ingredients to cauldron");
            
        var ingredient = AvailableIngredients.FirstOrDefault(i => i.Id == ingredientId);
        if (ingredient == null || ingredient.IsUsed)
            return (false, "Ingredient not available");
            
        // Add to cauldron
        ingredient.IsUsed = true;
        
        // Insert at specified position or add to end
        if (position >= 0 && position < CauldronContents.Count)
        {
            CauldronContents.Insert(position, ingredient);
        }
        else
        {
            CauldronContents.Add(ingredient);
        }
        
        return (true, $"{ingredient.Name} added to cauldron");
    }
    
    public (bool Success, string Message) SubmitPotion(string connectionId)
    {
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");
            
        Attempts++;
        Score = Math.Max(0, 100 - (Attempts - 1) * 10); // Lose 10 points per failed attempt
        
        var mistakes = ValidatePotion();
        
        if (mistakes.Count == 0)
        {
            IsCompleted = true;
            return (true, $"Perfect! Vi's healing potion is complete! Potion potency: {Score}%");
        }
        else
        {
            ProcessingErrors = mistakes;
            return (true, $"The potion failed... Something went wrong. Attempt {Attempts}");
        }
    }
    
    private List<string> ValidatePotion()
    {
        var mistakes = new List<string>();
        var recipe = HealingPotionRecipe;
        
        // Expected cauldron contents: Heated Shimmer Essence, Chopped Zaun Grey, Raw Vial of Tears
        var expectedCauldronIngredients = new[]
        {
            new { Id = "shimmer_essence", State = IngredientState.Heated },  // Step 2 result
            new { Id = "zaun_grey", State = IngredientState.Chopped },        // Step 3 result  
            new { Id = "vial_of_tears", State = IngredientState.Raw }          // Step 4 (raw catalyst)
        };
        
        // Check if correct number of ingredients are present
        if (CauldronContents.Count != expectedCauldronIngredients.Length)
        {
            mistakes.Add($"Wrong number of ingredients (expected {expectedCauldronIngredients.Length}, got {CauldronContents.Count})");
            return mistakes; // Can't validate further without correct count
        }
        
        // Check order and processing
        for (int i = 0; i < expectedCauldronIngredients.Length; i++)
        {
            var expected = expectedCauldronIngredients[i];
            var cauldronIngredient = CauldronContents[i];
            
            // Check correct ingredient
            if (cauldronIngredient.Id != expected.Id)
            {
                var expectedName = IngredientBank.First(ing => ing.Id == expected.Id).Name;
                mistakes.Add($"Position {i + 1}: Expected {expectedName}, but got {cauldronIngredient.Name}");
            }
            
            // Check correct processing state
            if (cauldronIngredient.State != expected.State)
            {
                mistakes.Add($"Position {i + 1}: {cauldronIngredient.Name} should be {expected.State}, but was {cauldronIngredient.State}");
            }
        }
        
        return mistakes;
    }
    
    public List<string> GetMistakes()
    {
        return ProcessingErrors;
    }
    
    public void ResetCauldron()
    {
        // Return ingredients to available state
        foreach (var ingredient in CauldronContents)
        {
            var originalIngredient = AvailableIngredients.First(i => i.Id == ingredient.Id);
            originalIngredient.IsUsed = false;
            // Keep processing state - they don't need to reprocess
        }
        
        CauldronContents.Clear();
    }
    
    private string GetProcessedImagePath(string ingredientId, IngredientState state)
    {
        var stateSuffix = state.ToString().ToLower();
        return $"images/alchemy/{ingredientId}_{stateSuffix}.png";
    }
    
    public AlchemyPlayerView GetPlayerView(string connectionId)
    {
        if (!Players.TryGetValue(connectionId, out var role))
            return new AlchemyPlayerView();
            
        if (role == PlayerRole.Piltover)
        {
            // Piltover player sees the recipe
            return new AlchemyPlayerView
            {
                Role = "Piltover",
                DisplayName = "Caitlyn (Master Alchemist)",
                Instruction = IsCompleted ? "Excellent work! Vi's healing potion is ready!" : 
                             "Guide your partner through brewing Vi's healing potion:",
                Recipe = HealingPotionRecipe,
                CurrentStepIndex = 0, // Show all steps
                IsCompleted = IsCompleted,
                Score = Score,
                CompletionMessage = IsCompleted ? $"Perfect brewing! Potion potency: {Score}%" : null,
                Mistakes = ProcessingErrors.ToArray()
            };
        }
        else
        {
            // Zaunite player sees the lab
            return new AlchemyPlayerView
            {
                Role = "Zaunite",
                DisplayName = "Vi (Lab Assistant)",
                Instruction = IsCompleted ? "Potion complete! Vi will recover quickly now!" :
                             "Follow Caitlyn's recipe instructions to brew the healing potion:",
                AvailableIngredients = AvailableIngredients.Where(i => !i.IsUsed).ToArray(),
                CauldronContents = CauldronContents.ToArray(),
                AvailableStations = new[] { ProcessingStation.MortarPestle, ProcessingStation.HeatingStation, 
                                          ProcessingStation.CuttingBoard, ProcessingStation.FilteringStation, ProcessingStation.Cauldron },
                IsCompleted = IsCompleted,
                Score = Score,
                CompletionMessage = IsCompleted ? $"Perfect brewing! Potion potency: {Score}%" : null,
                Mistakes = ProcessingErrors.ToArray()
            };
        }
    }
    
    public AlchemyGameState GetGameState()
    {
        return new AlchemyGameState
        {
            CurrentStepIndex = 0,
            TotalSteps = HealingPotionRecipe.Steps.Length,
            IsCompleted = IsCompleted,
            Score = Score,
            PlayerCount = Players.Count,
            PlayersNeeded = 2 - Players.Count,
            HasStarted = Players.Count == 2
        };
    }
    
    public void Reset()
    {
        // Reset available ingredients (exclude combined ingredients like shimmer_essence)
        AvailableIngredients = IngredientBank.Where(ingredient => ingredient.Id != "shimmer_essence")
            .Select(ingredient => new AlchemyIngredient
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Description = ingredient.Description,
                ImagePath = ingredient.ImagePath,
                State = IngredientState.Raw,
                IsUsed = false
            }).ToList();
        
        CauldronContents.Clear();
        IsCompleted = false;
        Score = 100;
        Attempts = 0;
        ProcessingErrors.Clear();
    }
}

public class TicTacToeGame
{
    public string[] Board { get; set; } = new string[9];
    public string CurrentPlayer { get; set; } = "X";
    public bool GameOver { get; set; } = false;
    public string? Winner { get; set; }
    public int[]? WinningLine { get; set; }
    public Dictionary<string, string> Players { get; set; } = new(); // ConnectionId -> Symbol
    public Dictionary<string, string> PlayerNames { get; set; } = new(); // ConnectionId -> Name

    public string? AddPlayer(string connectionId, string playerName)
    {
        if (Players.Count >= 2) return null;
        
        var symbol = Players.Count == 0 ? "X" : "O";
        Players[connectionId] = symbol;
        PlayerNames[connectionId] = playerName;
        return symbol;
    }

    public bool RemovePlayer(string connectionId)
    {
        var removed = Players.Remove(connectionId);
        PlayerNames.Remove(connectionId);
        
        if (removed)
        {
            Reset();
        }
        
        return removed;
    }

    public (bool Success, string Message) MakeMove(string connectionId, int position)
    {
        if (GameOver)
            return (false, "Game is over");
            
        if (!Players.TryGetValue(connectionId, out var playerSymbol))
            return (false, "You are not in this game");
            
        if (playerSymbol != CurrentPlayer)
            return (false, "It's not your turn");
            
        if (position < 0 || position > 8)
            return (false, "Invalid position");
            
        if (!string.IsNullOrEmpty(Board[position]))
            return (false, "Position already taken");

        Board[position] = playerSymbol;
        
        CheckWinner();
        
        if (!GameOver)
        {
            CurrentPlayer = CurrentPlayer == "X" ? "O" : "X";
        }

        return (true, "Move successful");
    }

    private void CheckWinner()
    {
        int[,] winConditions = {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, // Rows
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, // Columns
            {0, 4, 8}, {2, 4, 6}             // Diagonals
        };

        for (int i = 0; i < winConditions.GetLength(0); i++)
        {
            int a = winConditions[i, 0];
            int b = winConditions[i, 1];
            int c = winConditions[i, 2];

            if (!string.IsNullOrEmpty(Board[a]) && 
                Board[a] == Board[b] && 
                Board[b] == Board[c])
            {
                Winner = Board[a];
                GameOver = true;
                WinningLine = new[] { a, b, c };
                return;
            }
        }

        // Check for tie
        if (Board.All(cell => !string.IsNullOrEmpty(cell)))
        {
            GameOver = true;
            Winner = "TIE";
        }
    }

    public void Reset()
    {
        Board = new string[9];
        CurrentPlayer = "X";
        GameOver = false;
        Winner = null;
        WinningLine = null;
    }
}

public class CodeCrackerGame
{
    public enum PlayerRole { Piltover, Zaunite }
    
    private static readonly WordPuzzle[] WordBank = new[]
    {
        new WordPuzzle("murmur", "a soft, low sound", "Flüstern", "whisper", "m_r_u_"),
        new WordPuzzle("initiation", "the beginning of something", "Einweihung", "beginning", "in_t_at__n"),
        new WordPuzzle("revelation", "a surprising disclosure", "Offenbarung", "discovery", "r_v_l_t__n"),
        new WordPuzzle("sanctuary", "a place of safety", "Zufluchtsort", "refuge", "s_nc_u_ry"),
        new WordPuzzle("ancient", "very old", "uralt", "old", "_nc__nt"),
        new WordPuzzle("mysterious", "difficult to understand", "geheimnisvoll", "puzzling", "my_t_r__us"),
        new WordPuzzle("adventure", "an exciting experience", "Abenteuer", "journey", "_dv_nt_re"),
        new WordPuzzle("treasure", "valuable items", "Schatz", "riches", "tr__s_re"),
        new WordPuzzle("discovery", "finding something new", "Entdeckung", "revelation", "d_sc_v_ry"),
        new WordPuzzle("guardian", "a protector", "Wächter", "protector", "g_a_d__n")
    };

    public Dictionary<string, PlayerRole> Players { get; set; } = new();
    public Dictionary<string, string> PlayerNames { get; set; } = new();
    public int CurrentWordIndex { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public int Score { get; set; } = 0;
    public int HintsUsed { get; set; } = 0;
    public List<string> AttemptHistory { get; set; } = new();
    
    private WordPuzzle CurrentWord => WordBank[CurrentWordIndex];
    
    public PlayerRole? AddPlayer(string connectionId, string playerName)
    {
        // If player is already in the game, return their existing role
        if (Players.TryGetValue(connectionId, out var existingRole))
        {
            return existingRole;
        }
        
        if (Players.Count >= 2) return null;
        
        var role = Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite;
        Players[connectionId] = role;
        PlayerNames[connectionId] = playerName;
        return role;
    }
    
    public PlayerRole? AddPlayerWithRole(string connectionId, string playerName, string requestedRole)
    {
        // If player is already in the game, return their existing role
        if (Players.TryGetValue(connectionId, out var existingRole))
        {
            return existingRole;
        }
        
        if (Players.Count >= 2) return null;
        
        // Check if requested role is available
        var requestedEnum = requestedRole.ToLower() switch
        {
            "piltover" => PlayerRole.Piltover,
            "zaunite" => PlayerRole.Zaunite,
            _ => Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite
        };
        
        // If role is already taken, assign the other one
        if (Players.Values.Contains(requestedEnum))
        {
            requestedEnum = requestedEnum == PlayerRole.Piltover ? PlayerRole.Zaunite : PlayerRole.Piltover;
        }
        
        Players[connectionId] = requestedEnum;
        PlayerNames[connectionId] = playerName;
        return requestedEnum;
    }

    public bool RemovePlayer(string connectionId)
    {
        var removed = Players.Remove(connectionId);
        PlayerNames.Remove(connectionId);
        return removed;
    }

    public List<string> GetConnectedPlayers()
    {
        return Players.Keys.ToList();
    }

    public (bool Success, string Message) SubmitGuess(string connectionId, string guess)
    {
        if (IsCompleted)
            return (false, "Game is already completed");
            
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");

        guess = guess.Trim().ToLower();
        AttemptHistory.Add($"{PlayerNames[connectionId]}: {guess}");

        if (guess == CurrentWord.Answer.ToLower())
        {
            Score += Math.Max(10 - HintsUsed - AttemptHistory.Count, 1);
            CurrentWordIndex++;
            
            if (CurrentWordIndex >= WordBank.Length)
            {
                IsCompleted = true;
                return (true, $"Correct! Final word completed. Game finished with score: {Score}");
            }
            else
            {
                HintsUsed = 0;
                AttemptHistory.Clear();
                return (true, $"Correct! Moving to next word. Score: {Score}");
            }
        }
        else
        {
            return (false, $"Incorrect guess: {guess}");
        }
    }

    public string? GetHint(string connectionId)
    {
        if (!Players.ContainsKey(connectionId) || HintsUsed >= 3)
            return null;

        HintsUsed++;
        
        return HintsUsed switch
        {
            1 => $"Word length: {CurrentWord.Answer.Length} letters",
            2 => $"First letter: {CurrentWord.Answer[0].ToString().ToUpper()}",
            3 => $"Category hint: This word is related to {GetCategoryHint()}",
            _ => null
        };
    }

    private string GetCategoryHint()
    {
        return CurrentWord.Answer switch
        {
            "murmur" => "sounds",
            "initiation" => "beginnings", 
            "revelation" => "discoveries",
            "sanctuary" => "places",
            "ancient" => "time",
            "mysterious" => "qualities",
            "adventure" => "experiences",
            "treasure" => "valuable things",
            "discovery" => "findings",
            "guardian" => "people/roles",
            _ => "concepts"
        };
    }

    public PlayerViewData GetPlayerView(string connectionId)
    {
        if (!Players.TryGetValue(connectionId, out var role))
            return new PlayerViewData();

        if (role == PlayerRole.Piltover)
        {
            return new PlayerViewData
            {
                Role = "Piltover",
                DisplayName = "Caitlyn (Piltover Enforcer)",
                DistortedWord = CurrentWord.DistortedWord,
                Instruction = "Decode this corrupted word from Piltover's archives:",
                AttemptHistory = AttemptHistory.TakeLast(3).ToList()
            };
        }
        else
        {
            return new PlayerViewData
            {
                Role = "Zaunite", 
                DisplayName = "Vi (Zaunite Hacker)",
                Definition = CurrentWord.Definition,
                GermanTranslation = CurrentWord.GermanTranslation,
                Synonym = CurrentWord.Synonym,
                Instruction = "Help decode the word using these clues:",
                AttemptHistory = AttemptHistory.TakeLast(3).ToList()
            };
        }
    }

    public GameStateData GetGameState()
    {
        return new GameStateData
        {
            CurrentWordIndex = CurrentWordIndex,
            TotalWords = WordBank.Length,
            IsCompleted = IsCompleted,
            Score = Score,
            HintsUsed = HintsUsed,
            PlayerCount = Players.Count,
            PlayersNeeded = 2 - Players.Count
        };
    }

    public void Reset()
    {
        CurrentWordIndex = 0;
        IsCompleted = false;
        Score = 0;
        HintsUsed = 0;
        AttemptHistory.Clear();
    }
}

public class WordPuzzle
{
    public string Answer { get; set; }
    public string Definition { get; set; }
    public string GermanTranslation { get; set; }
    public string Synonym { get; set; }
    public string DistortedWord { get; set; }

    public WordPuzzle(string answer, string definition, string germanTranslation, string synonym, string distortedWord)
    {
        Answer = answer;
        Definition = definition;
        GermanTranslation = germanTranslation;
        Synonym = synonym;
        DistortedWord = distortedWord;
    }
}

public class PlayerViewData
{
    public string Role { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Instruction { get; set; } = "";
    public string? DistortedWord { get; set; }
    public string? Definition { get; set; }
    public string? GermanTranslation { get; set; }
    public string? Synonym { get; set; }
    public List<string> AttemptHistory { get; set; } = new();
}

public class GameStateData
{
    public int CurrentWordIndex { get; set; }
    public int TotalWords { get; set; }
    public bool IsCompleted { get; set; }
    public int Score { get; set; }
    public int HintsUsed { get; set; }
    public int PlayerCount { get; set; }
    public int PlayersNeeded { get; set; }
}

// SIMPLIFIED Signal Decoder Game - Single Audio/Sentence Approach
public class SimpleSignalDecoderGame
{
    public enum PlayerRole { Piltover, Zaunite }
    
    // Multiple simple sentences for progression
    private static readonly SimpleSignalData[] SignalBank = new[]
    {
        new SimpleSignalData
        {
            FullSentence = "Help! Fire spreading fast!",
            SentenceWithBlanks = "Help! {0} spreading {1}!",
            MissingWords = new[] { "fire", "fast" },
            AudioFile = "audio/signals/critical_01.mp3"
        },
        new SimpleSignalData
        {
            FullSentence = "Evacuate building now!",
            SentenceWithBlanks = "{0} {1} {2}!",
            MissingWords = new[] { "evacuate", "building", "now" },
            AudioFile = "audio/signals/critical_02.mp3"
        },
        new SimpleSignalData
        {
            FullSentence = "Medical emergency!",
            SentenceWithBlanks = "{0} {1}!",
            MissingWords = new[] { "medical", "emergency" },
            AudioFile = "audio/signals/critical_03.mp3"
        }
    };

    public Dictionary<string, PlayerRole> Players { get; set; } = new();
    public Dictionary<string, string> PlayerNames { get; set; } = new();
    public List<string> GuessedWords { get; set; } = new();
    public List<string> AttemptHistory { get; set; } = new();
    public bool IsCompleted { get; set; } = false;
    public int Score { get; set; } = 0;
    public int HintsUsed { get; set; } = 0;
    public int CurrentSignalIndex { get; set; } = 0;
    public int SignalsCompleted { get; set; } = 0;
    
    public int PlayerCount => Players.Count;
    public SimpleSignalData CurrentSignal => SignalBank[CurrentSignalIndex];
    public int RemainingWords => CurrentSignal.MissingWords.Length - GuessedWords.Count;
    
    public PlayerRole? AddPlayer(string connectionId, string playerName)
    {
        if (Players.TryGetValue(connectionId, out var existingRole))
        {
            return existingRole;
        }
        
        if (Players.Count >= 2) return null;
        
        var role = Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite;
        Players[connectionId] = role;
        PlayerNames[connectionId] = playerName;
        return role;
    }
    
    public PlayerRole? AddPlayerWithRole(string connectionId, string playerName, string requestedRole)
    {
        if (Players.TryGetValue(connectionId, out var existingRole))
        {
            return existingRole;
        }
        
        if (Players.Count >= 2) return null;
        
        // Check if requested role is available
        var requestedEnum = requestedRole.ToLower() switch
        {
            "piltover" => PlayerRole.Piltover,
            "zaunite" => PlayerRole.Zaunite,
            "zaun" => PlayerRole.Zaunite,
            _ => Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite
        };
        
        // If requested role is already taken, assign the other role
        if (Players.Values.Contains(requestedEnum))
        {
            requestedEnum = requestedEnum == PlayerRole.Piltover ? PlayerRole.Zaunite : PlayerRole.Piltover;
        }
        
        Players[connectionId] = requestedEnum;
        PlayerNames[connectionId] = playerName;
        return requestedEnum;
    }

    public bool RemovePlayer(string connectionId)
    {
        var removed = Players.Remove(connectionId);
        PlayerNames.Remove(connectionId);
        return removed;
    }

    public List<string> GetConnectedPlayers()
    {
        return Players.Keys.ToList();
    }

    public (bool Success, string Message) SubmitGuess(string connectionId, string guess)
    {
        if (IsCompleted)
            return (false, "Game is already completed");
            
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");

        guess = guess.Trim().ToLower();
        
        // Check if this word is one of the missing words
        var targetWords = CurrentSignal.MissingWords.Select(w => w.ToLower()).ToList();
        var matchingWord = targetWords.FirstOrDefault(w => w == guess);
        
        if (matchingWord != null && !GuessedWords.Contains(matchingWord))
        {
            GuessedWords.Add(matchingWord);
            AttemptHistory.Add($"{PlayerNames[connectionId]}: {guess} ✓");
            
            // Simple scoring
            Score += Math.Max(1, 10 - HintsUsed);
            
            if (GuessedWords.Count >= CurrentSignal.MissingWords.Length)
            {
                // Current signal completed
                SignalsCompleted++;
                
                if (CurrentSignalIndex >= SignalBank.Length - 1)
                {
                    // All signals completed - DON'T increment CurrentSignalIndex
                    IsCompleted = true;
                    return (true, $"🎉 All signals decoded! Final score: {Score}");
                }
                else
                {
                    // Move to next signal
                    CurrentSignalIndex++;
                    GuessedWords.Clear();
                    HintsUsed = 0;
                    return (true, $"📡 Signal {SignalsCompleted} completed! New signal incoming...");
                }
            }
            else
            {
                int remaining = CurrentSignal.MissingWords.Length - GuessedWords.Count;
                return (true, $"✅ Correct! {remaining} word(s) remaining.");
            }
        }
        else if (GuessedWords.Contains(guess))
        {
            return (false, "Word already found!");
        }
        else
        {
            AttemptHistory.Add($"{PlayerNames[connectionId]}: {guess} ❌");
            return (false, $"❌ '{guess}' is not one of the missing words");
        }
    }

    public string? GetHint(string connectionId)
    {
        if (!Players.ContainsKey(connectionId) || HintsUsed >= 3)
            return null;

        HintsUsed++;
        
        var unguessedWords = CurrentSignal.MissingWords
            .Where(w => !GuessedWords.Contains(w.ToLower())).ToList();
        if (!unguessedWords.Any()) return null;
        
        var targetWord = unguessedWords.First();
        
        return HintsUsed switch
        {
            1 => "🔍 Emergency signal - listen for action words",
            2 => $"📏 One word has {targetWord.Length} letters: {targetWord[0]}***",
            3 => $"🎯 Final hint: Think about what spreads in emergencies",
            _ => null
        };
    }

    public SimplePlayerView GetPlayerView(string connectionId)
    {
        if (!Players.TryGetValue(connectionId, out var role))
            return new SimplePlayerView();

        if (role == PlayerRole.Piltover)
        {
            // Piltover sees the sentence with proper word replacement
            var displaySentence = BuildDisplaySentence();
            
            return new SimplePlayerView
            {
                Role = "Piltover",
                DisplayName = "Caitlyn (Signal Interceptor)",
                Instruction = "Decode the intercepted transmission:",
                SentenceWithBlanks = displaySentence,
                GuessedWords = GuessedWords.ToList(),
                AttemptHistory = AttemptHistory.TakeLast(5).ToList()
            };
        }
        else
        {
            // Zaunite gets the audio
            return new SimplePlayerView
            {
                Role = "Zaunite",
                DisplayName = "Vi (Signal Analyst)",
                Instruction = "Listen to the audio and relay the missing words:",
                AudioFile = CurrentSignal.AudioFile,
                GuessedWords = GuessedWords.ToList(),
                AttemptHistory = AttemptHistory.TakeLast(5).ToList()
            };
        }
    }

    private string BuildDisplaySentence()
    {
        var template = CurrentSignal.SentenceWithBlanks;
        var words = new string[CurrentSignal.MissingWords.Length];
        
        // Fill in guessed words in their correct positions
        for (int i = 0; i < CurrentSignal.MissingWords.Length; i++)
        {
            var expectedWord = CurrentSignal.MissingWords[i].ToLower();
            if (GuessedWords.Contains(expectedWord))
            {
                words[i] = expectedWord.ToUpper();
            }
            else
            {
                words[i] = new string('_', expectedWord.Length);
            }
        }
        
        return string.Format(template, words);
    }

    public SimpleGameState GetGameState()
    {
        return new SimpleGameState
        {
            Score = Score,
            HintsUsed = HintsUsed,
            PlayerCount = Players.Count,
            PlayersNeeded = 2 - Players.Count,
            IsCompleted = IsCompleted,
            RemainingWords = RemainingWords,
            CurrentSignal = CurrentSignalIndex + 1,
            TotalSignals = SignalBank.Length,
            SignalsCompleted = SignalsCompleted
        };
    }

    public void Reset()
    {
        GuessedWords.Clear();
        AttemptHistory.Clear();
        IsCompleted = false;
        Score = 0;
        HintsUsed = 0;
        CurrentSignalIndex = 0;
        SignalsCompleted = 0;
    }
}

public class NavigationMazeGame
{
    public enum PlayerRole { Piltover, Zaunite }
    
    // Define all 5 locations with progressive difficulty
    private static readonly NavigationLocation[] LocationBank = new[]
    {
        new NavigationLocation
        {
            LocationId = 0,
            Name = "Sewer Entrance",
            Description = "Dark stone sewer entrance with three tunnel openings. Choose your path carefully...",
            ImagePath = "images/navigation/sewer-entrance.jpg",
            CorrectChoiceIndex = 2, // FORWARD
            SuccessMessage = "The central tunnel leads deeper into the undercity!",
            Choices = new[]
            {
                new NavigationChoice { Direction = "LEFT", Description = "Green chemical glow emanates from this tunnel", IsCorrect = false, GameOverMessage = "Vi got overwhelmed by toxic fumes! Even Vi needs backup sometimes... Try again!" },
                new NavigationChoice { Direction = "RIGHT", Description = "Completely dark with suspicious sounds", IsCorrect = false, GameOverMessage = "Something big lives down here... Vi had to retreat! That path was more dangerous than a Piltover Enforcer raid!" },
                new NavigationChoice { Direction = "FORWARD", Description = "Faint light visible at the end", IsCorrect = true, GameOverMessage = "" }
            }
        },
        new NavigationLocation
        {
            LocationId = 1,
            Name = "Industrial Pipe Junction",
            Description = "Large underground chamber filled with massive pipes. Steam hisses from various joints.",
            ImagePath = "images/navigation/industrial-pipes.jpg",
            CorrectChoiceIndex = 2, // AROUND right
            SuccessMessage = "Smart choice - avoiding the dangerous pipes led to safety!",
            Choices = new[]
            {
                new NavigationChoice { Direction = "THROUGH the large pipe", Description = "The main pipe looks old but passable", IsCorrect = false, GameOverMessage = "The old pipe couldn't hold Vi's weight! Jinx would have blown that up too... Reset and try again!" },
                new NavigationChoice { Direction = "UNDER the main pipe", Description = "Low crawl space beneath the pipes", IsCorrect = false, GameOverMessage = "Hot steam blocked Vi's path! Not even Vander could have survived that route!" },
                new NavigationChoice { Direction = "AROUND the pipes to the right", Description = "Safe path around the pipe system", IsCorrect = true, GameOverMessage = "" }
            }
        },
        new NavigationLocation
        {
            LocationId = 2,
            Name = "Chemical Processing Plant",
            Description = "Industrial chemical processing area with large vats of bubbling green chemicals.",
            ImagePath = "images/navigation/chemical-plant.jpg",
            CorrectChoiceIndex = 2, // BESIDE
            SuccessMessage = "Staying beside the main tank avoided the dangerous areas!",
            Choices = new[]
            {
                new NavigationChoice { Direction = "BETWEEN the chemical vats", Description = "Narrow path between bubbling containers", IsCorrect = false, GameOverMessage = "Chemtech spilled everywhere! Vi had to evacuate! The Undercity has claimed another victim... Restart?" },
                new NavigationChoice { Direction = "BEHIND the machinery", Description = "Path behind the processing equipment", IsCorrect = false, GameOverMessage = "Vi got trapped behind broken equipment! Even Vi needs backup sometimes... Try again!" },
                new NavigationChoice { Direction = "BESIDE the main tank", Description = "Safe walkway beside the primary tank", IsCorrect = true, GameOverMessage = "" }
            }
        },
        new NavigationLocation
        {
            LocationId = 3,
            Name = "Underground Market",
            Description = "Bustling underground marketplace built in a converted mine with multiple levels.",
            ImagePath = "images/navigation/underground-market.jpg",
            CorrectChoiceIndex = 2, // ACROSS OVER
            SuccessMessage = "The rope bridge was the secret route to freedom!",
            Choices = new[]
            {
                new NavigationChoice { Direction = "UP THROUGH the busy market", Description = "Stairs leading through the crowded marketplace", IsCorrect = false, GameOverMessage = "Enforcers were waiting in the crowd! That path was more dangerous than a Piltover Enforcer raid!" },
                new NavigationChoice { Direction = "DOWN INTO the old mines", Description = "Tunnel leading deeper into abandoned mines", IsCorrect = false, GameOverMessage = "The mines weren't stable... rocks are falling! Not even Vander could have survived that route!" },
                new NavigationChoice { Direction = "ACROSS OVER the rope bridge", Description = "Rope bridge spanning across the chasm", IsCorrect = true, GameOverMessage = "" }
            }
        },
        new NavigationLocation
        {
            LocationId = 4,
            Name = "Bridge to Piltover",
            Description = "You've reached the magnificent bridge spanning between the cities! Piltover's golden spires shine in the distance.",
            ImagePath = "images/navigation/bridge-to-piltover.jpg",
            CorrectChoiceIndex = 0, // Victory location
            SuccessMessage = "ESCAPE SUCCESSFUL - Welcome to Piltover! You've successfully navigated through the dangerous undercity!",
            Choices = Array.Empty<NavigationChoice>()
        }
    };
    
    public Dictionary<string, PlayerRole> Players { get; set; } = new();
    public Dictionary<string, string> PlayerNames { get; set; } = new();
    public int CurrentLocationId { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public bool IsGameOver { get; set; } = false;
    public string? GameOverMessage { get; set; }
    
    public int PlayerCount => Players.Count;
    public NavigationLocation CurrentLocation => LocationBank[CurrentLocationId];
    
    public PlayerRole? AddPlayer(string connectionId, string playerName)
    {
        if (Players.TryGetValue(connectionId, out var existingRole))
        {
            return existingRole;
        }
        
        if (Players.Count >= 2) return null;
        
        var role = Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite;
        Players[connectionId] = role;
        PlayerNames[connectionId] = playerName;
        return role;
    }
    
    public bool RemovePlayer(string connectionId)
    {
        var removed = Players.Remove(connectionId);
        PlayerNames.Remove(connectionId);
        return removed;
    }
    
    public List<string> GetConnectedPlayers()
    {
        return Players.Keys.ToList();
    }
    
    public (bool Success, string Message) MakeChoice(string connectionId, string choice)
    {
        if (IsCompleted)
            return (false, "Game is already completed");
            
        if (IsGameOver)
            return (false, "Game is over - restart to try again");
            
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");
            
        if (Players[connectionId] != PlayerRole.Zaunite)
            return (false, "Only the Zaunite player can make navigation choices");

        var location = CurrentLocation;
        choice = choice.Trim().ToUpper();
        
        // Find the matching choice
        var selectedChoice = location.Choices.FirstOrDefault(c => c.Direction.ToUpper() == choice);
        if (selectedChoice == null)
        {
            return (false, "Invalid choice - select one of the available options");
        }
        
        if (selectedChoice.IsCorrect)
        {
            // Correct choice - advance to next location
            CurrentLocationId++;
            
            if (CurrentLocationId >= LocationBank.Length - 1)
            {
                // Reached the final location - victory!
                CurrentLocationId = LocationBank.Length - 1; // Bridge to Piltover
                IsCompleted = true;
                return (true, LocationBank[CurrentLocationId].SuccessMessage);
            }
            else
            {
                return (true, location.SuccessMessage);
            }
        }
        else
        {
            // Wrong choice - game over
            IsGameOver = true;
            GameOverMessage = selectedChoice.GameOverMessage;
            return (true, selectedChoice.GameOverMessage);
        }
    }
    
    public NavigationPlayerView GetPlayerView(string connectionId)
    {
        if (!Players.TryGetValue(connectionId, out var role))
            return new NavigationPlayerView();

        if (role == PlayerRole.Piltover)
        {
            // Piltover player sees the map
            return new NavigationPlayerView
            {
                Role = "Piltover",
                DisplayName = "Caitlyn (Navigator)",
                Instruction = IsGameOver ? "Game Over! Guide your partner to restart." : 
                             IsCompleted ? "Mission Accomplished! You successfully guided Vi to safety!" :
                             "Guide Vi through the dangerous undercity using your map:",
                MapData = GetMapData(),
                IsGameOver = IsGameOver,
                GameOverMessage = GameOverMessage
            };
        }
        else
        {
            // Zaunite player sees first-person view
            var location = CurrentLocation;
            return new NavigationPlayerView
            {
                Role = "Zaunite",
                DisplayName = "Vi (Explorer)",
                Instruction = IsGameOver ? "You got caught! Ask your partner to restart the mission." :
                             IsCompleted ? "Success! You made it to Piltover safely!" :
                             "Choose your path based on Caitlyn's guidance:",
                LocationName = location.Name,
                LocationDescription = location.Description,
                LocationImage = location.ImagePath,
                AvailableChoices = IsCompleted ? Array.Empty<NavigationChoice>() : location.Choices,
                IsGameOver = IsGameOver,
                GameOverMessage = GameOverMessage
            };
        }
    }
    
    private NavigationMapData GetMapData()
    {
        return new NavigationMapData
        {
            CurrentLocationId = CurrentLocationId,
            Locations = LocationBank.Select((loc, index) => new NavigationMapLocation
            {
                LocationId = loc.LocationId,
                Name = loc.Name,
                X = GetLocationX(index),
                Y = GetLocationY(index),
                IsCurrentLocation = index == CurrentLocationId,
                IsCompleted = index < CurrentLocationId || IsCompleted
            }).ToArray(),
            CorrectPath = GetCorrectPathData()
        };
    }
    
    private NavigationMapPath[] GetCorrectPathData()
    {
        var paths = new List<NavigationMapPath>();
        
        // Define the correct path through all locations
        for (int i = 0; i < LocationBank.Length - 1; i++)
        {
            var location = LocationBank[i];
            var correctChoice = location.Choices[location.CorrectChoiceIndex];
            
            paths.Add(new NavigationMapPath
            {
                FromLocationId = i,
                ToLocationId = i + 1,
                Direction = correctChoice.Direction
            });
        }
        
        return paths.ToArray();
    }
    
    // Position locations on the map (as percentages for responsive design)
    private float GetLocationX(int locationIndex)
    {
        return locationIndex switch
        {
            0 => 20f,  // Sewer Entrance - left side
            1 => 35f,  // Industrial Pipes - moving right and down
            2 => 50f,  // Chemical Plant - center
            3 => 65f,  // Underground Market - right side
            4 => 80f,  // Bridge to Piltover - far right
            _ => 50f
        };
    }
    
    private float GetLocationY(int locationIndex)
    {
        return locationIndex switch
        {
            0 => 80f,  // Sewer Entrance - bottom (underground)
            1 => 65f,  // Industrial Pipes - moving up
            2 => 50f,  // Chemical Plant - middle level
            3 => 35f,  // Underground Market - higher up
            4 => 20f,  // Bridge to Piltover - top (surface level)
            _ => 50f
        };
    }
    
    public NavigationGameState GetGameState()
    {
        return new NavigationGameState
        {
            CurrentLocationId = CurrentLocationId,
            TotalLocations = LocationBank.Length,
            IsCompleted = IsCompleted,
            IsGameOver = IsGameOver,
            PlayerCount = Players.Count,
            PlayersNeeded = 2 - Players.Count,
            GameOverMessage = GameOverMessage
        };
    }
    
    public void Reset()
    {
        CurrentLocationId = 0;
        IsCompleted = false;
        IsGameOver = false;
        GameOverMessage = null;
    }
}

public class RuneProtocolGame
{
    public enum PlayerRole { Piltover, Zaunite }
    
    // Multi-level logic puzzle system with your specific puzzle
    private static readonly Arcane_Coop.Models.RuneProtocolLevel[] LevelBank = new[]
    {
        new Arcane_Coop.Models.RuneProtocolLevel
        {
            LevelNumber = 1,
            Title = "Logic Gateway Protocol - Alpha",
            Description = "The first barrier requires precise logical coordination. Each team must satisfy their own conditions while working together.",
            // Player A (Piltover) controls R1-R4 (indices 0-3)
            AlphaRules = new[]
            {
                new Arcane_Coop.Models.LogicRule("A1", Arcane_Coop.Models.RuleType.COUNT_EXACT, 
                    "Exactly three of your runes (R1-R4) must be active", new[] { 0, 1, 2, 3 }) 
                    { RequiredCount = 3 },
                new Arcane_Coop.Models.LogicRule("A2", Arcane_Coop.Models.RuleType.CONDITIONAL_IF,
                    "IF R5 is active, THEN R1 must be inactive", new[] { 4, 0 })
                    { RequiredStates = new[] { 1, 0 } }, // IF R5=UP THEN R1=DOWN
                new Arcane_Coop.Models.LogicRule("A3", Arcane_Coop.Models.RuleType.CONDITIONAL_IF,
                    "IF R7 is inactive, THEN R2 and R4 must match", new[] { 6, 1, 3 })
                    { RequiredStates = new[] { 0, -1, -1 } } // Special handling for "same state"
            },
            // Player B (Zaunite) controls R5-R8 (indices 4-7)
            BetaRules = new[]
            {
                new Arcane_Coop.Models.LogicRule("B1", Arcane_Coop.Models.RuleType.COUNT_EXACT,
                    "Exactly one of your runes (R5-R8) must be active", new[] { 4, 5, 6, 7 })
                    { RequiredCount = 1 },
                new Arcane_Coop.Models.LogicRule("B2", Arcane_Coop.Models.RuleType.EITHER_OR,
                    "Either R5 or R7 must be active (but not both)", new[] { 4, 6 }),
                new Arcane_Coop.Models.LogicRule("B3", Arcane_Coop.Models.RuleType.CONDITIONAL_IF,
                    "IF R1 is inactive, THEN R5 must be active", new[] { 0, 4 })
                    { RequiredStates = new[] { 0, 1 } } // IF R1=DOWN THEN R5=UP
            },
            // The unique solution: R1=DOWN, R2=UP, R3=UP, R4=UP, R5=UP, R6=DOWN, R7=DOWN, R8=DOWN
            Solution = new[] { false, true, true, true, true, false, false, false },
            SolutionExplanation = "R5 must be active (only Beta rune UP), which forces R1 inactive and the remaining Alpha runes (R2,R3,R4) active to satisfy the count rule."
        },
        // Advanced Level 2 - Complex interdependent logic puzzle
        new Arcane_Coop.Models.RuneProtocolLevel
        {
            LevelNumber = 2,
            Title = "Logic Gateway Protocol - Beta",
            Description = "Advanced interdependent logic requiring deep deduction. Hidden cascades of forced moves await discovery.",
            AlphaRules = new[]
            {
                new Arcane_Coop.Models.LogicRule("A1", Arcane_Coop.Models.RuleType.CONDITIONAL_IF,
                    "IF R5 and R6 are different positions, THEN R1 must equal R7", new[] { 4, 5, 6, 0 })
                    { RequiredStates = new[] { -2, -2, 6, 0 } }, // Special: -2 means "different check", 6=R7, 0=R1
                new Arcane_Coop.Models.LogicRule("A2", Arcane_Coop.Models.RuleType.CONDITIONAL_IF,
                    "IF R3 is active, THEN exactly 5 total runes must be active", new[] { 2 })
                    { RequiredStates = new[] { 1 }, RequiredCount = 5 }, // Special: global count check
                new Arcane_Coop.Models.LogicRule("A3", Arcane_Coop.Models.RuleType.CONDITIONAL_IF,
                    "Alpha UP count must equal Beta DOWN count", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })
                    { RequiredStates = new[] { -3 } } // Special: -3 means "alpha UP = beta DOWN"
            },
            BetaRules = new[]
            {
                new Arcane_Coop.Models.LogicRule("B1", Arcane_Coop.Models.RuleType.CONDITIONAL_IF,
                    "IF R1 equals R4, THEN R6 must be active", new[] { 0, 3, 5 })
                    { RequiredStates = new[] { -4, -4, 1 } }, // Special: -4 means "same state check"
                new Arcane_Coop.Models.LogicRule("B2", Arcane_Coop.Models.RuleType.CONDITIONAL_IF,
                    "IF R8 is inactive, THEN R2 and R3 must be different", new[] { 7, 1, 2 })
                    { RequiredStates = new[] { 0, -2, -2 } }, // Special: IF R8=DOWN THEN R2≠R3
                new Arcane_Coop.Models.LogicRule("B3", Arcane_Coop.Models.RuleType.CONDITIONAL_IF,
                    "Either R5 is active OR (R7 is inactive AND R4 is active)", new[] { 4, 6, 3 })
                    { RequiredStates = new[] { -5 } } // Special: -5 means "complex OR condition"
            },
            // The unique solution: R1=DOWN, R2=UP, R3=DOWN, R4=UP, R5=UP, R6=UP, R7=DOWN, R8=DOWN
            Solution = new[] { false, true, false, true, true, true, false, false },
            SolutionExplanation = "R3 must be DOWN (prevents impossible 5-UP requirement), forcing a cascade: Alpha UP = Beta DOWN = 2, leading to the unique solution through Beta Rule 3."
        }
    };
    
    public Dictionary<string, PlayerRole> Players { get; set; } = new();
    public Dictionary<string, string> PlayerNames { get; set; } = new();
    public bool[] RuneStates { get; set; } = new bool[8]; // R1-R8 states
    public int CurrentLevel { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public int Score { get; set; } = 0;
    public List<string> AttemptHistory { get; set; } = new();
    public int ToggleCount { get; set; } = 0; // Track number of rune toggles for scoring
    public bool ShowValidationStates { get; set; } = false; // Toggle for validation feedback
    
    public int PlayerCount => Players.Count;
    public Arcane_Coop.Models.RuneProtocolLevel CurrentLevelData => LevelBank[CurrentLevel];
    
    public PlayerRole? AddPlayer(string connectionId, string playerName)
    {
        if (Players.TryGetValue(connectionId, out var existingRole))
        {
            return existingRole;
        }
        
        if (Players.Count >= 2) return null;
        
        var role = Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite;
        Players[connectionId] = role;
        PlayerNames[connectionId] = playerName;
        return role;
    }

    public bool RemovePlayer(string connectionId)
    {
        var removed = Players.Remove(connectionId);
        PlayerNames.Remove(connectionId);
        return removed;
    }

    public List<string> GetConnectedPlayers()
    {
        return Players.Keys.ToList();
    }

    public (bool Success, string Message) ToggleRune(string connectionId, int runeIndex)
    {
        Console.WriteLine($"[DEBUG] RuneProtocolGame.ToggleRune - ConnectionId: {connectionId}, RuneIndex: {runeIndex}");
        
        if (!Players.ContainsKey(connectionId))
        {
            return (false, "You are not in this game");
        }
            
        if (runeIndex < 0 || runeIndex > 7)
        {
            return (false, $"Invalid rune index: {runeIndex} (must be 0-7)");
        }
            
        var role = Players[connectionId];
        
        // Check if player can control this rune (Piltover: R1-R4, Zaunite: R5-R8)
        if ((role == PlayerRole.Piltover && runeIndex > 3) || 
            (role == PlayerRole.Zaunite && runeIndex < 4))
        {
            return (false, $"You can only control your assigned runes ({(role == PlayerRole.Piltover ? "R1-R4" : "R5-R8")})");
        }
            
        // Toggle the rune
        RuneStates[runeIndex] = !RuneStates[runeIndex];
        ToggleCount++;
        
        var toggleMessage = $"R{runeIndex + 1} toggled to {(RuneStates[runeIndex] ? "ON" : "OFF")}";
        
        // Check if puzzle is solved by validating all rules are satisfied
        var (alphaSatisfied, alphaTotal, _) = ValidatePlayerRules(PlayerRole.Piltover);
        var (betaSatisfied, betaTotal, _) = ValidatePlayerRules(PlayerRole.Zaunite);
        
        var allRulesSatisfied = (alphaSatisfied == alphaTotal) && (betaSatisfied == betaTotal) && 
                               (alphaTotal > 0) && (betaTotal > 0); // Ensure both players have rules
        
        if (allRulesSatisfied)
        {
            IsCompleted = true;
            Score = Math.Max(100 - (ToggleCount - 8) * 5, 10); // Bonus for efficiency
            var level = CurrentLevelData;
            return (true, $"{toggleMessage} | 🎉 PUZZLE SOLVED! All rules satisfied! Score: {Score}");
        }
        
        return (true, toggleMessage);
    }

    public (bool Success, string Message) AdvanceLevel(string connectionId)
    {
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");
            
        if (!IsCompleted)
            return (false, "Complete current level first");
            
        if (CurrentLevel >= LevelBank.Length - 1)
            return (false, "Already at maximum level");
            
        CurrentLevel++;
        IsCompleted = false;
        RuneStates = new bool[8];
        AttemptHistory.Clear();
        ToggleCount = 0;
        
        return (true, $"Advanced to Level {CurrentLevel + 1}: {CurrentLevelData.Title}");
    }

    public (bool Success, string Message) ToggleValidationHints(string connectionId)
    {
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");

        ShowValidationStates = !ShowValidationStates;
        
        var statusMessage = ShowValidationStates 
            ? "🔍 Validation hints ENABLED - You can see rule status" 
            : "🎯 Validation hints DISABLED - Pure puzzle mode";
            
        return (true, statusMessage);
    }

    private (int satisfied, int total, string[] messages) ValidatePlayerRules(PlayerRole role)
    {
        var level = CurrentLevelData;
        var rules = role == PlayerRole.Piltover ? level.AlphaRules : level.BetaRules;
        var messages = new List<string>();
        int satisfied = 0;

        foreach (var rule in rules)
        {
            var result = ValidateSpecialRule(rule, RuneStates);
            if (result.IsValid) satisfied++;
            messages.Add($"{rule.Id}: {result.Message}");
        }

        return (satisfied, rules.Length, messages.ToArray());
    }

    private Arcane_Coop.Models.RuleValidationResult ValidateSpecialRule(Arcane_Coop.Models.LogicRule rule, bool[] runeStates)
    {
        var result = new Arcane_Coop.Models.RuleValidationResult();
        
        // Level 1 special rules
        if (rule.Id == "A3" && CurrentLevel == 0)
        {
            var r7State = runeStates[6]; // R7 (index 6)
            
            if (!r7State) // R7 is DOWN
            {
                var r2State = runeStates[1]; // R2 (index 1)
                var r4State = runeStates[3]; // R4 (index 3)
                result.IsValid = r2State == r4State;
                result.Message = result.IsValid
                    ? "✓ R7 is inactive, and R2 and R4 match"
                    : "✗ R7 is inactive, so R2 and R4 must be in the same state";
            }
            else
            {
                result.IsValid = true;
                result.Message = "✓ R7 is active, condition not triggered";
            }
            result.AffectedRunes = new[] { 6, 1, 3 };
            return result;
        }
        
        // Level 2 advanced rules
        if (CurrentLevel == 1) // Level 2 (0-indexed)
        {
            switch (rule.Id)
            {
                case "A1": // IF R5 and R6 are different positions, THEN R1 must equal R7
                    var r5StateA1 = runeStates[4];
                    var r6StateA1 = runeStates[5];
                    if (r5StateA1 != r6StateA1) // Different positions
                    {
                        var r1StateA1 = runeStates[0];
                        var r7StateA1 = runeStates[6];
                        result.IsValid = r1StateA1 == r7StateA1;
                        result.Message = result.IsValid
                            ? "✓ R5≠R6, and R1 equals R7"
                            : "✗ R5≠R6, so R1 must equal R7";
                    }
                    else
                    {
                        result.IsValid = true;
                        result.Message = "✓ R5=R6, condition not triggered";
                    }
                    result.AffectedRunes = new[] { 4, 5, 0, 6 };
                    return result;
                    
                case "A2": // IF R3 is active, THEN exactly 5 total runes must be active
                    var r3StateA2 = runeStates[2];
                    if (r3StateA2) // R3 is UP
                    {
                        var totalUpA2 = runeStates.Count(s => s);
                        result.IsValid = totalUpA2 == 5;
                        result.Message = result.IsValid
                            ? "✓ R3 is active, and exactly 5 total runes are active"
                            : $"✗ R3 is active, so exactly 5 total must be active (currently {totalUpA2})";
                    }
                    else
                    {
                        result.IsValid = true;
                        result.Message = "✓ R3 is inactive, condition not triggered";
                    }
                    result.AffectedRunes = new[] { 2 };
                    return result;
                    
                case "A3": // Alpha UP count must equal Beta DOWN count
                    var alphaUpA3 = runeStates.Take(4).Count(s => s); // R1-R4
                    var betaDownA3 = runeStates.Skip(4).Take(4).Count(s => !s); // R5-R8 DOWN
                    result.IsValid = alphaUpA3 == betaDownA3;
                    result.Message = result.IsValid
                        ? $"✓ Alpha UP count ({alphaUpA3}) equals Beta DOWN count ({betaDownA3})"
                        : $"✗ Alpha UP count ({alphaUpA3}) must equal Beta DOWN count ({betaDownA3})";
                    result.AffectedRunes = new[] { 0, 1, 2, 3, 4, 5, 6, 7 };
                    return result;
                    
                case "B1": // IF R1 equals R4, THEN R6 must be active
                    var r1StateB1 = runeStates[0];
                    var r4StateB1 = runeStates[3];
                    if (r1StateB1 == r4StateB1) // Same state
                    {
                        var r6StateB1 = runeStates[5];
                        result.IsValid = r6StateB1;
                        result.Message = result.IsValid
                            ? "✓ R1=R4, and R6 is active"
                            : "✗ R1=R4, so R6 must be active";
                    }
                    else
                    {
                        result.IsValid = true;
                        result.Message = "✓ R1≠R4, condition not triggered";
                    }
                    result.AffectedRunes = new[] { 0, 3, 5 };
                    return result;
                    
                case "B2": // IF R8 is inactive, THEN R2 and R3 must be different
                    var r8StateB2 = runeStates[7];
                    if (!r8StateB2) // R8 is DOWN
                    {
                        var r2StateB2 = runeStates[1];
                        var r3StateB2 = runeStates[2];
                        result.IsValid = r2StateB2 != r3StateB2;
                        result.Message = result.IsValid
                            ? "✓ R8 is inactive, and R2≠R3"
                            : "✗ R8 is inactive, so R2 and R3 must be different";
                    }
                    else
                    {
                        result.IsValid = true;
                        result.Message = "✓ R8 is active, condition not triggered";
                    }
                    result.AffectedRunes = new[] { 7, 1, 2 };
                    return result;
                    
                case "B3": // Either R5 is active OR (R7 is inactive AND R4 is active)
                    var r5StateB3 = runeStates[4];
                    var r7StateB3 = runeStates[6];
                    var r4StateB3 = runeStates[3];
                    
                    var condition1B3 = r5StateB3; // R5 is UP
                    var condition2B3 = !r7StateB3 && r4StateB3; // R7 is DOWN AND R4 is UP
                    result.IsValid = condition1B3 || condition2B3;
                    
                    if (result.IsValid)
                    {
                        if (condition1B3)
                            result.Message = "✓ R5 is active (satisfies OR condition)";
                        else
                            result.Message = "✓ R7 is inactive AND R4 is active (satisfies OR condition)";
                    }
                    else
                    {
                        result.Message = "✗ Either R5 must be active OR (R7 inactive AND R4 active)";
                    }
                    result.AffectedRunes = new[] { 4, 6, 3 };
                    return result;
            }
        }
        
        // Use the standard validator for other rules
        return Arcane_Coop.Models.LogicPuzzleSolver.ValidateRule(rule, runeStates);
    }

    public Arcane_Coop.Models.RuneProtocolPlayerView GetPlayerView(string connectionId)
    {
        if (!Players.TryGetValue(connectionId, out var role))
            return new Arcane_Coop.Models.RuneProtocolPlayerView();

        var level = CurrentLevelData;
        var (satisfied, total, messages) = ValidatePlayerRules(role);

        if (role == PlayerRole.Piltover)
        {
            return new Arcane_Coop.Models.RuneProtocolPlayerView
            {
                Role = "Piltover",
                DisplayName = "Caitlyn (Alpha Protocol Engineer)",
                Instruction = IsCompleted ? "🎉 Protocol synchronized! All conditions satisfied." : 
                             "Satisfy all Alpha conditions using your runes (R1-R4). Coordinate with Vi!",
                Rules = level.AlphaRules,
                ControllableRunes = new[] { 0, 1, 2, 3 }, // R1-R4
                RuneStates = RuneStates,
                SatisfiedRules = satisfied,
                TotalRules = total,
                IsCompleted = IsCompleted,
                Score = Score,
                CurrentLevel = CurrentLevel + 1,
                MaxLevel = LevelBank.Length,
                RuleValidationMessages = messages,
                ShowValidationStates = ShowValidationStates
            };
        }
        else
        {
            return new Arcane_Coop.Models.RuneProtocolPlayerView
            {
                Role = "Zaunite",
                DisplayName = "Vi (Beta Protocol Analyst)",
                Instruction = IsCompleted ? "🎉 Protocol synchronized! All conditions satisfied." :
                             "Satisfy all Beta conditions using your runes (R5-R8). Coordinate with Caitlyn!",
                Rules = level.BetaRules,
                ControllableRunes = new[] { 4, 5, 6, 7 }, // R5-R8
                RuneStates = RuneStates,
                SatisfiedRules = satisfied,
                TotalRules = total,
                IsCompleted = IsCompleted,
                Score = Score,
                CurrentLevel = CurrentLevel + 1,
                MaxLevel = LevelBank.Length,
                RuleValidationMessages = messages,
                ShowValidationStates = ShowValidationStates
            };
        }
    }

    public Arcane_Coop.Models.RuneProtocolGameState GetGameState()
    {
        var level = CurrentLevelData;
        var (alphaSatisfied, alphaTotal, _) = ValidatePlayerRules(PlayerRole.Piltover);
        var (betaSatisfied, betaTotal, _) = ValidatePlayerRules(PlayerRole.Zaunite);
        
        var totalSatisfied = alphaSatisfied + betaSatisfied;
        var totalRules = alphaTotal + betaTotal;
        var allSatisfied = totalSatisfied == totalRules;
        
        return new Arcane_Coop.Models.RuneProtocolGameState
        {
            RuneStates = RuneStates,
            CurrentLevel = CurrentLevel + 1,
            MaxLevel = LevelBank.Length,
            IsCompleted = IsCompleted,
            Score = Score,
            PlayerCount = Players.Count,
            PlayersNeeded = 2 - Players.Count,
            SatisfiedRules = totalSatisfied,
            TotalRules = totalRules,
            LevelTitle = level.Title,
            LevelDescription = level.Description,
            AllRulesSatisfied = allSatisfied,
            CompletionMessage = IsCompleted ? $"🎉 {level.SolutionExplanation}" : "",
            ShowValidationStates = ShowValidationStates
        };
    }

    public void Reset()
    {
        RuneStates = new bool[8];
        CurrentLevel = 0;
        IsCompleted = false;
        Score = 0;
        AttemptHistory.Clear();
        ToggleCount = 0;
    }
}

// Helper class for array comparison
public static class Arrays
{
    public static bool Equals<T>(T[] a, T[] b) where T : IEquatable<T>
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (!a[i].Equals(b[i])) return false;
        }
        return true;
    }
}

// Picture Explanation Game Implementation
public class PictureExplanationGame
{
    public enum PlayerRole { Piltover, Zaunite }
    
    // Picture bank with character images for demo
    private static readonly List<PictureData> PictureBank = new()
    {
        new PictureData
        {
            ImageUrl = "/images/Viktor.jpeg",
            CharacterName = "Viktor",
            Title = "Viktor - The Machine Herald",
            Category = "Scientist",
            DistractorImages = new() { "/images/Jayce.jpeg", "/images/Vi.jpeg", "/images/Cait.jpeg" },
            Description = "Pale scientist with mechanical augmentations, amber eyes, and dark hair",
            StoryContext = "Former Hextech partner, now pursuing dangerous evolution research"
        },
        new PictureData
        {
            ImageUrl = "/images/Jayce.jpeg",
            CharacterName = "Jayce",
            Title = "Jayce Talis",
            Category = "Scientist",
            DistractorImages = new() { "/images/Viktor.jpeg", "/images/Vi.jpeg", "/images/Cait.jpeg" },
            Description = "Tall inventor with strong build, dark hair, and confident demeanor",
            StoryContext = "Hextech inventor and Piltover councilor, defender of progress"
        },
        new PictureData
        {
            ImageUrl = "/images/Vi.jpeg",
            CharacterName = "Vi",
            Title = "Vi - The Enforcer",
            Category = "Enforcer",
            DistractorImages = new() { "/images/Cait.jpeg", "/images/Jayce.jpeg", "/images/Viktor.jpeg" },
            Description = "Pink-haired fighter with tattoos, gauntlets, and fierce expression",
            StoryContext = "Former Zaun undercity resident, now working with Piltover enforcers"
        },
        new PictureData
        {
            ImageUrl = "/images/Cait.jpeg",
            CharacterName = "Caitlyn",
            Title = "Caitlyn Kiramman",
            Category = "Enforcer",
            DistractorImages = new() { "/images/Vi.jpeg", "/images/Jayce.jpeg", "/images/Viktor.jpeg" },
            Description = "Blue-haired sharpshooter with aristocratic bearing and rifle",
            StoryContext = "Elite enforcer from noble family, partner to Vi"
        }
    };

    private readonly Dictionary<string, PlayerRole> Players = new();
    private readonly Dictionary<string, string> PlayerNames = new();
    
    public int CurrentRound { get; private set; } = 1;
    public int TotalRounds { get; private set; } = 4;
    public int Score { get; private set; } = 0;
    public bool IsGameCompleted { get; private set; } = false;
    
    private PictureData? CurrentPicture;
    private List<string> CurrentChoices = new();
    private int CorrectChoiceIndex = -1;
    private bool DescriptionFinished = false; // Piltover player pressed "Finished Describing"
    private bool ImageVisible = true; // Whether Piltover player can see the image
    private int? SubmittedChoice;
    private bool RoundComplete = false;
    private PictureRoundResult? LastRoundResult;

    public class GameActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public bool RoundComplete { get; set; } = false;
    }

    public string AddPlayer(string connectionId, string playerName)
    {
        if (Players.Count >= 2)
            return "";

        var role = Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite;
        Players[connectionId] = role;
        PlayerNames[connectionId] = playerName;
        
        if (Players.Count == 2)
        {
            StartNewRound();
        }
        
        return role == PlayerRole.Piltover ? "Piltover" : "Zaunite";
    }

    public string AddPlayer(string connectionId, string playerName, string? requestedRole)
    {
        if (Players.Count >= 2)
            return "";

        PlayerRole? desired = null;
        if (!string.IsNullOrWhiteSpace(requestedRole))
        {
            var r = requestedRole.Trim().ToLower();
            if (r == "piltover") desired = PlayerRole.Piltover;
            else if (r == "zaun" || r == "zaunite") desired = PlayerRole.Zaunite;
        }

        PlayerRole assigned;
        if (desired.HasValue)
        {
            // If desired role is taken, assign the other one; otherwise assign desired
            bool piltoverTaken = Players.Values.Contains(PlayerRole.Piltover);
            bool zauniteTaken = Players.Values.Contains(PlayerRole.Zaunite);
            if (desired.Value == PlayerRole.Piltover)
            {
                assigned = piltoverTaken ? PlayerRole.Zaunite : PlayerRole.Piltover;
            }
            else
            {
                assigned = zauniteTaken ? PlayerRole.Piltover : PlayerRole.Zaunite;
            }
        }
        else
        {
            // Fallback to default assignment order
            assigned = Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite;
        }

        Players[connectionId] = assigned;
        PlayerNames[connectionId] = playerName;

        if (Players.Count == 2)
        {
            StartNewRound();
        }

        return assigned == PlayerRole.Piltover ? "Piltover" : "Zaunite";
    }

    public bool RemovePlayer(string connectionId)
    {
        var removed = Players.Remove(connectionId);
        PlayerNames.Remove(connectionId);
        return removed;
    }

    public List<string> GetConnectedPlayers()
    {
        return Players.Keys.ToList();
    }

    private void StartNewRound()
    {
        if (CurrentRound > TotalRounds)
        {
            IsGameCompleted = true;
            return;
        }

        // For demo: cycle through the 4 characters in order
        CurrentPicture = PictureBank[(CurrentRound - 1) % PictureBank.Count];
        
        // Create shuffled choices (correct + 3 distractors)
        CurrentChoices = new List<string> { CurrentPicture.ImageUrl };
        CurrentChoices.AddRange(CurrentPicture.DistractorImages);
        
        // Shuffle the choices
        var random = new Random();
        for (int i = CurrentChoices.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (CurrentChoices[i], CurrentChoices[j]) = (CurrentChoices[j], CurrentChoices[i]);
        }
        
        // Find correct answer index after shuffling
        CorrectChoiceIndex = CurrentChoices.IndexOf(CurrentPicture.ImageUrl);
        
        // Reset round state
        DescriptionFinished = false;
        ImageVisible = true; // Image is visible at start of round
        SubmittedChoice = null;
        RoundComplete = false;
        LastRoundResult = null;
    }

    public GameActionResult FinishDescribing(string connectionId)
    {
        if (!Players.ContainsKey(connectionId))
            return new GameActionResult { Success = false, Message = "Player not in game" };
            
        if (Players[connectionId] != PlayerRole.Piltover)
            return new GameActionResult { Success = false, Message = "Only Piltover player can finish describing" };
            
        if (DescriptionFinished)
            return new GameActionResult { Success = false, Message = "Description already finished for this round" };

        DescriptionFinished = true;
        ImageVisible = false; // Hide the image once description is finished
        
        return new GameActionResult { Success = true, Message = "Description finished - image hidden" };
    }

    public GameActionResult SubmitChoice(string connectionId, int choiceIndex)
    {
        if (!Players.ContainsKey(connectionId))
            return new GameActionResult { Success = false, Message = "Player not in game" };
            
        if (Players[connectionId] != PlayerRole.Zaunite)
            return new GameActionResult { Success = false, Message = "Only Zaunite player can choose" };
            
        if (!DescriptionFinished)
            return new GameActionResult { Success = false, Message = "Wait for partner to finish describing" };
            
        if (choiceIndex < 0 || choiceIndex >= CurrentChoices.Count)
            return new GameActionResult { Success = false, Message = "Invalid choice index" };
            
        if (SubmittedChoice.HasValue)
            return new GameActionResult { Success = false, Message = "Choice already submitted for this round" };

        SubmittedChoice = choiceIndex;
        bool isCorrect = choiceIndex == CorrectChoiceIndex;
        int pointsEarned = isCorrect ? 10 : 0;
        
        if (isCorrect)
        {
            Score += pointsEarned;
        }

        LastRoundResult = new PictureRoundResult
        {
            IsCorrect = isCorrect,
            CorrectImageUrl = CurrentPicture?.ImageUrl ?? "",
            Description = "", // No written description in voice chat mode
            PointsEarned = pointsEarned,
            ResultMessage = isCorrect ? "🎉 Correct! Great teamwork!" : "❌ Incorrect. Better luck next round!"
        };

        RoundComplete = true;

        return new GameActionResult 
        { 
            Success = true, 
            Message = isCorrect ? "Correct answer!" : "Incorrect answer",
            RoundComplete = true
        };
    }

    public GameActionResult NextRound()
    {
        if (!RoundComplete)
            return new GameActionResult { Success = false, Message = "Current round not complete" };

        CurrentRound++;
        
        if (CurrentRound <= TotalRounds)
        {
            StartNewRound();
            return new GameActionResult { Success = true, Message = $"Starting round {CurrentRound}" };
        }
        else
        {
            IsGameCompleted = true;
            return new GameActionResult { Success = true, Message = "Game completed!" };
        }
    }

    public PictureExplanationPlayerView GetPlayerView(string connectionId)
    {
        if (!Players.ContainsKey(connectionId))
            return new PictureExplanationPlayerView();

        var role = Players[connectionId];
        var playerName = PlayerNames.GetValueOrDefault(connectionId, "Unknown");
        
        return new PictureExplanationPlayerView
        {
            Role = role == PlayerRole.Piltover ? "Piltover" : "Zaunite",
            DisplayName = playerName,
            CurrentImageUrl = role == PlayerRole.Piltover && ImageVisible ? (CurrentPicture?.ImageUrl ?? "") : "",
            ChoiceImages = role == PlayerRole.Zaunite && DescriptionFinished ? CurrentChoices : new(),
            CanFinishDescribing = role == PlayerRole.Piltover && ImageVisible && !DescriptionFinished && !RoundComplete,
            CanChoose = role == PlayerRole.Zaunite && DescriptionFinished && !SubmittedChoice.HasValue && !RoundComplete,
            SelectedChoice = SubmittedChoice,
            RoundCompleted = RoundComplete,
            RoundResult = LastRoundResult?.ResultMessage ?? "",
            Score = Score,
            ImageVisible = ImageVisible
        };
    }

    public PictureExplanationGameState GetGameState()
    {
        return new PictureExplanationGameState
        {
            CurrentRound = CurrentRound,
            TotalRounds = TotalRounds,
            Score = Score,
            IsCompleted = IsGameCompleted,
            GameStatus = IsGameCompleted ? "Completed" : (Players.Count < 2 ? "Waiting for players" : "In Progress"),
            PlayerCount = Players.Count,
            RoundStatus = GetRoundStatus(),
            ShowingResult = RoundComplete
        };
    }

    private string GetRoundStatus()
    {
        if (IsGameCompleted)
            return "Game Complete";
        if (Players.Count < 2)
            return "Waiting for players";
        if (!DescriptionFinished)
            return "Describing in progress (voice chat)";
        if (!SubmittedChoice.HasValue)
            return "Waiting for choice";
        if (RoundComplete)
            return "Round complete - ready for next";
        return "In progress";
    }

    public PictureRoundResult? GetLastRoundResult()
    {
        return LastRoundResult;
    }

    public int GetScore()
    {
        return Score;
    }

    public bool IsCompleted()
    {
        return IsGameCompleted;
    }

    public void Reset()
    {
        Players.Clear();
        PlayerNames.Clear();
        CurrentRound = 1;
        Score = 0;
        IsGameCompleted = false;
        CurrentPicture = null;
        CurrentChoices.Clear();
        CorrectChoiceIndex = -1;
        DescriptionFinished = false;
        ImageVisible = true;
        SubmittedChoice = null;
        RoundComplete = false;
        LastRoundResult = null;
    }
}

public class WordForgeGame
{
    public enum PlayerRole { Piltover, Zaunite }
    
    // A2-B2 Level Word Bank - 5 combinations
    private static readonly WordElement[] RootBank = new[]
    {
        new WordElement { Id = "rely", Text = "rely", Description = "to depend on", Type = WordElementType.Root },
        new WordElement { Id = "help", Text = "help", Description = "to assist", Type = WordElementType.Root },
        new WordElement { Id = "care", Text = "care", Description = "to look after", Type = WordElementType.Root },
        new WordElement { Id = "use", Text = "use", Description = "to utilize", Type = WordElementType.Root },
        new WordElement { Id = "read", Text = "read", Description = "to look at text", Type = WordElementType.Root }
    };
    
    private static readonly WordElement[] AffixBank = new[]
    {
        new WordElement { Id = "able", Text = "-able", Description = "capable of being", Type = WordElementType.Suffix },
        new WordElement { Id = "er", Text = "-er", Description = "one who does", Type = WordElementType.Suffix },
        new WordElement { Id = "ful", Text = "-ful", Description = "full of", Type = WordElementType.Suffix },
        new WordElement { Id = "re", Text = "re-", Description = "again", Type = WordElementType.Prefix },
        new WordElement { Id = "un", Text = "un-", Description = "not", Type = WordElementType.Prefix }
    };
    
    private static readonly WordCombination[] TargetCombinations = new[]
    {
        new WordCombination { Id = "reliable", RootId = "rely", AffixId = "able", ResultWord = "reliable", Definition = "can be trusted or depended on", Order = 1 },
        new WordCombination { Id = "helper", RootId = "help", AffixId = "er", ResultWord = "helper", Definition = "a person who helps", Order = 2 },
        new WordCombination { Id = "careful", RootId = "care", AffixId = "ful", ResultWord = "careful", Definition = "taking care to avoid mistakes", Order = 3 },
        new WordCombination { Id = "reuse", RootId = "use", AffixId = "re", ResultWord = "reuse", Definition = "to use again", Order = 4 },
        new WordCombination { Id = "unreadable", RootId = "read", AffixId = "un", ResultWord = "unreadable", Definition = "not able to be read", Order = 5 }
    };

    public Dictionary<string, PlayerRole> Players { get; set; } = new();
    public Dictionary<string, string> PlayerNames { get; set; } = new();
    public List<WordCombination> CompletedCombinations { get; set; } = new();
    public AnvilSlot CurrentAnvil { get; set; } = new();
    public GameMode Mode { get; set; } = GameMode.Assisted;
    public bool IsGameCompleted { get; set; } = false;
    
    // Instance copy of target combinations for this game
    public WordCombination[] GameTargetCombinations { get; set; }

    public WordForgeGame()
    {
        // Create fresh copy of target combinations for this game instance
        GameTargetCombinations = TargetCombinations.Select(tc => new WordCombination
        {
            Id = tc.Id,
            RootId = tc.RootId,
            AffixId = tc.AffixId,
            ResultWord = tc.ResultWord,
            Definition = tc.Definition,
            Order = tc.Order,
            IsCompleted = false // Start fresh
        }).ToArray();
    }

    public string? AddPlayer(string connectionId, string playerName, GameMode mode)
    {
        if (Players.Count >= 2)
            return null;

        var role = Players.Count == 0 ? PlayerRole.Piltover : PlayerRole.Zaunite;
        Players[connectionId] = role;
        PlayerNames[connectionId] = playerName;
        Mode = mode;

        return role.ToString();
    }

    public (bool Success, string Message) PlaceElement(string connectionId, string elementId, string slotType)
    {
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");

        var playerRole = Players[connectionId];
        
        // Validate element belongs to player
        var availableElements = GetPlayerElements(playerRole);
        var element = availableElements.FirstOrDefault(e => e.Id == elementId && !e.IsUsed);
        
        if (element == null)
            return (false, "Element not found or already used");

        // Place element on anvil
        if (slotType == "root" && element.Type == WordElementType.Root)
        {
            if (CurrentAnvil.RootElement != null)
                return (false, "Root slot already occupied");
            CurrentAnvil.RootElement = element;
        }
        else if (slotType == "affix" && (element.Type == WordElementType.Prefix || element.Type == WordElementType.Suffix))
        {
            if (CurrentAnvil.AffixElement != null)
                return (false, "Affix slot already occupied");
            CurrentAnvil.AffixElement = element;
        }
        else
        {
            return (false, "Invalid element for this slot");
        }

        return (true, "Element placed on anvil");
    }

    public (bool Success, string Message) RemoveElement(string connectionId, string elementId, string slotType)
    {
        if (!Players.ContainsKey(connectionId))
            return (false, "You are not in this game");

        var playerRole = Players[connectionId];
        
        // Validate player can only remove their own element types
        if (slotType == "root" && playerRole != PlayerRole.Piltover)
            return (false, "Only Piltover players can remove root elements");
        
        if (slotType == "affix" && playerRole != PlayerRole.Zaunite)
            return (false, "Only Zaunite players can remove affix elements");
        
        // Remove element from anvil
        if (slotType == "root" && CurrentAnvil.RootElement != null && CurrentAnvil.RootElement.Id == elementId)
        {
            CurrentAnvil.RootElement = null;
            return (true, "Root element removed from anvil");
        }
        else if (slotType == "affix" && CurrentAnvil.AffixElement != null && CurrentAnvil.AffixElement.Id == elementId)
        {
            CurrentAnvil.AffixElement = null;
            return (true, "Affix element removed from anvil");
        }
        
        return (false, "Element not found on anvil or doesn't belong to you");
    }

    public ForgeAttempt ForgeAttempt(string connectionId)
    {
        if (!CurrentAnvil.IsComplete)
            return new ForgeAttempt { IsSuccess = false, ResultMessage = "Anvil needs both root and affix elements" };

        var rootId = CurrentAnvil.RootElement!.Id;
        var affixId = CurrentAnvil.AffixElement!.Id;

        var targetCombo = GameTargetCombinations.FirstOrDefault(tc => 
            tc.RootId == rootId && tc.AffixId == affixId && !tc.IsCompleted);

        if (targetCombo != null)
        {
            // Success! Mark elements as used and combination as completed
            CurrentAnvil.RootElement.IsUsed = true;
            CurrentAnvil.AffixElement.IsUsed = true;
            targetCombo.IsCompleted = true;
            CompletedCombinations.Add(targetCombo);

            // Clear anvil
            CurrentAnvil.RootElement = null;
            CurrentAnvil.AffixElement = null;

            if (CompletedCombinations.Count >= GameTargetCombinations.Length)
            {
                IsGameCompleted = true;
            }

            return new ForgeAttempt 
            { 
                RootId = rootId,
                AffixId = affixId,
                ExpectedResult = targetCombo.ResultWord,
                IsSuccess = true, 
                ResultMessage = $"✨ Forged '{targetCombo.ResultWord}'! {targetCombo.Definition}"
            };
        }
        else
        {
            // Failed attempt - return elements to pools
            CurrentAnvil.RootElement = null;
            CurrentAnvil.AffixElement = null;

            return new ForgeAttempt 
            { 
                RootId = rootId,
                AffixId = affixId,
                IsSuccess = false, 
                ResultMessage = "❌ These elements don't combine into a valid word. Try again!" 
            };
        }
    }

    public bool IsCompleted()
    {
        return IsGameCompleted;
    }

    public WordForgePlayerView GetPlayerView(string connectionId)
    {
        if (!Players.ContainsKey(connectionId))
            return new WordForgePlayerView();

        var role = Players[connectionId];
        var playerName = PlayerNames[connectionId];

        return new WordForgePlayerView
        {
            Role = role.ToString(),
            DisplayName = $"{(role == PlayerRole.Piltover ? "Caitlyn" : "Vi")} ({playerName})",
            Instruction = role == PlayerRole.Piltover 
                ? "Drag root words to the anvil to forge new vocabulary!"
                : "Drag affixes to the anvil to combine with roots!",
            AvailableElements = GetPlayerElements(role).Where(e => !e.IsUsed).ToArray(),
            AnvilState = CurrentAnvil,
            TargetCombinations = Mode == GameMode.Assisted ? GameTargetCombinations : Array.Empty<WordCombination>(),
            CompletedCombinations = CompletedCombinations.ToArray(),
            ElementsRemaining = GetPlayerElements(role).Count(e => !e.IsUsed),
            IsCompleted = IsGameCompleted,
            Mode = Mode
        };
    }

    public WordForgeGameState GetGameState()
    {
        return new WordForgeGameState
        {
            CompletedCombinations = CompletedCombinations.Count,
            TotalCombinations = TargetCombinations.Length,
            IsCompleted = IsGameCompleted,
            PlayerCount = Players.Count,
            PlayersNeeded = 2,
            HasStarted = Players.Count == 2,
            Mode = Mode
        };
    }

    public List<string> GetConnectedPlayers()
    {
        return Players.Keys.ToList();
    }

    public bool RemovePlayer(string connectionId)
    {
        var removed = Players.Remove(connectionId);
        if (removed)
        {
            PlayerNames.Remove(connectionId);
            if (Players.Count == 0)
            {
                Reset(); // Reset game if no players left
            }
        }
        return removed;
    }

    private WordElement[] GetPlayerElements(PlayerRole role)
    {
        return role == PlayerRole.Piltover ? RootBank : AffixBank;
    }

    public void Reset()
    {
        CompletedCombinations.Clear();
        CurrentAnvil.RootElement = null;
        CurrentAnvil.AffixElement = null;
        IsGameCompleted = false;

        // Reset all elements to unused
        foreach (var element in RootBank.Concat(AffixBank))
        {
            element.IsUsed = false;
        }

        // Reset all combinations
        foreach (var combo in TargetCombinations)
        {
            combo.IsCompleted = false;
        }
    }
}

