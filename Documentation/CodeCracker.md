# CodeCracker - Lexical Puzzle

## Overview
CodeCracker is a cooperative vocabulary-building puzzle where players must decode Dr. Renni Stiltner's hidden message left on her apartment wall. After arriving at Renni's apartment in Zaun and meeting her desperate sister Kira, players discover Renni has left a coded message using their childhood word games. One player (Piltover/Caitlyn) sees the graffiti wall with missing letters while the other (Zaunite/Vi) has Renni's definition paper with clues to complete the message.

## Game Mechanics

### Player Roles
- **Piltover (Caitlyn - Code Analyst)**: Sees Renni's graffiti wall with words containing missing letters (e.g., "SH_MM_R")
- **Zaunite (Vi - Clue Reader)**: Has Renni's definition paper with synonyms and word game clues left for her sister

### Objective
Work together to decode Renni's hidden message by completing the words on the graffiti wall. The completed message will reveal where Renni went - her next location in the dangerous investigation of Project Safeguard.

### Gameplay Flow
1. Both players join the same room using a shared room ID
2. First player becomes Caitlyn (Code Analyst), second becomes Vi (Clue Reader)
3. Caitlyn examines the graffiti wall and sees words with missing letters
4. Vi reads Renni's definition paper and provides synonyms and clues through chat
5. Players work together to complete each word in Renni's coded message
6. Completing all words reveals the hidden message: "SHIMMER FACTORY LEVEL THREE"

### Scoring System
- **Correct Word**: Points awarded for each decoded word in Renni's message
- **Hint Usage**: Each hint slightly reduces points available for the current word
- **Message Revelation**: Major points awarded when the complete hidden message is revealed

## Educational Value

### Primary Skills
- **Vocabulary Building**: Learning Zaun/Piltover specific terminology and industrial vocabulary
- **Code Breaking**: Pattern recognition and logical deduction skills
- **Communication**: Effectively describing clues and working through word puzzles collaboratively
- **Story Comprehension**: Understanding character motivations and following narrative clues

### Target Audience
- ESL (English as Second Language) students learning through immersive storytelling
- Students developing collaborative problem-solving skills
- Players interested in mystery and detective story elements

### Language Features
- Arcane universe vocabulary related to shimmer, factories, and underground locations
- Sister-to-sister coded communication reflecting family bonds and shared childhood games
- Contextual clues that advance the story while building language skills

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
- **Dual-theme Design**: Piltover sees a graffiti wall with black spray-paint lettering; Zaunite sees Renni's lined-paper notebook with a red margin line and handwritten styling
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
- Zaunite initially sees only the definition; additional clues unlock via hints
- Players can communicate via chat at any time
- Each round has a time limit for urgency
- Incorrect guesses may result in point penalties

### Winning Conditions
- Complete all 10 words successfully
- Achieve target score threshold
- Cooperative success - both players win or lose together

## Solutions and Hints

### Current Word Set and Solutions
The puzzle currently uses the following 10 words. Each distorted form (as seen by Piltover) maps to its correct solution:

- sh_mm_r → SHIMMER
- f_ct_ry → FACTORY 
- l_v_l → LEVEL
- thr__ → THREE
- h_dd_n → HIDDEN
- d_ng_r → DANGER
- _sc_pe → ESCAPE
- p_rpl_ → PURPLE
- b_l_w → BELOW
- s_cr_t → SECRET

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

### Renni's Hidden Message Structure
The specific words in Renni's coded message relate to her investigation and whereabouts:

**Word Categories**:
- **Location Words**: SHIMMER, FACTORY (industrial Zaun locations)
- **Direction Words**: LEVEL, THREE (specific location within the factory)
- **Context Words**: Related vocabulary that builds the complete message

**Message Solution**: "SHIMMER FACTORY LEVEL THREE"
This reveals that Renni didn't go into hiding - she went to investigate the old shimmer refinement facility, specifically Level 3 where she believes the other scientists are being held.

### Story Context Integration
- **Kira's Desperation**: Her sister has been missing and she doesn't understand the coded message
- **Childhood Bond**: The word games reflect Renni and Kira's shared past
- **Urgency**: Decoding the message reveals Renni went into extreme danger alone
- **Next Story Beat**: The message triggers the team's realization that they must head to the shimmer factory immediately

## Technical Notes

### SignalR Methods
- `JoinRoom`: Connects player to game room
- `JoinGame` / `JoinGameWithRole`: Assigns role and starts gameplay
- `SubmitGuess`: Processes Piltover's word guess
- `RequestHint`: Requests a staged hint (2 total)
  - Hint 1: Reveals one more letter and unlocks the German translation
  - Hint 2: Reveals one more letter and unlocks the synonym
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