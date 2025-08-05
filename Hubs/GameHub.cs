using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Arcane_Coop.Models;

namespace Arcane_Coop.Hubs;

public class GameHub : Hub
{
    private static readonly ConcurrentDictionary<string, TicTacToeGame> _games = new();
    private static readonly ConcurrentDictionary<string, CodeCrackerGame> _codeCrackerGames = new();
    private static readonly ConcurrentDictionary<string, SimpleSignalDecoderGame> _signalDecoderGames = new();
    private static readonly ConcurrentDictionary<string, NavigationMazeGame> _navigationMazeGames = new();
    private static readonly ConcurrentDictionary<string, AlchemyGame> _alchemyGames = new();
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
        
        
        await base.OnDisconnectedAsync(exception);
    }
}

public class AlchemyGame
{
    public enum PlayerRole { Piltover, Zaunite }
    
    // Healing Potion Recipe for Vi
    private static readonly AlchemyRecipe HealingPotionRecipe = new AlchemyRecipe
    {
        Name = "Vi's Healing Potion",
        Description = "A powerful healing elixir to restore Vi's strength after her latest undercity adventure.",
        RequiredIngredients = new[] { "shimmer_crystal", "hex_berries", "zaun_grey", "piltover_mint", "vial_of_tears" },
        Steps = new[]
        {
            new RecipeStep
            {
                StepNumber = 1,
                Instruction = "Step 1: Prepare the base",
                IngredientId = "shimmer_crystal",
                RequiredStation = ProcessingStation.MortarPestle,
                RequiredState = IngredientState.Ground,
                DetailedDescription = "Grind the Shimmer Crystal into fine powder using the mortar and pestle. The crystal should sparkle like stardust when properly ground."
            },
            new RecipeStep
            {
                StepNumber = 2,
                Instruction = "Step 2: Extract berry essence",
                IngredientId = "hex_berries",
                RequiredStation = ProcessingStation.HeatingStation,
                RequiredState = IngredientState.Heated,
                DetailedDescription = "Heat the Hex Berries until they release their magical essence. They should glow with a soft blue light when ready."
            },
            new RecipeStep
            {
                StepNumber = 3,
                Instruction = "Step 3: Prepare the mushroom",
                IngredientId = "zaun_grey",
                RequiredStation = ProcessingStation.CuttingBoard,
                RequiredState = IngredientState.Chopped,
                DetailedDescription = "Carefully chop the Zaun Grey mushroom into small pieces. Be precise - uneven cuts will affect the potion's potency."
            },
            new RecipeStep
            {
                StepNumber = 4,
                Instruction = "Step 4: Purify the mint",
                IngredientId = "piltover_mint",
                RequiredStation = ProcessingStation.FilteringStation,
                RequiredState = IngredientState.Filtered,
                DetailedDescription = "Filter the Piltover Mint to remove impurities. The mint should have a crystal-clear appearance when properly filtered."
            },
            new RecipeStep
            {
                StepNumber = 5,
                Instruction = "Step 5: Combine in order",
                IngredientId = "vial_of_tears",
                RequiredStation = ProcessingStation.Cauldron,
                RequiredState = IngredientState.Raw,
                DetailedDescription = "Add ingredients to cauldron in this exact order: Ground Shimmer Crystal, Heated Hex Berries, Chopped Zaun Grey, Filtered Piltover Mint, and finally the Vial of Tears (unprocessed)."
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
            Id = "piltover_mint",
            Name = "Piltover Mint",
            Description = "A pure mint leaf from Piltover's pristine gardens",
            ImagePath = "images/alchemy/piltover_mint_raw.png"
        },
        new AlchemyIngredient
        {
            Id = "vial_of_tears",
            Name = "Vial of Tears",
            Description = "Precious tears collected from the Grey - use sparingly",
            ImagePath = "images/alchemy/vial_of_tears_raw.png"
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
        // Initialize available ingredients
        AvailableIngredients = IngredientBank.Select(ingredient => new AlchemyIngredient
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
        
        // Check if all ingredients are present
        if (CauldronContents.Count != recipe.Steps.Length)
        {
            mistakes.Add($"Wrong number of ingredients (expected {recipe.Steps.Length}, got {CauldronContents.Count})");
            return mistakes; // Can't validate further without correct count
        }
        
        // Check order and processing
        for (int i = 0; i < recipe.Steps.Length; i++)
        {
            var step = recipe.Steps[i];
            var cauldronIngredient = CauldronContents[i];
            
            // Check correct ingredient
            if (cauldronIngredient.Id != step.IngredientId)
            {
                var expectedName = IngredientBank.First(ing => ing.Id == step.IngredientId).Name;
                mistakes.Add($"Step {step.StepNumber}: Expected {expectedName}, but got {cauldronIngredient.Name}");
            }
            
            // Check correct processing state
            if (cauldronIngredient.State != step.RequiredState)
            {
                mistakes.Add($"Step {step.StepNumber}: {cauldronIngredient.Name} should be {step.RequiredState}, but was {cauldronIngredient.State}");
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
        AvailableIngredients = IngredientBank.Select(ingredient => new AlchemyIngredient
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

