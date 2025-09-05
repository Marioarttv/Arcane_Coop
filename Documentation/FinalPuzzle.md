# Final Puzzle - AI Debate System

## Overview
The Final Puzzle is an innovative cooperative debate system where two players engage in a high-stakes discussion with AI-controlled characters (Jinx and Silco) about the fate of a captured enforcer. Players use voice input to make their arguments, which are transcribed and processed through OpenAI's GPT-4 mini, with responses voiced by ElevenLabs TTS.

## Key Features
- **Voice-Based Interaction**: Players speak their arguments using microphone input
- **AI-Powered NPCs**: Jinx and Silco respond dynamically based on conversation context
- **Turn-Based Debate**: Structured alternation between players with server-enforced turns
- **Real-Time Transcription**: OpenAI Whisper converts speech to text
- **Character Voices**: ElevenLabs generates unique voices for each character
- **Visual Novel Display**: Beautiful character portraits with emotion states
- **Multiplayer Sync**: Both players see and hear everything in real-time

## Technical Architecture

### Components
1. **FinalPuzzle.razor**: Main UI component with audio recording and visual display
2. **FinalPuzzleGame.cs**: Game state model tracking conversation and turns
3. **DebateAIService.cs**: Service handling OpenAI and ElevenLabs API integration
4. **GameHub.cs**: SignalR hub methods for multiplayer coordination

### Flow
1. Both players join the debate room
2. Introduction scene with hardcoded Jinx & Silco dialogue
3. Player A (defender) records their argument
4. Audio → Whisper transcription → Server processing
5. GPT-4 mini generates Jinx/Silco response based on context
6. ElevenLabs creates voice for the response
7. Both players see text animation and hear audio
8. Player B (accuser) takes their turn
9. Continue until debate concludes

## Configuration Required

### 1. API Keys
Add your API keys to `appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key-here"
  },
  "ElevenLabs": {
    "ApiKey": "your-elevenlabs-api-key-here"
  }
}
```

### 2. ElevenLabs Voice Setup
The system uses specific voice IDs for characters. You need to either:
- Use the default voice IDs provided (may not work without proper account)
- Create/clone voices in your ElevenLabs account and update the IDs in `DebateAIService.cs`:
```csharp
private readonly Dictionary<DebateSpeaker, string> _voiceIds = new()
{
    { DebateSpeaker.Jinx, "your-jinx-voice-id" },
    { DebateSpeaker.Silco, "your-silco-voice-id" }
};
```

### 3. Character Images
Add character emotion portraits to `wwwroot/images/`:
- jinx_neutral.jpg, jinx_angry.jpg, jinx_amused.jpg, jinx_frustrated.jpg
- silco_neutral.jpg, silco_confident.jpg, silco_thoughtful.jpg, silco_frustrated.jpg
- council-chamber.jpg (background)

## Story Integration

### From Act 1 Scene
The puzzle integrates with the story progression system:
- Entry URL: `/finalpuzzle?role={role}&squad={squad}&story=true&transition=FromWarehouse`
- Completion redirects to: `/act1-multiplayer?sceneIndex=23` (truth_revealed scene)

### Room Management
- Unique room IDs prevent conflicts: `{squadName}_FinalDebate_{transition}`
- Automatic player kick for duplicate names (reconnection support)
- Thread-safe player management with ConcurrentDictionary

## Development Notes

### Browser Requirements
- **Microphone Permission**: Required for audio recording
- **Modern Browser**: Chrome/Edge recommended for best MediaRecorder support
- **HTTPS**: Required for microphone access in production

### Error Handling
- Graceful fallback if microphone unavailable
- Clear error messages for API failures
- Turn validation prevents out-of-order speaking
- Automatic cleanup on disconnect

### Performance Considerations
- Audio data sent as byte arrays via SignalR
- Base64 encoding for client playback
- Typewriter animation cancellable between messages
- Conversation history limited to last 10 messages for context

## Testing the Puzzle

### Standalone Testing
1. Navigate to `/finalpuzzle?role=piltover&squad=TestSquad&name=Player1`
2. Open another browser/tab with role=zaun
3. Both players will join and debate begins

### Story Mode Testing
1. Complete previous puzzles or use scene selection
2. System automatically redirects with proper parameters
3. Role preservation maintained from character selection

## Customization Options

### Debate Topics
Modify in `FinalPuzzleGame.cs`:
```csharp
public string DebateTopic { get; set; } = "Your custom debate topic";
```

### AI Personalities
Adjust prompts in `DebateAIService.cs`:
- `BuildJinxPrompt()`: Chaotic, explosive personality
- `BuildSilcoPrompt()`: Calculating, manipulative personality

### Turn Limits
Change conclusion conditions in `GameHub.cs`:
```csharp
if (game.TurnNumber >= 8 || ShouldConcludeDebate(game.ConversationHistory))
```

### Response Length
Modify in `DebateAIService.cs`:
```csharp
var chatRequest = new ChatRequest(messages, Model.GPT4oMini, 
    temperature: 0.8, 
    maxTokens: 150); // Adjust token limit
```

## Troubleshooting

### Common Issues

1. **"Microphone not available"**
   - Ensure browser has microphone permission
   - Check if another app is using the microphone
   - Try different browser

2. **"AI processing error"**
   - Verify API keys are correct
   - Check OpenAI/ElevenLabs account credits
   - Review console logs for specific error

3. **No Audio Playback**
   - Check browser autoplay policies
   - Ensure audio elements not blocked
   - Verify ElevenLabs voice IDs are valid

4. **Players Can't Join**
   - Ensure unique player names
   - Check SignalR connection status
   - Verify room ID parameters match

## Future Enhancements

- Multiple debate scenarios with branching outcomes
- Dynamic voice selection based on character emotion
- Save/replay debate transcripts
- AI-powered debate scoring and feedback
- Support for more than 2 players (jury system)
- Integration with player profiles for personalized debates