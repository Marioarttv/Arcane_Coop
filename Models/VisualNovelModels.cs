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
}