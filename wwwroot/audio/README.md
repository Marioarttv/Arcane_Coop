# Audio System Documentation

## Overview
The Visual Novel audio system supports three types of audio:
- **Background Music**: Loopable ambient music tracks
- **Sound Effects**: One-shot sound effects for actions and events
- **Voice Lines**: Character dialogue voiceovers

## File Structure
```
audio/
├── music/               # Background music tracks
│   ├── emergency_theme.mp3
│   ├── piltover_ambient.mp3
│   └── zaun_tension.mp3
├── sfx/                 # Sound effects
│   ├── alarm.mp3
│   ├── explosion.mp3
│   └── footsteps.mp3
└── voices/              # Voice lines organized by character
    ├── vi/
    │   └── vi_line_01.mp3
    └── caitlyn/
        └── cait_line_01.mp3
```

## Usage in Dialogue Lines

### Starting Background Music
```csharp
new DialogueLine 
{
    Text = "The emergency alarms blare across Piltover...",
    BackgroundMusic = "/audio/music/emergency_theme.mp3",
    BackgroundMusicLoop = true,
    BackgroundMusicVolume = 0.7f
}
```

### Stopping Background Music
```csharp
new DialogueLine 
{
    Text = "The alarms finally cease...",
    StopBackgroundMusic = true,
    CrossfadeDuration = 2000 // 2 second fade out
}
```

### Playing Sound Effects
```csharp
new DialogueLine 
{
    Text = "An explosion rocks the building!",
    SoundEffect = "/audio/sfx/explosion.mp3",
    SoundEffectVolume = 0.8f
}
```

### Playing Voice Lines
```csharp
new DialogueLine 
{
    CharacterId = "vi",
    Text = "We need to move fast!",
    VoiceLine = "/audio/voices/vi/vi_line_01.mp3",
    VoiceLineVolume = 1.0f
}
```

### Combining Multiple Audio Types
```csharp
new DialogueLine 
{
    CharacterId = "caitlyn",
    Text = "Vi, get down!",
    BackgroundMusic = "/audio/music/tension.mp3", // Changes background music
    SoundEffect = "/audio/sfx/gunshot.mp3",       // Plays gunshot sound
    VoiceLine = "/audio/voices/caitlyn/cait_get_down.mp3", // Plays voice line
    BackgroundMusicVolume = 0.5f,
    SoundEffectVolume = 0.9f,
    VoiceLineVolume = 1.0f
}
```

## Audio File Requirements

### Format Support
- **Recommended**: MP3, OGG
- **Also Supported**: WAV, M4A, WEBM

### File Size Guidelines
- **Background Music**: < 5MB per track
- **Sound Effects**: < 500KB per effect
- **Voice Lines**: < 1MB per line

### Audio Quality
- **Sample Rate**: 44.1kHz or 48kHz
- **Bit Rate**: 128-192 kbps for music, 96-128 kbps for voices
- **Channels**: Stereo for music, mono or stereo for effects/voices

## Testing Audio

1. Place your audio files in the appropriate directories
2. Reference them in dialogue lines using absolute paths (starting with `/audio/`)
3. The audio system will automatically handle:
   - Preloading when possible
   - Crossfading between background tracks
   - Stopping previous voice lines when new ones play
   - Volume normalization

## Example Scene with Audio

```csharp
// Scene 1 - Emergency Briefing
new DialogueLine 
{
    CharacterId = "caitlyn",
    Text = "Squad Alpha, thank you for responding.",
    BackgroundMusic = "/audio/music/emergency_theme.mp3",
    BackgroundMusicLoop = true,
    BackgroundMusicVolume = 0.6f,
    VoiceLine = "/audio/voices/caitlyn/cait_briefing_01.mp3"
},
new DialogueLine 
{
    CharacterId = "vi",
    Text = "What's the situation?",
    VoiceLine = "/audio/voices/vi/vi_situation.mp3"
},
new DialogueLine 
{
    CharacterId = "caitlyn",
    Text = "An explosion just hit the Hextech facility!",
    SoundEffect = "/audio/sfx/distant_explosion.mp3",
    SoundEffectVolume = 0.7f,
    VoiceLine = "/audio/voices/caitlyn/cait_explosion.mp3"
},
new DialogueLine 
{
    CharacterId = "vi",
    Text = "We need to move, now!",
    StopBackgroundMusic = true,
    CrossfadeDuration = 1000,
    BackgroundMusic = "/audio/music/action_theme.mp3",
    BackgroundMusicVolume = 0.8f,
    VoiceLine = "/audio/voices/vi/vi_move_now.mp3"
}
```

## Troubleshooting

### Audio Not Playing
- Check browser console for errors
- Ensure file paths start with `/audio/`
- Verify file exists in wwwroot/audio directory
- Check browser audio permissions

### Audio Cutting Out
- Reduce concurrent audio streams
- Check file sizes are within guidelines
- Ensure proper audio format

### Volume Issues
- Adjust individual volume properties (0.0 to 1.0)
- Check system/browser volume settings
- Use AudioManager.SetGlobalVolumeAsync() for master volume