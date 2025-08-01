using Arcane_Coop.Models;
using System.Text.Json;

namespace Arcane_Coop.Services;

public interface IPuzzleEngine
{
    Task<PuzzleData?> GetPuzzleForPlayerAsync(string roomId, string playerId, int puzzleId);
    Task<PuzzleValidationResult> ValidateAnswerAsync(string roomId, int puzzleId, string playerId, string answer);
    Task<bool> IsPuzzleCompletedAsync(string roomId, int puzzleId);
    Task<List<int>> GetAvailablePuzzlesAsync(string roomId);
    Task<PuzzleProgress> GetPuzzleProgressAsync(string roomId, int puzzleId);
}

public class PuzzleEngine : IPuzzleEngine
{
    private readonly IGameRoomService _gameRoomService;
    private readonly IStateManager _stateManager;
    private readonly ILogger<PuzzleEngine> _logger;

    // Static puzzle definitions - in a real app, these could come from a database
    private readonly Dictionary<int, CooperativePuzzle> _puzzleDefinitions;

    public PuzzleEngine(IGameRoomService gameRoomService, IStateManager stateManager, ILogger<PuzzleEngine> logger)
    {
        _gameRoomService = gameRoomService;
        _stateManager = stateManager;
        _logger = logger;
        _puzzleDefinitions = InitializePuzzles();
    }

    public async Task<PuzzleData?> GetPuzzleForPlayerAsync(string roomId, string playerId, int puzzleId)
    {
        try
        {
            if (!_puzzleDefinitions.ContainsKey(puzzleId))
            {
                _logger.LogWarning("Puzzle {PuzzleId} not found", puzzleId);
                return null;
            }

            var room = await _gameRoomService.GetRoomAsync(roomId);
            if (room == null) return null;

            var player = room.Players.FirstOrDefault(p => p.PlayerId.ToString() == playerId);
            if (player == null) return null;

            var puzzle = _puzzleDefinitions[puzzleId];
            var playerCity = player.City.ToLower();

            // Get player-specific puzzle data based on their city
            var puzzleData = new PuzzleData
            {
                PuzzleId = puzzleId,
                Title = puzzle.Title,
                Description = playerCity == "zaun" ? puzzle.ZaunDescription : puzzle.PiltoverDescription,
                CluesForPlayer = playerCity == "zaun" ? puzzle.ZaunClues : puzzle.PiltoverClues,
                SharedClues = puzzle.SharedClues,
                RequiresCooperation = puzzle.RequiresCooperation,
                ImageUrl = playerCity == "zaun" ? puzzle.ZaunImageUrl : puzzle.PiltoverImageUrl
            };

            return puzzleData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting puzzle {PuzzleId} for player {PlayerId}", puzzleId, playerId);
            return null;
        }
    }

    public async Task<PuzzleValidationResult> ValidateAnswerAsync(string roomId, int puzzleId, string playerId, string answer)
    {
        try
        {
            if (!_puzzleDefinitions.ContainsKey(puzzleId))
            {
                return new PuzzleValidationResult { IsCorrect = false, Message = "Puzzle not found" };
            }

            var puzzle = _puzzleDefinitions[puzzleId];

            // Check if this is a cooperative puzzle requiring both players
            if (puzzle.RequiresCooperation)
            {
                return await ValidateCooperativeAnswerAsync(roomId, puzzleId, playerId, answer);
            }

            // Simple single-player validation
            var isCorrect = puzzle.AcceptedAnswers.Any(a => 
                string.Equals(a, answer, StringComparison.OrdinalIgnoreCase));

            if (isCorrect)
            {
                await MarkPuzzleCompletedAsync(roomId, puzzleId);
                return new PuzzleValidationResult 
                { 
                    IsCorrect = true, 
                    Message = "Correct! Puzzle completed.",
                    PuzzleCompleted = true
                };
            }

            return new PuzzleValidationResult 
            { 
                IsCorrect = false, 
                Message = "That's not quite right. Keep trying!" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating answer for puzzle {PuzzleId}", puzzleId);
            return new PuzzleValidationResult { IsCorrect = false, Message = "Error validating answer" };
        }
    }

    private async Task<PuzzleValidationResult> ValidateCooperativeAnswerAsync(string roomId, int puzzleId, string playerId, string answer)
    {
        var room = await _gameRoomService.GetRoomAsync(roomId);
        if (room == null)
        {
            return new PuzzleValidationResult { IsCorrect = false, Message = "Room not found" };
        }

        var player = room.Players.FirstOrDefault(p => p.PlayerId.ToString() == playerId);
        if (player == null)
        {
            return new PuzzleValidationResult { IsCorrect = false, Message = "Player not found" };
        }

        var puzzle = _puzzleDefinitions[puzzleId];
        var playerCity = player.City.ToLower();

        // Store the player's answer
        await _stateManager.SetPlayerStateAsync(roomId, playerId, $"puzzle_{puzzleId}_answer", answer);

        // Check if we have answers from both players
        var zaunPlayer = room.Players.FirstOrDefault(p => p.City.Equals("Zaun", StringComparison.OrdinalIgnoreCase));
        var piltoverPlayer = room.Players.FirstOrDefault(p => p.City.Equals("Piltover", StringComparison.OrdinalIgnoreCase));

        if (zaunPlayer == null || piltoverPlayer == null)
        {
            return new PuzzleValidationResult 
            { 
                IsCorrect = false, 
                Message = "Waiting for both Zaun and Piltover players..." 
            };
        }

        var zaunAnswer = await _stateManager.GetPlayerStateAsync<string>(roomId, zaunPlayer.PlayerId.ToString(), $"puzzle_{puzzleId}_answer");
        var piltoverAnswer = await _stateManager.GetPlayerStateAsync<string>(roomId, piltoverPlayer.PlayerId.ToString(), $"puzzle_{puzzleId}_answer");

        if (string.IsNullOrEmpty(zaunAnswer) || string.IsNullOrEmpty(piltoverAnswer))
        {
            return new PuzzleValidationResult 
            { 
                IsCorrect = false, 
                Message = "Answer submitted. Waiting for your partner..." 
            };
        }

        // Combine answers and validate
        var combinedAnswer = $"{zaunAnswer}|{piltoverAnswer}";
        var isCorrect = puzzle.AcceptedAnswers.Any(a => 
            string.Equals(a, combinedAnswer, StringComparison.OrdinalIgnoreCase));

        if (isCorrect)
        {
            await MarkPuzzleCompletedAsync(roomId, puzzleId);
            // Clear the stored answers
            await _stateManager.ClearPlayerStateAsync(roomId, zaunPlayer.PlayerId.ToString(), $"puzzle_{puzzleId}_answer");
            await _stateManager.ClearPlayerStateAsync(roomId, piltoverPlayer.PlayerId.ToString(), $"puzzle_{puzzleId}_answer");

            return new PuzzleValidationResult 
            { 
                IsCorrect = true, 
                Message = "Excellent teamwork! You solved the puzzle together!",
                PuzzleCompleted = true
            };
        }

        return new PuzzleValidationResult 
        { 
            IsCorrect = false, 
            Message = "The combined answer isn't correct. Try communicating with your partner!" 
        };
    }

    public async Task<bool> IsPuzzleCompletedAsync(string roomId, int puzzleId)
    {
        var completedPuzzles = await _stateManager.GetSharedStateAsync<List<string>>(roomId, "completed_puzzles") ?? new List<string>();
        return completedPuzzles.Contains(puzzleId.ToString());
    }

    public async Task<List<int>> GetAvailablePuzzlesAsync(string roomId)
    {
        var completedPuzzles = await _stateManager.GetSharedStateAsync<List<string>>(roomId, "completed_puzzles") ?? new List<string>();
        var currentPhase = await _stateManager.GetSharedStateAsync<int>(roomId, "current_phase");

        return _puzzleDefinitions
            .Where(kvp => kvp.Value.Phase <= currentPhase && !completedPuzzles.Contains(kvp.Key.ToString()))
            .Select(kvp => kvp.Key)
            .ToList();
    }

    public async Task<PuzzleProgress> GetPuzzleProgressAsync(string roomId, int puzzleId)
    {
        var isCompleted = await IsPuzzleCompletedAsync(roomId, puzzleId);
        var room = await _gameRoomService.GetRoomAsync(roomId);
        
        if (room == null)
        {
            return new PuzzleProgress { PuzzleId = puzzleId, IsCompleted = false, PlayersWithAnswers = new List<string>() };
        }

        var playersWithAnswers = new List<string>();
        
        if (_puzzleDefinitions[puzzleId].RequiresCooperation)
        {
            foreach (var player in room.Players)
            {
                var answer = await _stateManager.GetPlayerStateAsync<string>(roomId, player.PlayerId.ToString(), $"puzzle_{puzzleId}_answer");
                if (!string.IsNullOrEmpty(answer))
                {
                    playersWithAnswers.Add(player.PlayerName);
                }
            }
        }

        return new PuzzleProgress
        {
            PuzzleId = puzzleId,
            IsCompleted = isCompleted,
            PlayersWithAnswers = playersWithAnswers
        };
    }

    private async Task MarkPuzzleCompletedAsync(string roomId, int puzzleId)
    {
        var completedPuzzles = await _stateManager.GetSharedStateAsync<List<string>>(roomId, "completed_puzzles") ?? new List<string>();
        
        if (!completedPuzzles.Contains(puzzleId.ToString()))
        {
            completedPuzzles.Add(puzzleId.ToString());
            await _stateManager.SetSharedStateAsync(roomId, "completed_puzzles", completedPuzzles);
        }
    }

    private Dictionary<int, CooperativePuzzle> InitializePuzzles()
    {
        return new Dictionary<int, CooperativePuzzle>
        {
            {
                1,
                new CooperativePuzzle
                {
                    Id = 1,
                    Title = "The Bridge Connection",
                    Phase = 1,
                    RequiresCooperation = true,
                    ZaunDescription = "You see ancient hextech conduits running beneath the bridge. There are symbols carved into the metal: ⚡🔧⚙️",
                    PiltoverDescription = "From above, you can see the bridge's golden structure. Engraved in the railings are symbols: 🔩⚡🛠️",
                    ZaunClues = new List<string> { "The conduits pulse with blue energy", "Symbol sequence from left to right: Lightning, Wrench, Gear" },
                    PiltoverClues = new List<string> { "The golden bridge gleams in sunlight", "Symbol sequence from left to right: Bolt, Lightning, Hammer" },
                    SharedClues = new List<string> { "Both cities must work together to activate the bridge" },
                    AcceptedAnswers = new List<string> { "LIGHTNING|LIGHTNING", "⚡|⚡" },
                    ZaunImageUrl = "/images/bridge-zaun.jpg",
                    PiltoverImageUrl = "/images/bridge-piltover.jpg"
                }
            },
            {
                2,
                new CooperativePuzzle
                {
                    Id = 2,
                    Title = "Synchronized Crystals",
                    Phase = 1,
                    RequiresCooperation = true,
                    ZaunDescription = "A glowing purple crystal sits in a corrupted chamber. Strange numbers float around it: 3, 7, 9",
                    PiltoverDescription = "An elegant blue hextech crystal hovers in a pristine laboratory. Holographic numbers circle it: 2, 4, 8",
                    ZaunClues = new List<string> { "The purple crystal resonates with odd numbers", "The sequence seems to be increasing" },
                    PiltoverClues = new List<string> { "The blue crystal harmonizes with even numbers", "The pattern appears to be doubling" },
                    SharedClues = new List<string> { "The crystals must be synchronized to unlock the next area" },
                    AcceptedAnswers = new List<string> { "11|16", "ELEVEN|SIXTEEN" },
                    ZaunImageUrl = "/images/crystal-zaun.jpg",
                    PiltoverImageUrl = "/images/crystal-piltover.jpg"
                }
            }
        };
    }
}

// Supporting classes
public class CooperativePuzzle
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Phase { get; set; }
    public bool RequiresCooperation { get; set; }
    public string ZaunDescription { get; set; } = string.Empty;
    public string PiltoverDescription { get; set; } = string.Empty;
    public List<string> ZaunClues { get; set; } = new();
    public List<string> PiltoverClues { get; set; } = new();
    public List<string> SharedClues { get; set; } = new();
    public List<string> AcceptedAnswers { get; set; } = new();
    public string? ZaunImageUrl { get; set; }
    public string? PiltoverImageUrl { get; set; }
}

public class PuzzleData
{
    public int PuzzleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> CluesForPlayer { get; set; } = new();
    public List<string> SharedClues { get; set; } = new();
    public bool RequiresCooperation { get; set; }
    public string? ImageUrl { get; set; }
}

public class PuzzleValidationResult
{
    public bool IsCorrect { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool PuzzleCompleted { get; set; }
}

public class PuzzleProgress
{
    public int PuzzleId { get; set; }
    public bool IsCompleted { get; set; }
    public List<string> PlayersWithAnswers { get; set; } = new();
}