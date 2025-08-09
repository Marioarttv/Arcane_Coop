# Visual Novel System - Complete Implementation

## Overview

The Visual Novel System is a sophisticated narrative engine designed specifically for the Arcane-themed escape room experience. It features advanced text animation, modular character management, and dual-theme support for both Piltover and Zaun aesthetics.

## Architecture

### Core Components

1. **VisualNovel.razor** - Main component with full-screen immersive experience
2. **VisualNovelModels.cs** - Data models and enums for type-safe operations  
3. **VisualNovelService.cs** - Service layer for scene management and data persistence
4. **VisualNovelDemo.razor** - Demo interface for testing and showcasing features

### Key Features

#### 🎭 Advanced Text Animation System
- **Typewriter Effect**: NaniNovel-style character-by-character text reveal
- **Skip Functionality**: Instantly reveal full text with smooth transitions
- **Multiple Animation Types**: Typewriter, FadeIn, SlideUp, and Instant
- **Configurable Speed**: Adjustable timing per dialogue line
- **Auto-continue**: Optional automatic progression with delays

#### 🎨 Dual-Theme Visual Design
- **Piltover Theme**: Hextech-inspired design with gold/blue aesthetics
- **Zaun Theme**: Underground atmosphere with green/teal/black styling
- **Dynamic Theming**: Seamless theme switching based on narrative context
- **Atmospheric Effects**: CSS animations for immersive backgrounds
- **Character Highlighting**: Active speaker emphasis with glowing effects

#### 👥 Advanced Character System
- **Dynamic Portraits**: Character images with smooth transitions and expression changes
- **Multi-layout Support**: Single center, dual character, and narrator modes
- **Character State Management**: Active/inactive highlighting system with enhanced visual feedback
- **Flexible Positioning**: Left, right, center character placement
- **Theme-aware Styling**: Character-specific color schemes and effects
- **Expression System**: 10 different character expressions (Default, Happy, Sad, Angry, Surprised, Worried, Determined, Smug, Confused, Serious)
- **Per-Dialogue Expressions**: Dynamic expression changes on a line-by-line basis
- **Structured Asset Organization**: `/images/Characters/{CharacterName}/{CharacterName}_{expression}.png`

#### 🎮 Enhanced Interactive Controls
- **Skip Button**: Reveal text instantly while typing with smooth entrance animations
- **Continue Button**: Progress to next dialogue with enhanced visual feedback and hover effects
- **Improved Positioning**: Controls positioned outside dialogue box to prevent text overlap
- **Auto-play Mode**: Optional hands-free progression
- **Desktop-First Design**: Optimized for desktop experience with professional animations
- **Hardware Acceleration**: 60fps animations with CSS transforms and cubic-bezier easing

### Multiplayer (Act 1) Integration

The Visual Novel engine powers a synchronized, two-player story intro for Act 1 via SignalR. The multiplayer page (`Components/Pages/Act1Multiplayer.razor`) mirrors the single-player experience while coordinating state with the server.

- Hub events (subset):
  - `Act1GameJoined(Act1PlayerView)`, `Act1PlayerViewUpdated(Act1PlayerView)`
  - `Act1TextSkipped()`, `Act1DialogueContinued(int)`
  - `Act1SceneTransition(string)`, `Act1RedirectToNextGame(string url)`
  - `Act1GameRestarted()`, `Act1GameCompleted()`
- Client state flags in `Act1PlayerView` control UI buttons:
  - `CanSkip` shows Skip only while text is animating
  - `CanContinue` shows Continue only when text is fully displayed
- Typing completion handshake:
  - Client calls `Act1TypingCompleted(roomId)` when a line finishes animating or is shown instantly
  - Server sets `IsTextAnimating=false`, `GameState.IsTextFullyDisplayed=true`, then broadcasts updated views
- Scene transition:
  - Server emits `Act1SceneTransition` and per-player `Act1RedirectToNextGame(/picture-explanation?...params...)`
  - Client has a 4s local fallback to navigate if a redirect is missed

## Usage Guide

### Basic Implementation

```csharp
// Create a scene
var scene = new VisualNovelScene
{
    Name = "My Scene",
    Layout = SceneLayout.DualCharacters,
    Theme = NovelTheme.Piltover
};

// Add characters with expression support
scene.Characters.Add(new VisualNovelCharacter
{
    Id = "jayce",
    Name = "Jayce",
    DisplayName = "Jayce Talis",
    ImagePath = "/images/Characters/Jayce/Jayce_default.png",
    Position = CharacterPosition.Left,
    ExpressionPaths = new Dictionary<CharacterExpression, string>
    {
        { CharacterExpression.Default, "/images/Characters/Jayce/Jayce_default.png" },
        { CharacterExpression.Happy, "/images/Characters/Jayce/Jayce_happy.png" },
        { CharacterExpression.Serious, "/images/Characters/Jayce/Jayce_serious.png" }
    }
});

// Add dialogue with expressions
scene.DialogueLines.Add(new DialogueLine
{
    CharacterId = "jayce",
    Text = "Welcome to the world of Hextech innovation!",
    AnimationType = TextAnimationType.Typewriter,
    TypewriterSpeed = 40,
    SpeakerExpression = CharacterExpression.Happy
});
```

### Component Usage

```razor
<VisualNovel 
    Scene="@myScene" 
    Configuration="@config"
    OnStateChanged="@HandleStateChanged"
    OnSceneComplete="@HandleSceneComplete" />
```

## 🌳 Player Choice System

The Visual Novel system features a sophisticated branching dialogue system that supports both simple flow-back choices and complex dialogue trees with state management.

### Choice System Architecture

The choice system operates on two levels:

#### **1. Simple Flow-Back Choices**
Single response that returns to the main narrative flow:
```
Main Story → Player Choice → Single Response → Continue Main Story
```

#### **2. Full Dialogue Trees**  
Complex branching narratives with multiple paths:
```
Main Story → Choice A → Response A1 → Sub-choice A1.1 → Response A1.1.1
                   ↘ Response A2 → Sub-choice A1.2 → Response A1.2.1  
           → Choice B → Response B1 → Continue Story
```

### Technical Implementation

#### **DialogueChoice Model**
```csharp
public class DialogueChoice
{
    public string Id { get; set; }                          // Unique identifier
    public string Text { get; set; }                        // Player-visible choice text
    public string? NextDialogueId { get; set; }             // 🔥 Branching control
    public string? RequiredRole { get; set; }               // Role restriction ("piltover"/"zaun")
    public Dictionary<string, object> Consequences;         // Game state changes
    public CharacterExpression? ResultExpression;           // Character reaction
}
```

#### **The NextDialogueId Magic**
- **`NextDialogueId = null`**: Proceeds to next dialogue line in sequence (simple flow-back)
- **`NextDialogueId = "branch_id"`**: Jumps to dialogue with matching `Id` (true branching)

#### **Creating Choice Dialogue**
```csharp
new DialogueLine
{
    CharacterId = "vi",
    Text = "How should we approach this crisis?",
    IsPlayerChoice = true,                          // Marks as choice point
    ChoiceOwnerRole = "zaun",                      // Only Zaun player can choose
    Choices = new List<DialogueChoice>
    {
        new DialogueChoice
        {
            Id = "stealth_approach",
            Text = "We go in quiet. Use the maintenance tunnels - I know every back route.",
            NextDialogueId = "stealth_response",    // 🎯 Branches to specific dialogue
            ResultExpression = CharacterExpression.Determined,
            Consequences = new Dictionary<string, object> { { "approach", "stealth" } }
        },
        new DialogueChoice
        {
            Id = "direct_approach", 
            Text = "No time for subtlety. We hit them hard and fast before they can react.",
            NextDialogueId = "direct_response",     // 🎯 Different branch
            ResultExpression = CharacterExpression.Angry,
            Consequences = new Dictionary<string, object> { { "approach", "direct" } }
        },
        new DialogueChoice
        {
            Id = "diplomatic_approach",
            Text = "Let me reach out to my contacts first. The Firelights might help if we ask right.",
            NextDialogueId = "diplomatic_response", // 🎯 Third branch
            Consequences = new Dictionary<string, object> { { "approach", "diplomatic" } }
        }
    }
}
```

#### **Branch Response Dialogues**
```csharp
// Stealth Branch Response
new DialogueLine
{
    Id = "stealth_response",                        // 🔗 Matches NextDialogueId
    CharacterId = "caitlyn",
    Text = "Smart. The element of surprise could give us the edge we need. I'll mark the blind spots in their surveillance.",
    AnimationType = TextAnimationType.Typewriter,
    TypewriterSpeed = 45
},

// Direct Branch Response  
new DialogueLine
{
    Id = "direct_response",                         // 🔗 Matches NextDialogueId
    CharacterId = "caitlyn", 
    Text = "Bold, but risky. If we're doing this, we'll need backup. I'll mobilize the Enforcers.",
    AnimationType = TextAnimationType.Typewriter,
    TypewriterSpeed = 45,
    SpeakerExpression = CharacterExpression.Worried
},

// Diplomatic Branch Response
new DialogueLine
{
    Id = "diplomatic_response",                     // 🔗 Matches NextDialogueId
    CharacterId = "caitlyn",
    Text = "Good thinking. The Firelights know Zaun better than any Enforcer ever could. I'll prepare backup plans while you make contact.",
    AnimationType = TextAnimationType.Typewriter,
    TypewriterSpeed = 45
}
```

### Advanced Choice Features

#### **1. Multiplayer Role-Based Choices**
```csharp
// Only specific players can make certain choices
ChoiceOwnerRole = "zaun"        // Only Zaun player sees choice UI
ChoiceOwnerRole = "piltover"    // Only Piltover player sees choice UI  
ChoiceOwnerRole = null          // Either player can choose
```

In multiplayer mode:
- The designated player sees choice options and makes the decision
- The other player sees "Your partner is making a critical decision..." 
- After selection, **both players see the chosen option displayed prominently**
- Both players then continue together with the chosen narrative branch

#### **2. Individual Choice Restrictions**
```csharp
new DialogueChoice
{
    Text = "Use my Enforcer clearance to access restricted areas",
    RequiredRole = "piltover",              // Only Piltover players can select this
    NextDialogueId = "enforcer_route"
}
```

#### **3. Consequence System**
Choices can modify game state that affects future story branches:

```csharp
// Choice stores consequence
Consequences = new Dictionary<string, object> 
{ 
    { "approach", "stealth" },
    { "trust_level", 85 },
    { "alertness", "low" }
}

// Later dialogue can check game state
var approach = gameState.GameState["approach"].ToString();
if (approach == "stealth")
{
    // Show different dialogue options or outcomes
}
```

#### **4. Character Expression Changes**
```csharp
// Vi's expression changes based on player choice
ResultExpression = CharacterExpression.Determined  // Vi looks determined after stealth choice
ResultExpression = CharacterExpression.Angry       // Vi looks angry after direct choice
```

### Nested Choice Trees

The system supports multiple levels of branching:

```csharp
// Primary Choice
Choice: "How do we approach?"
├── Stealth → "stealth_response" 
│   └── Sub-choice: "Which tunnel system?"
│       ├── Old Mining Tunnels → "mining_route"
│       └── Maintenance Shafts → "maintenance_route"
├── Direct → "direct_response"
│   └── Sub-choice: "What's our backup plan?"
│       ├── Call Enforcers → "enforcer_backup"  
│       └── Alert the Council → "council_backup"
└── Diplomatic → "diplomatic_response"
    └── Continue main story
```

### Current Act1 Implementation

The Act1 Emergency Briefing scene demonstrates the full choice system with:

1. **First Choice Point**: Crisis Approach (3 branches)
   - Stealth approach → Stealth response
   - Direct approach → Direct response  
   - Diplomatic approach → Diplomatic response

2. **Second Choice Point**: Priority Decision (3 branches)
   - Secure data → Data priority response
   - Evacuate civilians → Civilian priority response
   - Track saboteur → Investigation priority response

3. **Flow Convergence**: All branches eventually lead to the transition to Picture Explanation puzzle

### Best Practices for Choice Design

#### **Story Flow**
1. **Plan your branches**: Decide if choices lead back to main story or create lasting narrative splits
2. **Use meaningful IDs**: `"stealth_response"` is better than `"choice_a_result"`
3. **Balance complexity**: Too many nested choices can overwhelm players

#### **Multiplayer Considerations**  
1. **Assign choice ownership thoughtfully**: Consider which character would logically make each decision
2. **Provide context for waiting players**: Clear messaging when partner is choosing
3. **Show choice results prominently**: Both players should see and understand what was chosen

#### **Technical Implementation**
1. **Test all branches**: Ensure every `NextDialogueId` has a matching dialogue `Id`
2. **Use consequences strategically**: Store important decisions that might affect future scenes
3. **Consider character expressions**: Choices should feel emotionally consistent

#### **Performance & UX**
1. **Keep choice text concise**: 1-2 lines maximum for readability
2. **Provide clear Continue flow**: Players should always know how to progress after seeing choice results
3. **Use role restrictions wisely**: Don't lock players out unnecessarily

### Debugging Choice Trees

The system includes comprehensive console logging for debugging dialogue flow and choice trees:

#### **Client-Side Logging (Act1Multiplayer)**
- **Dialogue Progression**: Every dialogue line displays with speaker, ID, and choice indicators
- **Choice Selection**: Logs player name, role, choice text, and choice ID when selections are made

#### **Server-Side Logging (GameHub)**  
- **Branch Navigation**: Shows when choices trigger branches vs sequential flow
- **Branch Errors**: Alerts when `NextDialogueId` references missing dialogue IDs
- **Game State Changes**: Logs all consequence applications with key-value pairs
- **Character Expressions**: Tracks expression changes from choice results
- **Dialogue Continuation**: Shows normal progression between dialogue lines

#### **Example Console Output**
```
[Act1Multiplayer] Dialogue #5: vi (ID: approach_choice) [CHOICE POINT] - "How should we approach this crisis?"
[Act1Multiplayer] CHOICE MADE: Alex (zaun) selected "We go in quiet. Use the maintenance tunnels - I know every back route." (ID: stealth_approach)
[Act1GameHub] CONSEQUENCES: Applying 1 game state changes:
  - approach = stealth
[Act1GameHub] EXPRESSION: Vi expression changed Default → Determined  
[Act1GameHub] BRANCH TAKEN: Choice 'We go in quiet. Use the maintenance tunnels - I know every back route.' (ID: stealth_approach) → branching to dialogue ID 'stealth_response' (index: 12)
[Act1Multiplayer] Dialogue #12: caitlyn (ID: stealth_response) - "Smart. The element of surprise could give us the edge we need. I'll mark the blind spots in their surveillance."
```

#### **Using the Debug Output**
1. **Open browser Developer Tools** (F12) → Console tab
2. **Filter by component**: Search for `[Act1Multiplayer]` or `[Act1GameHub]`
3. **Track dialogue flow**: Verify dialogue indices progress correctly
4. **Verify branches**: Ensure `NextDialogueId` values match actual dialogue IDs
5. **Monitor state**: Check that consequences are applied as expected
6. **Debug missing branches**: Look for "BRANCH ERROR" messages indicating broken references

## Configuration Options

### VisualNovelConfig

```csharp
var config = new VisualNovelConfig
{
    Theme = NovelTheme.Piltover,
    ShowSkipButton = true,
    ShowContinueButton = true,
    ShowAutoPlayButton = false,
    EnableSoundEffects = true,
    EnableBackgroundMusic = true,
    DefaultTypewriterSpeed = 50,
    ShowCharacterNames = true
};
```

## Demo Access

Navigate to `/visual-novel-demo` to experience the system:

1. **Theme Selection** - Choose between Piltover and Zaun themes
2. **Demo Scenes** - Pre-built scenarios showcasing all features
3. **Custom Examples** - Advanced usage patterns and configurations

## Integration with Escape Room

### Scene Templates

The service provides pre-built scene templates:

- `CreatePiltoverIntroScene()` - Piltover introduction with Jayce
- `CreateZaunIntroScene()` - Zaun introduction with Vi  
- `CreateEscapeRoomScene(theme, context)` - Dynamic context-aware scenes

### Narrative Branching

```csharp
// Context-aware scene creation
var scene = visualNovelService.CreateEscapeRoomScene(
    NovelTheme.Zaun, 
    "The chemical readings are critical. You must work together to prevent disaster."
);
```

## Technical Specifications

### Performance Features
- **Hardware Acceleration**: GPU-accelerated animations with transform3d and will-change properties
- **Efficient Rendering**: Minimal DOM manipulation with optimized state updates
- **Memory Management**: Automatic cleanup and disposal with proper timer management
- **State Persistence**: Save/load progress functionality
- **Type Safety**: Strongly-typed models throughout with expression system integration
- **Smooth Transitions**: 60fps animations using cubic-bezier easing functions

### Desktop-First Design
- **Professional Polish**: Optimized for desktop experience with enhanced visual effects
- **Advanced Animations**: Sophisticated character transitions and UI feedback
- **High-Quality Interactions**: Enhanced hover states and button animations
- **Visual Hierarchy**: Improved positioning to prevent UI element overlap
- **Accessibility**: Proper ARIA labels and keyboard navigation support

### Browser Compatibility
- Modern browsers with CSS Grid and Flexbox support
- Backdrop-filter support for glassmorphism effects
- CSS animations and transforms support

## Sound Integration (Future)

The system is designed with audio support in mind:

```csharp
new DialogueLine
{
    Text = "The experiment begins now!",
    SoundEffect = "/audio/sfx/explosion.mp3",
    BackgroundMusic = "/audio/music/tension.mp3"
}
```

## Advanced Features

### Enhanced Character Expression System

#### Available Expressions
```csharp
public enum CharacterExpression
{
    Default,      // Neutral/resting expression
    Happy,        // Joyful, pleased
    Sad,          // Sorrowful, dejected
    Angry,        // Furious, irritated
    Surprised,    // Shocked, amazed
    Worried,      // Anxious, concerned
    Determined,   // Focused, resolute
    Smug,         // Self-satisfied, confident
    Confused,     // Puzzled, uncertain
    Serious       // Stern, grave
}
```

#### Expression Usage Examples
```csharp
// Simple speaker expression change
new DialogueLine
{
    CharacterId = "vi",
    Text = "Shit. That's where families live, not just the Chem-Barons and their labs.",
    SpeakerExpression = CharacterExpression.Worried
}

// Multiple character expression changes
new DialogueLine
{
    CharacterId = "caitlyn",
    Text = "We need to coordinate our efforts carefully.",
    SpeakerExpression = CharacterExpression.Serious,
    CharacterExpressions = new Dictionary<string, CharacterExpression>
    {
        { "vi", CharacterExpression.Determined }
    }
}
```

#### Asset Organization Structure
```
wwwroot/images/Characters/
├── Vi/
│   ├── Vi_default.png
│   ├── Vi_happy.png
│   ├── Vi_angry.png
│   ├── Vi_worried.png
│   └── Vi_determined.png
├── Caitlyn/
│   ├── Caitlyn_default.png
│   ├── Caitlyn_serious.png
│   ├── Caitlyn_worried.png
│   └── Caitlyn_determined.png
```

### Enhanced Visual Animations
- Advanced character state transitions with cubic-bezier easing
- Smooth expression changes with fade transitions
- Enhanced dialogue box entrance animations
- Professional button hover effects with scale transforms
- Improved character glow effects with opacity and transform animations
- Name tag entrance animations with slide-in effects

### State Management
- Progress tracking
- Save/load functionality  
- Multiple player support
- Context preservation

### Visual Editor and Typewriter Reliability (2025 Update)

To ensure predictable behavior in multiplayer and the visual editor:

- Strong cancellation for the typewriter animation
  - `CancellationTokenSource` with explicit `CancelTypewriter()`
  - Reset `displayedText`/`currentTextIndex` before starting a new animation
  - Force a render between reset and start to prevent first-character carry-over
- Skip behavior
  - Cancels any running animation, replaces the full line text once (no duplication)
  - Calls `Act1TypingCompleted` when appropriate so buttons switch from Skip → Continue automatically
- Continue behavior
  - Cancels animation, advances dialogue, then starts fresh animation on the next line

### Modular Architecture
- Service-based design
- Dependency injection
- Interface-driven development
- Easy testing and mocking

## Best Practices

### Scene Design
1. Keep dialogue lines concise (2-3 sentences max)
2. Use appropriate animation types for pacing
3. Balance character screen time in dual layouts
4. Consider theme consistency throughout scenes
5. **Use expressions strategically** to enhance emotional impact
6. **Plan expression transitions** for smooth character development

### Character Expression Management
1. **Organize assets consistently** using the `/images/Characters/{Name}/{Name}_{expression}.png` structure
2. **Set up ExpressionPaths** dictionary for each character during initialization
3. **Use SpeakerExpression** for the active character's emotion
4. **Use CharacterExpressions** to update non-speaking characters when needed
5. **Start with Default expressions** and add more as needed

### Performance
1. Preload character images for smooth transitions
2. Use appropriate typewriter speeds (30-50ms recommended)
3. Dispose of timers and resources properly
4. Test on desktop devices for performance validation
5. **Optimize expression assets** - keep consistent image sizes across expressions
6. **Use hardware acceleration** for smooth character transitions

### User Experience  
1. Provide clear visual feedback for interactions
2. Ensure skip functionality is always available
3. Use consistent theming within scene contexts
4. Consider accessibility for all users
5. **Position controls outside dialogue** to prevent text overlap
6. **Provide smooth visual transitions** for professional feel

## Recent Improvements (2024 Update)

### ✨ Enhanced Expression System
- **Dynamic Character Expressions**: Added 10 different character expressions with per-dialogue line control
- **Structured Asset Organization**: Implemented `/images/Characters/{Name}/{Name}_{expression}.png` file structure
- **Smooth Expression Transitions**: Characters now smoothly transition between emotions during dialogue

### 🎨 Improved Visual Polish
- **Enhanced Button Positioning**: Moved Skip/Continue buttons outside dialogue box to prevent text overlap
- **Professional Animations**: Added cubic-bezier easing and sophisticated transition effects
- **Desktop-First Design**: Optimized for desktop experience with enhanced visual feedback
- **Character State Improvements**: Better active/inactive character highlighting with blur and scale effects

### 🚀 Performance Enhancements  
- **Hardware Acceleration**: Implemented GPU-accelerated animations with proper CSS transforms
- **Smooth 60fps Animations**: All transitions now use optimized animation curves
- **Type Safety**: Full integration of expression system with strongly-typed models

### 🎯 Implementation Examples
See the multiplayer story intro in **Act1Multiplayer.razor** for practical examples of the expression system in action, including Vi showing worry and determination, and Caitlyn displaying serious expressions during critical story moments.

---

This Visual Novel System provides a robust foundation for immersive storytelling within your Arcane escape room experience, combining technical excellence with authentic Arcane theming and professional-grade animation polish.