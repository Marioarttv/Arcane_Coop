using System.ComponentModel.DataAnnotations;

namespace Arcane_Coop.Models
{
    // Game state data model
    public class PictureExplanationGameState
    {
        public int CurrentRound { get; set; } = 1;
        public int TotalRounds { get; set; } = 5;
        public int Score { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public string GameStatus { get; set; } = "Waiting";
        public int PlayerCount { get; set; } = 0;
        public string RoundStatus { get; set; } = "";
        public bool ShowingResult { get; set; } = false;
    }

    // Player-specific view data
    public class PictureExplanationPlayerView
    {
        public string Role { get; set; } = ""; // "Piltover" (Describer) or "Zaunite" (Guesser)
        public string DisplayName { get; set; } = "";
        public string CurrentImageUrl { get; set; } = "";
        public List<string> ChoiceImages { get; set; } = new();
        public bool CanFinishDescribing { get; set; } = false; // Can press "Finished Describing" button
        public bool CanChoose { get; set; } = false;
        public int? SelectedChoice { get; set; }
        public bool RoundCompleted { get; set; } = false;
        public string RoundResult { get; set; } = "";
        public int Score { get; set; } = 0;
        public bool ImageVisible { get; set; } = false; // Controls if Piltover player can see the image
    }

    // Picture data structure
    public class PictureData
    {
        public string ImageUrl { get; set; } = "";
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public List<string> DistractorImages { get; set; } = new();
        public string Description { get; set; } = ""; // For debugging/hints
    }

    // Round result data
    public class PictureRoundResult
    {
        public bool IsCorrect { get; set; }
        public string CorrectImageUrl { get; set; } = "";
        public string Description { get; set; } = "";
        public int PointsEarned { get; set; }
        public string ResultMessage { get; set; } = "";
    }
}