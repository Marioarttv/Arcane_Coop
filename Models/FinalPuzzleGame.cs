using System.Collections.Concurrent;

namespace Arcane_Coop.Models
{
    public class FinalPuzzleGame
    {
        public string RoomId { get; set; } = "";
        public ConcurrentDictionary<string, FinalPuzzlePlayer> Players { get; set; } = new();
        public FinalPuzzleGameState GameState { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsFromStory { get; set; } = false;

        public bool TryAddPlayer(string connectionId, string name, string role = "")
        {
            if (Players.Count >= 2)
                return false;

            var playerRole = DeterminePlayerRole(role);
            if (string.IsNullOrEmpty(playerRole))
                return false;

            var player = new FinalPuzzlePlayer
            {
                ConnectionId = connectionId,
                Name = name,
                Role = playerRole,
                JoinedAt = DateTime.UtcNow
            };

            return Players.TryAdd(connectionId, player);
        }

        public bool TryAddPlayerWithRole(string connectionId, string name, string requestedRole)
        {
            if (Players.Count >= 2)
                return false;

            var availableRole = DetermineAvailableRole(requestedRole);
            if (string.IsNullOrEmpty(availableRole))
                return false;

            var player = new FinalPuzzlePlayer
            {
                ConnectionId = connectionId,
                Name = name,
                Role = availableRole,
                JoinedAt = DateTime.UtcNow
            };

            return Players.TryAdd(connectionId, player);
        }

        private string DeterminePlayerRole(string requestedRole)
        {
            if (!string.IsNullOrEmpty(requestedRole))
            {
                var normalizedRole = NormalizeRole(requestedRole);
                if (!IsRoleTaken(normalizedRole))
                    return normalizedRole;
                
                return GetOppositeRole(normalizedRole);
            }

            if (Players.Count == 0)
                return "A";
            
            return Players.Values.Any(p => p.Role == "A") ? "B" : "A";
        }

        private string DetermineAvailableRole(string requestedRole)
        {
            var normalizedRole = NormalizeRole(requestedRole);
            
            if (Players.Count == 0)
                return normalizedRole;
            
            if (!IsRoleTaken(normalizedRole))
                return normalizedRole;
            
            return GetOppositeRole(normalizedRole);
        }

        private string NormalizeRole(string role)
        {
            return role?.ToLower() switch
            {
                "piltover" => "A",
                "zaun" => "B",
                "a" => "A",
                "b" => "B",
                _ => "A"
            };
        }

        private bool IsRoleTaken(string role)
        {
            return Players.Values.Any(p => p.Role == role);
        }

        private string GetOppositeRole(string role)
        {
            return role == "A" ? "B" : "A";
        }

        public void RemovePlayer(string connectionId)
        {
            Players.TryRemove(connectionId, out _);
        }

        public List<string> GetConnectedPlayers()
        {
            return Players.Keys.ToList();
        }

        public bool IsFull => Players.Count >= 2;
        public bool IsEmpty => Players.Count == 0;
    }

    public class FinalPuzzlePlayer
    {
        public string ConnectionId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime JoinedAt { get; set; }
    }

    public class FinalPuzzleGameState
    {
        public List<int> MatchedPairs { get; set; } = new();
        public int SelectedFactId { get; set; } = 0;
        public int SelectedMemoryId { get; set; } = 0;
        public int CurrentLieIndex { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }

        public void RecordMatch(int factId, int memoryId)
        {
            MatchedPairs.Add(factId);
            MatchedPairs.Add(memoryId);
            
            if (MatchedPairs.Count >= 12)
            {
                IsCompleted = true;
                CompletedAt = DateTime.UtcNow;
            }
            
            CurrentLieIndex = MatchedPairs.Count / 2;
            SelectedFactId = 0;
            SelectedMemoryId = 0;
        }

        public void Reset()
        {
            MatchedPairs.Clear();
            SelectedFactId = 0;
            SelectedMemoryId = 0;
            CurrentLieIndex = 0;
            IsCompleted = false;
            CompletedAt = null;
        }

        public bool CheckPairing(int factId, int memoryId)
        {
            return factId == memoryId;
        }
    }
}