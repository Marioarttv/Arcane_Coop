using System.Collections.Concurrent;

namespace Arcane_Coop.Models
{
    public enum FinalPuzzleGameStatus
    {
        WaitingForPlayers,
        Introduction,
        PlayerATurn,
        PlayerBTurn,
        ProcessingAI,
        DebateConclusion,
        Completed
    }

    public enum DebateSpeaker
    {
        Narrator,
        PlayerA,
        PlayerB,
        Jinx,
        Silco
    }

    public class FinalPuzzleGame
    {
        public string RoomId { get; set; } = "";
        public FinalPuzzleGameStatus Status { get; set; } = FinalPuzzleGameStatus.WaitingForPlayers;
        public ConcurrentDictionary<string, FinalPuzzlePlayer> Players { get; set; } = new();
        public List<DebateDialogue> ConversationHistory { get; set; } = new();
        public string? CurrentSpeakingPlayerId { get; set; }
        public int TurnNumber { get; set; } = 0;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public bool IsProcessingAI { get; set; } = false;
        public string? PendingTranscription { get; set; }
        
        // Story context for the debate
        public string DebateTopic { get; set; } = "The fate of a captured enforcer who claims to have information about a conspiracy";
        public string JinxContext { get; set; } = "Jinx is suspicious and wants immediate action";
        public string SilcoContext { get; set; } = "Silco is calculating and wants to extract maximum value";

        public bool AddPlayer(string connectionId, string playerName, string requestedRole)
        {
            if (Players.Count >= 2) return false;

            var player = new FinalPuzzlePlayer
            {
                ConnectionId = connectionId,
                PlayerName = playerName,
                IsConnected = true
            };

            // Assign roles based on request or availability
            if (Players.IsEmpty)
            {
                player.Role = requestedRole;
            }
            else
            {
                // Second player gets the opposite role
                var existingRole = Players.Values.First().Role;
                player.Role = existingRole == "piltover" ? "zaun" : "piltover";
            }

            Players[connectionId] = player;

            if (Players.Count == 2)
            {
                Status = FinalPuzzleGameStatus.Introduction;
            }

            LastActivity = DateTime.UtcNow;
            return true;
        }

        public void RemovePlayer(string connectionId)
        {
            if (Players.TryRemove(connectionId, out var player))
            {
                player.IsConnected = false;
            }
        }

        public List<string> GetConnectedPlayers()
        {
            return Players.Values
                .Where(p => p.IsConnected)
                .Select(p => p.ConnectionId)
                .ToList();
        }

        public FinalPuzzlePlayer? GetCurrentSpeaker()
        {
            if (string.IsNullOrEmpty(CurrentSpeakingPlayerId)) return null;
            Players.TryGetValue(CurrentSpeakingPlayerId, out var player);
            return player;
        }

        public void AddDialogue(DebateSpeaker speaker, string text, string? audioUrl = null, CharacterEmotion emotion = CharacterEmotion.Neutral)
        {
            ConversationHistory.Add(new DebateDialogue
            {
                Speaker = speaker,
                Text = text,
                AudioUrl = audioUrl,
                Emotion = emotion,
                Timestamp = DateTime.UtcNow
            });
            LastActivity = DateTime.UtcNow;
        }

        public void NextTurn()
        {
            TurnNumber++;
            
            // Alternate between players
            var players = Players.Values.ToList();
            if (players.Count == 2)
            {
                if (CurrentSpeakingPlayerId == players[0].ConnectionId)
                {
                    CurrentSpeakingPlayerId = players[1].ConnectionId;
                    Status = FinalPuzzleGameStatus.PlayerBTurn;
                }
                else
                {
                    CurrentSpeakingPlayerId = players[0].ConnectionId;
                    Status = FinalPuzzleGameStatus.PlayerATurn;
                }
            }
        }
    }

    public class FinalPuzzlePlayer
    {
        public string ConnectionId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public string Role { get; set; } = "zaun";
        public bool IsConnected { get; set; }
        public bool HasSpoken { get; set; } = false;
        public int TurnsTaken { get; set; } = 0;
    }

    public class DebateDialogue
    {
        public DebateSpeaker Speaker { get; set; }
        public string Text { get; set; } = "";
        public string? AudioUrl { get; set; }
        public CharacterEmotion Emotion { get; set; } = CharacterEmotion.Neutral;
        public DateTime Timestamp { get; set; }
    }

    public enum CharacterEmotion
    {
        Neutral,
        Angry,
        Suspicious,
        Confident,
        Thoughtful,
        Amused,
        Frustrated
    }

    public class FinalPuzzlePlayerView
    {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public string PlayerRole { get; set; } = "zaun";
        public FinalPuzzleGameStatus GameStatus { get; set; }
        public List<DebateDialogue> ConversationHistory { get; set; } = new();
        public bool IsMyTurn { get; set; }
        public bool CanSpeak { get; set; }
        public string? CurrentSpeaker { get; set; }
        public string DebateTopic { get; set; } = "";
        public int TurnNumber { get; set; }
        public List<string> ConnectedPlayers { get; set; } = new();
        public bool IsProcessingAI { get; set; }
    }
}