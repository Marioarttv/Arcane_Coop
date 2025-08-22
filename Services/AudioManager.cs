using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arcane_Coop.Services
{
    public interface IAudioManager
    {
        Task InitializeAsync();
        Task PlayBackgroundMusicAsync(string src, float volume = 0.7f, bool loop = true, int fadeIn = 1000, bool crossfade = true, int crossfadeDuration = 2000);
        Task StopBackgroundMusicAsync(int fadeOut = 1000);
        Task PlaySoundEffectAsync(string src, float volume = 0.8f, float rate = 1.0f);
        Task PlayVoiceLineAsync(string src, float volume = 1.0f, bool stopPrevious = true);
        Task StopAllVoiceLinesAsync();
        Task StopAllSoundEffectsAsync();
        Task StopAllAsync();
        Task SetGlobalVolumeAsync(float volume);
        Task SetBackgroundMusicVolumeAsync(float volume);
        Task PauseAllAsync();
        Task ResumeAllAsync();
        Task<bool> IsBackgroundMusicPlayingAsync();
        Task PreloadAsync(string[] urls);
        Task DisposeAsync();
    }

    public class AudioManager : IAudioManager, IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isInitialized = false;
        private readonly HashSet<string> _preloadedFiles = new();

        public AudioManager(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                // Initialize the audio manager (this will be called when the service is first used)
                await Task.Delay(100); // Small delay to ensure JS is ready
                _isInitialized = true;
                Console.WriteLine("[AudioManager] Initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Failed to initialize: {ex.Message}");
            }
        }

        public async Task PlayBackgroundMusicAsync(string src, float volume = 0.7f, bool loop = true, int fadeIn = 1000, bool crossfade = true, int crossfadeDuration = 2000)
        {
            if (!_isInitialized) await InitializeAsync();
            if (string.IsNullOrEmpty(src)) return;

            try
            {
                var options = new
                {
                    volume,
                    loop,
                    fadeIn,
                    crossfade,
                    crossfadeDuration
                };

                await _jsRuntime.InvokeVoidAsync("audioManager.playBackgroundMusic", src, options);
                Console.WriteLine($"[AudioManager] Playing background music: {src}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error playing background music: {ex.Message}");
            }
        }

        public async Task StopBackgroundMusicAsync(int fadeOut = 1000)
        {
            if (!_isInitialized) return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("audioManager.stopBackgroundMusic", fadeOut);
                Console.WriteLine("[AudioManager] Stopped background music");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error stopping background music: {ex.Message}");
            }
        }

        public async Task PlaySoundEffectAsync(string src, float volume = 0.8f, float rate = 1.0f)
        {
            if (!_isInitialized) await InitializeAsync();
            if (string.IsNullOrEmpty(src)) return;

            try
            {
                var options = new
                {
                    volume,
                    rate
                };

                await _jsRuntime.InvokeVoidAsync("audioManager.playSoundEffect", src, options);
                Console.WriteLine($"[AudioManager] Playing sound effect: {src}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error playing sound effect: {ex.Message}");
            }
        }

        public async Task PlayVoiceLineAsync(string src, float volume = 1.0f, bool stopPrevious = true)
        {
            if (!_isInitialized) await InitializeAsync();
            if (string.IsNullOrEmpty(src)) return;

            try
            {
                var options = new
                {
                    volume,
                    stopPrevious
                };

                await _jsRuntime.InvokeVoidAsync("audioManager.playVoiceLine", src, options);
                Console.WriteLine($"[AudioManager] Playing voice line: {src}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error playing voice line: {ex.Message}");
            }
        }

        public async Task StopAllVoiceLinesAsync()
        {
            if (!_isInitialized) return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("audioManager.stopAllVoiceLines");
                Console.WriteLine("[AudioManager] Stopped all voice lines");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error stopping voice lines: {ex.Message}");
            }
        }

        public async Task StopAllSoundEffectsAsync()
        {
            if (!_isInitialized) return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("audioManager.stopAllSoundEffects");
                Console.WriteLine("[AudioManager] Stopped all sound effects");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error stopping sound effects: {ex.Message}");
            }
        }

        public async Task StopAllAsync()
        {
            if (!_isInitialized) return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("audioManager.stopAll");
                Console.WriteLine("[AudioManager] Stopped all audio");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error stopping all audio: {ex.Message}");
            }
        }

        public async Task SetGlobalVolumeAsync(float volume)
        {
            if (!_isInitialized) await InitializeAsync();

            try
            {
                await _jsRuntime.InvokeVoidAsync("audioManager.setGlobalVolume", volume);
                Console.WriteLine($"[AudioManager] Set global volume to: {volume}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error setting global volume: {ex.Message}");
            }
        }

        public async Task SetBackgroundMusicVolumeAsync(float volume)
        {
            if (!_isInitialized) return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("audioManager.setBackgroundMusicVolume", volume);
                Console.WriteLine($"[AudioManager] Set background music volume to: {volume}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error setting background music volume: {ex.Message}");
            }
        }

        public async Task PauseAllAsync()
        {
            if (!_isInitialized) return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("audioManager.pauseAll");
                Console.WriteLine("[AudioManager] Paused all audio");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error pausing audio: {ex.Message}");
            }
        }

        public async Task ResumeAllAsync()
        {
            if (!_isInitialized) return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("audioManager.resumeAll");
                Console.WriteLine("[AudioManager] Resumed all audio");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error resuming audio: {ex.Message}");
            }
        }

        public async Task<bool> IsBackgroundMusicPlayingAsync()
        {
            if (!_isInitialized) return false;

            try
            {
                return await _jsRuntime.InvokeAsync<bool>("audioManager.isBackgroundMusicPlaying");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error checking background music status: {ex.Message}");
                return false;
            }
        }

        public async Task PreloadAsync(string[] urls)
        {
            if (!_isInitialized) await InitializeAsync();
            if (urls == null || urls.Length == 0) return;

            try
            {
                // Filter out already preloaded files
                var urlsToPreload = new List<string>();
                foreach (var url in urls)
                {
                    if (!_preloadedFiles.Contains(url))
                    {
                        urlsToPreload.Add(url);
                        _preloadedFiles.Add(url);
                    }
                }

                if (urlsToPreload.Count > 0)
                {
                    await _jsRuntime.InvokeVoidAsync("audioManager.preload", urlsToPreload.ToArray(), null);
                    Console.WriteLine($"[AudioManager] Preloading {urlsToPreload.Count} audio files");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error preloading audio: {ex.Message}");
            }
        }

        public async Task DisposeAsync()
        {
            if (!_isInitialized) return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("audioManager.dispose");
                _preloadedFiles.Clear();
                _isInitialized = false;
                Console.WriteLine("[AudioManager] Disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioManager] Error during disposal: {ex.Message}");
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsync();
        }
    }
}