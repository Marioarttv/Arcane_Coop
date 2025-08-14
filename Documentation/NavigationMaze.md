# NavigationMaze - Underground Escape

## Overview
NavigationMaze is a cooperative spatial reasoning puzzle where players must navigate through the dangerous shimmer factory using Dr. Renni's hand-drawn map. After decoding Renni's message revealing she went to "SHIMMER FACTORY LEVEL THREE," the team arrives at the abandoned facility to find an explosion has occurred. One player (Piltover/Caitlyn) reads Renni's tactical map with route annotations, while the other (Zaunite/Vi) navigates through the unstable factory interior, avoiding hazards and Silco's remaining guards.

## Game Mechanics

### Player Roles
- **Piltover (Caitlyn - Map Reader)**: Studies Renni's hand-drawn factory map with route annotations, safe passages, and hazard warnings
- **Zaunite (Vi - Factory Explorer)**: Sees first-person view of the damaged factory interior and makes navigation choices through unstable areas

### Objective
Guide Vi safely through the shimmer factory's 5 levels to reach Level 3 where the holding cells are located, using Renni's map annotations and avoiding structural damage, chemical hazards, and remaining guards.

### Story Integration (Act 1)
- Triggered after Scene 7 (`code_decoded`) via `navigation_maze_transition`
- Auto-join via URL parameters: `?role=...&avatar=...&name=...&squad=...&story=true&transition=FromCodeDecoded`
- Unique room naming: `{squad}_FromCodeDecoded`
- On completion, hub `ContinueStoryAfterNavigationMaze(roomId)` redirects both players to Scene 8 (`empty_cells`, `sceneIndex=10`)

### Gameplay Flow
1. Both players join the same room using a shared room ID
2. First player becomes Caitlyn (Navigator), second becomes Vi (Explorer)
3. Navigator sees tactical intelligence and safe route information
4. Explorer sees immersive location imagery and must choose direction
5. Players communicate via chat to coordinate navigation decisions
6. Progress through 5 increasingly challenging locations

## Factory Navigation Progression

### Level 1: Main Processing Floor (Beginner)
**ESL Focus**: Basic factory directions (left, right, forward)
**Scenario**: Large industrial floor with conveyor belts and processing equipment, purple shimmer residue everywhere
**Correct Path**: FORWARD through the main walkway (leads to Storage Tanks)
**Wrong Choices**:
- LEFT: Collapsed ceiling from recent explosion → Game Over
- RIGHT: Active shimmer leak creating toxic fumes → Game Over

### Level 2: Storage Tank Area (Intermediate)
**ESL Focus**: Directional prepositions (through, under, around)
**Scenario**: Massive shimmer storage tanks with leaking purple chemical and steam
**Correct Path**: AROUND the leaking tank to avoid toxic exposure (leads to Maintenance Corridor)
**Wrong Choices**:
- THROUGH tank corridor: Chemical exposure from shimmer leak → Game Over
- UNDER main pipes: Steam burst from damaged systems → Game Over

### Level 3: Maintenance Corridor (Intermediate+)
**ESL Focus**: Spatial relationships (between, behind, beside)
**Scenario**: Narrow maintenance area with electrical hazards and damaged machinery
**Correct Path**: BESIDE the main electrical panel to avoid live wires (leads to Guard Station)
**Wrong Choices**:
- BETWEEN electrical panels: Electrical shock from damaged wiring → Game Over
- BEHIND machinery: Structural collapse from explosion damage → Game Over

### Level 4: Abandoned Guard Station (Advanced)
**ESL Focus**: Combined spatial vocabulary (up through, down into, across over)
**Scenario**: Multi-level security area with catwalks, stairs, and surveillance equipment
**Correct Path**: ACROSS OVER the metal catwalk (leads to Holding Cells Level 3)
**Wrong Choices**:
- UP THROUGH guard tower: Remaining Silco guards spot movement → Game Over
- DOWN INTO basement: Flooded area with chemical contamination → Game Over

### Level 5: Holding Cells - Level 3 (Discovery)
**ESL Focus**: Success vocabulary and discovery terms
**Scenario**: The destination from Renni's message - prison cells and laboratory equipment
**Outcome**: SUCCESS - Team reaches the holding cells but finds them empty, leading to the discovery of the adjacent laboratory

## Educational Value

### Primary Skills
- **Spatial Reasoning**: Understanding directional relationships and navigation
- **Preposition Mastery**: Learning complex English prepositions through visual context
- **Communication Precision**: Describing spatial relationships clearly
- **Collaborative Problem-Solving**: Working together under pressure

### Target Audience
- ESL students learning spatial vocabulary and prepositions
- Students developing directional communication skills
- Anyone improving spatial reasoning and navigation abilities
- Collaborative learning environments

### Language Features
- **Progressive Difficulty**: From basic directions to complex spatial relationships
- **Visual Context**: Each preposition reinforced with clear visual meaning
- **Memorable Consequences**: Wrong choices provide immediate, story-rich feedback
- **Repetition with Variation**: Same concepts applied across different environments

## Technical Implementation

### Key Components
- **First-Person Imagery**: High-resolution location visuals for Explorer
- **Tactical Map Display**: Strategic overview and route planning for Navigator
- **Choice Validation**: Server-side verification of navigation decisions
- **Atmospheric Storytelling**: Immersive Zaun environment with thematic game-over messages

### Data Models
- **Location**: Contains imagery, descriptions, available choices, and correct paths
- **NavigationChoice**: Represents available directions with outcomes
- **GameProgress**: Tracks current location and player decisions
- **PlayerView**: Role-specific information (tactical maps vs. first-person view)

### User Interface
- **Immersive Location Display**: Full-screen atmospheric imagery for Explorer
- **Tactical Map Interface**: Strategic overview with route indicators for Navigator
- **Choice Selection**: Clear directional options with preposition labels
- **Progress Tracking**: Visual indicators showing advancement through locations

## Game Rules

### Setup
1. Both players must enter the same Room ID
2. Players need unique player names
3. First to join becomes Caitlyn (Navigator), second becomes Vi (Explorer)
4. Navigator receives tactical intelligence, Explorer sees location imagery

### During Gameplay
- Explorer sees detailed location imagery and must choose navigation direction
- Navigator has access to maps, safe routes, and hazard information
- Players communicate via chat to coordinate navigation decisions
- Wrong choices result in thematic game-over scenarios requiring restart
- Correct choices advance to the next location

### Winning Conditions
- Successfully navigate through all 5 factory levels using Renni's map
- Reach the Holding Cells on Level 3 without triggering hazards
- Discover the empty cells and adjacent laboratory where the Alchemy Lab puzzle begins
- Cooperative success - both players advance the investigation together

## Game Over Scenarios

### Factory-Specific Failure Messages
- "The shimmer fumes are too toxic! Even Vi can't push through... Try another route!"
- "That explosion damage was worse than it looked... Reset and find a safer path!"
- "Silco's guards are still patrolling! Stay hidden and try again!"
- "The factory's structural damage is too dangerous... Renni would want you to be careful!"
- "Chemical exposure levels are critical! Find the safe route Renni marked on her map!"

### Educational Value of Failure
- **Immediate Feedback**: Wrong choices provide memorable consequences
- **Thematic Integration**: Game-over messages maintain Arcane universe immersion
- **Learning Reinforcement**: Failures help cement correct spatial vocabulary
- **Low-Stakes Environment**: Safe space to make mistakes and learn

## Solutions and Teaching Tips

### For Game Masters
- Encourage Navigator to describe spatial relationships precisely
- Emphasize the importance of clear communication between players
- Use location imagery to reinforce preposition meanings
- Celebrate successful navigation and spatial vocabulary usage

### Vocabulary Progression
1. **Level 1**: Basic directions (left, right, forward, straight)
2. **Level 2**: Simple prepositions (through, under, around)
3. **Level 3**: Spatial relationships (between, behind, beside)
4. **Level 4**: Complex combinations (up through, down into, across over)

### Teaching Applications
- Spatial vocabulary instruction and assessment
- Preposition usage in context
- Collaborative communication skills
- Navigation and direction-following abilities

## Technical Notes

### SignalR Methods
- `JoinRoom`: Connects players to navigation session
- `JoinGame`: Assigns Navigator/Explorer roles
- `MakeNavigationChoice`: Processes Explorer's directional decisions
- `RequestLocationData`: Provides Navigator with tactical information
- `RestartMission`: Handles game-over restart functionality

### Visual Requirements
- **High-Resolution Imagery**: 1920x1080 location visuals for immersion
- **Consistent Art Style**: Dark, atmospheric Zaun aesthetic
- **Clear Choice Indicators**: Visual distinction between navigation options
- **Accessibility Features**: Text labels and alternative descriptions

### Performance Considerations
- Optimized image loading for smooth location transitions
- Efficient state synchronization between Navigator and Explorer views
- Minimal latency for real-time communication during navigation
- Graceful handling of connection issues during critical decision points

## File Structure
- **Component**: `Components/Pages/NavigationMaze.razor`
- **Location Data**: Detailed progression information (see navigation-locations.md)
- **Visual Assets**: Location imagery in `wwwroot/images/locations/`
- **Hub Methods**: Implemented in `GameHub.cs`
- **Documentation**: This file (`NavigationMaze.md`) and `navigation-locations.md`

## Renni's Hand-Drawn Map (Navigator View)

### Factory Map Layout
```
[START: Main Processing Floor]
        |
        ↓ (FORWARD through walkway)
[Storage Tank Area]
        |
        ↓ (AROUND leaking tank)
[Maintenance Corridor]
        |
        ↓ (BESIDE electrical panel)
[Guard Station]
        |
        ↓ (ACROSS OVER catwalk)
[HOLDING CELLS LEVEL 3 - TARGET!]
```

### Renni's Map Annotations
- **Safe Route**: Hand-drawn red arrows indicating Renni's tested path
- **Hazard Warnings**: Skull symbols marking dangerous areas (chemical leaks, structural damage)
- **Current Position**: Player's location marked with "YOU ARE HERE" in Renni's handwriting
- **Wrong Paths**: X marks over dangerous routes with notes like "TOXIC!" or "UNSTABLE!"
- **Objective**: Large circle around "HOLDING CELLS - LEVEL 3" with question marks indicating uncertainty
- **Personal Notes**: Renni's worried annotations like "Where are they?" and "Too quiet..."

## Future Enhancements
- **Dynamic Locations**: Procedurally generated maze layouts
- **Difficulty Scaling**: Adaptive complexity based on player performance
- **Voice Navigation**: Audio-based direction giving for accessibility
- **Achievement System**: Recognition for perfect navigation or communication
- **Additional Language Support**: Multi-language preposition learning
- **Virtual Reality Integration**: Immersive first-person exploration experience
- **Tournament Mode**: Competitive navigation challenges with leaderboards