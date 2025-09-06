using System.Collections.Concurrent;

namespace Arcane_Coop.Models
{
    // Enums for Final Puzzle
    public enum FinalPuzzleGameState
    {
        WaitingForPlayers,
        InDebate,
        Completed
    }

    public enum DebateTurn
    {
        Jinx,
        Silco,
        PlayerA,
        PlayerB
    }

    public enum PlayerRole
    {
        Piltover,
        Zaunite
    }

    // Main game state for Final Puzzle
    public class FinalPuzzleGame
    {
        public string RoomId { get; set; } = "";
        public FinalPuzzleGameState State { get; set; } = FinalPuzzleGameState.WaitingForPlayers;
        public ConcurrentDictionary<string, PlayerRole> Players { get; set; } = new();
        public ConcurrentDictionary<string, string> PlayerNames { get; set; } = new();
        public ConcurrentDictionary<string, string> PlayerAvatars { get; set; } = new();
        
        // Debate state
        public DebateTurn CurrentTurn { get; set; } = DebateTurn.Jinx;
        public int CurrentDialogueIndex { get; set; } = 0;
        public List<DebateDialogue> DialogueLines { get; set; } = new();
        public bool IsTextAnimating { get; set; } = false;
        public bool IsTextFullyDisplayed { get; set; } = false;
        public string DisplayedText { get; set; } = "";
        public string CurrentSpeaker { get; set; } = "jinx";
        public string CurrentSpeakerName { get; set; } = "Jinx";
        
        // Player response state
        public bool IsWaitingForPlayerResponse { get; set; } = false;
        public string? WaitingForPlayerId { get; set; } = null;
        public string? PlayerTranscript { get; set; } = null;
        public bool IsProcessingResponse { get; set; } = false;
        
        // Game completion
        public bool IsCompleted { get; set; } = false;
        public int Score { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Story mode support
        public bool IsStoryMode { get; set; } = false;
        public string? OriginalSquadName { get; set; } = null;
        public string? TransitionSource { get; set; } = null;

        public bool CanPlayerAct(string connectionId)
        {
            if (!Players.ContainsKey(connectionId))
                return false;
                
            if (!IsWaitingForPlayerResponse)
                return false;
                
            return WaitingForPlayerId == connectionId;
        }

        public PlayerRole? GetPlayerRole(string connectionId)
        {
            return Players.TryGetValue(connectionId, out var role) ? role : null;
        }

        public string? GetPlayerName(string connectionId)
        {
            return PlayerNames.TryGetValue(connectionId, out var name) ? name : null;
        }

        public List<string> GetConnectedPlayers()
        {
            return Players.Keys.ToList();
        }

        public bool IsFull()
        {
            return Players.Count >= 2;
        }

        public (bool Success, string Message) AddPlayer(string connectionId, string playerName, string avatar, PlayerRole role)
        {
            if (IsFull())
                return (false, "Game is full");

            // Check if a player with the same name exists and kick them out
            var existingPlayerWithSameName = PlayerNames.FirstOrDefault(kvp => kvp.Value == playerName && kvp.Key != connectionId);
            if (!string.IsNullOrEmpty(existingPlayerWithSameName.Key))
            {
                Players.TryRemove(existingPlayerWithSameName.Key, out _);
                PlayerNames.TryRemove(existingPlayerWithSameName.Key, out _);
                PlayerAvatars.TryRemove(existingPlayerWithSameName.Key, out _);
                Console.WriteLine($"[FinalPuzzle] Removed old instance of player {playerName}");
            }

            Players[connectionId] = role;
            PlayerNames[connectionId] = playerName;
            PlayerAvatars[connectionId] = avatar;

            if (Players.Count == 2)
            {
                State = FinalPuzzleGameState.InDebate;
            }

            return (true, "Player added successfully");
        }

        public void RemovePlayer(string connectionId)
        {
            Players.TryRemove(connectionId, out _);
            PlayerNames.TryRemove(connectionId, out _);
            PlayerAvatars.TryRemove(connectionId, out _);
            
            if (WaitingForPlayerId == connectionId)
            {
                IsWaitingForPlayerResponse = false;
                WaitingForPlayerId = null;
            }
        }

        public FinalPuzzleGameStateData GetGameState()
        {
            return new FinalPuzzleGameStateData
            {
                State = this.State,
                CurrentTurn = this.CurrentTurn,
                CurrentDialogueIndex = this.CurrentDialogueIndex,
                IsTextAnimating = this.IsTextAnimating,
                IsTextFullyDisplayed = this.IsTextFullyDisplayed,
                DisplayedText = this.DisplayedText,
                CurrentSpeaker = this.CurrentSpeaker,
                CurrentSpeakerName = this.CurrentSpeakerName,
                IsWaitingForPlayerResponse = this.IsWaitingForPlayerResponse,
                WaitingForPlayerId = this.WaitingForPlayerId,
                IsProcessingResponse = this.IsProcessingResponse,
                IsCompleted = this.IsCompleted,
                Score = this.Score,
                ConnectedPlayersCount = this.Players.Count
            };
        }

        public FinalPuzzlePlayerView GetPlayerView(string connectionId)
        {
            var role = GetPlayerRole(connectionId);
            var name = GetPlayerName(connectionId);
            
            // Determine if we should show text controls for this player
            // Show text controls when:
            // 1. Not waiting for any player response (normal AI dialogue)
            // 2. Waiting for a player response but the transcript has been submitted (showing transcribed text)
            // Never show text controls when waiting for a player to record
            bool shouldShowTextControls = false;
            if (!IsWaitingForPlayerResponse)
            {
                // Normal AI dialogue - everyone can see skip/continue
                shouldShowTextControls = IsTextFullyDisplayed || IsTextAnimating;
            }
            else if (!string.IsNullOrEmpty(PlayerTranscript))
            {
                // Player has submitted transcript - everyone can see continue button
                shouldShowTextControls = IsTextFullyDisplayed || IsTextAnimating;
            }
            // else: waiting for player to record - no text controls for anyone
            
            return new FinalPuzzlePlayerView
            {
                PlayerId = connectionId,
                PlayerRole = role?.ToString() ?? "",
                PlayerName = name ?? "",
                IsPlayerTurn = CanPlayerAct(connectionId),
                CanSkip = IsTextAnimating && shouldShowTextControls,
                CanContinue = IsTextFullyDisplayed && !IsTextAnimating && shouldShowTextControls,
                CanRecord = CanPlayerAct(connectionId) && !IsProcessingResponse,
                ShowRecordingControls = CanPlayerAct(connectionId) && !IsProcessingResponse,
                ShowTextControls = shouldShowTextControls,
                GameState = GetGameState(),
                IsStoryMode = this.IsStoryMode
            };
        }
    }

    // Game state for client synchronization
    public class FinalPuzzleGameStateData
    {
        public FinalPuzzleGameState State { get; set; } = FinalPuzzleGameState.WaitingForPlayers;
        public DebateTurn CurrentTurn { get; set; } = DebateTurn.Jinx;
        public int CurrentDialogueIndex { get; set; } = 0;
        public bool IsTextAnimating { get; set; } = false;
        public bool IsTextFullyDisplayed { get; set; } = false;
        public string DisplayedText { get; set; } = "";
        public string CurrentSpeaker { get; set; } = "jinx";
        public string CurrentSpeakerName { get; set; } = "Jinx";
        public bool IsWaitingForPlayerResponse { get; set; } = false;
        public string? WaitingForPlayerId { get; set; } = null;
        public bool IsProcessingResponse { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        public int Score { get; set; } = 0;
        public int ConnectedPlayersCount { get; set; } = 0;
    }

    // Player-specific view
    public class FinalPuzzlePlayerView
    {
        public string PlayerId { get; set; } = "";
        public string PlayerRole { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public bool IsPlayerTurn { get; set; } = false;
        public bool CanSkip { get; set; } = false;
        public bool CanContinue { get; set; } = false;
        public bool CanRecord { get; set; } = false;
        public bool ShowRecordingControls { get; set; } = false;
        public bool ShowTextControls { get; set; } = false;
        public FinalPuzzleGameStateData GameState { get; set; } = new();
        public bool IsStoryMode { get; set; } = false;
    }

    // Dialogue structure for debate
    public class DebateDialogue
    {
        public int Id { get; set; }
        public string Speaker { get; set; } = ""; // "jinx", "silco", "player_a", "player_b"
        public string SpeakerName { get; set; } = "";
        public string Text { get; set; } = "";
        public bool IsPlayerResponse { get; set; } = false;
        public PlayerRole? RequiredPlayerRole { get; set; } = null; // Which player should respond
        public bool IsFinalDialogue { get; set; } = false;
        public int TypewriterSpeed { get; set; } = 50; // milliseconds per character
    }

    // Recording and transcription result
    public class PlayerResponse
    {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public string Transcript { get; set; } = "";
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public bool IsProcessed { get; set; } = false;
    }

    // AI response from ChatGPT
    public class AIResponse
    {
        public string Speaker { get; set; } = ""; // "jinx" or "silco"
        public string SpeakerName { get; set; } = "";
        public string Text { get; set; } = "";
        public int TypewriterSpeed { get; set; } = 50;
        public bool IsFinalResponse { get; set; } = false;
    }
}
