# Visual Novel System - Complete Implementation

## Overview

The Visual Novel System is a sophisticated narrative engine designed specifically for the Arcane-themed escape room experience. It features advanced text animation, modular character management, and dual-theme support for both Piltover and Zaun aesthetics.

## Architecture

### Core Components

1. **VisualNovel.razor** - Main component with full-screen immersive experience
2. **VisualNovelModels.cs** - Data models and enums for type-safe operations  
3. **VisualNovelService.cs** - Service layer for scene management and data persistence
4. **VisualNovelDemo.razor** - Demo interface for testing and showcasing features
5. **Act1StoryEngine.cs (2025)** - Server-side story engine for Act 1 content, branching, and player view construction

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
- **Multi-layout Support**: Single center, dual character, and narrator modes, now including 4-character and 5-character layouts
- **Character State Management**: Active/inactive highlighting system with enhanced visual feedback
- **Flexible Positioning**: Left, right, center character placement with expanded position options for larger layouts
- **Theme-aware Styling**: Character-specific color schemes and effects
- **Expression System**: 10 different character expressions (Default, Happy, Sad, Angry, Surprised, Worried, Determined, Smug, Confused, Serious)
- **Per-Dialogue Expressions**: Dynamic expression changes on a line-by-line basis
- **Character Visibility Control**: Hide/show characters dynamically with HiddenUntilFirstLine and manual visibility control
- **Per-Dialogue Position Changes**: Dynamically reposition characters on any dialogue line
- **Structured Asset Organization**: `/images/Characters/{CharacterName}/{CharacterName}_{expression}.png`

#### 🎮 Enhanced Interactive Controls
- **Skip Button**: Reveal text instantly while typing with smooth entrance animations
- **Continue Button**: Progress to next dialogue with enhanced visual feedback and hover effects
- **Improved Positioning**: Controls positioned outside dialogue box to prevent text overlap
- **Auto-play Mode**: Optional hands-free progression
- **Desktop-First Design**: Optimized for desktop experience with professional animations
- **Hardware Acceleration**: 60fps animations with CSS transforms and cubic-bezier easing

### Multiplayer (Act 1) Integration

The Visual Novel engine powers a synchronized, two-player story intro for Act 1 via SignalR. The multiplayer page (`Components/Pages/Storyline/Act1Multiplayer.razor`) mirrors the single-player experience while coordinating state with the server.

#### Core Hub Events
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

#### Story-Puzzle Transition System

**Scene Transitions to Puzzles:**
- Server emits `Act1SceneTransition` and per-player `Act1RedirectToNextGame(/picture-explanation?...params...)`
- Client has a 4s local fallback to navigate if a redirect is missed
- Parameters include: `role`, `avatar`, `name`, `squad`, `story=true` for role preservation

**Puzzle to Scene Transitions:**
- New hub methods: `JoinAct1GameAtScene(roomId, playerName, originalSquadName, role, avatar, sceneIndex)`
- Scene index support: `?sceneIndex=2` allows direct navigation to specific scenes
- URL parameter handling: Both `roomId` and `squad` parameters for consistency
- Bidirectional role preservation throughout the entire story flow

**Story Progression Control:**
```csharp
public List<string> StoryProgression = new() 
{ 
    "emergency_briefing",            // Scene 1 & 2 (Visual Novel)
    "picture_explanation_transition",// Puzzle - Picture Explanation
    "database_revelation",           // Scene 3 (Visual Novel)
    "signal_decoder_transition",     // Puzzle - Signal Decoder
    "radio_decoded",                 // Scene 4 (Visual Novel)
    "renni_apartment",              // Scene 5 (Visual Novel)
    "code_cracker_transition",      // Puzzle - Code Cracker
    "code_decoded",                 // Scene 6 (Visual Novel)
    "shimmer_factory_entrance",     // Scene 7 (Visual Novel)
    "navigation_maze_transition",   // Puzzle - Navigation Maze
    "empty_cells",                  // Scene 8 (Visual Novel)
    "alchemy_lab_transition"        // Puzzle - Alchemy Lab
};
```

**Transition Parameter Implementation:**
```csharp
// Scene-to-Puzzle URL Construction with Transition Parameter
var parameters = $"role={p.PlayerRole}&avatar={p.PlayerAvatar}&name={Uri.EscapeDataString(p.PlayerName)}&squad={Uri.EscapeDataString(p.OriginalSquadName)}&story=true&transition={transitionSource}";
var url = $"/picture-explanation?{parameters}";

// Puzzle Room ID Generation
string transitionParam = HttpUtility.ParseQueryString(uri.Query)["transition"];
string uniqueRoomId = !string.IsNullOrEmpty(transitionParam) ? 
    $"{squadName}_{transitionParam}" : squadName;

// Squad Name Extraction for Next Phase
string originalSquadName = roomId.Contains("_") ? 
    roomId.Substring(0, roomId.IndexOf("_")) : roomId;
string nextPhaseRoomId = $"{originalSquadName}_FromPicturePuzzle";
```

**Transition Flow with Unique Room IDs:**
```
Scene 1&2 → Picture Explanation:
  URL: /picture-explanation?squad=SquadAlpha&transition=FromScene1and2&story=true
  Room ID: SquadAlpha_FromScene1and2

Picture Explanation → Scene 3:
  Extract: SquadAlpha (from SquadAlpha_FromScene1and2)
  New Room ID: SquadAlpha_FromPicturePuzzle
  URL: /act1-multiplayer?roomId=SquadAlpha_FromPicturePuzzle&sceneIndex=2

Scene 3 → Signal Decoder:
  URL: /signal-decoder?squad=SquadAlpha_FromPicturePuzzle&transition=FromScene3&story=true
  Room ID: SquadAlpha_FromPicturePuzzle_FromScene3

Signal Decoder → Next Scene:
  Extract: SquadAlpha (from SquadAlpha_FromPicturePuzzle_FromScene3)
  New Room ID: SquadAlpha_FromSignalDecoder
  URL: /act1-multiplayer?roomId=SquadAlpha_FromSignalDecoder&sceneIndex=4

 Scene 6 → Navigation Maze:
  URL: /navigation-maze?squad=SquadAlpha&transition=FromCodeDecoded&story=true
  Room ID: SquadAlpha_FromCodeDecoded

 Navigation Maze → Scene 8:
  Extract: SquadAlpha (from SquadAlpha_FromCodeDecoded)
  New Room ID: SquadAlpha_FromNavigationMaze
  URL: /act1-multiplayer?roomId=SquadAlpha_FromNavigationMaze&sceneIndex=10

 Scene 8 → Alchemy Lab:
  URL: /alchemy-lab?squad=SquadAlpha&transition=FromEmptyCells&story=true
  Room ID: SquadAlpha_FromEmptyCells
```

**Critical Implementation Details:**
- `MainContentEndIndex` properly set to prevent premature scene transitions
- Scene index handling for both first and second players joining
- Parameter name consistency: `roomId` (not just `squad`) required for proper game room joining
- Debug logging for transition flow troubleshooting

#### Server-authoritative story engine (2025)
- Act 1 content, branching, and player-view construction live in `Services/Act1StoryEngine.cs` via `IAct1StoryEngine`.
- `GameHub` delegates: scene creation, player-view building, scene progression (transition + per-player redirect URLs).
- Benefits: thin hub, testable content, secure multiplayer flow.

### Adding New Acts (per‑act story engine pattern)

To keep multiplayer robust and the hub thin, each new Act should have its own server-side story engine following the Act 1 pattern.

- Create a new engine file and interface:
  - `Services/Act2StoryEngine.cs` implementing `IAct2StoryEngine` (mirror Act 1 signatures: scene creation, player view, progression)
- Register in DI (`Program.cs`):
  - `builder.Services.AddSingleton<IAct2StoryEngine, Act2StoryEngine>();`
- Add hub methods for the act (naming mirrors Act 1):
  - `JoinAct2Game`, `SkipAct2Text`, `ContinueAct2`, `Act2TypingCompleted`, `MakeAct2Choice`, `RestartAct2`
  - Each method delegates to the Act 2 engine for content/branching and broadcasts `Act2PlayerView`
- Keep the page component (e.g., `Act2Multiplayer.razor`) presentational:
  - Render data from `Act2PlayerView`, handle local typewriter animation, and relay user intents to the hub

Notes and future flexibility:
- If the number of Acts grows, consider unifying to a generic `IStoryEngine` and a content provider/factory. For now, per‑act engines maximize clarity and speed of development.

## Usage Guide

### Basic Implementation

```csharp
// Create a scene with new layout options
var scene = new VisualNovelScene
{
    Name = "My Scene",
    Layout = SceneLayout.FiveCharacters, // New layout options: FourCharacters, FiveCharacters
    Theme = NovelTheme.Piltover
};

// Add characters with expression support and visibility control
scene.Characters.Add(new VisualNovelCharacter
{
    Id = "jayce",
    Name = "Jayce",
    DisplayName = "Jayce Talis",
    ImagePath = "/images/Characters/Jayce/Jayce_default.png",
    Position = CharacterPosition.Left_5Characters, // New position options for 5-character layout
    HiddenUntilFirstLine = true, // Character starts hidden
    ExpressionPaths = new Dictionary<CharacterExpression, string>
    {
        { CharacterExpression.Default, "/images/Characters/Jayce/Jayce_default.png" },
        { CharacterExpression.Happy, "/images/Characters/Jayce/Jayce_happy.png" },
        { CharacterExpression.Serious, "/images/Characters/Jayce/Jayce_serious.png" }
    }
});

// Add dialogue with expressions, position changes, and visibility control
scene.DialogueLines.Add(new DialogueLine
{
    CharacterId = "jayce",
    Text = "Welcome to the world of Hextech innovation!",
    AnimationType = TextAnimationType.Typewriter,
    TypewriterSpeed = 40,
    SpeakerExpression = CharacterExpression.Happy,
    CharacterPositions = new Dictionary<string, CharacterPosition>
    {
        { "vi", CharacterPosition.Rightmost_5Characters }, // Move Vi to rightmost position
        { "caitlyn", CharacterPosition.Center_5Characters } // Move Caitlyn to center
    },
    CharacterVisibility = new Dictionary<string, bool>
    {
        { "viktor", false } // Hide Viktor during this line
    }
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

1. **First Choice Point**: Investigation Approach (Piltover player chooses - 3 branches)
   - Decode messages → "messages_response" → "messages_continue" → back to "priority_choice"
   - Examine theft sites → "sites_response" → "sites_continue" → back to "priority_choice"  
   - Split up approach → "split_response" → "split_continue" → back to "priority_choice"

2. **Second Choice Point**: Priority Decision (Zaun player chooses - 3 branches)
   - Save people first → "people_priority" → back to "mission_briefing"
   - Prevent war → "war_priority" → back to "mission_briefing"
   - Catch the thief → "thief_priority" → back to "mission_briefing"

3. **Flow Convergence**: All branches converge back to "mission_briefing" and continue to scene completion, leading to transition to Picture Explanation puzzle

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

## 🌊 Dialogue Flow & Branching Architecture

The Visual Novel system implements a sophisticated dialogue branching pattern that separates main story content from branch dialogues to ensure proper scene flow and transitions.

### Core Branching Pattern

#### **The Fundamental Structure**
```
DialogueLines Array Structure:
[0-N]     Main Content Dialogues (sequential story flow)
[N+1...]  Branch Response Dialogues (accessed via NextDialogueId jumps)
```

#### **MainContentEndIndex Property**
The `MainContentEndIndex` property marks the boundary between main content and branch dialogues:
```csharp
scene.MainContentEndIndex = 16; // Last main content dialogue at index 16
```

This critical property ensures:
- Scene completion is triggered only when reaching the end of main content
- Branch dialogues (after MainContentEndIndex) never trigger scene transitions
- Proper flow control when jumping between dialogue sections

### Implementation Pattern

#### **1. Main Content First (Indices 0 to N)**
```csharp
// Sequential main story dialogues
scene.DialogueLines.AddRange(new[]
{
    new DialogueLine { /* Index 0: Story intro */ },
    new DialogueLine { /* Index 1: Character setup */ },
    // ... more main content ...
    new DialogueLine 
    { 
        Id = "first_choice",
        IsPlayerChoice = true,
        Choices = new List<DialogueChoice>
        {
            new DialogueChoice
            {
                Text = "Take the stealth approach",
                NextDialogueId = "stealth_branch", // Jump to branch dialogue at end
            }
        }
    },
    new DialogueLine { /* More main content after choice */ },
    new DialogueLine { /* Index N: Final main dialogue */ }
});

// Mark end of main content (CRITICAL - must be set before adding branches)
scene.MainContentEndIndex = N; // Index of last main dialogue
```

#### **2. Branch Dialogues at End (Indices N+1 onwards)**
```csharp
// ALL branch dialogues placed after main content
scene.DialogueLines.AddRange(new[]
{
    new DialogueLine
    {
        Id = "stealth_branch", // Matches NextDialogueId from choice
        Text = "You choose the quiet approach...",
        NextDialogueId = "stealth_continue" // Continue to next branch dialogue
    },
    new DialogueLine
    {
        Id = "stealth_continue",
        Text = "Moving through the shadows...",
        NextDialogueId = "next_main_choice" // Jump back to main content ID
    },
    new DialogueLine
    {
        Id = "combat_branch", // Different branch
        Text = "You charge into battle...",
        NextDialogueId = "next_main_choice" // Same return point
    }
    // ... more branch dialogues
});
```

#### **3. Proper Flow Control**
```csharp
// Branch responses that continue the story
new DialogueLine
{
    Id = "branch_response",
    Text = "The consequence of your choice...",
    NextDialogueId = "next_choice_point" // Jump to next main content point
}

// Branch responses that converge back to main story
new DialogueLine
{
    Id = "branch_ending",
    Text = "Your path leads back to...",
    NextDialogueId = "story_continues" // Return to main content
}
```

### Critical Implementation Details

#### **Scene Completion Logic**
The `ContinueAct1` method in GameHub implements this logic:

```csharp
// Get the boundary between main content and branches
var mainContentEndIndex = scene.MainContentEndIndex ?? (scene.DialogueLines.Count - 1);
var currentIndex = game.CurrentDialogueIndex;

// Only check for scene end in main content range
var isInMainContent = currentIndex <= mainContentEndIndex;
var isLastDialogueInScene = isInMainContent && currentIndex == mainContentEndIndex;

if (isLastDialogueInScene)
{
    // Trigger scene transition - only from main content end
    await ProgressToNextScene();
}
```

**Why This Matters:**
- Branch dialogues (indices > MainContentEndIndex) never trigger scene completion
- Only reaching the actual end of main content advances to the next scene
- Prevents premature scene transitions when jumping to branch responses

#### **Branching Navigation**
```csharp
// Check for branch jumps
if (!string.IsNullOrEmpty(dialogue.NextDialogueId))
{
    // Find target dialogue by ID (can be anywhere in the array)
    var nextIndex = scene.DialogueLines.FindIndex(d => d.Id == dialogue.NextDialogueId);
    
    if (nextIndex >= 0)
    {
        // Jump to the target dialogue (usually a branch)
        game.CurrentDialogueIndex = nextIndex;
    }
}
else
{
    // Normal sequential progression (main content flow)
    game.CurrentDialogueIndex++;
}
```

### Act1 Emergency Briefing Example

Here's how the Act1StoryEngine implements this pattern:

#### **Main Content Structure (Indices 0-16)**
```csharp
scene.DialogueLines.AddRange(new[]
{
    // Indices 0-10: Story introduction and setup
    new DialogueLine { /* Caitlyn: "Squad Alpha, thank you for responding..." */ },
    new DialogueLine { /* Vi: "You two are the only ones we can trust..." */ },
    // ... more story setup ...
    
    // Index 11: First player choice (Piltover)
    new DialogueLine
    {
        Id = "investigation_choice",
        IsPlayerChoice = true,
        ChoiceOwnerRole = "piltover",
        Choices = new List<DialogueChoice>
        {
            new DialogueChoice
            {
                Id = "investigate_messages",
                NextDialogueId = "messages_response", // Jump to index 17
            }
            // ... other choices
        }
    },
    
    // Index 12: Second player choice (Zaun)
    new DialogueLine
    {
        Id = "priority_choice",
        IsPlayerChoice = true,
        ChoiceOwnerRole = "zaun",
        Choices = new List<DialogueChoice>
        {
            new DialogueChoice
            {
                Id = "save_people",
                NextDialogueId = "people_priority", // Jump to index 23
            }
            // ... other choices
        }
    },
    
    // Indices 13-16: Mission briefing and finale
    new DialogueLine { Id = "mission_briefing", /* Vi: "Alright, Squad Alpha..." */ },
    new DialogueLine { /* Caitlyn: "Your first task..." */ },
    new DialogueLine { /* Vi: "The images are fragmented..." */ },
    new DialogueLine { /* Vi: "No pressure, rookies..." */ } // Index 16 - last main dialogue
});

scene.MainContentEndIndex = 16; // Scene ends at index 16
```

#### **Branch Dialogues (Indices 17-25)**
```csharp
// ALL branches placed after main content
scene.DialogueLines.AddRange(new[]
{
    // First choice branches (indices 17-22)
    new DialogueLine
    {
        Id = "messages_response", // Index 17
        CharacterId = "vi",
        Text = "Smart call. Those codes use both Piltovan and Zaunite encryption...",
        NextDialogueId = "messages_continue"
    },
    new DialogueLine
    {
        Id = "messages_continue", // Index 18
        CharacterId = "caitlyn", 
        Text = "One of you identifies the patterns, the other provides context...",
        NextDialogueId = "priority_choice" // Jump back to index 12
    },
    
    // More first choice branches...
    new DialogueLine { Id = "sites_response", NextDialogueId = "sites_continue" },
    new DialogueLine { Id = "sites_continue", NextDialogueId = "priority_choice" },
    new DialogueLine { Id = "split_response", NextDialogueId = "split_continue" },
    new DialogueLine { Id = "split_continue", NextDialogueId = "priority_choice" },
    
    // Second choice branches (indices 23-25)
    new DialogueLine
    {
        Id = "people_priority", // Index 23
        CharacterId = "caitlyn",
        Text = "Good. That's exactly the mindset we need...",
        NextDialogueId = "mission_briefing" // Jump back to index 13
    },
    new DialogueLine { Id = "war_priority", NextDialogueId = "mission_briefing" },
    new DialogueLine { Id = "thief_priority", NextDialogueId = "mission_briefing" }
});
```

### Common Pitfalls and Solutions

#### **❌ WRONG: Branch Dialogues in Middle of Main Content**
```csharp
scene.DialogueLines.AddRange(new[]
{
    new DialogueLine { /* Index 0: Main content */ },
    new DialogueLine { /* Index 1: Main content */ },
    new DialogueLine 
    { 
        Id = "branch_response", // ❌ WRONG: Branch in main content
        Text = "Branch response..." 
    },
    new DialogueLine { /* Index 3: More main content */ }
});
scene.MainContentEndIndex = 3; // ❌ Branch dialogue at index 2 is before MainContentEndIndex!
```

**Problem:** Branch dialogue at index 2 will be reached sequentially during main story flow, causing incorrect story progression and potentially triggering scene completion at the wrong time.

#### **✅ CORRECT: All Branches at End**
```csharp
scene.DialogueLines.AddRange(new[]
{
    new DialogueLine { /* Index 0: Main content */ },
    new DialogueLine { /* Index 1: Main content */ },
    new DialogueLine { /* Index 2: Main content */ },
    new DialogueLine { /* Index 3: Final main content */ }
});
scene.MainContentEndIndex = 3; // Main content ends here

// All branches after main content
scene.DialogueLines.AddRange(new[]
{
    new DialogueLine 
    { 
        Id = "branch_response", // ✅ CORRECT: Branch after main content
        Text = "Branch response..." 
    }
});
```

#### **❌ WRONG: Forgetting MainContentEndIndex**
```csharp
// Scene with branches but no MainContentEndIndex set
var scene = new VisualNovelScene { /* ... */ };
// scene.MainContentEndIndex not set - defaults to last dialogue index!
```

**Problem:** Scene completion triggers at wrong time, potentially from branch dialogues.

#### **✅ CORRECT: Always Set MainContentEndIndex**
```csharp
var scene = new VisualNovelScene { /* ... */ };
// Add main content dialogues (indices 0-10)
// Add branch dialogues (indices 11+)
scene.MainContentEndIndex = 10; // ✅ Explicitly mark main content end
```

#### **❌ WRONG: Sequential Branch Dialogues Without Purpose**
```csharp
scene.DialogueLines.AddRange(new[]
{
    new DialogueLine 
    { 
        Id = "branch_a",
        NextDialogueId = "branch_b" // ❌ Points to next sequential dialogue unnecessarily
    },
    new DialogueLine 
    { 
        Id = "branch_b", // Sequential to branch_a - this is OK for multi-part responses
        NextDialogueId = "branch_c" // But make sure this eventually returns to main content
    }
});
```

**Problem:** Sequential branch dialogues are acceptable for multi-part responses, but they must eventually return to main content via NextDialogueId to continue the story properly.

#### **✅ CORRECT: Strategic Branch Placement**
```csharp
scene.DialogueLines.AddRange(new[]
{
    new DialogueLine 
    { 
        Id = "branch_a",
        NextDialogueId = "priority_choice" // ✅ Jump back to main content by ID
    },
    new DialogueLine 
    { 
        Id = "different_branch", // ✅ Separate branch, not sequential
        NextDialogueId = "priority_choice" // ✅ Same return point for convergence
    },
    new DialogueLine 
    { 
        Id = "multi_part_branch", // ✅ Multi-part branch response
        NextDialogueId = "multi_part_continue"
    },
    new DialogueLine 
    { 
        Id = "multi_part_continue", // ✅ Second part of branch
        NextDialogueId = "priority_choice" // ✅ Eventually returns to main content
    }
});
```

### Best Practices for Dialogue Structure

#### **1. Plan Your Content Boundaries**
```csharp
// Always determine your main story flow first
var mainContentDialogues = new[]
{
    /* intro, character setup, choice points, conclusions - sequential flow */
    new DialogueLine { /* Story intro */ },
    new DialogueLine { Id = "first_choice", IsPlayerChoice = true, Choices = { /* ... */ } },
    new DialogueLine { Id = "second_choice", IsPlayerChoice = true, Choices = { /* ... */ } },
    new DialogueLine { Id = "conclusion" /* Final main content */ }
};

// Set the boundary BEFORE adding branches
scene.DialogueLines.AddRange(mainContentDialogues);
scene.MainContentEndIndex = mainContentDialogues.Length - 1;

var branchDialogues = new[]
{
    /* all choice responses and branches - placed AFTER main content */
    new DialogueLine { Id = "choice_response_1", NextDialogueId = "second_choice" },
    new DialogueLine { Id = "choice_response_2", NextDialogueId = "conclusion" }
};

scene.DialogueLines.AddRange(branchDialogues);
```

#### **2. Use Descriptive IDs for Branches**
```csharp
new DialogueChoice
{
    Id = "stealth_approach",
    NextDialogueId = "stealth_response", // ✅ Clear, descriptive
}

// Instead of:
new DialogueChoice
{
    Id = "choice1",
    NextDialogueId = "response_a", // ❌ Unclear purpose
}
```

#### **3. Create Clear Convergence Points**
```csharp
// Multiple branches can converge to same main content point
// These would be in the branch dialogues section (after MainContentEndIndex)
new DialogueLine { Id = "stealth_end", NextDialogueId = "mission_briefing" },
new DialogueLine { Id = "combat_end", NextDialogueId = "mission_briefing" },
new DialogueLine { Id = "diplomatic_end", NextDialogueId = "mission_briefing" },

// Common continuation in main content (this would be in main content section)
new DialogueLine { Id = "mission_briefing", Text = "Now for your mission..." }

// Example from Act1: all priority choice branches converge back to mission_briefing
// "people_priority" → "mission_briefing"
// "war_priority" → "mission_briefing"  
// "thief_priority" → "mission_briefing"
```

#### **4. Validate Branch References**
```csharp
// Always verify NextDialogueId targets exist
foreach (var dialogue in scene.DialogueLines.Where(d => !string.IsNullOrEmpty(d.NextDialogueId)))
{
    var targetExists = scene.DialogueLines.Any(d => d.Id == dialogue.NextDialogueId);
    if (!targetExists)
    {
        throw new InvalidOperationException($"NextDialogueId '{dialogue.NextDialogueId}' not found");
    }
}
```

### Debugging Dialogue Flow

#### **Console Logging for Branch Detection**
The system provides comprehensive logging to debug dialogue flow:

```
[Act1StoryEngine] Scene created with 26 dialogues:
  [0] no ID - caitlyn: "Squad Alpha, thank you for responding. We..."
  [1] no ID - vi: "You two are the only ones we can trust..."
  ...
  [11] ID='investigation_choice' - caitlyn: "Your Piltover operative has tactical..." [CHOICE]
  [12] ID='priority_choice' - vi: "Your Zaun operative knows the streets..." [CHOICE]
  ...
  [16] no ID - vi: "No pressure, rookies. Just the difference..."
  [17] ID='messages_response' - vi: "Smart call. Those codes use both..." → NextDialogueId='messages_continue'
  [18] ID='messages_continue' - caitlyn: "One of you identifies the patterns, the..." → NextDialogueId='priority_choice'
  [19] ID='sites_response' - vi: "Good instincts. That residue is unlike..." → NextDialogueId='sites_continue'
  ...
MainContentEndIndex = 16
```

#### **Runtime Flow Validation**
The GameHub logs show the exact branching behavior during gameplay:

```
[Act1GameHub] BRANCH TAKEN: Choice 'investigate_messages' (ID: investigate_messages) → branching to dialogue ID 'messages_response' (index: 17)
[Act1GameHub] Target dialogue: Speaker=vi, Text="Smart call. Those codes use both Piltovan and Zaunite..."
[Act1GameHub] BRANCH JUMP: Dialogue #17 (ID: messages_response) → jumping to dialogue ID 'messages_continue' (index: 18)
[Act1GameHub] BRANCH JUMP: Dialogue #18 (ID: messages_continue) → jumping to dialogue ID 'priority_choice' (index: 12)
[Act1GameHub] Target dialogue: Speaker=vi, ID=priority_choice, Text="Your Zaun operative knows the streets. What's your gut..."
```

This comprehensive branching system ensures reliable dialogue flow, proper scene transitions, and maintainable story structure while supporting complex choice-driven narratives.

### ⚠️ Critical Implementation Warning

**ALWAYS set `MainContentEndIndex` correctly!** This is the most common source of dialogue flow bugs:

```csharp
// ✅ CORRECT: Set MainContentEndIndex BEFORE adding branch dialogues
scene.DialogueLines.AddRange(mainContentDialogues);
scene.MainContentEndIndex = mainContentDialogues.Length - 1; // Last main content index
scene.DialogueLines.AddRange(branchDialogues); // Now add branches

// ❌ WRONG: Adding branches before setting MainContentEndIndex
scene.DialogueLines.AddRange(mainContentDialogues);
scene.DialogueLines.AddRange(branchDialogues); // ❌ Branches added first
scene.MainContentEndIndex = ...; // ❌ Too late! Will cause scene completion issues
```

**What happens if MainContentEndIndex is wrong:**
- Scene completes too early (if set too low)
- Scene never completes (if set too high)
- Branch dialogues might trigger scene transitions
- Players get stuck or skip content unexpectedly

**The Act1StoryEngine sets `MainContentEndIndex = 16` because dialogue index 16 contains the final main story dialogue before transitioning to the Picture Explanation puzzle.**

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

#### Act 1 Story Scenes (Act1StoryEngine)
- `CreateEmergencyBriefingScene(squadName)` - Scene 1 & 2: Emergency Briefing with dual player choices
- `CreateDatabaseRevelationScene(squadName, game)` - Scene 3: Database discovery and radio setup, accessed after Picture Explanation puzzle

**Scene 3 Implementation Features:**
- 29 dialogue lines covering database discovery, Project Safeguard revelation, and radio interruption
- Database access simulation with character expressions (Surprised, Worried, Determined)
- Radio setup sequence preparing for future missions
- Smooth integration with story-puzzle transition system

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

### 4-Character and 5-Character Layout Support

The Visual Novel system now supports scenes with up to 5 characters displayed simultaneously, providing more flexibility for complex narrative scenes.

#### New Scene Layout Options

```csharp
public enum SceneLayout
{
    SingleCharacterCenter,
    DualCharacters,
    NarratorOnly,
    FourCharacters,    // NEW: Display 4 characters
    FiveCharacters     // NEW: Display 5 characters
}
```

#### Character Position Options

**For 4-Character Layout:**
```csharp
CharacterPosition.Leftmost_4Characters   // Far left position
CharacterPosition.Left_4Characters       // Left-center position
CharacterPosition.Right_4Characters      // Right-center position
CharacterPosition.Rightmost_4Characters  // Far right position
```

**For 5-Character Layout:**
```csharp
CharacterPosition.Leftmost_5Characters   // Far left position
CharacterPosition.Left_5Characters       // Left-center position
CharacterPosition.Center_5Characters     // Center position
CharacterPosition.Right_5Characters      // Right-center position
CharacterPosition.Rightmost_5Characters  // Far right position
```

#### CSS Layout Adjustments

The system automatically adjusts character sizes and spacing based on the number of characters:

- **4-Character Layout**: Characters are sized to fit 4 portraits comfortably
- **5-Character Layout**: Characters are automatically scaled smaller to accommodate all 5 characters
- **Responsive Spacing**: Character spacing adjusts dynamically to prevent overlap

#### Usage Example

```csharp
// Create a dramatic 5-character scene
var scene = new VisualNovelScene
{
    Name = "Council Assembly",
    Layout = SceneLayout.FiveCharacters,
    Theme = NovelTheme.Piltover
};

// Position characters across the scene
scene.Characters.AddRange(new[]
{
    new VisualNovelCharacter
    {
        Id = "jayce",
        Position = CharacterPosition.Leftmost_5Characters
    },
    new VisualNovelCharacter
    {
        Id = "viktor",
        Position = CharacterPosition.Left_5Characters
    },
    new VisualNovelCharacter
    {
        Id = "mel",
        Position = CharacterPosition.Center_5Characters
    },
    new VisualNovelCharacter
    {
        Id = "caitlyn",
        Position = CharacterPosition.Right_5Characters
    },
    new VisualNovelCharacter
    {
        Id = "vi",
        Position = CharacterPosition.Rightmost_5Characters
    }
});
```

### Per-Dialogue Character Position Changes

Characters can be dynamically repositioned during any dialogue line, enabling fluid scene choreography and dramatic positioning changes.

#### CharacterPositions Dictionary

The `CharacterPositions` property allows you to move specific characters to new positions on any dialogue line:

```csharp
new DialogueLine
{
    CharacterId = "mel",
    Text = "The Council must hear all perspectives on this matter.",
    CharacterPositions = new Dictionary<string, CharacterPosition>
    {
        { "jayce", CharacterPosition.Left_5Characters },      // Move Jayce from leftmost to left-center
        { "viktor", CharacterPosition.Rightmost_5Characters }, // Move Viktor to far right
        { "caitlyn", CharacterPosition.Center_5Characters }    // Move Caitlyn to center stage
    }
}
```

#### Dramatic Scene Examples

**Confrontation Scene:**
```csharp
// Initial positioning: characters spread across scene
new DialogueLine
{
    CharacterId = "jayce",
    Text = "We've always disagreed, but this time it's different.",
    CharacterPositions = new Dictionary<string, CharacterPosition>
    {
        { "jayce", CharacterPosition.Left_4Characters },
        { "viktor", CharacterPosition.Right_4Characters },
        { "mel", CharacterPosition.Leftmost_4Characters },
        { "caitlyn", CharacterPosition.Rightmost_4Characters }
    }
},

// Dramatic repositioning: opposing sides
new DialogueLine
{
    CharacterId = "viktor",
    Text = "Then perhaps it's time we stopped pretending we're on the same side.",
    CharacterPositions = new Dictionary<string, CharacterPosition>
    {
        { "jayce", CharacterPosition.Leftmost_4Characters },  // Jayce moves to far left
        { "mel", CharacterPosition.Left_4Characters },        // Mel joins Jayce's side
        { "viktor", CharacterPosition.Rightmost_4Characters }, // Viktor moves to far right
        { "caitlyn", CharacterPosition.Right_4Characters }     // Caitlyn joins Viktor's side
    }
}
```

**Gathering Around a Central Figure:**
```csharp
new DialogueLine
{
    CharacterId = "council_speaker",
    Text = "The decision affects all of us. Come closer so we can discuss this properly.",
    CharacterPositions = new Dictionary<string, CharacterPosition>
    {
        { "jayce", CharacterPosition.Left_5Characters },
        { "viktor", CharacterPosition.Right_5Characters },
        { "mel", CharacterPosition.Center_5Characters },
        { "caitlyn", CharacterPosition.Leftmost_5Characters },
        { "vi", CharacterPosition.Rightmost_5Characters }
    }
}
```

### Character Visibility Control

The system provides sophisticated visibility control for creating dramatic entrances, exits, and scene management.

#### HiddenUntilFirstLine Property

Characters can be configured to start hidden and automatically appear when they first speak:

```csharp
new VisualNovelCharacter
{
    Id = "mysterious_figure",
    Name = "???",
    DisplayName = "Mysterious Figure",
    Position = CharacterPosition.Center_5Characters,
    HiddenUntilFirstLine = true, // Character starts invisible
    ExpressionPaths = new Dictionary<CharacterExpression, string>
    {
        { CharacterExpression.Default, "/images/Characters/Mysterious/Mysterious_default.png" }
    }
};

// Character automatically becomes visible when they first speak
new DialogueLine
{
    CharacterId = "mysterious_figure", // Character appears with this line
    Text = "You didn't expect to see me here, did you?",
    SpeakerExpression = CharacterExpression.Smug
}
```

#### Manual Visibility Control

Use the `CharacterVisibility` dictionary to manually control character visibility on any dialogue line:

```csharp
// Hide character during dialogue
new DialogueLine
{
    CharacterId = "caitlyn",
    Text = "Where did Viktor go? He was just here a moment ago.",
    CharacterVisibility = new Dictionary<string, bool>
    {
        { "viktor", false } // Viktor becomes invisible
    }
},

// Bring character back
new DialogueLine
{
    CharacterId = "viktor",
    Text = "I'm still here, Caitlyn. Just... changed.",
    CharacterVisibility = new Dictionary<string, bool>
    {
        { "viktor", true } // Viktor reappears
    }
}
```

#### IsVisible Property

The system tracks current visibility state through the `IsVisible` property:

```csharp
// Check character visibility state
if (character.IsVisible)
{
    // Character is currently visible
}
else
{
    // Character is currently hidden
}
```

#### Dramatic Scene Examples with Visibility

**Dramatic Entrance:**
```csharp
// Scene starts with main characters
new DialogueLine
{
    CharacterId = "jayce",
    Text = "The meeting is about to begin. Are we missing anyone?"
},

// Mysterious character appears
new DialogueLine
{
    CharacterId = "silco", // Character with HiddenUntilFirstLine = true
    Text = "You're missing someone very important indeed.",
    SpeakerExpression = CharacterExpression.Smug,
    CharacterPositions = new Dictionary<string, CharacterPosition>
    {
        { "silco", CharacterPosition.Center_5Characters } // Appears in center stage
    }
}
```

**Character Exit:**
```csharp
new DialogueLine
{
    CharacterId = "vi",
    Text = "I've heard enough. I'm leaving.",
    SpeakerExpression = CharacterExpression.Angry,
    CharacterVisibility = new Dictionary<string, bool>
    {
        { "vi", false } // Vi disappears after speaking
    }
},

new DialogueLine
{
    CharacterId = "caitlyn",
    Text = "Vi, wait!" // Vi is no longer visible
}
```

### Expression Behavior with Multiple Characters

When working with multiple characters, the expression system follows these behavior patterns:

#### Active Speaker Behavior
- **Active Speaker**: Shows their specified expression (`SpeakerExpression`)
- **Inactive Characters**: Automatically revert to their default expression
- **Manual Expression Override**: Use `CharacterExpressions` to set specific expressions for non-speaking characters

#### Example with Expression Management

```csharp
// Setup: 5 characters in a tense council meeting
new DialogueLine
{
    CharacterId = "mel",
    Text = "The situation in the Undercity is deteriorating rapidly.",
    SpeakerExpression = CharacterExpression.Worried,
    CharacterExpressions = new Dictionary<string, CharacterExpression>
    {
        { "jayce", CharacterExpression.Serious },    // Jayce looks serious
        { "viktor", CharacterExpression.Confused },  // Viktor looks puzzled
        { "caitlyn", CharacterExpression.Determined }, // Caitlyn looks resolved
        { "vi", CharacterExpression.Angry }          // Vi looks angry
    }
},

// Next speaker - others revert to default unless specified
new DialogueLine
{
    CharacterId = "vi",
    Text = "That's because the Council refuses to listen!",
    SpeakerExpression = CharacterExpression.Angry,
    // Other characters automatically revert to Default expression
    // unless overridden in CharacterExpressions
}
```

### Complex Scene Choreography Example

Here's a comprehensive example showing all new features working together:

```csharp
// Create a dramatic 5-character council scene
var councilScene = new VisualNovelScene
{
    Name = "The Critical Vote",
    Layout = SceneLayout.FiveCharacters,
    Theme = NovelTheme.Piltover
};

// Setup characters with strategic positioning and visibility
councilScene.Characters.AddRange(new[]
{
    new VisualNovelCharacter
    {
        Id = "mel",
        Position = CharacterPosition.Center_5Characters,
        DisplayName = "Councilor Medarda"
    },
    new VisualNovelCharacter
    {
        Id = "jayce",
        Position = CharacterPosition.Left_5Characters,
        DisplayName = "Councilor Talis"
    },
    new VisualNovelCharacter
    {
        Id = "viktor",
        Position = CharacterPosition.Right_5Characters,
        DisplayName = "Viktor"
    },
    new VisualNovelCharacter
    {
        Id = "caitlyn",
        Position = CharacterPosition.Leftmost_5Characters,
        DisplayName = "Sheriff Kiramman"
    },
    new VisualNovelCharacter
    {
        Id = "silco",
        Position = CharacterPosition.Rightmost_5Characters,
        HiddenUntilFirstLine = true, // Dramatic entrance
        DisplayName = "???"
    }
});

// Dialogue sequence with full feature usage
councilScene.DialogueLines.AddRange(new[]
{
    new DialogueLine
    {
        CharacterId = "mel",
        Text = "The Council is now in session. We must address the growing tensions.",
        SpeakerExpression = CharacterExpression.Serious
    },
    
    new DialogueLine
    {
        CharacterId = "jayce",
        Text = "The situation requires immediate action, not more debate.",
        SpeakerExpression = CharacterExpression.Determined,
        CharacterPositions = new Dictionary<string, CharacterPosition>
        {
            { "jayce", CharacterPosition.Left_5Characters }, // Jayce steps forward
        }
    },
    
    new DialogueLine
    {
        CharacterId = "viktor",
        Text = "Perhaps... there is another perspective to consider.",
        SpeakerExpression = CharacterExpression.Worried,
        CharacterExpressions = new Dictionary<string, CharacterExpression>
        {
            { "jayce", CharacterExpression.Surprised } // Jayce reacts
        }
    },
    
    new DialogueLine
    {
        CharacterId = "silco", // Dramatic entrance - character appears
        Text = "Indeed there is. One that none of you have considered.",
        SpeakerExpression = CharacterExpression.Smug,
        CharacterPositions = new Dictionary<string, CharacterPosition>
        {
            { "silco", CharacterPosition.Center_5Characters }, // Takes center stage
            { "mel", CharacterPosition.Right_5Characters }     // Mel moves aside
        },
        CharacterExpressions = new Dictionary<string, CharacterExpression>
        {
            { "mel", CharacterExpression.Surprised },
            { "jayce", CharacterExpression.Angry },
            { "viktor", CharacterExpression.Confused },
            { "caitlyn", CharacterExpression.Worried }
        }
    },
    
    new DialogueLine
    {
        CharacterId = "caitlyn",
        Text = "Security! How did you get in here?",
        SpeakerExpression = CharacterExpression.Angry,
        CharacterPositions = new Dictionary<string, CharacterPosition>
        {
            { "caitlyn", CharacterPosition.Left_5Characters } // Caitlyn moves to confront
        }
    },
    
    new DialogueLine
    {
        CharacterId = "silco",
        Text = "I have my ways, Sheriff. The question is: are you ready to hear the truth?",
        SpeakerExpression = CharacterExpression.Serious,
        CharacterVisibility = new Dictionary<string, bool>
        {
            { "viktor", false } // Viktor mysteriously disappears
        }
    }
});
```

This example demonstrates:
- **5-character layout** with strategic positioning
- **Dynamic repositioning** during dialogue for dramatic effect
- **Character visibility control** with dramatic entrance and mysterious exit
- **Complex expression management** showing multiple character reactions
- **Coordinated scene choreography** that tells a story through positioning

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

### Story-Puzzle Transition Debugging (2025)

#### Common Transition Issues and Solutions (2025 Fixes)

**"Squad Synchronization" Loop:** ✅ FIXED
```
Problem: Players stuck waiting after puzzle completion
Cause: Missing or incorrect roomId parameter in transition URL
Solution: Enhanced parameter validation and debug logging added
Implementation: Both roomId and squad parameters now properly passed in all transitions
```

**Scene Index Not Working:** ✅ FIXED
```
Problem: Players start at wrong scene after puzzle
Cause: Scene index parameter ignored for second player
Solution: JoinAct1GameAtScene now handles startAtSceneIndex for all players
Implementation: Scene index handling updated for both first and second players joining
```

**Role Assignment Problems:** ✅ IMPROVED
```  
Problem: Player roles switch between puzzle and scene
Cause: Relying on join order instead of explicit role preservation
Solution: Signal Decoder now uses JoinSignalDecoderGameWithRole() and AddPlayerWithRole()
Implementation: All story-mode puzzles now have dedicated hub methods for role preservation
```

**Scene 3 Wrong Transition:** ✅ FIXED
```
Problem: Scene 3 (Database Revelation) incorrectly transitioned to Picture Explanation
Cause: Hardcoded fallback URL always redirected to Picture Explanation regardless of story position
Solution: Dynamic fallback URL generation based on current story progression
Implementation: GetFallbackTransitionUrl() method with story progression awareness
```

**Signal Decoder "Game is Full" Errors:** ✅ FIXED
```
Problem: Signal Decoder showing "Game is Full" when joining from story mode
Cause: Missing hub methods for story mode integration and role preservation
Solution: Complete Signal Decoder story integration with JoinSignalDecoderGameWithRole() hub method
Implementation: Added AddPlayerWithRole() game logic for proper story mode support
```

**Fallback Mechanism Improvements:** ✅ ENHANCED
```
Problem: Scene index lost during transition state, causing incorrect fallback behavior
Cause: Transition state clearing current scene index before fallback mechanism could use it
Solution: Scene index storage with transitionFromSceneIndex variable
Implementation: Store scene index when transition starts, use stored value for dynamic URL generation
```

#### Debug Logging Strategy
The system provides comprehensive logging for transition debugging and the new testing features:

**Story Progression and Scene Navigation:**
```
[Act1StoryEngine] Scene progression tracking: currentSceneIndex=2, nextPhase=signal_decoder_transition
[Act1Multiplayer] Fallback triggered - stored scene index: 2, next phase: signal_decoder_transition
[Act1Multiplayer] Dynamic fallback URL generated: /signal-decoder?role=zaun&story=true&...
```

**Scene Selection Testing:**
```
[CharacterLobby] Scene selection testing - Starting from scene 2 (Database Revelation)
[GameHub] RedirectPlayersToAct1WithScene: roomId=squad_alpha, sceneIndex=2
[GameHub] Generated shared test room: squad_alpha_test_2_143052
[CharacterLobby] Both players transported to testing room: squad_alpha_test_2_143052
```

**Signal Decoder Story Integration:**
```
[GameHub] JoinSignalDecoderGameWithRole: player joining with role=piltover, story=true
[SignalDecoder] AddPlayerWithRole: player added with preserved role from story mode
[SignalDecoder] Story mode active: isFromStory=true, Continue Story button enabled
```

**Transition State Management:**
```
[Act1Multiplayer] Parsed URL params - RoomId: 'test', SceneIndex: 2
[GameHub] Redirecting player {connectionId} to: /act1-multiplayer?role=zaun&roomId=test&squad=test&sceneIndex=2  
[Act1Multiplayer] Joining Act1 game at scene 2 - Room: test, Player: Alex, Role: zaun
[GameHub] Act1 game started with 2 players at scene index 2 (database_revelation)
[Act1Multiplayer] Scene index setup tracking: transitionFromSceneIndex stored as 2
```

This logging helps identify parameter parsing issues, URL construction problems, scene initialization failures, testing workflow issues, and Signal Decoder integration problems.

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

## Recent Improvements

### 🔄 2025 Update: Story-Puzzle Transition System
- **Seamless Puzzle Integration**: Full bidirectional transitions between visual novel scenes and puzzle games
- **Role Preservation**: Maintains Piltover/Zaun player assignments across story and puzzle phases
- **Scene Index Navigation**: Direct scene targeting with `?sceneIndex=N` parameter support
- **Hub Method Extensions**: New `JoinAct1GameAtScene` for puzzle-to-scene transitions
- **Scene 3 Implementation**: Database Revelation scene with 29 dialogue lines and character expressions
- **Debug Infrastructure**: Comprehensive logging for transition troubleshooting
- **Parameter Consistency**: Proper `roomId`/`squad` parameter handling for multiplayer sync

### 🔄 2025 Update: Complete Signal Decoder Integration
- **Fixed Scene 3 to Signal Decoder Transition**: Scene 3 (Database Revelation) now correctly transitions to Signal Decoder instead of Picture Explanation
- **Dynamic Fallback System**: Enhanced fallback mechanism with scene index storage and dynamic URL generation based on story progression
- **Complete Hub Integration**: New `JoinSignalDecoderGameWithRole()` hub method eliminates "Game is Full" errors during story transitions
- **Role Preservation**: Signal Decoder now uses `AddPlayerWithRole()` to maintain Piltover/Zaun assignments from story mode
- **Enhanced Debug Logging**: Comprehensive logging throughout progression and fallback systems for transition troubleshooting

### 🔄 2025 Update: Transition Parameter System
- **Unique Lobby Name Generation**: Sophisticated transition parameter system creates unique lobby names for each transition between scenes and puzzles
- **Room Conflict Prevention**: Eliminates "party is already full" errors by ensuring each transition uses unique room identifiers
- **Squad Name Preservation**: Original squad name maintained for display purposes while using unique internal room IDs
- **Synchronized Navigation**: Both players receive redirect messages simultaneously with coordinated room creation/joining
- **Parameter Implementation**: All scene-to-puzzle transitions include transition parameters (e.g., `&transition=FromScene1and2`)
- **Unique Room ID Format**: `{squadName}_{transitionSource}` pattern creates distinct lobbies for each story phase

### 🧪 2025 Update: Scene Selection Testing System
- **Developer Testing Interface**: Added comprehensive scene selection testing in Character Lobby
- **Two-Step Testing Process**: "Continue Where You Left Off" button reveals full scene selection grid
- **Synchronized Player Transport**: Both players transported to same testing room simultaneously via SignalR
- **Scene & Puzzle Options**: Direct access to visual novel scenes (Emergency Briefing, Database Revelation) or puzzle transitions (Picture Explanation, Signal Decoder)
- **Role Preservation in Testing**: Maintains Piltover/Zaun assignments throughout testing workflow
- **Rapid Development Workflow**: Jump directly to any story content without playing through prerequisites (saves ~10+ minutes per test cycle)

### ✨ 2024 Update: Enhanced Expression System
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
See the multiplayer story intro in **Act1Multiplayer.razor** for practical examples of the expression system in action, including Vi showing worry and determination, and Caitlyn displaying serious expressions during critical story moments. The new Scene 3 demonstrates seamless integration with the Picture Explanation puzzle transition system.

---

This Visual Novel System provides a robust foundation for immersive storytelling within your Arcane escape room experience, combining technical excellence with authentic Arcane theming and professional-grade animation polish.