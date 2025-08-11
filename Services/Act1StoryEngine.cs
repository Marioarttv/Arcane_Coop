using Arcane_Coop.Models;
using System.Linq;

namespace Arcane_Coop.Services
{
    public interface IAct1StoryEngine
    {
        // Content builders
        VisualNovelScene CreateEmergencyBriefingScene(string squadName, Act1MultiplayerGame game);

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
        public VisualNovelScene CreateEmergencyBriefingScene(string squadName, Act1MultiplayerGame game)
        {
            // Get player names for dynamic character naming
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));
            
            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Enforcer Recruit";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Vi's Old Friend";
            
            var scene = new VisualNovelScene
            {
                Id = "council_antechamber",
                Name = "Council Chamber Antechamber - After the Meeting",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Piltover
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
                    Position = CharacterPosition.Left,
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
                    DisplayName = "Caitlyn",
                    ImagePath = "/images/cait.jpeg",
                    Position = CharacterPosition.Right,
                    ThemeColor = "#c8aa6e",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "/images/cait.jpeg" },
                        { CharacterExpression.Serious, "/images/cait.jpeg" },
                        { CharacterExpression.Worried, "/images/cait.jpeg" },
                        { CharacterExpression.Surprised, "/images/cait.jpeg" },
                        { CharacterExpression.Determined, "/images/cait.jpeg" }
                    }
                },
                new VisualNovelCharacter
                {
                    Id = "playerA",
                    Name = piltoverPlayerName,
                    DisplayName = piltoverPlayerName,
                    ImagePath = "/images/enforcer.png",
                    Position = CharacterPosition.Center,
                    ThemeColor = "#c8aa6e",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "/images/enforcer.png" },
                        { CharacterExpression.Worried, "/images/enforcer.png" },
                        { CharacterExpression.Determined, "/images/enforcer.png" }
                    }
                },
                new VisualNovelCharacter
                {
                    Id = "playerB",
                    Name = zaunPlayerName,
                    DisplayName = zaunPlayerName,
                    ImagePath = "/images/zaun_friend.png",
                    Position = CharacterPosition.Center,
                    ThemeColor = "#00d4aa",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "/images/zaun_friend.png" },
                        { CharacterExpression.Worried, "/images/zaun_friend.png" },
                        { CharacterExpression.Serious, "/images/zaun_friend.png" }
                    }
                }
            });

            // Dialogue
            scene.DialogueLines.AddRange(new[]
            {
                // Scene: Council Chamber Antechamber - Moments after the meeting
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "A blockade. A bloody BLOCKADE! While Silco sits comfortable and Jinx... Powder is out there with him.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Vi, the Council needs time to—",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Time? How much more time do they need? Every second we waste, she slips further away!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                // Enforcer Recruit (Player A) enters
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "Caitlyn! Thank the gears I found you. I know you're not supposed to be on active cases, but... this couldn't wait.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "What is it? If Marcus finds out you're—",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "It's about the blue-haired girl. Someone down in Zaun claims they saw her. Recently. Like, yesterday recently.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Who? Who saw her?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = $"That's the thing... they'll only talk to someone they trust. Said they knew you from before, Vi. Goes by the name {zaunPlayerName}.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                // Player B enters from shadows
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "Still grabbing people when you're excited, eh Vi?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Default
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "No way... is that really you? I thought you were—",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "Dead? Locked up? Nah, just keeping my head down since... since that night. But when I saw what I saw, I knew I had to find you.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "How did you two even connect?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "Pure chance. I was following up on witness reports near the docks when they approached ME. Said they had information but would only share it with Vi present.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Default
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = $"And I wasn't about to trust some random enforcer with this. But when {piltoverPlayerName} mentioned working with you, Caitlyn, and that Vi was actually alive and out of Stillwater... I took the risk.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "You really saw her? Powder?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "Not here. Too many ears. But yeah, Vi. I saw her. And there's more - something about files with red marks, people being hunted. It's all connected to that night at the warehouse.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Do you trust them?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "They knew things... details about Vi that checked out. And they seemed genuinely scared about what they'd seen.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Forget the Council and their blockade. This is our lead.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "We do this together. All four of us.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "We'll need to be careful. If Marcus finds out we're investigating without authorization...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "Then we better not let him find out. Come on, there was a break-in last night. That's where this all starts. Someone dropped something that'll interest you.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Then what are we waiting for?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                // Player choice - what approach to take
                new DialogueLine
                {
                    Id = "investigation_choice",
                    CharacterId = "caitlyn",
                    Text = "Before we go - we need a plan. How should we approach this?",
                    IsPlayerChoice = true,
                    ChoiceOwnerRole = "piltover",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice
                        {
                            Id = "investigate_stealthy",
                            Text = "We go in quiet. No one can know we're working together - it would cause panic.",
                            NextDialogueId = "stealthy_response",
                            ResultExpression = CharacterExpression.Serious,
                            Consequences = new Dictionary<string, object> { { "approach", "stealthy" } }
                        },
                        new DialogueChoice
                        {
                            Id = "investigate_official",
                            Text = "We make this semi-official. Use my enforcer credentials to access what we need.",
                            NextDialogueId = "official_response",
                            ResultExpression = CharacterExpression.Determined,
                            Consequences = new Dictionary<string, object> { { "approach", "official" } }
                        },
                        new DialogueChoice
                        {
                            Id = "investigate_split",
                            Text = "Split up - Piltover team takes topside, Zaun team handles the underground.",
                            NextDialogueId = "split_response",
                            ResultExpression = CharacterExpression.Determined,
                            Consequences = new Dictionary<string, object> { { "approach", "split" } }
                        }
                    }
                },
                // === SECOND CHOICE - Zaun player ===
                new DialogueLine
                {
                    Id = "trust_choice",
                    CharacterId = "playerB",
                    Text = "One more thing - who do we trust with what we find?",
                    IsPlayerChoice = true,
                    ChoiceOwnerRole = "zaun",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice
                        {
                            Id = "trust_nobody",
                            Text = "No one. We keep this between us four until we have proof.",
                            NextDialogueId = "trust_nobody_response",
                            ResultExpression = CharacterExpression.Serious,
                            Consequences = new Dictionary<string, object> { { "trust", "nobody" } }
                        },
                        new DialogueChoice
                        {
                            Id = "trust_vi_cait",
                            Text = "Vi and Caitlyn have proven themselves. We follow their lead.",
                            NextDialogueId = "trust_heroes_response",
                            ResultExpression = CharacterExpression.Default,
                            Consequences = new Dictionary<string, object> { { "trust", "heroes" } }
                        },
                        new DialogueChoice
                        {
                            Id = "trust_network",
                            Text = "I have contacts in the underground. They can help us move unseen.",
                            NextDialogueId = "trust_network_response",
                            ResultExpression = CharacterExpression.Determined,
                            Consequences = new Dictionary<string, object> { { "trust", "network" } }
                        }
                    }
                },
                // === FINAL SCENE - Ready to investigate ===
                new DialogueLine
                {
                    Id = "final_brief",
                    CharacterId = "vi",
                    Text = "Alright, we have our team. Let's move before someone notices we're all together.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Remember - we're looking for anything that connects to Jinx or these warehouse break-ins.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "The break-in site has surveillance footage, but it's corrupted. You'll need to work together to piece it together.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "One of us can describe what we see in the fragments, the other can help identify the locations and people.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "This is it. Whatever you find could be the key to everything. To finding Powder.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Let's go. And be careful - we don't know who else might be watching.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                
                // === ALL BRANCH DIALOGUES AT THE END ===
                // First choice branches (Piltover player)
                new DialogueLine
                {
                    Id = "stealthy_response",
                    CharacterId = "vi",
                    Text = "Smart. We stay under the radar. No uniforms, no badges.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined,
                    NextDialogueId = "trust_choice"
                },
                new DialogueLine
                {
                    Id = "official_response",
                    CharacterId = "caitlyn",
                    Text = "That could work. My suspension isn't public knowledge yet. We can use that.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious,
                    NextDialogueId = "trust_choice"
                },
                new DialogueLine
                {
                    Id = "split_response",
                    CharacterId = "playerB",
                    Text = "Cover more ground that way. But we share everything we find. Agreed?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious,
                    NextDialogueId = "trust_choice"
                },
                // Second choice branches (Zaun player)
                new DialogueLine
                {
                    Id = "trust_nobody_response",
                    CharacterId = "vi",
                    Text = "Paranoid but smart. Until we know who's behind this, trust is a luxury we can't afford.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious,
                    NextDialogueId = "final_brief"
                },
                new DialogueLine
                {
                    Id = "trust_heroes_response",
                    CharacterId = "caitlyn",
                    Text = "We won't let you down. We've come too far to fail now.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined,
                    NextDialogueId = "final_brief"
                },
                new DialogueLine
                {
                    Id = "trust_network_response",
                    CharacterId = "vi",
                    Text = "Your contacts could be useful. But be careful who you bring in - one leak and we're all done.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried,
                    NextDialogueId = "final_brief"
                }
            });

            // Mark the end of main content (before branch dialogues)
            // The last main dialogue is "Let's go. And be careful..." which should be at index 31
            scene.MainContentEndIndex = 31;

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

