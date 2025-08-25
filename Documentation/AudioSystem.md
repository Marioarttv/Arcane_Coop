# 🎵 Visual Novel Audio System Documentation

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Setup & Installation](#setup--installation)
4. [Usage Guide](#usage-guide)
5. [API Reference](#api-reference)
6. [Troubleshooting](#troubleshooting)
7. [Best Practices](#best-practices)

---

## Overview

The Visual Novel Audio System is a comprehensive audio management solution that provides:
- **Background Music** - Loopable ambient tracks with crossfading
- **Sound Effects** - One-shot audio for actions and events
- **Voice Lines** - Character dialogue voiceovers
- **Browser Compatibility** - Handles modern browser autoplay policies
- **Multiple Layers** - Play music, SFX, and voices simultaneously

### Key Features
✅ Automatic browser audio unlock handling  
✅ Smooth crossfading between music tracks  
✅ Volume control per audio type  
✅ Fade in/out transitions  
✅ Audio preloading support  
✅ Memory-efficient cleanup  

---

## Architecture

### Component Structure
```
Audio System
├── Frontend (JavaScript)
│   ├── Howler.js (Audio Library)
│   └── audioManager.js (Management Layer)
├── Backend (C#)
│   ├── IAudioManager (Interface)
│   ├── AudioManager (Service)
│   └── VisualNovelModels (Data Models)
└── Integration
    ├── Blazor Components
    └── SignalR Hubs
```

### Technology Stack
- **Howler.js 2.2.4** - Cross-browser audio library
- **Blazor Server** - Real-time UI updates
- **JavaScript Interop** - Bridge between C# and JS
- **Web Audio API** - Modern browser audio (with HTML5 fallback)

---

## Setup & Installation

### 1. File Structure
Ensure your project has the following structure:
```
wwwroot/
├── lib/
│   └── howler/
│       └── howler.min.js
├── js/
│   └── audioManager.js
└── audio/
    ├── music/
    │   ├── emergency_theme.mp3
    │   └── calm_ambient.mp3
    ├── sfx/
    │   ├── explosion.mp3
    │   └── button_click.mp3
    └── voices/
        ├── vi/
        │   └── vi_line_01.mp3
        └── caitlyn/
            └── cait_line_01.mp3
```

### 2. Configure App.razor
Add scripts to your `Components/App.razor`:
```html
<body>
    <Routes @rendermode="InteractiveServer" />
    <!-- Audio Libraries - Load before Blazor -->
    <script src="https://cdn.jsdelivr.net/npm/howler@2.2.4/dist/howler.min.js"></script>
    <script src="/js/audioManager.js"></script>
    <script src="_framework/blazor.web.js"></script>
</body>
```

### 3. Configure Program.cs
Register the audio service and ensure static files are served:
```csharp
// Add Audio Manager Service
builder.Services.AddScoped<IAudioManager, AudioManager>();

// In the pipeline configuration
app.UseStaticFiles(); // Important for serving audio files
app.MapStaticAssets();
```

### 4. Add Audio Properties to Models
Update your `DialogueLine` model:
```csharp
public class DialogueLine
{
    // ... existing properties ...
    
    // Audio properties
    public string? BackgroundMusic { get; set; }
    public bool BackgroundMusicLoop { get; set; } = true;
    public float BackgroundMusicVolume { get; set; } = 0.7f;
    public bool StopBackgroundMusic { get; set; } = false;
    public int CrossfadeDuration { get; set; } = 2000;
    
    public string? SoundEffect { get; set; }
    public float SoundEffectVolume { get; set; } = 0.8f;
    
    public string? VoiceLine { get; set; }
    public float VoiceLineVolume { get; set; } = 1.0f;
}
```

---

## Usage Guide

### Basic Implementation

#### 1. Inject the Audio Service
```csharp
@page "/my-visual-novel"
@using Arcane_Coop.Services
@inject IAudioManager AudioManager

@code {
    protected override async Task OnInitializedAsync()
    {
        await AudioManager.InitializeAsync();
    }
}
```

#### 2. Play Background Music
```csharp
// Start background music with fade-in
await AudioManager.PlayBackgroundMusicAsync(
    "/audio/music/emergency_theme.mp3",
    volume: 0.7f,
    loop: true,
    fadeIn: 1000
);

// Change music with crossfade
await AudioManager.PlayBackgroundMusicAsync(
    "/audio/music/calm_ambient.mp3",
    crossfade: true,
    crossfadeDuration: 2000
);

// Stop music with fade-out
await AudioManager.StopBackgroundMusicAsync(fadeOut: 1000);
```

#### 3. Play Sound Effects
```csharp
// Play a sound effect
await AudioManager.PlaySoundEffectAsync(
    "/audio/sfx/explosion.mp3",
    volume: 0.8f
);
```

#### 4. Play Voice Lines
```csharp
// Play character voice (stops previous voice)
await AudioManager.PlayVoiceLineAsync(
    "/audio/voices/vi/vi_line_01.mp3",
    volume: 1.0f,
    stopPrevious: true
);
```

### Using with Dialogue System

#### Example 1: Scene with Background Music
```csharp
var scene = new VisualNovelScene
{
    DialogueLines = new List<DialogueLine>
    {
        new DialogueLine
        {
            CharacterId = "narrator",
            Text = "The emergency alarms blare across Piltover...",
            // Start dramatic music
            BackgroundMusic = "/audio/music/emergency_theme.mp3",
            BackgroundMusicLoop = true,
            BackgroundMusicVolume = 0.6f,
            // Play alarm sound effect
            SoundEffect = "/audio/sfx/alarm.mp3",
            SoundEffectVolume = 0.8f
        },
        new DialogueLine
        {
            CharacterId = "vi",
            Text = "We need to move, now!",
            // Play Vi's voice line
            VoiceLine = "/audio/voices/vi/vi_move_now.mp3",
            VoiceLineVolume = 1.0f
        },
        new DialogueLine
        {
            CharacterId = "caitlyn",
            Text = "Wait, something's not right...",
            // Change to suspenseful music
            BackgroundMusic = "/audio/music/suspense.mp3",
            CrossfadeDuration = 2000,
            VoiceLine = "/audio/voices/caitlyn/cait_suspicious.mp3"
        }
    }
};
```

#### Example 2: Action Sequence
```csharp
new DialogueLine
{
    Text = "The door explodes!",
    // Stop current music dramatically
    StopBackgroundMusic = true,
    CrossfadeDuration = 500,
    // Play explosion
    SoundEffect = "/audio/sfx/door_explosion.mp3",
    SoundEffectVolume = 1.0f
},
new DialogueLine
{
    Text = "Through the smoke, figures emerge...",
    // Start action music
    BackgroundMusic = "/audio/music/combat.mp3",
    BackgroundMusicVolume = 0.8f,
    // Footsteps sound
    SoundEffect = "/audio/sfx/footsteps.mp3"
}
```

### Handling Browser Autoplay Policies

Modern browsers block audio until user interaction. The system handles this automatically, but you can also manually unlock:

```csharp
@if (!audioUnlocked)
{
    <button @onclick="UnlockAudio">Click to Enable Audio</button>
}

@code {
    private bool audioUnlocked = false;
    
    private async Task UnlockAudio()
    {
        await JSRuntime.InvokeVoidAsync("audioManager.manualUnlock");
        audioUnlocked = true;
    }
}
```

---

## API Reference

### C# IAudioManager Interface

| Method | Description | Parameters |
|--------|-------------|------------|
| `InitializeAsync()` | Initialize the audio system | None |
| `PlayBackgroundMusicAsync()` | Play background music | `src`, `volume`, `loop`, `fadeIn`, `crossfade`, `crossfadeDuration` |
| `StopBackgroundMusicAsync()` | Stop background music | `fadeOut` |
| `PlaySoundEffectAsync()` | Play a sound effect | `src`, `volume`, `rate` |
| `PlayVoiceLineAsync()` | Play a voice line | `src`, `volume`, `stopPrevious` |
| `StopAllVoiceLinesAsync()` | Stop all voice lines | None |
| `StopAllSoundEffectsAsync()` | Stop all sound effects | None |
| `StopAllAsync()` | Stop all audio | None |
| `SetGlobalVolumeAsync()` | Set master volume | `volume` (0.0 - 1.0) |
| `SetBackgroundMusicVolumeAsync()` | Set music volume | `volume` (0.0 - 1.0) |
| `SetMusicMutedAsync(muted)` | Persistently mute/unmute background music | `muted` (bool) |
| `SetSfxMutedAsync(muted)` | Persistently mute/unmute SFX | `muted` (bool) |
| `SetVoiceMutedAsync(muted)` | Persistently mute/unmute voice lines | `muted` (bool) |
| `SetAllMutedAsync(muted)` | Persistently mute/unmute all audio | `muted` (bool) |
| `PauseAllAsync()` | Pause all audio | None |
| `ResumeAllAsync()` | Resume all audio | None |
| `PreloadAsync()` | Preload audio files | `string[] urls` |
| `DisposeAsync()` | Clean up resources | None |

### JavaScript audioManager API

| Method | Description |
|--------|-------------|
| `playBackgroundMusic(src, options)` | Play background music |
| `stopBackgroundMusic(fadeOut)` | Stop background music |
| `playSoundEffect(src, options)` | Play sound effect |
| `playVoiceLine(src, options)` | Play voice line |
| `stopAll()` | Stop all audio |
| `setGlobalVolume(volume)` | Set master volume |
| `setMusicMuted(muted)` | Persistently mute/unmute background music (gates queued plays) |
| `setSfxMuted(muted)` | Persistently mute/unmute SFX (gates queued plays) |
| `setVoiceMuted(muted)` | Persistently mute/unmute voice lines (gates queued plays) |
| `setAllMuted(muted)` | Persistently mute/unmute all audio; also calls Howler.mute |
| `getStatus()` | Get audio system status |
| `manualUnlock()` | Manually unlock audio context |

---

## Troubleshooting

### Common Issues and Solutions

#### 1. No Audio Playing
**Problem**: Audio commands execute but no sound is heard.

**Solution**:
- Check browser console for errors
- Ensure user has interacted with the page (click/touch)
- Verify audio files exist at specified paths
- Check browser audio permissions
- Try the manual unlock button

#### 2. "Howler is not defined" Error
**Problem**: Howler.js library not loading.

**Solution**:
- Ensure scripts are in correct order in App.razor
- Clear browser cache (Ctrl+F5)
- Check network tab for 404 errors
- Verify CDN is accessible

#### 3. Audio Files Not Found (404)
**Problem**: Audio files return 404 errors.

**Solution**:
- Ensure `app.UseStaticFiles()` is in Program.cs
- Verify file paths start with `/`
- Check file exists in wwwroot folder
- Ensure correct file extension

#### 4. Audio Delays or Stuttering
**Problem**: Audio playback is choppy or delayed.
- If muted audio resumes unexpectedly after tab focus or user interaction, ensure you call the persistent mute APIs (`SetMusicMutedAsync`, `SetSfxMutedAsync`, `SetVoiceMutedAsync`, or `SetAllMutedAsync`). The JS layer now gates queued and resume behaviors by these flags.

**Solution**:
- Use smaller file sizes (compress audio)
- Preload critical audio files
- Use MP3 or OGG format
- Check network latency

### Debug Tools

#### Check Audio Status
```csharp
// In your Blazor component
private async Task CheckAudioStatus()
{
    var status = await JSRuntime.InvokeAsync<object>("audioManager.getStatus");
    Console.WriteLine($"Audio Status: {status}");
}
```

#### Browser Console Commands
```javascript
// Check if Howler loaded
console.log(typeof Howler);

// Check audio manager status
console.log(audioManager.getStatus());

// Manually unlock audio
audioManager.manualUnlock();

// Test simple sound
new Howl({ src: ['/audio/sfx/test.mp3'] }).play();
```

---

## Best Practices

### 1. File Organization
- Keep audio files organized by type (music/sfx/voices)
- Use descriptive filenames
- Maintain consistent naming conventions

### 2. Performance
- **Compress audio files** - Use appropriate bitrates
- **Preload critical audio** - Load important sounds early
- **Limit concurrent sounds** - Don't play too many at once
- **Clean up** - Dispose audio manager when done

### 3. User Experience
- **Provide audio controls** - Volume, mute options
- **Handle unlock gracefully** - Clear messaging about audio
- **Use appropriate volumes** - Music quieter than voices
- **Smooth transitions** - Use crossfades for music changes

### 4. File Formats
- **Music**: MP3 or OGG, 128-192 kbps
- **SFX**: MP3 or OGG, 96-128 kbps
- **Voices**: MP3 or OGG, 96-128 kbps, mono acceptable

### 5. Code Examples

#### Preloading Critical Audio
```csharp
protected override async Task OnInitializedAsync()
{
    // Preload important audio files
    await AudioManager.PreloadAsync(new[] {
        "/audio/music/main_theme.mp3",
        "/audio/sfx/button_click.mp3",
        "/audio/voices/intro_narration.mp3"
    });
}
```

#### Volume Management
```csharp
// Create volume settings component
@code {
    private float masterVolume = 0.7f;
    private float musicVolume = 0.6f;
    
    private async Task UpdateVolumes()
    {
        await AudioManager.SetGlobalVolumeAsync(masterVolume);
        await AudioManager.SetBackgroundMusicVolumeAsync(musicVolume);
    }
}
```

#### Clean Disposal
```csharp
@implements IAsyncDisposable

@code {
    public async ValueTask DisposeAsync()
    {
        await AudioManager.StopAllAsync();
        await AudioManager.DisposeAsync();
    }
}
```

---

## Testing

### Test Page Usage
Navigate to `/test-audio` to access the audio system test page:

1. **Check Status** - View audio system state
2. **Unlock Audio** - Manually unlock browser audio
3. **Test Controls** - Play various audio types
4. **Volume Control** - Adjust audio levels

### Integration Testing
```csharp
// Example test scene with all audio types
var testScene = new VisualNovelScene
{
    DialogueLines = new List<DialogueLine>
    {
        new DialogueLine
        {
            Text = "Testing all audio systems...",
            BackgroundMusic = "/audio/music/test.mp3",
            SoundEffect = "/audio/sfx/test.mp3",
            VoiceLine = "/audio/voices/test.mp3"
        }
    }
};
```

---

## Summary

The Visual Novel Audio System provides a robust, browser-compatible solution for managing game audio. Key points:

1. **Always handle browser autoplay** - Users must interact first
2. **Use proper file paths** - Start with `/audio/`
3. **Manage resources** - Clean up when done
4. **Test thoroughly** - Use the test page
5. **Follow best practices** - Compress files, organize well

For additional help, check:
- Browser console for errors
- `/test-audio` page for testing
- Network tab for file loading issues

The system is designed to be simple to use while providing powerful features for creating immersive audio experiences in your Visual Novel game.