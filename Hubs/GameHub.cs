using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq;
using Arcane_Coop.Models;

namespace Arcane_Coop.Hubs;

public class GameHub : Hub
{
    private static readonly ConcurrentDictionary<string, TicTacToeGame> _games = new();
    private static readonly ConcurrentDictionary<string, CodeCrackerGame> _codeCrackerGames = new();
    private static readonly ConcurrentDictionary<string, SimpleSignalDecoderGame> _signalDecoderGames = new();
    private static readonly ConcurrentDictionary<string, NavigationMazeGame> _navigationMazeGames = new();
    private static readonly ConcurrentDictionary<string, AlchemyGame> _alchemyGames = new();
    private static readonly ConcurrentDictionary<string, RuneProtocolGame> _runeProtocolGames = new();
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _roomPlayers = new();

    public async Task JoinRoom(string roomId, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        // Add player to room tracking
        var roomPlayerDict = _roomPlayers.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, string>());
        roomPlayerDict.TryAdd(Context.ConnectionId, playerName);
        
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
        
        await Clients.Group(roomId).SendAsync("PlayerLeft", playerName, Context.ConnectionId);
    }

    public async Task SendMessageToRoom(string roomId, string playerName, string message)
    {
        await Clients.Group(roomId).SendAsync("ReceiveMessage", playerName, message);
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

    public async Task RequestRuneProtocolHint(string roomId, string ruleId)
    {
        if (_runeProtocolGames.TryGetValue(roomId, out var game))
        {
            var hint = game.GetHint(Context.ConnectionId, ruleId);
            if (hint != null)
            {
                await Clients.Caller.SendAsync("RuneProtocolHintReceived", hint);
                await Clients.Group(roomId).SendAsync("RuneProtocolGameStateUpdated", game.GetGameState());
            }
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
        
        await base.OnDisconnectedAsync(exception);
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
    
    // 4 Progressive difficulty levels
    private static readonly RuneProtocolLevel[] LevelBank = new[]
    {
        // Level 1: Basic Conditionals (Tutorial)
        new RuneProtocolLevel
        {
            LevelNumber = 1,
            Title = "Emergency Protocol Activation",
            Description = "Critical system instability detected. Two engineers must coordinate to safely activate the emergency stabilization protocol.",
            PiltoverRules = new[]
            {
                new ConditionalRule("P1", "Safety Protocol 1.1: If Primary Stabilizer (R1) is active, Backup Dampener (R5) must be offline", new[] { 0, 4 }),
                new ConditionalRule("P2", "Resonance Prevention 2.3: Power Regulator (R2) and Energy Buffer (R6) cannot operate simultaneously", new[] { 1, 5 }),
                new ConditionalRule("P3", "Activation Sequence 3.7: When Main Controller (R3) is offline, Emergency Override (R4) must be active", new[] { 2, 3 })
            },
            ZauniteRules = new[]
            {
                new ConditionalRule("Z1", "Observation Log #15: The Blue Catalyst (R7) only stabilizes when Primary Stabilizer (R1) is running", new[] { 0, 6 }),
                new ConditionalRule("Z2", "Warning Report #22: If Red Inhibitor (R8) is active, Power Regulator (R2) will overload and must be shut down", new[] { 1, 7 }),
                new ConditionalRule("Z3", "System Balance #9: Either Energy Buffer (R6) or Blue Catalyst (R7) must be online, never both offline", new[] { 5, 6 })
            },
            Solution = new[] { true, true, false, true, false, false, true, false } // R1=ON, R2=ON, R3=OFF, R4=ON, R5=OFF, R6=OFF, R7=ON, R8=OFF
        },
        
        // Level 2: Compound Logic (Intermediate)
        new RuneProtocolLevel
        {
            LevelNumber = 2,
            Title = "Hextech Resonance Stabilization",
            Description = "Multiple cascade failures detected. Advanced coordination required to establish stable hextech resonance patterns.",
            PiltoverRules = new[]
            {
                new ConditionalRule("P1", "Hextech Protocol 4.2: If both Crystal Matrix (R1) and Harmonic Lens (R2) are active, then Power Core (R5) must be offline", new[] { 0, 1, 4 }),
                new ConditionalRule("P2", "Safety Override 7.1: Either Thermal Regulator (R3) or Cooling System (R4) must be active, but not both", new[] { 2, 3 }),
                new ConditionalRule("P3", "Matrix Stability 8.5: When Crystal Matrix (R1) is offline, Backup Generator (R6) must compensate", new[] { 0, 5 })
            },
            ZauniteRules = new[]
            {
                new ConditionalRule("Z1", "Field Test #31: Shimmer Conduit (R7) requires both Thermal Regulator (R3) and Power Core (R5) to be active", new[] { 2, 4, 6 }),
                new ConditionalRule("Z2", "Chemical Analysis #18: If Neutralizer (R8) is active, then Harmonic Lens (R2) must be offline to prevent reaction", new[] { 1, 7 }),
                new ConditionalRule("Z3", "Power Grid #12: When Backup Generator (R6) is active, exactly one of {R7, R8} must be online for balance", new[] { 5, 6, 7 })
            },
            Solution = new[] { false, false, true, false, true, true, true, false } // R1=OFF, R2=OFF, R3=ON, R4=OFF, R5=ON, R6=ON, R7=ON, R8=OFF
        },
        
        // Level 3: Complex Dependencies (Advanced)  
        new RuneProtocolLevel
        {
            LevelNumber = 3,
            Title = "Cascade Prevention Protocol",
            Description = "Critical cascade failures throughout the system. Master-level coordination required to prevent total collapse.",
            PiltoverRules = new[]
            {
                new ConditionalRule("P1", "Master Protocol 9.7: If Primary Array (R1,R2) has exactly one active unit, then Secondary Array (R5,R6) must have exactly two active units", new[] { 0, 1, 4, 5 }),
                new ConditionalRule("P2", "Cascade Prevention 10.3: When Control Node (R3) activates, then either (R4 and R6) or (R7 and R8) must form active pairs", new[] { 2, 3, 5, 6, 7 }),
                new ConditionalRule("P3", "Critical Balance 11.1: The total number of active runes in positions 1-4 must equal the total in positions 5-8", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })
            },
            ZauniteRules = new[]
            {
                new ConditionalRule("Z1", "Catalyst Chain #47: When Reaction Trigger (R7) and Stabilizer (R8) both activate, Primary Array (R1,R2) must have exactly one unit offline", new[] { 0, 1, 6, 7 }),
                new ConditionalRule("Z2", "Overflow Protection #28: If more than 2 runes in the Secondary Array (R5,R6,R7,R8) are active, then R1 must be offline as safety measure", new[] { 0, 4, 5, 6, 7 }),
                new ConditionalRule("Z3", "System Harmony #55: Exactly 4 runes total must be active for stable operation", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })
            },
            Solution = new[] { false, true, false, true, true, true, false, false } // R1=OFF, R2=ON, R3=OFF, R4=ON, R5=ON, R6=ON, R7=OFF, R8=OFF
        },
        
        // Level 4: Master Protocol (Expert)
        new RuneProtocolLevel
        {
            LevelNumber = 4,
            Title = "Master Synchronization Protocol",
            Description = "Ultimate test of coordination. Perfect synchronization required to achieve master-level system harmony.",
            PiltoverRules = new[]
            {
                new ConditionalRule("P1", "Master Theorem 12.9: If not(R1 or R2) then (R3 if and only if R4)", new[] { 0, 1, 2, 3 }),
                new ConditionalRule("P2", "Harmonic Resolution 13.4: The sum of active runes in each row {R1,R2,R3,R4} and {R5,R6,R7,R8} must be equal", new[] { 0, 1, 2, 3, 4, 5, 6, 7 }),
                new ConditionalRule("P3", "Resonance Lock 14.2: If exactly 3 runes are active in total, then R5 and R6 cannot both be active", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })
            },
            ZauniteRules = new[]
            {
                new ConditionalRule("Z1", "Quantum Entanglement #61: If (R7 and R8) are both active, then exactly one of (R1, R2) must be active", new[] { 0, 1, 6, 7 }),
                new ConditionalRule("Z2", "Phase Alignment #73: When R3 is active, then R7 must be active, and when R4 is active, then R8 must be active", new[] { 2, 3, 6, 7 }),
                new ConditionalRule("Z3", "Master Balance #88: Exactly 3 runes must be active, and they must form a symmetric pattern", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })
            },
            Solution = new[] { true, false, false, false, false, false, true, true } // R1=ON, R2=OFF, R3=OFF, R4=OFF, R5=OFF, R6=OFF, R7=ON, R8=ON
        }
    };
    
    public Dictionary<string, PlayerRole> Players { get; set; } = new();
    public Dictionary<string, string> PlayerNames { get; set; } = new();
    public bool[] RuneStates { get; set; } = new bool[8]; // R1-R8 states
    public int CurrentLevel { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public int Score { get; set; } = 0;
    public int HintsUsed { get; set; } = 0;
    public List<string> AttemptHistory { get; set; } = new();
    
    public int PlayerCount => Players.Count;
    public RuneProtocolLevel CurrentLevelData => LevelBank[CurrentLevel];
    
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
        Console.WriteLine($"[DEBUG] Players in game: {string.Join(", ", Players.Keys)}");
        
        if (!Players.ContainsKey(connectionId))
        {
            Console.WriteLine($"[DEBUG] Player not in game. Available players: {string.Join(", ", Players.Keys)}");
            return (false, "You are not in this game");
        }
            
        if (runeIndex < 0 || runeIndex > 7)
        {
            Console.WriteLine($"[DEBUG] Invalid rune index: {runeIndex}");
            return (false, $"Invalid rune index: {runeIndex} (must be 0-7)");
        }
            
        var role = Players[connectionId];
        Console.WriteLine($"[DEBUG] Player role: {role}");
        
        // Check if player can control this rune
        if ((role == PlayerRole.Piltover && runeIndex > 3) || 
            (role == PlayerRole.Zaunite && runeIndex < 4))
        {
            Console.WriteLine($"[DEBUG] Player cannot control rune. Role: {role}, RuneIndex: {runeIndex}");
            return (false, $"You can only control your assigned runes. Role: {role}, attempted rune: R{runeIndex + 1}");
        }
            
        Console.WriteLine($"[DEBUG] Toggling rune R{runeIndex + 1} from {RuneStates[runeIndex]} to {!RuneStates[runeIndex]}");
        RuneStates[runeIndex] = !RuneStates[runeIndex];
        
        var validation = ValidateCurrentState();
        Console.WriteLine($"[DEBUG] Validation result - IsComplete: {validation.IsComplete}, Satisfied: {validation.SatisfiedRules}/{validation.TotalRules}");
        
        if (validation.IsComplete)
        {
            IsCompleted = true;
            Score += Math.Max(100 - HintsUsed * 10 - AttemptHistory.Count * 5, 20);
            return (true, $"🎉 Level {CurrentLevel + 1} Complete! Perfect synchronization achieved! Score: {Score}");
        }
        
        return (true, $"Rune R{runeIndex + 1} toggled. {validation.SatisfiedRules}/{validation.TotalRules} conditions satisfied.");
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
        HintsUsed = 0;
        AttemptHistory.Clear();
        
        return (true, $"Advanced to Level {CurrentLevel + 1}: {CurrentLevelData.Title}");
    }

    public string? GetHint(string connectionId, string ruleId)
    {
        if (!Players.ContainsKey(connectionId) || HintsUsed >= 3)
            return null;

        HintsUsed++;
        
        var level = CurrentLevelData;
        var allRules = level.PiltoverRules.Concat(level.ZauniteRules).ToList();
        var rule = allRules.FirstOrDefault(r => r.Id == ruleId);
        
        if (rule != null)
        {
            return HintsUsed switch
            {
                1 => $"💡 Focus on runes: {string.Join(", ", rule.RelatedRunes.Select(r => $"R{r + 1}"))}",
                2 => $"🔍 This rule is currently: {(ValidateRule(rule) ? "✅ SATISFIED" : "❌ VIOLATED")}",
                3 => $"🎯 Try toggling: {GetSuggestedToggle(rule)}",
                _ => null
            };
        }
        
        return "General hint: Check which conditions are not yet satisfied";
    }

    private ValidationResult ValidateCurrentState()
    {
        var level = CurrentLevelData;
        var allRules = level.PiltoverRules.Concat(level.ZauniteRules).ToList();
        
        Console.WriteLine($"[DEBUG] ValidateCurrentState - Level {CurrentLevel + 1}, Total rules: {allRules.Count}");
        Console.WriteLine($"[DEBUG] Current rune states: [{string.Join(", ", RuneStates.Select((state, i) => $"R{i+1}={state}"))}]");
        
        int satisfiedCount = 0;
        foreach (var rule in allRules)
        {
            var isValid = ValidateRule(rule);
            Console.WriteLine($"[DEBUG] Rule {rule.Id}: {(isValid ? "✅ SATISFIED" : "❌ VIOLATED")} - {rule.Description}");
            if (isValid)
            {
                satisfiedCount++;
            }
        }
        
        Console.WriteLine($"[DEBUG] Validation summary: {satisfiedCount}/{allRules.Count} rules satisfied. Complete: {satisfiedCount == allRules.Count}");
        
        return new ValidationResult
        {
            IsComplete = satisfiedCount == allRules.Count,
            SatisfiedRules = satisfiedCount,
            TotalRules = allRules.Count
        };
    }

    private bool ValidateRule(ConditionalRule rule)
    {
        // Implement actual rule validation based on rule IDs and current level
        var level = CurrentLevelData;
        
        // Level 1 rule validation
        if (CurrentLevel == 0)
        {
            return rule.Id switch
            {
                "P1" => !RuneStates[0] || !RuneStates[4], // If R1 active, R5 must be offline (R1 → ¬R5)
                "P2" => !(RuneStates[1] && RuneStates[5]), // R2 and R6 cannot both be active (¬(R2 ∧ R6))
                "P3" => RuneStates[2] || RuneStates[3], // If R3 offline, R4 must be active (¬R3 → R4) ≡ (R3 ∨ R4)
                "Z1" => !RuneStates[6] || RuneStates[0], // R7 only stable when R1 running (R7 → R1)
                "Z2" => !RuneStates[7] || !RuneStates[1], // If R8 active, R2 must be shut down (R8 → ¬R2)
                "Z3" => RuneStates[5] || RuneStates[6], // Either R6 or R7 must be online (R6 ∨ R7)
                _ => false
            };
        }
        
        // Level 2 rule validation
        if (CurrentLevel == 1)
        {
            return rule.Id switch
            {
                "P1" => !(RuneStates[0] && RuneStates[1]) || !RuneStates[4], // If R1 and R2 active, R5 offline
                "P2" => RuneStates[2] ^ RuneStates[3], // Either R3 or R4, but not both
                "P3" => RuneStates[0] || RuneStates[5], // If R1 offline, R6 must compensate
                "Z1" => !RuneStates[6] || (RuneStates[2] && RuneStates[4]), // R7 requires R3 and R5
                "Z2" => !RuneStates[7] || !RuneStates[1], // If R8 active, R2 offline
                "Z3" => !RuneStates[5] || (RuneStates[6] ^ RuneStates[7]), // If R6 active, exactly one of R7,R8
                _ => false
            };
        }
        
        // Level 3 rule validation
        if (CurrentLevel == 2)
        {
            var r1r2Count = (RuneStates[0] ? 1 : 0) + (RuneStates[1] ? 1 : 0);
            var r5r6Count = (RuneStates[4] ? 1 : 0) + (RuneStates[5] ? 1 : 0);
            var r1r4Count = (RuneStates[0] ? 1 : 0) + (RuneStates[1] ? 1 : 0) + (RuneStates[2] ? 1 : 0) + (RuneStates[3] ? 1 : 0);
            var r5r8Count = (RuneStates[4] ? 1 : 0) + (RuneStates[5] ? 1 : 0) + (RuneStates[6] ? 1 : 0) + (RuneStates[7] ? 1 : 0);
            var totalActive = RuneStates.Count(x => x);
            
            return rule.Id switch
            {
                "P1" => (r1r2Count == 1) ? (r5r6Count == 2) : true, // If exactly one of R1,R2 then exactly two of R5,R6
                "P2" => !RuneStates[2] || ((RuneStates[3] && RuneStates[5]) || (RuneStates[6] && RuneStates[7])), // If R3 then (R4&R6) or (R7&R8)
                "P3" => r1r4Count == r5r8Count, // Equal active runes in each half
                "Z1" => !(RuneStates[6] && RuneStates[7]) || (r1r2Count == 1), // If R7&R8 then exactly one of R1,R2 offline
                "Z2" => (r5r8Count <= 2) || !RuneStates[0], // If >2 in R5-R8, then R1 offline
                "Z3" => totalActive == 4, // Exactly 4 runes active
                _ => false
            };
        }
        
        // Level 4 rule validation
        if (CurrentLevel == 3)
        {
            var r1r4Count = (RuneStates[0] ? 1 : 0) + (RuneStates[1] ? 1 : 0) + (RuneStates[2] ? 1 : 0) + (RuneStates[3] ? 1 : 0);
            var r5r8Count = (RuneStates[4] ? 1 : 0) + (RuneStates[5] ? 1 : 0) + (RuneStates[6] ? 1 : 0) + (RuneStates[7] ? 1 : 0);
            var totalActive = RuneStates.Count(x => x);
            
            return rule.Id switch
            {
                "P1" => !(RuneStates[0] || RuneStates[1]) ? (RuneStates[2] == RuneStates[3]) : true, // If not(R1 or R2) then (R3 iff R4)
                "P2" => r1r4Count == r5r8Count, // Equal active in each row
                "P3" => (totalActive != 3) || !(RuneStates[4] && RuneStates[5]), // If exactly 3 active, not both R5&R6
                "Z1" => !(RuneStates[6] && RuneStates[7]) || ((RuneStates[0] ? 1 : 0) + (RuneStates[1] ? 1 : 0) == 1), // If R7&R8 then exactly one of R1,R2
                "Z2" => (!RuneStates[2] || RuneStates[6]) && (!RuneStates[3] || RuneStates[7]), // R3->R7 and R4->R8
                "Z3" => totalActive == 3, // Exactly 3 active and symmetric pattern
                _ => false
            };
        }
        
        return false;
    }

    private string GetSuggestedToggle(ConditionalRule rule)
    {
        var relatedRunes = rule.RelatedRunes;
        if (relatedRunes.Any())
        {
            var rune = relatedRunes.First();
            return $"R{rune + 1}";
        }
        return "R1";
    }

    public RuneProtocolPlayerView GetPlayerView(string connectionId)
    {
        if (!Players.TryGetValue(connectionId, out var role))
            return new RuneProtocolPlayerView();

        var level = CurrentLevelData;
        var validation = ValidateCurrentState();

        if (role == PlayerRole.Piltover)
        {
            return new RuneProtocolPlayerView
            {
                Role = "Piltover",
                DisplayName = "Caitlyn (Hextech Protocol Engineer)",
                Instruction = IsCompleted ? $"Level {CurrentLevel + 1} Complete! Ready for next challenge?" : 
                    $"Configure the hextech protocol using official maintenance procedures:",
                Rules = level.PiltoverRules,
                ControllableRunes = new[] { 0, 1, 2, 3 }, // R1-R4
                RuneStates = RuneStates,
                SatisfiedRules = validation.SatisfiedRules,
                TotalRules = validation.TotalRules,
                IsCompleted = IsCompleted,
                Score = Score,
                CurrentLevel = CurrentLevel + 1,
                MaxLevel = LevelBank.Length
            };
        }
        else
        {
            return new RuneProtocolPlayerView
            {
                Role = "Zaunite",
                DisplayName = "Vi (Chemtech Systems Analyst)",
                Instruction = IsCompleted ? $"Level {CurrentLevel + 1} Complete! System synchronized!" :
                    $"Analyze chemtech reactions and coordinate with your partner:",
                Rules = level.ZauniteRules,
                ControllableRunes = new[] { 4, 5, 6, 7 }, // R5-R8
                RuneStates = RuneStates,
                SatisfiedRules = validation.SatisfiedRules,
                TotalRules = validation.TotalRules,
                IsCompleted = IsCompleted,
                Score = Score,
                CurrentLevel = CurrentLevel + 1,
                MaxLevel = LevelBank.Length
            };
        }
    }

    public RuneProtocolGameState GetGameState()
    {
        var validation = ValidateCurrentState();
        
        return new RuneProtocolGameState
        {
            RuneStates = RuneStates,
            CurrentLevel = CurrentLevel + 1,
            MaxLevel = LevelBank.Length,
            IsCompleted = IsCompleted,
            Score = Score,
            HintsUsed = HintsUsed,
            PlayerCount = Players.Count,
            PlayersNeeded = 2 - Players.Count,
            SatisfiedRules = validation.SatisfiedRules,
            TotalRules = validation.TotalRules,
            LevelTitle = CurrentLevelData.Title,
            LevelDescription = CurrentLevelData.Description
        };
    }

    public void Reset()
    {
        RuneStates = new bool[8];
        CurrentLevel = 0;
        IsCompleted = false;
        Score = 0;
        HintsUsed = 0;
        AttemptHistory.Clear();
    }
}

// Helper classes for Rune Protocol
public class RuneProtocolLevel
{
    public int LevelNumber { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public ConditionalRule[] PiltoverRules { get; set; } = Array.Empty<ConditionalRule>();
    public ConditionalRule[] ZauniteRules { get; set; } = Array.Empty<ConditionalRule>();
    public bool[] Solution { get; set; } = new bool[8];
}

public class ConditionalRule
{
    public string Id { get; set; }
    public string Description { get; set; }
    public int[] RelatedRunes { get; set; }

    public ConditionalRule(string id, string description, int[] relatedRunes)
    {
        Id = id;
        Description = description;
        RelatedRunes = relatedRunes;
    }
}

public class ValidationResult
{
    public bool IsComplete { get; set; }
    public int SatisfiedRules { get; set; }
    public int TotalRules { get; set; }
}

public class RuneProtocolPlayerView
{
    public string Role { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Instruction { get; set; } = "";
    public ConditionalRule[] Rules { get; set; } = Array.Empty<ConditionalRule>();
    public int[] ControllableRunes { get; set; } = Array.Empty<int>();
    public bool[] RuneStates { get; set; } = new bool[8];
    public int SatisfiedRules { get; set; }
    public int TotalRules { get; set; }
    public bool IsCompleted { get; set; }
    public int Score { get; set; }
    public int CurrentLevel { get; set; }
    public int MaxLevel { get; set; }
}

public class RuneProtocolGameState
{
    public bool[] RuneStates { get; set; } = new bool[8];
    public int CurrentLevel { get; set; }
    public int MaxLevel { get; set; }
    public bool IsCompleted { get; set; }
    public int Score { get; set; }
    public int HintsUsed { get; set; }
    public int PlayerCount { get; set; }
    public int PlayersNeeded { get; set; }
    public int SatisfiedRules { get; set; }
    public int TotalRules { get; set; }
    public string LevelTitle { get; set; } = "";
    public string LevelDescription { get; set; } = "";
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

