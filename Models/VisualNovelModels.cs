namespace Arcane_Coop.Models
{
    public enum NovelTheme
    {
        Piltover,
        Zaun
    }

    public enum SceneLayout
    {
        SingleCenter,
        DualCharacters,
        Narrator
    }

    public enum TextAnimationType
    {
        Typewriter,
        FadeIn,
        SlideUp,
        Instant
    }

    public enum CharacterPosition
    {
        Left,
        Right,
        Center
    }

    public enum CharacterExpression
    {
        Default,
        Happy,
        Sad,
        Angry,
        Surprised,
        Worried,
        Determined,
        Smug,
        Confused,
        Serious
    }

    public class VisualNovelCharacter
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public string? VoicePath { get; set; }
        public string ThemeColor { get; set; } = "#ffffff";
        public bool IsActive { get; set; } = false;
        public CharacterPosition Position { get; set; } = CharacterPosition.Center;
        
        // Expression support
        public CharacterExpression CurrentExpression { get; set; } = CharacterExpression.Default;
        public Dictionary<CharacterExpression, string> ExpressionPaths { get; set; } = new();
        
        // Helper method to get current expression image path
        public string GetCurrentImagePath()
        {
            if (ExpressionPaths.ContainsKey(CurrentExpression))
                return ExpressionPaths[CurrentExpression];
            
            if (ExpressionPaths.ContainsKey(CharacterExpression.Default))
                return ExpressionPaths[CharacterExpression.Default];
            
            return ImagePath; // Fallback to original image path
        }
    }

    public class DialogueChoice
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; } = "";
        public string? NextDialogueId { get; set; }
        public string? RequiredRole { get; set; } // "piltover", "zaun", or null for either
        public Dictionary<string, object> Consequences { get; set; } = new();
        public CharacterExpression? ResultExpression { get; set; } // Expression after choice is made
    }

    public class DialogueLine
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? CharacterId { get; set; }
        public string Text { get; set; } = "";
        public TextAnimationType AnimationType { get; set; } = TextAnimationType.Typewriter;
        public int TypewriterSpeed { get; set; } = 50; // milliseconds per character
        public bool AutoContinue { get; set; } = false;
        public int AutoContinueDelay { get; set; } = 3000;
        public string? BackgroundMusic { get; set; }
        public string? SoundEffect { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        // Expression support - allows setting character expressions for this dialogue line
        public CharacterExpression? SpeakerExpression { get; set; } = null;
        public Dictionary<string, CharacterExpression> CharacterExpressions { get; set; } = new();
        
        // Player choice support
        public bool IsPlayerChoice { get; set; } = false;
        public string? ChoiceOwnerRole { get; set; } // Which player makes this choice ("piltover" or "zaun")
        public List<DialogueChoice> Choices { get; set; } = new();
        public string? SelectedChoiceId { get; set; } // Track what was chosen
        
        // Branching support - allows jumping to a specific dialogue ID after this one
        public string? NextDialogueId { get; set; } // If set, jump to this dialogue ID instead of continuing linearly
    }

    public class VisualNovelScene
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public SceneLayout Layout { get; set; } = SceneLayout.SingleCenter;
        public List<VisualNovelCharacter> Characters { get; set; } = new();
        public List<DialogueLine> DialogueLines { get; set; } = new();
        public string? BackgroundImage { get; set; }
        public string? BackgroundMusic { get; set; }
        public NovelTheme Theme { get; set; } = NovelTheme.Piltover;
        public int? MainContentEndIndex { get; set; } // Index of last main dialogue before branches
    }

    public class VisualNovelState
    {
        public string CurrentSceneId { get; set; } = "";
        public int CurrentDialogueIndex { get; set; } = 0;
        public bool IsTextFullyDisplayed { get; set; } = false;
        public bool IsSkipping { get; set; } = false;
        public bool IsAutoPlay { get; set; } = false;
        public Dictionary<string, object> GameState { get; set; } = new();
    }

    public class VisualNovelConfig
    {
        public NovelTheme Theme { get; set; } = NovelTheme.Piltover;
        public bool ShowSkipButton { get; set; } = true;
        public bool ShowContinueButton { get; set; } = true;
        public bool ShowAutoPlayButton { get; set; } = false;
        public bool EnableSoundEffects { get; set; } = true;
        public bool EnableBackgroundMusic { get; set; } = true;
        public int DefaultTypewriterSpeed { get; set; } = 50;
        public bool ShowCharacterNames { get; set; } = true;
        public string DialogueBoxStyle { get; set; } = "standard";
    }

    // Multiplayer Models
    public enum VisualNovelGameStatus
    {
        WaitingForPlayers,
        InProgress,
        Completed
    }

    public class VisualNovelMultiplayerGame
    {
        public string RoomId { get; set; } = "";
        public VisualNovelGameStatus Status { get; set; } = VisualNovelGameStatus.WaitingForPlayers;
        public VisualNovelScene CurrentScene { get; set; } = new();
        public VisualNovelState GameState { get; set; } = new();
        public List<VisualNovelPlayer> Players { get; set; } = new();
        public DateTime LastActionTime { get; set; } = DateTime.UtcNow;
        public int DebounceDelayMs { get; set; } = 500; // Prevent simultaneous button presses
        public bool IsTextAnimating { get; set; } = false;
        public DateTime TextAnimationStartTime { get; set; } = DateTime.UtcNow;
        
        public VisualNovelPlayer? GetPlayer(string playerId)
        {
            return Players.FirstOrDefault(p => p.PlayerId == playerId);
        }
        
        public bool CanPerformAction()
        {
            var timeSinceLastAction = DateTime.UtcNow - LastActionTime;
            return timeSinceLastAction.TotalMilliseconds > DebounceDelayMs;
        }
        
        public void RecordAction()
        {
            LastActionTime = DateTime.UtcNow;
        }
    }

    public enum VisualNovelPlayerRole 
    { 
        Piltover, 
        Zaunite 
    }

    public class VisualNovelPlayer
    {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public VisualNovelPlayerRole PlayerRole { get; set; } = VisualNovelPlayerRole.Piltover; // First player = Piltover, Second = Zaunite
        public bool IsConnected { get; set; } = true;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }

    public class VisualNovelPlayerView
    {
        public string RoomId { get; set; } = "";
        public string PlayerId { get; set; } = "";
        public VisualNovelPlayerRole PlayerRole { get; set; } = VisualNovelPlayerRole.Piltover;
        public VisualNovelGameStatus GameStatus { get; set; } = VisualNovelGameStatus.WaitingForPlayers;
        public VisualNovelScene? CurrentScene { get; set; }
        public VisualNovelState? GameState { get; set; }
        public List<string> ConnectedPlayers { get; set; } = new();
        public bool CanSkip { get; set; } = false;
        public bool CanContinue { get; set; } = false;
        public bool IsTextAnimating { get; set; } = false;
        public string StatusMessage { get; set; } = "";
    }

    // Act 1 Story Campaign Multiplayer Models
    public enum Act1GameStatus
    {
        WaitingForPlayers,
        InProgress,
        SceneTransition,
        Completed
    }

    public class Act1MultiplayerGame
    {
        public string RoomId { get; set; } = "";
        public Act1GameStatus Status { get; set; } = Act1GameStatus.WaitingForPlayers;
        public VisualNovelScene CurrentScene { get; set; } = new();
        public VisualNovelState GameState { get; set; } = new();
        public List<Act1Player> Players { get; set; } = new();
        public DateTime LastActionTime { get; set; } = DateTime.UtcNow;
        public int DebounceDelayMs { get; set; } = 500; // Prevent simultaneous button presses
        public bool IsTextAnimating { get; set; } = false;
        public DateTime TextAnimationStartTime { get; set; } = DateTime.UtcNow;
        public int CurrentSceneIndex { get; set; } = 0;
        public List<string> StoryProgression { get; set; } = new() { "emergency_briefing", "picture_explanation_transition" };
        public string NextGameName { get; set; } = "";
        public bool ShowTransition { get; set; } = false;
        public List<string> ChoiceHistory { get; set; } = new(); // Track all choices made
        
        public Act1Player? GetPlayer(string playerId)
        {
            return Players.FirstOrDefault(p => p.PlayerId == playerId);
        }
        
        public bool CanPerformAction()
        {
            var timeSinceLastAction = DateTime.UtcNow - LastActionTime;
            return timeSinceLastAction.TotalMilliseconds > DebounceDelayMs;
        }
        
        public void RecordAction()
        {
            LastActionTime = DateTime.UtcNow;
        }

        public bool IsLastScene()
        {
            return CurrentSceneIndex >= StoryProgression.Count - 1;
        }
    }

    public class Act1Player
    {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public string OriginalSquadName { get; set; } = ""; // Original squad name without modifiers
        public string SquadName { get; set; } = ""; // Full squad name with modifiers for lobby separation
        public string PlayerRole { get; set; } = "zaun"; // "zaun" or "piltover"
        public string PlayerAvatar { get; set; } = "1";
        public bool IsConnected { get; set; } = true;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }

    public class Act1PlayerView
    {
        public string RoomId { get; set; } = "";
        public string PlayerId { get; set; } = "";
        public string PlayerRole { get; set; } = "zaun";
        public Act1GameStatus GameStatus { get; set; } = Act1GameStatus.WaitingForPlayers;
        public VisualNovelScene? CurrentScene { get; set; }
        public VisualNovelState? GameState { get; set; }
        public List<string> ConnectedPlayers { get; set; } = new();
        public bool CanSkip { get; set; } = false;
        public bool CanContinue { get; set; } = false;
        public bool IsTextAnimating { get; set; } = false;
        public string StatusMessage { get; set; } = "";
        public bool ShowTransition { get; set; } = false;
        public string NextGameName { get; set; } = "";
        public int CurrentSceneIndex { get; set; } = 0;
        public int TotalScenes { get; set; } = 0;
        
        // Player choice support
        public DialogueLine? PendingChoice { get; set; } // Current choice to be made
        public bool CanMakeChoice { get; set; } = false; // Whether this player can make the current choice
        public List<string> ChoiceHistory { get; set; } = new(); // Track all choices made in this session
        public bool IsWaitingForOtherPlayer { get; set; } = false; // True when other player is making a choice
    }
}