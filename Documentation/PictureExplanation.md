# PictureExplanation - Visual Communication Challenge

## Overview
PictureExplanation is a cooperative visual communication puzzle where players must identify scientists from corrupted surveillance files. Following the discovery of Jinx's break-in at the records storage facility, players work together to identify four scientists from damaged photos marked with red X's. One player (Piltover/Caitlyn) sees the evidence photos clearly while the other (Zaunite/Vi) must identify them from multiple damaged options based on verbal descriptions.

## Game Mechanics

### Player Roles
- **Piltover (Caitlyn - Archive Analyst)**: Views clear surveillance photos and evidence files from Marcus's private collection
- **Zaunite (Vi - Intelligence Agent)**: Has access to corrupted database files and must identify the correct scientist from 4 damaged photos

### Objective
Work together to identify all four scientists from Project Safeguard files before Jinx finds them. The Analyst must accurately describe evidence photos while the Agent correctly identifies scientists from corrupted surveillance images based purely on verbal descriptions.

### Gameplay Flow
1. Both players join the same room using a shared room ID
2. First player becomes Caitlyn (Archive Analyst), second becomes Vi (Intelligence Agent)
3. Analyst views a clear evidence photo and describes the scientist over voice chat (Discord/external voice)
4. After description is complete, Analyst hides the evidence file
5. Agent selects the correct scientist from 4 corrupted surveillance photos based on the description
6. Process repeats for 4 rounds - one for each Project Safeguard scientist

### Scoring System
- **Correct Identification**: Points for accurately identifying each scientist
- **Investigation Speed**: Time-based rewards for efficient evidence analysis
- **Perfect Intelligence**: Bonus points when description leads to immediate correct identification
- **Team Success**: Cooperative success - both players advance the investigation together

## Educational Value

### Primary Skills
- **Descriptive Language**: Developing precise, detailed vocabulary for visual elements
- **Active Listening**: Processing and interpreting spoken descriptions accurately
- **Visual Communication**: Translating visual information into verbal descriptions
- **Critical Thinking**: Distinguishing between similar visual elements

### Target Audience
- ESL students developing descriptive and listening skills
- Communication training programs
- Students learning visual vocabulary and adjectives
- Anyone improving verbal description abilities

### Language Features
- **Visual Vocabulary**: Colors, shapes, sizes, positions, and relationships
- **Descriptive Adjectives**: Detailed modifiers for precise communication
- **Spatial Language**: Positional and comparative descriptions
- **Progressive Complexity**: From simple objects to complex scenes

## Technical Implementation

### Key Components
- **Voice Chat Integration**: Designed for external voice communication (Discord, etc.)
- **Image Hiding System**: Describer controls when image becomes hidden
- **Multiple Choice Interface**: Clean, accessible option selection for Guesser
- **Story Integration**: Seamlessly connects with visual novel narrative
- **Round Progression**: Automated advancement through 5 challenge rounds

### Data Models
- **ImageChallenge**: Contains target image and 3 distractor options
- **RoundData**: Tracks current round, scoring, and player progress
- **GameSession**: Manages overall challenge state and communication
- **PlayerView**: Role-specific interface (image view vs. choice selection)

### User Interface
- **Full-Screen Image Display**: Clear, detailed images for Describer
- **Hide/Show Controls**: Describer can control image visibility
- **Choice Grid**: Clean 2x2 layout for Guesser selections
- **Round Progress**: Visual indicators showing challenge advancement
- **Story Context**: Thematic integration with Arcane narrative

## Game Rules

### Setup
1. Both players must enter the same Room ID
2. Players need unique player names
3. Voice chat system required (Discord, etc.) - not provided by game
4. First to join becomes Caitlyn (Describer), second becomes Vi (Guesser)

### During Gameplay
- **Describer Phase**: Views image, describes it verbally over voice chat
- **Hide Phase**: Describer clicks to hide image after description is complete
- **Selection Phase**: Guesser chooses from 4 similar options
- **Reveal Phase**: Correct answer shown, round advances
- **No Text Chat**: Communication must happen via external voice chat

### Round Structure
- **Round 1-2**: Simple objects and scenes (easy vocabulary)
- **Round 3-4**: Complex scenes with multiple elements
- **Round 5**: Advanced challenge with detailed, similar images

### Winning Conditions
- Complete all 5 rounds successfully
- Achieve target accuracy across all rounds
- Cooperative success - both players win together

## Voice Chat Requirements

### External Voice Communication
- **Discord Integration**: Game designed for Discord voice channels
- **Alternative Platforms**: Teams, Zoom, or any voice chat service
- **Real-Time Requirement**: Low-latency voice communication essential
- **Quality Standards**: Clear audio required for detailed descriptions

### Communication Guidelines
- **Describer Tips**: Use specific adjectives, spatial relationships, and distinctive features
- **Guesser Tips**: Ask clarifying questions, take notes mentally, focus on unique details
- **Time Management**: Efficient but thorough descriptions work best

## Story-Specific Image Challenges

### The Four Scientists of Project Safeguard
Based on the story dialogue, these are the specific scientists that need to be identified:

1. **Dr. Werner Steinberg**: Elderly scientist with distinctive wire-rimmed glasses and formal academy attire
2. **Dr. Renni Stiltner**: Middle-aged woman with practical lab clothing and tool belt, known for mechanical expertise
3. **Professor Albus Ferros**: Distinguished professor with white beard and academy medallion, formal robes
4. **Dr. Corin Reveck**: Younger scientist with dark hair and experimental equipment, known for theoretical work

### Evidence Photo Categories
1. **Personnel Files**: Individual scientist identification photos with academy credentials
2. **Laboratory Settings**: Scientists working with early Hextech prototypes
3. **Project Documentation**: Group photos from Project Safeguard meetings
4. **Surveillance Images**: Recent photos showing scientists being marked by unknown assailant

### Investigation Difficulty Progression
- **Round 1 - Dr. Werner**: Clear identification photos with obvious differences (age, glasses, attire)
- **Round 2 - Dr. Renni**: Laboratory photos requiring attention to tools and equipment details
- **Round 3 - Professor Albus**: Group photos where specific facial features and medallions distinguish him
- **Round 4 - Dr. Corin**: Surveillance photos with corrupted image quality requiring precise description of subtle features

### Red X Marking Mechanic
Each correct scientist identification reveals a red X marking on their photo, showing they've been targeted by Jinx for elimination. This creates urgency as players realize these scientists are in immediate danger.

## Educational Applications

### Teaching Use Cases
- **Descriptive Writing Preparation**: Practice before written assignments
- **Vocabulary Assessment**: Test knowledge of visual and spatial terms
- **Listening Comprehension**: Evaluate ability to process spoken descriptions
- **Communication Training**: Develop clear, precise speaking skills

### Learning Objectives
- Master descriptive vocabulary for visual elements
- Develop active listening and interpretation skills
- Practice spatial and positional language
- Improve real-time communication effectiveness

## Story Integration

### Act 1 Scene 1&2 → Picture Explanation Transition
**Narrative Context**: Following the crime scene investigation where Jinx's break-in revealed files marked with red X's, the team heads to Piltover Enforcer HQ to identify the four scientists from corrupted surveillance photos. 

**Story Beats**:
- Discovery of Project Safeguard files at the crime scene
- Marcus's private collection containing scientist personnel files
- Jinx's red X markings indicating targets for elimination
- Urgent need to identify scientists before they're found and eliminated

**Character Motivations**:
- **Caitlyn**: Using her enforcer access to analyze evidence files properly
- **Vi**: Desperate to understand what Silco has told Jinx about these scientists
- **Players**: Racing against time to save innocent people from Jinx's misguided vengeance

### Thematic Elements
### Act 1 Visual Novel → Picture Explanation Transition

- Per-player redirect
  - Server sends `Act1RedirectToNextGame` to each connection with individualized URL parameters
  - Parameters include: `role`, `avatar`, `name`, `squad`, `story=true`, and `transition`
- Role preservation
  - `PictureExplanation.razor` reads `role` from URL and calls `JoinPictureExplanationGameWithRole`
  - If both request the same role, the second is assigned the remaining role automatically
- Lobby bypass for story
  - When `story=true`, the UI immediately sets `inGame = true` upon join to skip the lobby panel for both Piltover and Zaun

### Transition Parameter System (2025 Update)

**Unique Room ID Generation:**
The Picture Explanation puzzle now implements a sophisticated transition parameter system that prevents "party is already full" errors during story transitions.

**URL Parameter Processing:**
```csharp
// Example transition URL from Scene 1&2:
/picture-explanation?role=piltover&squad=SquadAlpha&transition=FromScene1and2&story=true

// Transition parameter parsing
string transitionParam = HttpUtility.ParseQueryString(uri.Query)["transition"];
string uniqueRoomId = !string.IsNullOrEmpty(transitionParam) ? 
    $"{squadName}_{transitionParam}" : squadName;

// Result: SquadAlpha_FromScene1and2
```

**Squad Name Extraction for Return Transition:**
```csharp
// When transitioning back to visual novel
string originalSquadName = roomId.Contains("_") ? 
    roomId.Substring(0, roomId.IndexOf("_")) : roomId;
string nextPhaseRoomId = $"{originalSquadName}_FromPicturePuzzle";

// From: SquadAlpha_FromScene1and2
// Extract: SquadAlpha  
// Create: SquadAlpha_FromPicturePuzzle
```

**Critical Benefits:**
- **Eliminates Room Conflicts**: Each story transition uses a unique room identifier
- **Preserves Squad Identity**: Original squad name maintained for display purposes
- **Synchronized Navigation**: Both players join the same unique puzzle room
- **Seamless Story Flow**: No interruption from "Game is Full" errors during transitions

### Thematic Elements
- **Surveillance Technology**: Fits Piltover's technological aesthetic
- **Underground Intelligence**: Matches Zaun's information networks
- **Cooperative Mission**: Emphasizes cross-faction collaboration
- **Emergency Response**: Time pressure from story context

## Technical Notes

### SignalR Methods
- `JoinRoom(roomId, playerName)`: Connects players to challenge session
- `JoinPictureExplanationGame(roomId, playerName)`: Assigns Describer/Guesser roles
- `JoinPictureExplanationGameWithRole(roomId, playerName, requestedRole)`: Honors requested role from story URL while preventing duplicates
- `ContinueStoryToScene3(roomId)`: Transitions both players back to visual novel Scene 3 after puzzle completion
- `FinishDescribing(roomId)`: Describer triggers image hiding
- `SubmitPictureChoice(roomId, choiceIndex)`: Processes Guesser's image selection
- `NextPictureRound(roomId)`: Moves to next challenge round
- `RestartPictureExplanationGame(roomId)`: Resets the challenge

**Transition Parameter Integration:**
- Room ID creation uses transition parameter: `{squadName}_{transitionParam}`
- Squad name extraction for story continuation: `roomId.Substring(0, roomId.IndexOf("_"))`
- Unique room IDs prevent conflicts during story-puzzle transitions

### Image Requirements
- **Resolution**: High-quality images for detailed description
- **Similarity Design**: Carefully crafted distractor options
- **Accessibility**: Alt text available for screen readers
- **Loading Optimization**: Pre-cached images for smooth gameplay

### Performance Considerations
- Efficient image loading and caching
- Minimal UI updates during voice communication
- Quick response times for choice selection
- Reliable state synchronization across players

## Story-Specific Image Assets

### Directory Structure
All scientist identification images should be placed in: `/wwwroot/images/pictures/scientists/`

### Required Scientist Photos

#### Dr. Werner Steinberg Set
- **werner_personnel.jpg**: Clear academy ID photo showing elderly man with wire-rimmed glasses
- **werner_surveillance1.jpg**: Blurry surveillance photo (distractor)
- **werner_surveillance2.jpg**: Photo of different elderly man (distractor) 
- **werner_corrupted.jpg**: Heavily pixelated photo (distractor)

#### Dr. Renni Stiltner Set
- **renni_lab.jpg**: Clear photo of woman in practical lab attire with tool belt
- **renni_workshop1.jpg**: Blurry workshop photo (distractor)
- **renni_workshop2.jpg**: Photo of different woman with tools (distractor)
- **renni_damaged.jpg**: Water-damaged photo with missing sections (distractor)

#### Professor Albus Ferros Set
- **albus_formal.jpg**: Clear photo showing white-bearded professor with academy medallion
- **albus_group1.jpg**: Group photo where his face is partially obscured (distractor)
- **albus_group2.jpg**: Photo of different bearded professor (distractor)
- **albus_torn.jpg**: Torn photo missing key identifying features (distractor)

#### Dr. Corin Reveck Set
- **corin_research.jpg**: Clear photo of young scientist with dark hair and experimental equipment
- **corin_shadow.jpg**: Photo taken in shadows obscuring features (distractor)
- **corin_equipment1.jpg**: Photo of different young scientist (distractor)
- **corin_static.jpg**: Photo with digital corruption/static (distractor)

### Red X Overlay Assets
- **red_x_overlay.png**: Transparent red X marking to overlay on identified scientists
- **target_marked.png**: "MARKED FOR ELIMINATION" stamp overlay

### Technical Specifications
- **Format**: JPG preferred for smaller file sizes
- **Resolution**: 400x300 pixels minimum for clear visibility
- **Aspect Ratio**: 4:3 recommended for consistent display
- **File Size**: Keep under 200KB each for fast loading
- **Style**: Should match Arcane's art style and color palette

### Voice-Friendly Design Requirements for Scientist Photos
- **Distinguishable Features**: Clear facial features, clothing details, and equipment that can be verbally described
- **Age Differences**: Obvious age distinctions between scientists (elderly Werner vs young Corin)
- **Clothing Cues**: Distinctive attire (formal robes, lab coats, tool belts, academy medallions)
- **Equipment Context**: Scientific instruments and tools that help identification
- **Lighting Quality**: Clear lighting on main photos, degraded lighting on distractors
- **Corruption Patterns**: Realistic damage patterns (water damage, digital corruption, shadows) that obscure but don't eliminate key features

### Color Palette Guidelines
- **Piltover**: Golden (#c89b3c), white, clean blues
- **Zaun**: Teal (#00c8c8), greens, industrial grays, shimmer purple/pink
- **General**: Maintain contrast between upper and lower city aesthetics

## File Structure
- **Component**: `Components/Pages/PictureExplanation.razor`
- **Image Assets**: Challenge images in `wwwroot/images/pictures/`
- **Models**: `ImageChallenge`, `RoundData`, `GameSession`
- **Hub Methods**: Implemented in `GameHub.cs`
- **Documentation**: This file (`PictureExplanation.md`)

## Accessibility Features
- **Screen Reader Support**: Alt text for all images
- **Keyboard Navigation**: Full keyboard control of interface
- **High Contrast Options**: Visual accessibility accommodations
- **Text Size Control**: Adjustable text for readability
- **Voice Chat Independence**: Works with any accessible voice solution

## Assessment and Feedback

### Performance Metrics
- **Accuracy Rate**: Percentage of correct image selections
- **Communication Efficiency**: Time taken per round
- **Description Quality**: Subjective assessment of verbal descriptions
- **Learning Progress**: Improvement across multiple play sessions

### Feedback Mechanisms
- **Immediate Results**: Show correct answer after each round
- **Progress Tracking**: Visual indicators of round completion
- **Performance Summary**: End-game statistics and achievements
- **Improvement Suggestions**: Tips for better communication

## Future Enhancements
- **AI Description Analysis**: Automatic evaluation of description quality
- **Custom Image Sets**: Teacher-uploadable image collections
- **Difficulty Adjustment**: Dynamic complexity based on performance
- **Recording Features**: Playback of descriptions for review
- **Multi-language Support**: Images and vocabulary in multiple languages
- **Achievement System**: Recognition for communication excellence
- **Tournament Mode**: Competitive description challenges with leaderboards