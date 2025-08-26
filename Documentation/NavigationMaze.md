# NavigationMaze - Underground Escape (Notes + First-Person v2)

## Overview
NavigationMaze is now a cooperative deduction puzzle based on jumbled field notes and first-person visuals. After decoding Renni's message revealing she went to "SHIMMER FACTORY LEVEL THREE," the team reaches the abandoned facility. One player (Piltover/Caitlyn) receives a shuffled set of Renni's cryptic notes. The other (Zaunite/Vi) sees a first-person view of the current area. By describing what they see (features/landmarks), the Navigator selects the relevant note to call the correct direction and guide the Explorer through 5 levels.

## Game Mechanics

### Player Roles
- **Piltover (Caitlyn - Notes Navigator)**: Reads a shuffled list of Renni's notes. Must match notes to Vi's described landmarks and call the correct direction.
- **Zaunite (Vi - Factory Explorer)**: Sees first-person images with distinct features (tags) and makes choices based on the Navigator's guidance.

### Objective
Guide Vi safely through the shimmer factory's 5 levels to reach Level 3 where the holding cells are located, using Renni's map annotations and avoiding structural damage, chemical hazards, and remaining guards.

### Story Integration (Act 1)
- Triggered after Scene 7 (`code_decoded`) via `navigation_maze_transition`
- Auto-join via URL parameters: `?role=...&avatar=...&name=...&squad=...&story=true&transition=FromCodeDecoded`
- Unique room naming: `{squad}_FromCodeDecoded`
- On completion, hub `ContinueStoryAfterNavigationMaze(roomId)` redirects both players to Scene 8 (`empty_cells`, `sceneIndex=10`)

### Gameplay Flow
1. Both players join the same room (`{squad}_{transition}` in story mode)
2. Roles assign: Piltover = Notes, Zaun = First-person
3. Navigator reads jumbled notes; Explorer describes the scene (landmarks/features)
4. Navigator identifies the matching note and calls the direction; Explorer selects it
5. Correct directions advance to next level; wrong directions end the run
6. Progress through 5 increasingly challenging locations

## Factory Navigation Progression (5 Levels)

Each level includes: visual features (tags), available choices (displayed to Zaun), and one correct move implied by a note. The Piltover player sees a shuffled list of notes (see Prompts below) and must match by features.
 - Design constraint: Each first-person scene must clearly depict three distinct path options to choose from (except the final victory view).

### Level 1: Sewer Entrance (Beginner)
- **Visual tags**: sewer, three-tunnels, slime, rats, faint-light
- **Choices**: LEFT (green glow), RIGHT (pitch black), FORWARD (faint light)
- **Correct**: FORWARD
- **Story beat**: Entry into shimmer tunnels beneath the factory

### Level 2: Industrial Pipe Junction (Easy)
- **Visual tags**: pipes, steam, valves, gauges
- **Choices**: THROUGH the large pipe, UNDER the main pipe, AROUND the pipes to the RIGHT
- **Correct**: AROUND the pipes to the RIGHT
- **Story beat**: Avoiding failing steam pipes after prior explosions

### Level 3: Pump Room Junction (Normal)
- **Visual tags**: pump-room, green-glow, hazard-signs
- **Choices**: BETWEEN the vats, BEHIND the machinery, BESIDE the main tank
- **Correct**: BESIDE the main tank
- **Story beat**: Simple pump chamber with a clear safe walkway

### Level 4: Catwalk Junction (Hard)
- **Visual tags**: catwalk, stairwell, maintenance-ramp, handrails
- **Choices**: UP the STAIRS, DOWN the RAMP, ACROSS the CATWALK
- **Correct**: ACROSS the CATWALK
- **Story beat**: Cross the maintenance catwalk to reach the service bridge

### Level 5: Storage Access Bay (Victory)
- **Visual tags**: storage-bay, roll-up-door, keypad, warning-signs
- **Choices**: LEFT (blocked storage aisle), RIGHT (blocked forklift lane), FORWARD (secured storage door - destination)
- **Outcome**: Mission complete; continue story
- **Story beat**: Arrive at secured storage access where hostages are suspected; transition to `empty_cells`

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
- ~~Tactical Map Display: Strategic overview and route planning for Navigator~~
- **Jumbled Notes Display**: Shuffled list of field notes for Navigator
- **Choice Validation**: Server-side verification of navigation decisions
- **Atmospheric Storytelling**: Immersive Zaun environment with thematic game-over messages

### Data Models (v2)
- **NavigationLocation**: imagery, descriptions, choices, `Tags`
- **NavigationNote/NavigationNotePublic**: jumbled notes (server/client)
- **NavigationPlayerView**: Piltover gets `Notes`; Zaun gets first-person fields
- **NavigationGameState**: unchanged

### User Interface
- **Immersive Location Display**: Full-screen first-person imagery for Explorer with three clear path options
- **Jumbled Notes List (Navigator)**: Shuffled field notes to match against described features
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
- **Visual Assets**: Location imagery in `wwwroot/images/navigation/`
- **Hub Methods**: Implemented in `GameHub.cs`
- **Documentation**: This file (`NavigationMaze.md`) and `navigation-locations.md`

## Image Prompts (for artists or generators)
Use these prompts to create consistent, recognizably distinct first-person images that match the tags and support the notes.

1) Sewer Entrance — first-person view
- Prompt: "Arcane-inspired undercity sewer entrance at night, first-person perspective. Three stone tunnels ahead (show three distinct path options): left tunnel glows toxic green, right tunnel is pitch black, center tunnel shows a faint distant light. Wet stone, puddles, subtle rats, green slime. Cinematic rim lighting, moody teal/purple palette."

2) Industrial Pipe Junction — first-person view
- Prompt: "Massive rusted industrial pipes crisscross a low chamber, first-person. Steam vents hiss; analog gauges rattle; valve wheels drip. Present three path options: THROUGH the main pipe, UNDER the main pipe, AROUND to the RIGHT on a safer walkway. Harsh side-light, volumetric steam, cyan highlights, brass accents."

3) Pump Room Junction — first-person view
- Prompt: "Simple industrial pump room, first-person. One big cylindrical tank on the left with a sturdy walkway BESIDE it, a cramped gap BETWEEN two small tanks ahead, and a cluttered space BEHIND basic machinery on the right. Clear signage, soft green glow, minimal clutter. Three obvious path options."

4) Catwalk Junction — first-person view
- Prompt: "Industrial catwalk junction inside a factory, first-person. Three clear path options: UP a metal STAIRWELL to a higher platform, DOWN a gentle MAINTENANCE RAMP, and ACROSS a straight CATWALK with handrails. No people, clean composition, yellow safety paint, overhead piping."

5) Storage Access Bay — first-person view
- Prompt: "Factory storage access bay, first-person. Present three clear path options: LEFT into a blocked STORAGE AISLE (yellow caution tape/barrier), RIGHT into a blocked FORKLIFT LANE (cones or barrier), and FORWARD toward a WIDE ROLL-UP STORAGE DOOR with an industrial KEYPAD panel (destination). Concrete floor markings, pallet racks to the sides, overhead strip lights. Clean, no people, minimal clutter, lore-neutral."

## Future Enhancements
- **Dynamic Locations**: Procedurally generated maze layouts
- **Difficulty Scaling**: Adaptive complexity based on player performance
- **Voice Navigation**: Audio-based direction giving for accessibility
- **Achievement System**: Recognition for perfect navigation or communication
- **Additional Language Support**: Multi-language preposition learning
- **Virtual Reality Integration**: Immersive first-person exploration experience
- **Tournament Mode**: Competitive navigation challenges with leaderboards