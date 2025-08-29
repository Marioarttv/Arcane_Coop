// Audio Manager for Visual Novel System
// Manages background music, sound effects, and voice lines using Howler.js

console.log('[AudioManager] Script loaded, initializing...');

// Create audioManager immediately with stub methods if Howler isn't ready
window.audioManager = (function () {
    // Check if Howler is available
    let howlerReady = typeof Howler !== 'undefined';
    
    if (!howlerReady) {
        console.warn('[AudioManager] Howler.js not yet loaded, creating stub methods');
    }
    let tracks = {
        backgroundMusic: null,
        voiceLines: {},
        soundEffects: {}
    };
    
    let currentBackgroundMusic = null;
    let fadeInterval = null;
    let isUnlocked = false;
    let pendingActions = [];
    
    // Persistent mute state per channel and global
    let muteState = {
        music: false,
        sfx: false,
        voice: false,
        all: false
    };

    // Persistent per-channel volume multipliers (0.0 - 1.0) that override per-line volumes
    // Load from localStorage if available
    function readStoredVolume(key, fallback) {
        const raw = localStorage.getItem(key);
        const num = raw !== null ? parseFloat(raw) : NaN;
        if (isNaN(num)) return fallback;
        return Math.max(0, Math.min(1, num));
    }

    const volumeMultipliers = {
        music: readStoredVolume('audioVolume_music', 1.0),
        sfx: readStoredVolume('audioVolume_sfx', 1.0),
        voice: readStoredVolume('audioVolume_voice', 1.0)
    };

    // Base volume for current background music (pre-multiplier)
    let baseMusicVolume = 1.0;

    function clamp01(value) {
        return Math.max(0, Math.min(1, value));
    }
    
    // Initialize the audio manager
    function init() {
        // Check if Howler loaded correctly
        if (typeof Howler === 'undefined') {
            console.error('[AudioManager] Howler.js not loaded! Retrying...');
            setTimeout(init, 100);
            return;
        }
        
        if (!howlerReady) {
            howlerReady = true;
            console.log('[AudioManager] Howler.js now available, initializing audio system');
        }
        
        // Set global volume
        Howler.volume(1.0);
        
        // Setup unlock handler for browser autoplay policies
        setupUnlockHandler();
        
        console.log('[AudioManager] Initialized - Audio will unlock on first user interaction');
    }
    
    // Setup unlock handler for browser autoplay policies
    function setupUnlockHandler() {
        // Handle audio context suspension
        if (Howler.ctx) {
            console.log('[AudioManager] Audio context state:', Howler.ctx.state);
            
            if (Howler.ctx.state === 'suspended') {
                // Try to resume on any user interaction
                const resumeAudio = () => {
                    Howler.ctx.resume().then(() => {
                        console.log('[AudioManager] Audio context resumed');
                        isUnlocked = true;
                        processPendingActions();
                    });
                };
                
                ['click', 'touchstart', 'touchend', 'keydown'].forEach(event => {
                    document.addEventListener(event, resumeAudio, { once: true });
                });
            } else if (Howler.ctx.state === 'running') {
                isUnlocked = true;
            }
        }
    }
    
    // Process pending audio actions after unlock
    function processPendingActions() {
        while (pendingActions.length > 0) {
            const action = pendingActions.shift();
            action();
        }
    }
    
    // Play background music with optional fade-in
    function playBackgroundMusic(src, options = {}) {
        const {
            volume = 0.7,
            loop = true,
            fadeIn = 1000,
            crossfade = true,
            crossfadeDuration = 2000
        } = options;
        
        console.log(`[AudioManager] Playing background music: ${src}`);
        
        // Respect mute state
        if (muteState.all || muteState.music) {
            console.log('[AudioManager] Music play suppressed due to mute');
            return;
        }
        
        // If audio is not unlocked yet, queue this action
        if (!isUnlocked && Howler.ctx && Howler.ctx.state === 'suspended') {
            console.log('[AudioManager] Audio not unlocked yet, queueing background music');
            pendingActions.push(() => playBackgroundMusic(src, options));
            return;
        }
        
        // If same track is already playing, don't restart
        if (currentBackgroundMusic === src && tracks.backgroundMusic && tracks.backgroundMusic.playing()) {
            console.log('[AudioManager] Background music already playing');
            return;
        }
        
        // Handle crossfade if there's existing music
        if (tracks.backgroundMusic && tracks.backgroundMusic.playing() && crossfade) {
            const oldTrack = tracks.backgroundMusic;
            
            // Fade out old track
            oldTrack.fade(oldTrack.volume(), 0, crossfadeDuration);
            oldTrack.once('fade', () => {
                oldTrack.stop();
                oldTrack.unload();
            });
        } else if (tracks.backgroundMusic) {
            // Stop immediately if not crossfading
            tracks.backgroundMusic.stop();
            tracks.backgroundMusic.unload();
        }
        
        // Track and base volume (pre-multiplier)
        baseMusicVolume = clamp01(volume);

        // Create and play new track
        tracks.backgroundMusic = new Howl({
            src: [src],
            loop: loop,
            volume: fadeIn > 0 ? 0 : clamp01(baseMusicVolume * volumeMultipliers.music),
            html5: true, // Use HTML5 audio for better streaming
            preload: true,
            autoplay: false, // Don't autoplay, we'll call play() manually
            onload: function() {
                console.log(`[AudioManager] Background music loaded: ${src}`);
            },
            onloaderror: function(id, error) {
                console.error(`[AudioManager] Failed to load background music: ${src}`, error);
                console.error('Make sure the audio file exists at the specified path');
            },
            onplayerror: function(id, error) {
                console.error(`[AudioManager] Playback error for background music: ${src}`, error);
                // Try to unlock and play again
                tracks.backgroundMusic.once('unlock', function() {
                    if (!(muteState.all || muteState.music)) {
                        tracks.backgroundMusic.play();
                    }
                });
            },
            onplay: function() {
                console.log(`[AudioManager] Background music started: ${src}`);
                if (fadeIn > 0) {
                    tracks.backgroundMusic.fade(0, clamp01(baseMusicVolume * volumeMultipliers.music), fadeIn);
                }
            }
        });
        
        tracks.backgroundMusic.play();
        currentBackgroundMusic = src;
        // Attach base volume so multiplier changes can be applied later
        try { tracks.backgroundMusic._baseVolume = baseMusicVolume; } catch {}
    }
    
    // Stop background music with optional fade-out
    function stopBackgroundMusic(fadeOut = 1000) {
        if (!tracks.backgroundMusic) return;
        
        console.log('[AudioManager] Stopping background music');
        
        if (fadeOut > 0) {
            tracks.backgroundMusic.fade(tracks.backgroundMusic.volume(), 0, fadeOut);
            tracks.backgroundMusic.once('fade', () => {
                tracks.backgroundMusic.stop();
                tracks.backgroundMusic.unload();
                tracks.backgroundMusic = null;
                currentBackgroundMusic = null;
            });
        } else {
            tracks.backgroundMusic.stop();
            tracks.backgroundMusic.unload();
            tracks.backgroundMusic = null;
            currentBackgroundMusic = null;
        }
    }
    
    // Play a sound effect
    function playSoundEffect(src, options = {}) {
        const {
            volume = 0.8,
            rate = 1.0,
            onEnd = null
        } = options;
        
        console.log(`[AudioManager] Playing sound effect: ${src}`);
        
        // Respect mute state
        if (muteState.all || muteState.sfx) {
            console.log('[AudioManager] SFX play suppressed due to mute');
            return;
        }
        
        // If audio is not unlocked yet, queue this action
        if (!isUnlocked && Howler.ctx && Howler.ctx.state === 'suspended') {
            console.log('[AudioManager] Audio not unlocked yet, queueing sound effect');
            pendingActions.push(() => playSoundEffect(src, options));
            return;
        }
        
        // Create unique ID for this sound effect instance
        const id = `sfx_${Date.now()}_${Math.random()}`;
        
        const baseVolume = clamp01(volume);
        tracks.soundEffects[id] = new Howl({
            src: [src],
            volume: clamp01(baseVolume * volumeMultipliers.sfx),
            rate: rate,
            html5: false, // Use Web Audio for sound effects (better for short sounds)
            onend: function() {
                console.log(`[AudioManager] Sound effect ended: ${src}`);
                // Clean up
                if (tracks.soundEffects[id]) {
                    tracks.soundEffects[id].unload();
                    delete tracks.soundEffects[id];
                }
                if (onEnd) onEnd();
            },
            onloaderror: function(id, error) {
                console.error(`[AudioManager] Failed to load sound effect: ${src}`, error);
                console.error('Make sure the audio file exists at the specified path');
                delete tracks.soundEffects[id];
            },
            onplayerror: function(id, error) {
                console.error(`[AudioManager] Playback error for sound effect: ${src}`, error);
                // Try to unlock and play again
                tracks.soundEffects[id].once('unlock', function() {
                    if (!(muteState.all || muteState.sfx)) {
                        tracks.soundEffects[id].play();
                    }
                });
            }
        });
        
        tracks.soundEffects[id].play();
        try { tracks.soundEffects[id]._baseVolume = baseVolume; } catch {}
        return id;
    }
    
    // Play a voice line
    function playVoiceLine(src, options = {}) {
        const {
            volume = 1.0,
            onEnd = null,
            stopPrevious = true
        } = options;
        
        console.log(`[AudioManager] Playing voice line: ${src}`);
        
        // Respect mute state
        if (muteState.all || muteState.voice) {
            console.log('[AudioManager] Voice play suppressed due to mute');
            return;
        }
        
        // If audio is not unlocked yet, queue this action
        if (!isUnlocked && Howler.ctx && Howler.ctx.state === 'suspended') {
            console.log('[AudioManager] Audio not unlocked yet, queueing voice line');
            pendingActions.push(() => playVoiceLine(src, options));
            return;
        }
        
        // Stop previous voice line if requested
        if (stopPrevious) {
            stopAllVoiceLines();
        }
        
        // Create unique ID for this voice line
        const id = `voice_${Date.now()}_${Math.random()}`;
        
        const baseVolume = clamp01(volume);
        tracks.voiceLines[id] = new Howl({
            src: [src],
            volume: clamp01(baseVolume * volumeMultipliers.voice),
            html5: false, // Use Web Audio for voice lines
            onend: function() {
                console.log(`[AudioManager] Voice line ended: ${src}`);
                // Clean up
                if (tracks.voiceLines[id]) {
                    tracks.voiceLines[id].unload();
                    delete tracks.voiceLines[id];
                }
                if (onEnd) onEnd();
            },
            onloaderror: function(id, error) {
                console.error(`[AudioManager] Failed to load voice line: ${src}`, error);
                console.error('Make sure the audio file exists at the specified path');
                delete tracks.voiceLines[id];
            },
            onplayerror: function(id, error) {
                console.error(`[AudioManager] Playback error for voice line: ${src}`, error);
                // Try to unlock and play again
                tracks.voiceLines[id].once('unlock', function() {
                    if (!(muteState.all || muteState.voice)) {
                        tracks.voiceLines[id].play();
                    }
                });
            }
        });
        
        tracks.voiceLines[id].play();
        try { tracks.voiceLines[id]._baseVolume = baseVolume; } catch {}
        return id;
    }
    
    // Stop all voice lines
    function stopAllVoiceLines() {
        Object.keys(tracks.voiceLines).forEach(id => {
            if (tracks.voiceLines[id]) {
                tracks.voiceLines[id].stop();
                tracks.voiceLines[id].unload();
                delete tracks.voiceLines[id];
            }
        });
    }
    
    // Stop all sound effects
    function stopAllSoundEffects() {
        Object.keys(tracks.soundEffects).forEach(id => {
            if (tracks.soundEffects[id]) {
                tracks.soundEffects[id].stop();
                tracks.soundEffects[id].unload();
                delete tracks.soundEffects[id];
            }
        });
    }
    
    // Stop all audio
    function stopAll() {
        stopBackgroundMusic(0);
        stopAllVoiceLines();
        stopAllSoundEffects();
    }
    
    // Set global volume
    function setGlobalVolume(volume) {
        Howler.volume(Math.max(0, Math.min(1, volume)));
    }
    
    // Set background music volume
    function setBackgroundMusicVolume(volume) {
        // Update base (pre-multiplier) volume and apply multiplier
        baseMusicVolume = clamp01(volume);
        if (tracks.backgroundMusic) {
            try { tracks.backgroundMusic._baseVolume = baseMusicVolume; } catch {}
            tracks.backgroundMusic.volume(clamp01(baseMusicVolume * volumeMultipliers.music));
        }
    }
    
    // Pause all audio
    function pauseAll() {
        if (tracks.backgroundMusic) {
            tracks.backgroundMusic.pause();
        }
        Object.values(tracks.voiceLines).forEach(track => track.pause());
        Object.values(tracks.soundEffects).forEach(track => track.pause());
    }
    
    // Ensure background music is playing if available and not muted
    function resumeBackgroundMusicIfAny(volume) {
        if (!tracks.backgroundMusic) return;
        if (typeof volume === 'number') {
            baseMusicVolume = clamp01(volume);
            try { tracks.backgroundMusic._baseVolume = baseMusicVolume; } catch {}
            tracks.backgroundMusic.volume(clamp01(baseMusicVolume * volumeMultipliers.music));
        }
        if (!(muteState.all || muteState.music)) {
            if (!tracks.backgroundMusic.playing()) {
                tracks.backgroundMusic.play();
            } else if (typeof volume !== 'number') {
                // Ensure current volume reflects multiplier if no explicit volume provided
                const effective = clamp01((tracks.backgroundMusic._baseVolume ?? baseMusicVolume) * volumeMultipliers.music);
                tracks.backgroundMusic.volume(effective);
            }
        }
    }

    // Resume all audio
    function resumeAll() {
        if (tracks.backgroundMusic) {
            if (!(muteState.all || muteState.music)) {
                tracks.backgroundMusic.play();
            }
        }
        Object.values(tracks.voiceLines).forEach(track => {
            if (!(muteState.all || muteState.voice)) {
                track.play();
            }
        });
        Object.values(tracks.soundEffects).forEach(track => {
            if (!(muteState.all || muteState.sfx)) {
                track.play();
            }
        });
    }
    
    // Check if background music is playing
    function isBackgroundMusicPlaying() {
        return tracks.backgroundMusic && tracks.backgroundMusic.playing();
    }
    
    // Preload audio files
    function preload(urls, onComplete) {
        let loaded = 0;
        const total = urls.length;
        
        if (total === 0) {
            if (onComplete) onComplete();
            return;
        }
        
        urls.forEach(url => {
            const sound = new Howl({
                src: [url],
                preload: true,
                onload: function() {
                    loaded++;
                    console.log(`[AudioManager] Preloaded: ${url} (${loaded}/${total})`);
                    if (loaded === total && onComplete) {
                        onComplete();
                    }
                },
                onloaderror: function() {
                    loaded++;
                    console.error(`[AudioManager] Failed to preload: ${url}`);
                    if (loaded === total && onComplete) {
                        onComplete();
                    }
                }
            });
        });
    }
    
    // Cleanup and dispose
    function dispose() {
        console.log('[AudioManager] Disposing all audio');
        stopAll();
        tracks = {
            backgroundMusic: null,
            voiceLines: {},
            soundEffects: {}
        };
        currentBackgroundMusic = null;
    }
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        // DOM is already loaded
        init();
    }
    
    // Get audio status for debugging
    function getStatus() {
        const howlerLoaded = typeof Howler !== 'undefined';
        return {
            howlerLoaded: howlerLoaded,
            howlerReady: howlerReady,
            contextState: howlerLoaded && Howler.ctx ? Howler.ctx.state : 'no context',
            isUnlocked: isUnlocked,
            pendingActions: pendingActions.length,
            backgroundMusic: currentBackgroundMusic,
            isPlaying: isBackgroundMusicPlaying(),
            muteState: { ...muteState },
            volumeMultipliers: { ...volumeMultipliers }
        };
    }
    
    // Manually unlock audio (useful for testing)
    function manualUnlock() {
        if (typeof Howler === 'undefined') {
            console.error('[AudioManager] Cannot unlock - Howler.js not loaded');
            return;
        }
        
        if (Howler.ctx && Howler.ctx.state === 'suspended') {
            Howler.ctx.resume().then(() => {
                console.log('[AudioManager] Manually unlocked audio context');
                isUnlocked = true;
                processPendingActions();
            });
        } else {
            console.log('[AudioManager] Audio context already running or not available');
        }
    }
    
    // Public API
    return {
        playBackgroundMusic,
        stopBackgroundMusic,
        playSoundEffect,
        playVoiceLine,
        stopAllVoiceLines,
        stopAllSoundEffects,
        stopAll,
        setGlobalVolume,
        setBackgroundMusicVolume,
        pauseAll,
        resumeAll,
        resumeBackgroundMusicIfAny,
        isBackgroundMusicPlaying,
        preload,
        dispose,
        getStatus,
        manualUnlock,
        // Per-channel master volume multipliers (persisted)
        setMusicVolumeMultiplier: function(multiplier) {
            const m = clamp01(parseFloat(multiplier));
            volumeMultipliers.music = m;
            try { localStorage.setItem('audioVolume_music', String(m)); } catch {}
            if (tracks.backgroundMusic) {
                const base = typeof tracks.backgroundMusic._baseVolume === 'number' ? tracks.backgroundMusic._baseVolume : baseMusicVolume;
                tracks.backgroundMusic.volume(clamp01(base * volumeMultipliers.music));
            }
        },
        setSfxVolumeMultiplier: function(multiplier) {
            const m = clamp01(parseFloat(multiplier));
            volumeMultipliers.sfx = m;
            try { localStorage.setItem('audioVolume_sfx', String(m)); } catch {}
            // Update currently playing SFX volumes
            Object.keys(tracks.soundEffects).forEach(id => {
                const howl = tracks.soundEffects[id];
                if (!howl) return;
                const base = typeof howl._baseVolume === 'number' ? howl._baseVolume : howl.volume();
                howl.volume(clamp01(base * volumeMultipliers.sfx));
            });
        },
        setVoiceVolumeMultiplier: function(multiplier) {
            const m = clamp01(parseFloat(multiplier));
            volumeMultipliers.voice = m;
            try { localStorage.setItem('audioVolume_voice', String(m)); } catch {}
            // Update currently playing voice line volumes
            Object.keys(tracks.voiceLines).forEach(id => {
                const howl = tracks.voiceLines[id];
                if (!howl) return;
                const base = typeof howl._baseVolume === 'number' ? howl._baseVolume : howl.volume();
                howl.volume(clamp01(base * volumeMultipliers.voice));
            });
        },
        // Mute controls
        setMusicMuted: function(muted) {
            muteState.music = !!muted;
            if (tracks.backgroundMusic) {
                if (muted) {
                    // Reduce to silence but keep state so it can resume instantly
                    tracks.backgroundMusic.volume(0);
                }
                // On unmute, the app will set the desired volume explicitly
            }
            // Recompute all flag and clear global mute if any channel is unmuted
            muteState.all = !!(muteState.music && muteState.sfx && muteState.voice);
            if (!muteState.all && typeof Howler !== 'undefined') {
                Howler.mute(false);
            }
        },
        setSfxMuted: function(muted) {
            muteState.sfx = !!muted;
            if (muted) {
                stopAllSoundEffects();
            }
            // Recompute all flag and clear global mute if any channel is unmuted
            muteState.all = !!(muteState.music && muteState.sfx && muteState.voice);
            if (!muteState.all && typeof Howler !== 'undefined') {
                Howler.mute(false);
            }
        },
        setVoiceMuted: function(muted) {
            muteState.voice = !!muted;
            if (muted) {
                stopAllVoiceLines();
            }
            // Recompute all flag and clear global mute if any channel is unmuted
            muteState.all = !!(muteState.music && muteState.sfx && muteState.voice);
            if (!muteState.all && typeof Howler !== 'undefined') {
                Howler.mute(false);
            }
        },
        setAllMuted: function(muted) {
            muteState.all = !!muted;
            // Update per-channel as well
            this.setMusicMuted(muted);
            this.setSfxMuted(muted);
            this.setVoiceMuted(muted);
            if (typeof Howler !== 'undefined') {
                Howler.mute(!!muted);
            }
        }
    };
})();

console.log('[AudioManager] Audio manager created and available on window.audioManager');