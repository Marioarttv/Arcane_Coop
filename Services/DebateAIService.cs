using OpenAI;
using OpenAI.Chat;
using OpenAI.Audio;
using ElevenLabs;
using ElevenLabs.TextToSpeech;
using ElevenLabs.Voices;
using Arcane_Coop.Models;

namespace Arcane_Coop.Services
{
    public interface IDebateAIService
    {
        Task<string> TranscribeAudioAsync(byte[] audioData);
        Task<(string text, CharacterEmotion emotion)> GenerateAIResponseAsync(
            DebateSpeaker character, 
            List<DebateDialogue> conversationHistory, 
            string playerInput,
            string debateTopic);
        Task<byte[]> GenerateSpeechAsync(string text, DebateSpeaker character);
    }

    public class DebateAIService : IDebateAIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DebateAIService> _logger;
        private readonly string _openAIKey;
        private readonly string _elevenLabsKey;
        
        // ElevenLabs voice IDs - you'll need to set these up in your ElevenLabs account
        private readonly Dictionary<DebateSpeaker, string> _voiceIds = new()
        {
            { DebateSpeaker.Jinx, "EXAVITQu4vr4xnSDxMaL" }, // Use a chaotic female voice
            { DebateSpeaker.Silco, "pNInz6obpgDQGcFmaJgB" } // Use a calculated male voice
        };

        public DebateAIService(IConfiguration configuration, ILogger<DebateAIService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _openAIKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
            _elevenLabsKey = configuration["ElevenLabs:ApiKey"] ?? throw new InvalidOperationException("ElevenLabs API key not configured");
        }

        public async Task<string> TranscribeAudioAsync(byte[] audioData)
        {
            try
            {
                using var openAI = new OpenAIClient(_openAIKey);
                
                // Save audio to temporary file (Whisper requires a file path)
                var tempFile = Path.GetTempFileName();
                var audioFile = Path.ChangeExtension(tempFile, ".webm");
                await File.WriteAllBytesAsync(audioFile, audioData);

                try
                {
                    using var request = new AudioTranscriptionRequest(audioFile, language: "en");
                    var transcription = await openAI.AudioEndpoint.CreateTranscriptionTextAsync(request);
                    return transcription;
                }
                finally
                {
                    // Clean up temp file
                    if (File.Exists(audioFile))
                        File.Delete(audioFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transcribing audio");
                throw;
            }
        }

        public async Task<(string text, CharacterEmotion emotion)> GenerateAIResponseAsync(
            DebateSpeaker character,
            List<DebateDialogue> conversationHistory,
            string playerInput,
            string debateTopic)
        {
            try
            {
                using var openAI = new OpenAIClient(_openAIKey);

                // Build the system prompt based on character
                var systemPrompt = character switch
                {
                    DebateSpeaker.Jinx => BuildJinxPrompt(debateTopic),
                    DebateSpeaker.Silco => BuildSilcoPrompt(debateTopic),
                    _ => throw new ArgumentException($"Invalid AI character: {character}")
                };

                // Build conversation context
                var messages = new List<Message>
                {
                    new Message(Role.System, systemPrompt)
                };

                // Add conversation history (last 10 messages for context)
                foreach (var dialogue in conversationHistory.TakeLast(10))
                {
                    var role = dialogue.Speaker switch
                    {
                        DebateSpeaker.PlayerA or DebateSpeaker.PlayerB => Role.User,
                        _ => Role.Assistant
                    };
                    messages.Add(new Message(role, $"{dialogue.Speaker}: {dialogue.Text}"));
                }

                // Add current player input
                messages.Add(new Message(Role.User, $"Player: {playerInput}"));

                // Generate response
                var chatRequest = new ChatRequest(messages, OpenAI.Models.Model.GPT4oMini, temperature: 0.8, maxTokens: 150);
                var response = await openAI.ChatEndpoint.GetCompletionAsync(chatRequest);
                var responseText = response.FirstChoice.Message.Content?.ToString() ?? "";

                // Detect emotion
                var emotion = await DetectEmotionAsync(openAI, responseText, character);

                return (responseText, emotion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI response");
                throw;
            }
        }

        public async Task<byte[]> GenerateSpeechAsync(string text, DebateSpeaker character)
        {
            try
            {
                var elevenLabs = new ElevenLabsClient(_elevenLabsKey);
                
                if (!_voiceIds.TryGetValue(character, out var voiceId))
                {
                    throw new ArgumentException($"No voice configured for {character}");
                }

                // Get the voice object
                var voice = await elevenLabs.VoicesEndpoint.GetVoiceAsync(voiceId);
                
                // Create request with voice object
                var request = new TextToSpeechRequest(
                    voice,
                    text,
                    null, // Use default model
                    new VoiceSettings(0.5f, 0.75f, 0.5f, true)); // Adjust for character personality

                var audioClip = await elevenLabs.TextToSpeechEndpoint.TextToSpeechAsync(request);
                
                // The audio clip is already a byte array-like structure
                return audioClip.ClipData.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating speech with ElevenLabs");
                throw;
            }
        }

        private string BuildJinxPrompt(string debateTopic)
        {
            return $@"You are Jinx from Arcane. You are chaotic, unpredictable, brilliant but unstable.
You speak in a manic, energetic way with sudden mood swings. You're suspicious of everyone.
You use dark humor and make explosive references. You're debating about: {debateTopic}

Key traits:
- Interrupt yourself mid-thought
- Make sudden topic changes
- Use nicknames and mock others
- Reference explosions and chaos
- Show paranoia and distrust
- Speak in short, punchy sentences

Keep responses under 100 words. Don't use any formatting like asterisks or bold text.
React directly to what was just said.";
        }

        private string BuildSilcoPrompt(string debateTopic)
        {
            return $@"You are Silco from Arcane. You are calculating, manipulative, and eloquent.
You speak with authority and use psychological manipulation. You see the bigger picture.
You believe in the Nation of Zaun and will do anything for power. You're debating about: {debateTopic}

Key traits:
- Speak calmly and deliberately
- Use metaphors about vision and sacrifice
- Manipulate through logic and emotion
- Reference the greater good of Zaun
- Show controlled anger when challenged
- Make veiled threats

Keep responses under 100 words. Don't use any formatting like asterisks or bold text.
React directly to what was just said.";
        }

        private async Task<CharacterEmotion> DetectEmotionAsync(OpenAIClient openAI, string text, DebateSpeaker character)
        {
            try
            {
                var validEmotions = character switch
                {
                    DebateSpeaker.Jinx => "angry, suspicious, amused, frustrated",
                    DebateSpeaker.Silco => "neutral, confident, thoughtful, frustrated",
                    _ => "neutral"
                };

                var emotionRequest = new ChatRequest(
                    new List<Message>
                    {
                        new Message(Role.System, $"Analyze the emotion in the text and respond with ONE word from: {validEmotions}"),
                        new Message(Role.User, text)
                    },
                    OpenAI.Models.Model.GPT4oMini,
                    temperature: 0.3,
                    maxTokens: 10
                );

                var response = await openAI.ChatEndpoint.GetCompletionAsync(emotionRequest);
                var emotionStr = response.FirstChoice.Message.Content?.ToString()?.Trim().ToLower() ?? "neutral";

                return emotionStr switch
                {
                    "angry" => CharacterEmotion.Angry,
                    "suspicious" => CharacterEmotion.Suspicious,
                    "confident" => CharacterEmotion.Confident,
                    "thoughtful" => CharacterEmotion.Thoughtful,
                    "amused" => CharacterEmotion.Amused,
                    "frustrated" => CharacterEmotion.Frustrated,
                    _ => CharacterEmotion.Neutral
                };
            }
            catch
            {
                return CharacterEmotion.Neutral;
            }
        }
    }
}