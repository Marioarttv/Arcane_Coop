# SignalDecoder - Communication Under Pressure

## Overview
SignalDecoder is a cooperative listening comprehension puzzle where players must work together to decode fragmented emergency transmissions. One player (Piltover/Caitlyn) sees incomplete text while the other (Zaunite/Vi) receives audio and signal data to help complete the message.

## Game Mechanics

### Player Roles
- **Piltover (Caitlyn - Signal Interceptor)**: Sees corrupted transmissions with missing words (e.g., "Emergency: Code *** in sector *")
- **Zaunite (Vi - Signal Analyst)**: Receives full audio transmission, frequency data, and technical signals like Morse code

### Objective
Work together under time pressure to decode emergency transmissions by filling in the missing words before the signal degrades completely.

### Gameplay Flow
1. Both players join the same room using a shared room ID
2. First player becomes Caitlyn (Interceptor), second becomes Vi (Analyst)
3. Caitlyn sees fragmented emergency messages with missing words
4. Vi receives audio playback and technical signal data
5. Players must collaborate quickly as signals decay over time
6. Complete multiple emergency scenarios with increasing complexity

### Scoring System
- **Speed Bonus**: Points for quick completion of transmissions
- **Accuracy Bonus**: Extra points for correct decoding without errors
- **Emergency Response**: Bonus points for successfully handling crisis situations

## Educational Value

### Primary Skills
- **Listening Comprehension**: Processing audio information accurately
- **Emergency Vocabulary**: Learning crisis-related terminology
- **Real-time Communication**: Coordinating under time pressure
- **Technical Literacy**: Understanding radio frequencies and signal concepts

### Target Audience
- ESL students developing listening skills
- Emergency response trainees
- Students learning crisis communication protocols
- Anyone developing real-time collaboration skills

### Language Features
- Emergency service terminology and protocols
- Technical vocabulary (frequencies, signal types, codes)
- Clear pronunciation for audio comprehension practice
- Progressive complexity from simple alerts to complex emergency scenarios

## Technical Implementation

### Key Components
- **Audio Integration**: Real-time audio playback for signal transmission
- **Signal Decay System**: Time-based degradation of signal quality
- **Emergency Scenarios**: Scripted crisis situations with varying complexity
- **Technical Data Display**: Frequency, Morse code, and signal strength visualization

### Data Models
- **SignalTransmission**: Contains complete message, fragmented version, and audio data
- **TechnicalData**: Frequency information, signal strength, and Morse code patterns
- **EmergencyScenario**: Context and background for each transmission
- **PlayerView**: Role-specific information (fragmented text vs. complete audio/data)

### User Interface
- **Signal Strength Indicator**: Visual representation of transmission quality
- **Frequency Display**: Technical readouts for the Analyst role
- **Audio Controls**: Play/pause/replay functionality for transmissions
- **Time Pressure Indicators**: Countdown and signal decay visualization

## Game Rules

### Setup
1. Both players must enter the same Room ID
2. Players need unique player names
3. Audio permissions required for full gameplay experience
4. First to join becomes Caitlyn (Interceptor), second becomes Vi (Analyst)

### During Gameplay
- Caitlyn types missing words to complete fragmented transmissions
- Vi can replay audio and access technical data but cannot see the text fragments
- Signal quality degrades over time, adding urgency
- Players can communicate via chat for coordination
- Some scenarios may have multiple correct interpretations

### Winning Conditions
- Successfully decode all emergency transmissions
- Respond within time limits before signal loss
- Maintain accuracy across multiple crisis scenarios
- Cooperative success - both players succeed together

## Emergency Scenarios

### Scenario Types
1. **Medical Emergency**: Hospital codes, patient information, medical terminology
2. **Fire Department**: Location data, equipment needs, evacuation procedures
3. **Police Dispatch**: Incident reports, suspect descriptions, unit assignments
4. **Natural Disaster**: Weather alerts, evacuation routes, resource coordination
5. **Technical Failures**: System malfunctions, repair instructions, safety protocols

### Difficulty Progression
- **Level 1**: Simple, clear transmissions with obvious missing words
- **Level 2**: Multiple missing words requiring context understanding
- **Level 3**: Technical jargon and emergency codes
- **Level 4**: Overlapping transmissions and background interference
- **Level 5**: Critical scenarios requiring perfect accuracy and speed

## Solutions and Teaching Tips

### For Game Masters
- Emphasize the importance of clear communication between players
- Encourage the Analyst (Vi) to describe audio details precisely
- Use real emergency protocols as learning material
- Practice emergency vocabulary before gameplay

### Audio Design Principles
- Clear pronunciation with realistic radio static
- Graduated difficulty in audio complexity
- Authentic emergency service terminology
- Background noise and interference for realism

### Teaching Applications
- Emergency response training scenarios
- Listening comprehension assessment
- Real-time communication skills development
- Technical vocabulary acquisition

## Story Integration

### Act 1 Visual Novel → Signal Decoder Transition (2025 Complete Integration)

**Scene 3 to Signal Decoder Transition:**
- Server sends `Act1RedirectToNextGame` to each connection with individualized URL parameters
- Parameters include: `role`, `avatar`, `name`, `squad`, `story=true`, and `transition`
- Example URL: `/signal-decoder?role=zaun&squad=SquadAlpha_FromPicturePuzzle&transition=FromScene3&story=true`

**Role Preservation:**
- `SignalDecoder.razor` reads `role` from URL and calls `JoinSignalDecoderGameWithRole`
- Uses `AddPlayerWithRole()` game logic to maintain Piltover/Zaun assignments from story mode
- Prevents "Game is Full" errors that previously occurred during story transitions

**Story Mode Features:**
- When `story=true`, the UI immediately sets `inGame = true` to skip lobby setup
- Shows "Continue Story" button instead of "Play Again" after puzzle completion
- Seamless integration with visual novel narrative flow

### Transition Parameter System (2025 Update)

**Unique Room ID Generation:**
The Signal Decoder puzzle implements a sophisticated transition parameter system that prevents room name conflicts during story transitions.

**URL Parameter Processing:**
```csharp
// Example transition URL from Scene 3:
/signal-decoder?role=zaun&squad=SquadAlpha_FromPicturePuzzle&transition=FromScene3&story=true

// Transition parameter parsing
string transitionParam = HttpUtility.ParseQueryString(uri.Query)["transition"];
string uniqueRoomId = !string.IsNullOrEmpty(transitionParam) ? 
    $"{squadName}_{transitionParam}" : squadName;

// Result: SquadAlpha_FromPicturePuzzle_FromScene3
```

**Squad Name Extraction for Return Transition:**
```csharp
// When transitioning back to next visual novel scene
string originalSquadName = roomId.Contains("_") ? 
    roomId.Substring(0, roomId.IndexOf("_")) : roomId;
string nextPhaseRoomId = $"{originalSquadName}_FromSignalDecoder";

// From: SquadAlpha_FromPicturePuzzle_FromScene3
// Extract: SquadAlpha
// Create: SquadAlpha_FromSignalDecoder
```

**Critical Benefits:**
- **Eliminates "Game is Full" Errors**: Each story transition uses a unique room identifier
- **Preserves Squad Identity**: Original squad name maintained throughout the story flow
- **Complete Story Integration**: Fixed Scene 3 to Signal Decoder transition that previously went to Picture Explanation
- **Synchronized Navigation**: Both players join the same unique puzzle room
- **Role Continuity**: Player roles (Piltover/Zaun) preserved across story-puzzle boundaries

## Technical Notes

### SignalR Methods
- `JoinRoom`: Connects player to game session
- `JoinGame`: Assigns Interceptor/Analyst roles (legacy method)
- `JoinSignalDecoderGameWithRole(roomId, playerName, requestedRole)`: New story mode method with role preservation
- `PlayAudio`: Triggers audio transmission for Analyst
- `SubmitDecoding`: Processes Interceptor's word guesses
- `RequestReplay`: Allows audio replay with limitations
- `UpdateSignalStrength`: Handles signal decay over time

**Transition Parameter Integration:**
- Room ID creation uses transition parameter: `{squadName}_{transitionParam}`
- `AddPlayerWithRole()` handles story mode player assignment with role preservation
- Squad name extraction for story continuation: `roomId.Substring(0, roomId.IndexOf("_"))`
- Unique room IDs prevent conflicts during story-puzzle transitions

### Audio Requirements
- Browser audio permissions for full experience
- WebRTC support for real-time audio streaming
- Fallback text descriptions for accessibility
- Optimized audio files for quick loading

### Performance Considerations
- Efficient audio streaming to minimize latency
- Progressive signal degradation calculations
- Real-time synchronization between player views
- Graceful handling of connection issues during audio playback

## File Structure
- **Component**: `Components/Pages/SignalDecoder.razor`
- **Models**: `SignalTransmission`, `TechnicalData`, `EmergencyScenario`
- **Audio Assets**: Emergency transmission recordings in `wwwroot/audio/`
- **Hub Methods**: Implemented in `GameHub.cs`
- **Documentation**: This file (`SignalDecoder.md`)

## Accessibility Features
- Visual indicators for audio cues
- Text descriptions of technical data
- Keyboard navigation for all controls
- Adjustable time limits for different skill levels
- Subtitle options for audio transmissions

## Future Enhancements
- Multi-language emergency protocols
- Voice recognition for hands-free operation
- Integration with real emergency service training programs
- Difficulty adjustment based on player performance
- Achievement system for emergency response scenarios
- Multiplayer tournaments with leaderboards