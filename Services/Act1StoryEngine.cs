using Arcane_Coop.Models;
using System.Linq;

namespace Arcane_Coop.Services
{
    public interface IAct1StoryEngine
    {
        // Content builders
        VisualNovelScene CreateEmergencyBriefingScene(string squadName);

        // View builders
        Act1PlayerView CreatePlayerView(Act1MultiplayerGame game, string playerId);

        // Progression
        Act1ProgressionResult ProgressToNextScene(Act1MultiplayerGame game);
    }

    public sealed class Act1ProgressionResult
    {
        public bool TransitionStarted { get; set; }
        public string? NextGameName { get; set; }
        public Dictionary<string, string>? RedirectUrlsByPlayerId { get; set; }
        public bool StoryCompleted { get; set; }
    }

    public class Act1StoryEngine : IAct1StoryEngine
    {
        public VisualNovelScene CreateEmergencyBriefingScene(string squadName)
        {
            var scene = new VisualNovelScene
            {
                Id = "emergency_briefing",
                Name = "Emergency Briefing - Tech Theft Crisis",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Zaun
            };

            // Characters
            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter
                {
                    Id = "vi",
                    Name = "Vi",
                    DisplayName = "Vi",
                    ImagePath = "/images/vi.jpeg",
                    Position = CharacterPosition.Right,
                    ThemeColor = "#00d4aa",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "/images/vi.jpeg" },
                        { CharacterExpression.Determined, "/images/vi.jpeg" },
                        { CharacterExpression.Angry, "/images/vi.jpeg" },
                        { CharacterExpression.Happy, "/images/vi.jpeg" },
                        { CharacterExpression.Worried, "/images/vi.jpeg" },
                        { CharacterExpression.Surprised, "/images/vi.jpeg" }
                    }
                },
                new VisualNovelCharacter
                {
                    Id = "caitlyn",
                    Name = "Caitlyn",
                    DisplayName = "Officer Caitlyn",
                    ImagePath = "/images/cait.jpeg",
                    Position = CharacterPosition.Left,
                    ThemeColor = "#c8aa6e",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "/images/cait.jpeg" },
                        { CharacterExpression.Serious, "/images/cait.jpeg" },
                        { CharacterExpression.Worried, "/images/cait.jpeg" },
                        { CharacterExpression.Surprised, "/images/cait.jpeg" },
                        { CharacterExpression.Determined, "/images/cait.jpeg" }
                    }
                }
            });

            // Dialogue
            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = $"Squad {squadName}, thank you for responding. We have a crisis that threatens both cities.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "You two are the only ones we can trust with this. One from each side, working together.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Three hextech prototypes were stolen from the Academy vault last night. Security footage was corrupted.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "And before your Piltover operative gets any ideas - Renni's chemtech warehouse was hit too. Same night.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "The thefts happened within minutes of each other. Someone's playing both sides.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Who's crazy enough to steal from both the Academy AND the chembarons? That's painting a target on your back.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "The Council is already blaming Zaun. The chembarons are pointing fingers at Piltover. You see the problem.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Yeah - we're one angry mob away from all-out war. That's why we need you two.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "We have leads. I intercepted encrypted messages from a relay station near the border.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "And I found chemical residue at the warehouse. Not shimmer, not standard chemtech. Something new.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Default
                },
                // Player choice - Piltover player decides investigation approach
                new DialogueLine
                {
                    Id = "investigation_choice",
                    CharacterId = "caitlyn",
                    Text = "Your Piltover operative has tactical training. What's your assessment of where to start?",
                    IsPlayerChoice = true,
                    ChoiceOwnerRole = "piltover",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice
                        {
                            Id = "investigate_messages",
                            Text = "Those encrypted messages are our best lead. We should decode them immediately.",
                            NextDialogueId = "messages_response",
                            ResultExpression = CharacterExpression.Determined,
                            Consequences = new Dictionary<string, object> { { "initial_lead", "messages" } }
                        },
                        new DialogueChoice
                        {
                            Id = "investigate_theft_sites",
                            Text = "The theft sites might have physical evidence. We should examine them first.",
                            NextDialogueId = "sites_response",
                            ResultExpression = CharacterExpression.Serious,
                            Consequences = new Dictionary<string, object> { { "initial_lead", "sites" } }
                        },
                        new DialogueChoice
                        {
                            Id = "investigate_both",
                            Text = "We should split up - cover both leads simultaneously. Time is critical.",
                            NextDialogueId = "split_response",
                            ResultExpression = CharacterExpression.Determined,
                            Consequences = new Dictionary<string, object> { { "initial_lead", "split" } }
                        }
                    }
                },
                // Messages response branch
                new DialogueLine
                {
                    Id = "messages_response",
                    CharacterId = "vi",
                    Text = "Smart call. Those codes use both Piltovan and Zaunite encryption. You'll need to work together.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                    // No NextDialogueId - will naturally continue to next dialogue in sequence
                },
                new DialogueLine
                {
                    Id = "messages_continue",
                    CharacterId = "caitlyn",
                    Text = "One of you identifies the patterns, the other provides context. Communication is key.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious,
                    NextDialogueId = "priority_choice" // Jump to next choice point
                },
                // Sites response branch
                new DialogueLine
                {
                    Id = "sites_response",
                    CharacterId = "vi",
                    Text = "Good instincts. That residue is unlike anything I've seen. You'll need to analyze it carefully.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                    // No NextDialogueId - will naturally continue to next dialogue in sequence
                },
                new DialogueLine
                {
                    Id = "sites_continue",
                    CharacterId = "caitlyn",
                    Text = "If someone's mixing hextech and chemtech, you need to identify what they're building. Fast.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried,
                    NextDialogueId = "priority_choice" // Jump to next choice point
                },
                // Split response branch
                new DialogueLine
                {
                    Id = "split_response",
                    CharacterId = "vi",
                    Text = "Ambitious. Alright, you two split the leads. But stay in contact - this could get dangerous.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                    // No NextDialogueId - will naturally continue to next dialogue in sequence
                },
                new DialogueLine
                {
                    Id = "split_continue",
                    CharacterId = "caitlyn",
                    Text = "Share everything through our secure channels. No secrets between partners.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined,
                    NextDialogueId = "priority_choice" // Jump to next choice point
                },
                // Zaun player choice - decides priorities
                new DialogueLine
                {
                    Id = "priority_choice",
                    CharacterId = "vi",
                    Text = "Your Zaun operative knows the streets. What's your gut telling you about priorities?",
                    IsPlayerChoice = true,
                    ChoiceOwnerRole = "zaun",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice
                        {
                            Id = "save_people",
                            Text = "The people come first. If this goes bad, we evacuate civilians before anything else.",
                            NextDialogueId = "people_priority",
                            ResultExpression = CharacterExpression.Determined,
                            Consequences = new Dictionary<string, object> { { "priority", "people" } }
                        },
                        new DialogueChoice
                        {
                            Id = "stop_war",
                            Text = "We can't let them start a war. That's exactly what the thief wants.",
                            NextDialogueId = "war_priority",
                            ResultExpression = CharacterExpression.Worried,
                            Consequences = new Dictionary<string, object> { { "priority", "peace" } }
                        },
                        new DialogueChoice
                        {
                            Id = "catch_thief",
                            Text = "Find who's behind this. They're playing us like puppets - time to cut the strings.",
                            NextDialogueId = "thief_priority",
                            ResultExpression = CharacterExpression.Angry,
                            Consequences = new Dictionary<string, object> { { "priority", "justice" } }
                        }
                    }
                },
                // People priority response
                new DialogueLine
                {
                    Id = "people_priority",
                    CharacterId = "caitlyn",
                    Text = "Good. That's exactly the mindset we need. I'll coordinate evacuation routes with the Enforcers.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                    // No NextDialogueId - will naturally continue to next dialogue in sequence
                },
                // War priority response
                new DialogueLine
                {
                    Id = "war_priority",
                    CharacterId = "caitlyn",
                    Text = "Exactly right. The thief wants chaos - we give them cooperation instead.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                    // No NextDialogueId - will naturally continue to next dialogue in sequence
                },
                // Thief priority response
                new DialogueLine
                {
                    Id = "thief_priority",
                    CharacterId = "caitlyn",
                    Text = "Agreed. But remember - we need evidence both sides will accept. Do this right.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                    // No NextDialogueId - will naturally continue to next dialogue in sequence
                },
                new DialogueLine
                {
                    Id = "mission_briefing",
                    CharacterId = "vi",
                    Text = $"Alright, Squad {squadName}. You're our best shot at stopping this war.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Your first task - analyze corrupted surveillance footage from both theft sites.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "The images are fragmented. One of you will need to describe what you see, the other identifies it.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Work together. Trust each other. The fate of both cities depends on what you find.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "No pressure, rookies. Just the difference between peace and all-out war. You got this.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                }
            });

            // Debug: Log all dialogue IDs and their indices
            Console.WriteLine($"[Act1StoryEngine] Scene created with {scene.DialogueLines.Count} dialogues:");
            for (int i = 0; i < scene.DialogueLines.Count; i++)
            {
                var dl = scene.DialogueLines[i];
                var idInfo = !string.IsNullOrEmpty(dl.Id) ? $"ID='{dl.Id}'" : "no ID";
                var nextInfo = !string.IsNullOrEmpty(dl.NextDialogueId) ? $" → NextDialogueId='{dl.NextDialogueId}'" : "";
                var choiceInfo = dl.IsPlayerChoice ? " [CHOICE]" : "";
                Console.WriteLine($"  [{i}] {idInfo} - {dl.CharacterId}: \"{dl.Text.Substring(0, Math.Min(40, dl.Text.Length))}...\"{choiceInfo}{nextInfo}");
            }

            return scene;
        }

        public Act1PlayerView CreatePlayerView(Act1MultiplayerGame game, string playerId)
        {
            var player = game.GetPlayer(playerId);
            if (player == null) return new Act1PlayerView();

            var connectedPlayers = game.Players.Where(p => p.IsConnected).Select(p => p.PlayerName).ToList();

            // Pending choice detection
            DialogueLine? pendingChoice = null;
            bool canMakeChoice = false;
            bool isWaitingForOtherPlayer = false;

            if (game.GameState.CurrentDialogueIndex < game.CurrentScene.DialogueLines.Count)
            {
                var currentDialogue = game.CurrentScene.DialogueLines[game.GameState.CurrentDialogueIndex];
                if (currentDialogue.IsPlayerChoice && currentDialogue.Choices.Count > 0 && game.GameState.IsTextFullyDisplayed)
                {
                    if (string.IsNullOrEmpty(currentDialogue.SelectedChoiceId))
                    {
                        pendingChoice = currentDialogue;
                        if (string.IsNullOrEmpty(currentDialogue.ChoiceOwnerRole))
                        {
                            canMakeChoice = true;
                        }
                        else if (player.PlayerRole.Equals(currentDialogue.ChoiceOwnerRole, StringComparison.OrdinalIgnoreCase))
                        {
                            canMakeChoice = true;
                        }
                        else
                        {
                            isWaitingForOtherPlayer = true;
                        }
                    }
                }
            }

            return new Act1PlayerView
            {
                RoomId = game.RoomId,
                PlayerId = playerId,
                PlayerRole = player.PlayerRole,
                GameStatus = game.Status,
                CurrentScene = game.CurrentScene,
                GameState = game.GameState,
                ConnectedPlayers = connectedPlayers,
                CanSkip = game.IsTextAnimating && !game.GameState.IsTextFullyDisplayed && pendingChoice == null,
                CanContinue = game.GameState.IsTextFullyDisplayed && !game.IsTextAnimating && pendingChoice == null,
                IsTextAnimating = game.IsTextAnimating,
                ShowTransition = game.ShowTransition,
                NextGameName = game.NextGameName,
                CurrentSceneIndex = game.CurrentSceneIndex,
                TotalScenes = game.StoryProgression.Count,
                PendingChoice = pendingChoice,
                CanMakeChoice = canMakeChoice,
                IsWaitingForOtherPlayer = isWaitingForOtherPlayer,
                ChoiceHistory = game.ChoiceHistory,
                StatusMessage = game.Status switch
                {
                    Act1GameStatus.WaitingForPlayers => $"Waiting for players... ({game.Players.Count}/2)",
                    Act1GameStatus.InProgress => isWaitingForOtherPlayer ? "Waiting for partner's choice..." : "Story in progress",
                    Act1GameStatus.SceneTransition => $"Transitioning to {game.NextGameName}...",
                    Act1GameStatus.Completed => "Act 1 completed",
                    _ => ""
                }
            };
        }

        public Act1ProgressionResult ProgressToNextScene(Act1MultiplayerGame game)
        {
            var result = new Act1ProgressionResult();

            game.CurrentSceneIndex++;

            if (game.CurrentSceneIndex < game.StoryProgression.Count)
            {
                var nextPhase = game.StoryProgression[game.CurrentSceneIndex];

                if (nextPhase == "picture_explanation_transition")
                {
                    game.Status = Act1GameStatus.SceneTransition;
                    game.ShowTransition = true;
                    game.NextGameName = "Visual Intelligence Analysis";

                    result.TransitionStarted = true;
                    result.NextGameName = game.NextGameName;

                    // Build per-player redirect URLs
                    var urls = new Dictionary<string, string>();
                    foreach (var p in game.Players)
                    {
                        var parameters =
                            $"role={p.PlayerRole}&avatar={p.PlayerAvatar}&name={Uri.EscapeDataString(p.PlayerName)}&squad={Uri.EscapeDataString(p.OriginalSquadName)}&story=true";
                        urls[p.PlayerId] = $"/picture-explanation?{parameters}";
                    }
                    result.RedirectUrlsByPlayerId = urls;
                }
            }
            else
            {
                game.Status = Act1GameStatus.Completed;
                result.StoryCompleted = true;
            }

            return result;
        }
    }
}

