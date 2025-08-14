# SignalDecoder - Communication Under Pressure

## Overview
SignalDecoder is a cooperative listening comprehension puzzle where players intercept and decode fragmented enforcer radio transmissions about the ongoing hunt for Project Safeguard scientists. Following the identification of the four targeted scientists, players discover radio chatter revealing attacks in progress and cover-ups by corrupt enforcers. One player (Piltover/Caitlyn) sees corrupted text transcripts while the other (Zaunite/Vi) receives clear audio transmissions to help complete the urgent intelligence.

**ESL-Friendly Version**: The story mode transmissions have been updated to use common, widely-known words instead of proper names and technical jargon, making the puzzle more accessible to ESL students while maintaining the same story context and gameplay mechanics.

## Game Mechanics

### Player Roles
- **Piltover (Caitlyn - Signal Interceptor)**: Sees corrupted enforcer transmissions with missing words (e.g., "Emergency: *** workshop on *** Street")
- **Zaunite (Vi - Signal Analyst)**: Receives full audio transmissions revealing the attacks on Project Safeguard scientists and Deputy Stanton's cover-up orders

### Objective
Work together under time pressure to decode the urgent enforcer transmissions revealing which scientists have been attacked, where the survivors might be hiding, and what Deputy Stanton is covering up before more lives are lost.

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

## Story-Specific Transmissions (Act 1 Scene 3 → Signal Decoder)

### Context
After identifying the four Project Safeguard scientists through the Picture Explanation puzzle, the team's confiscated enforcer radio begins crackling with urgent transmissions. Players discover that Jinx has already begun her hunt - Werner's workshop has been attacked, Dr. Renni has gone into hiding, and Deputy Stanton is actively covering up evidence. Players must decode these fragmented transmissions to understand the scope of the threat and locate the remaining scientists before it's too late.

**Note**: The transmissions below use ESL-friendly vocabulary, replacing proper names and technical terms with common words to improve accessibility for ESL students.

### Required Voice Lines / Audio Transmissions

#### Transmission 1: Workshop Attack
**Full Audio (Vi hears):** "Emergency dispatch - explosion at small workshop on Main Street. Blue-haired suspect fled the scene. One person confirmed. All units respond."

**Fragmented Text (Caitlyn sees):** "Emergency dispatch - explosion at *** workshop on *** Street. ***-haired suspect fled the scene. One *** confirmed. All units respond."

**Missing Words:** small, Main, blue, person

**Voice Line Files Needed:**
- `transmission1_full.mp3` - Complete emergency dispatch
- Background: Radio static, enforcer sirens

#### Transmission 2: Doctor's Protective Custody Failure
**Full Audio (Vi hears):** "Alert - Dr. old Stiltner failed to report for safe custody. Last known location: repair-tech repair shop above big Market. Consider subject in immediate danger."

**Fragmented Text (Caitlyn sees):** "Alert - Dr. *** Stiltner failed to report for *** custody. Last known location: ***-tech repair shop above *** Market. Consider subject in immediate ***."

**Missing Words:** old, safe, repair, big, danger

**Voice Line Files Needed:**
- `transmission2_full.mp3` - Protective custody alert
- Background: Typing sounds, dispatch center ambiance

#### Transmission 3: Deputy's Cover-up Orders
**Full Audio (Vi hears):** "Deputy chief directive - avoid warehouse district tonight. Evidence removal in progress. old's files require immediate cleaning. No patrol units until further notice."

**Fragmented Text (Caitlyn sees):** "Deputy *** directive - avoid *** district tonight. Evidence *** in progress. ***'s files require immediate ***. No patrol units until further notice."

**Missing Words:** chief, warehouse, removal, old, cleaning

**Voice Line Files Needed:**
- `transmission3_full.mp3` - Stanton's directive
- Background: Muffled conversation, paper shredding

#### Transmission 4: Project Reference
**Full Audio (Vi hears):** "Update on Project secret personnel - two workers confirmed missing. first and second locations unknown. science lab security compromised. Initiate lockdown protocols."

**Fragmented Text (Caitlyn sees):** "Update on Project *** personnel - two *** confirmed missing. *** and *** locations unknown. *** lab security compromised. Initiate lockdown protocols."

**Missing Words:** secret, workers, first, second, science

**Voice Line Files Needed:**
- `transmission4_full.mp3` - Project Safeguard update
- Background: Alarm sounds, security alerts

### Voice Acting Direction

#### General Guidelines
- **Tone:** Professional enforcer dispatch voice, urgent but controlled
- **Clarity:** Clear pronunciation despite radio effects
- **Pacing:** Moderate speed with emphasis on critical information
- **Effects:** Radio compression, slight static, authentic emergency dispatch feel

#### Character-Specific Notes
- **Dispatcher (Transmissions 1-2):** Female voice, professional, concerned but maintaining protocol
- **Enforcer Officer (Transmission 3):** Male voice, slightly nervous about Stanton's orders
- **Security Chief (Transmission 4):** Male voice, authoritative, experienced

### Audio File Structure and Naming Convention
```
/wwwroot/audio/signal-decoder/
├── story/
│   ├── intro_caitlyn.mp3         # "The radio's picking up enforcer chatter!"
│   ├── intro_vi.mp3              # "I can hear the audio clearly..."
│   ├── transmission1_full.mp3    # Werner's workshop attack
│   ├── transmission1_ambient.mp3 # Sirens and radio static
│   ├── transmission1_success.mp3 # "Werner's workshop... too late"
│   ├── transmission2_full.mp3    # Renni's custody failure
│   ├── transmission2_ambient.mp3 # Dispatch center sounds
│   ├── transmission2_success.mp3 # "Renni didn't report..."
│   ├── transmission3_full.mp3    # Stanton's directive
│   ├── transmission3_ambient.mp3 # Paper shredding
│   ├── transmission3_success.mp3 # "Evidence disposal..."
│   ├── transmission4_full.mp3    # Project Safeguard update
│   ├── transmission4_ambient.mp3 # Alarm sounds
│   ├── transmission4_success.mp3 # "Project Safeguard..."
│   └── completion_both.mp3       # "We have to find Renni..."
└── generic/                       # Non-story emergency scenarios
    └── [existing files]
```

### Recording Checklist for Voice Actors

#### Session 1: Main Transmissions (Dispatcher Voice - Female)
- [ ] transmission1_full.mp3 - 10-12 seconds
- [ ] transmission2_full.mp3 - 12-15 seconds
- [ ] Backup takes with varying urgency levels

#### Session 2: Authority Figure (Male Voice)
- [ ] transmission3_full.mp3 - 10-12 seconds
- [ ] transmission4_full.mp3 - 10-12 seconds
- [ ] Additional enforcer background chatter

#### Session 3: Character Reactions (Caitlyn & Vi)
- [ ] intro_caitlyn.mp3 - 3-5 seconds
- [ ] intro_vi.mp3 - 3-5 seconds
- [ ] All success feedback lines - 3-5 seconds each
- [ ] completion_both.mp3 - 5-7 seconds

#### Audio Processing Requirements
- Apply radio filter/compression to all transmission files
- Keep character voices clean with minimal processing
- Ambient tracks should loop seamlessly
- Normalize all files to -3dB peak
- Export as MP3 192kbps for web optimization

### Implementation Notes for Story Mode
- Transmissions should play in sequence when in story mode
- Each transmission increases in urgency and complexity
- Success on all four transmissions triggers story continuation
- Failure allows retry but maintains story context

### Dialogue Context Integration

#### Pre-Puzzle Setup (From Scene 3)
The puzzle begins after the following story beats:
- Players have identified four scientists through the Picture Explanation puzzle
- They discovered these scientists worked on "Project Safeguard"
- An enforcer radio starts crackling with emergency transmissions
- Caitlyn and Vi realize they need to decode these transmissions to find the scientists

#### Key Character Motivations
- **Caitlyn:** Needs to understand Deputy Stanton's cover-up and save the scientists
- **Vi:** Desperate to find out what Silco told Jinx about the warehouse incident
- **Players:** Must decode transmissions quickly to locate Dr. Renni before Jinx does

#### Post-Puzzle Transition (To Scene 4 - Radio Decoded)
After successfully decoding all transmissions, players have uncovered crucial intelligence:
- **Werner Confirmed Dead**: The explosion at his workshop was fatal
- **Renni Still Alive**: She wisely refused protective custody and went into hiding
- **Stanton's Corruption Exposed**: Evidence disposal and cover-up operations revealed
- **Urgent Mission**: Team must immediately head to Renni's apartment in Zaun before Jinx tracks her down
- **Time Pressure**: Every minute wasted increases the danger to the remaining scientists

The decoded transmissions reveal the full scope of Jinx's hunt and Stanton's complicity, driving the team to take immediate action to save Dr. Renni Stiltner.

### Additional Voice Lines for Story Context

#### Introduction (When joining from story)
**Caitlyn:** "The radio's picking up enforcer chatter! Quick, we need to decode this."
**Vi:** "I can hear the audio clearly. Tell me what you're seeing on the transcript."

#### Success Feedback
**Transmission 1 Success:** "Small workshop... an explosion. We're too late for them."
**Transmission 2 Success:** "The old doctor didn't report for custody. Smart person - they don't trust the deputy."
**Transmission 3 Success:** "Evidence removal? The chief's covering up the old files!"
**Transmission 4 Success:** "Project secret... all these workers are being hunted."

#### Completion
**Both Players:** "We have to find Renni before Jinx does. Her apartment in Zaun - let's go!"

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