using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Arcane_Coop.Models;

namespace Arcane_Coop.Hubs;

public class GameHub : Hub
{
    private static readonly ConcurrentDictionary<string, TicTacToeGame> _games = new();
    private static readonly ConcurrentDictionary<string, CodeCrackerGame> _codeCrackerGames = new();
    private static readonly ConcurrentDictionary<string, SimpleSignalDecoderGame> _signalDecoderGames = new();

    public async Task JoinRoom(string roomId, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("PlayerJoined", playerName, Context.ConnectionId);
    }

    public async Task LeaveRoom(string roomId, string playerName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
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

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
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
        
        await base.OnDisconnectedAsync(exception);
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