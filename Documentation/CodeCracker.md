# CodeCracker - Lexical Puzzle

## Overview
CodeCracker is a cooperative vocabulary-building puzzle where two players must work together to decode corrupted words. Set in the Arcane universe, one player (Piltover/Caitlyn) sees distorted words while the other (Zaunite/Vi) receives clues to help decode them.

## Game Mechanics

### Player Roles
- **Piltover (Caitlyn)**: Sees corrupted words with missing letters (e.g., "m_r_u_")
- **Zaunite (Vi)**: Receives detailed clues including definitions, German translations, and synonyms

### Objective
Work together to correctly decode all 10 corrupted words within the time limit.

### Gameplay Flow
1. Both players join the same room using a shared room ID
2. First player becomes Piltover, second becomes Zaunite
3. Piltover sees a corrupted word and must guess the complete word
4. Zaunite receives clues and helps Piltover through chat communication
5. Players score points for correct answers and speed
6. Game progresses through 10 rounds with increasing difficulty

### Scoring System
- **Correct Answer**: Base points awarded
- **Time Bonus**: Extra points for quick responses
- **Streak Bonus**: Additional points for consecutive correct answers

## Educational Value

### Primary Skills
- **Vocabulary Building**: Expanding English word knowledge
- **Communication**: Describing and interpreting clues
- **Collaboration**: Working together under time pressure

### Target Audience
- ESL (English as Second Language) students
- German speakers learning English (includes German translations)
- Students developing communication skills

### Language Features
- English-German word pairs for bilingual learning
- Definitions and synonyms for comprehensive understanding
- Progressive difficulty to scaffold learning

## Technical Implementation

### Key Components
- **SignalR Integration**: Real-time multiplayer communication
- **Room-based Architecture**: Private game sessions with shared codes
- **Role Assignment**: Automatic Piltover/Zaunite assignment based on join order
- **State Synchronization**: Server-side validation with client updates

### Data Models
- **GameState**: Tracks current round, scores, and game status
- **WordData**: Contains corrupted word, solution, clues, and translations
- **PlayerView**: Role-specific information (corrupted word vs. clues)

### User Interface
- **Dual-theme Design**: Piltover (blue/gold) and Zaunite (teal/green) aesthetics
- **Real-time Chat**: Built-in communication system
- **Progress Tracking**: Visual indicators for rounds and scores
- **Responsive Layout**: Works on desktop and mobile devices

## Game Rules

### Setup
1. Both players must enter the same Room ID
2. Players need unique player names
3. Connection to SignalR hub required
4. First to join becomes Piltover, second becomes Zaunite

### During Gameplay
- Piltover types their guess for the corrupted word
- Zaunite can see all clues but not the corrupted version
- Players can communicate via chat at any time
- Each round has a time limit for urgency
- Incorrect guesses may result in point penalties

### Winning Conditions
- Complete all 10 words successfully
- Achieve target score threshold
- Cooperative success - both players win or lose together

## Solutions and Hints

### For Game Masters
The puzzle includes a variety of word types:
- Common vocabulary (everyday words)
- Academic vocabulary (more challenging terms)
- Thematic words related to Arcane universe when appropriate

### Teaching Tips
- Encourage players to communicate clearly
- Use German translations as scaffolding for German speakers
- Focus on word patterns and letter combinations
- Celebrate collaborative problem-solving

### Difficulty Progression
- **Early rounds**: Simple, common words with obvious patterns
- **Middle rounds**: More complex vocabulary with subtle clues
- **Late rounds**: Advanced words requiring strong collaboration

## Technical Notes

### SignalR Methods
- `JoinRoom`: Connects player to game room
- `JoinGame`: Assigns role and starts gameplay
- `SubmitGuess`: Processes Piltover's word guess
- `SendMessage`: Handles chat communication
- `LeaveRoom`: Graceful disconnection

### Error Handling
- Connection loss recovery
- Invalid room ID handling
- Player limit enforcement (max 2 players)
- Graceful degradation for network issues

### Performance Considerations
- Efficient state updates to minimize SignalR traffic
- Client-side validation with server-side verification
- Optimized UI updates for smooth real-time experience

## File Structure
- **Component**: `Components/Pages/CodeCracker.razor`
- **Models**: Word data and game state models
- **Hub Methods**: Implemented in `GameHub.cs`
- **Documentation**: This file (`CodeCracker.md`)

## Future Enhancements
- Dynamic difficulty adjustment based on player performance
- Expanded word database with categories
- Audio pronunciation guides
- Achievement system for long-term engagement
- Tournament mode for classroom competitions