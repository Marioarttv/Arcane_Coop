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
                },
                new VisualNovelCharacter
                {
                    Id = "stanton",
                    Name = "Deputy Stanton",
                    DisplayName = "Deputy Stanton",
                    ImagePath = "/images/stanton.png",
                    Position = CharacterPosition.Center,
                    ThemeColor = "#c8aa6e",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "/images/stanton.png" },
                        { CharacterExpression.Angry, "/images/stanton.png" },
                        { CharacterExpression.Surprised, "/images/stanton.png" }
                    }
                },
                new VisualNovelCharacter
                {
                    Id = "narrator",
                    Name = "Narrator",
                    DisplayName = "Narrator",
                    ImagePath = "", // No image for narrator
                    Position = CharacterPosition.Center,
                    ThemeColor = "#888888",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "" }
                    }
                }
            });

            // Dialogue
            scene.DialogueLines.AddRange(new[]
            {
                // Opening Scene
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "Council Chamber Antechamber - Moments after the meeting",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
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
                // Player Introduction - simplified
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "Footsteps echo in the marble hallway...",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
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
                    Text = "What is it? If Stanton finds out you're—",
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
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "A voice emerges from the shadows...",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
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
                    Text = "We'll need to be careful. If Stanton finds out we're investigating without authorization... he's been paranoid since taking over from Marcus.",
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
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "The group heads into the night, leaving Piltover's gleaming towers for Zaun's shadowed alleys...",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                
                // === SCENE 2: THE CRIME SCENE ===
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "The Crime Scene - Dark alleyway in Zaun",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                // Investigation Begins
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "This is it. Saw the whole thing from my window last night - explosion around 2 AM, then someone bolting from that door.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "These burn patterns... definitely Jinx's work. She always did like her powder bombs.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Forced entry, but sophisticated. She picked the lock first, then blew it when that didn't work fast enough. She was looking for something specific.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "This was a records storage facility. Why would she—",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Because she's not random anymore. Silco's turned her into his weapon. Everything has a purpose now, even if it's twisted.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                // The Discovery
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "Guys... there's something over here.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "These files... they've got photos. Four people, all marked with red crosses. And look at this stamp - 'PROJECT SAFEGUARD - CLASSIFIED'.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Let me see those. These are personnel files, but old ones. These scientists worked on early Hextech prototypes before Jayce's breakthrough.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Why would Powder care about old scientists?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "Maybe she doesn't. Maybe Silco does.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                // Stanton's Interruption
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "Heavy enforcer boots echo down the alley...",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "stanton",
                    Text = "Well, well. Kiramman. I should have known you'd show up where you don't belong.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Deputy Stanton. We're investigating—",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "stanton",
                    Text = "You're interfering with an active crime scene is what you're doing. Those files... where did you get those?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "They were on the ground. The suspect must have dropped—",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "stanton",
                    Text = "These are... these were in Marcus's private— I mean, these are classified enforcer documents!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Marcus's private files? How would Jinx know about those?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "stanton",
                    Text = "That's none of your concern! This is evidence in an ongoing investigation. Way above your clearance, Kiramman.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Deputy, if this is connected to the attacks—",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "stanton",
                    Text = "The only thing connected here is you to a violation of protocol! Get out of here before I report this to the Council. Sheriff Marcus may have tolerated your meddling, but I won't.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "He's sweating. He knows something.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "stanton",
                    Text = "What was that, recruit? You want to join your friend in Stillwater?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "stanton",
                    Text = "Oh yes, I know all about you. One word from me and you're back in that cell.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Try it.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Vi, don't. We're leaving.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "This isn't over, Deputy.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "stanton",
                    Text = "It is if you know what's good for you. Now GET OUT!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                // After Leaving
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "After leaving the crime scene...",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "He's dirty. Did you see his face when he mentioned Marcus's files?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Those weren't just classified documents. They were personal files Marcus kept hidden. But how did Jinx know they existed?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "And why these four scientists specifically? What connects them?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "We need to find out who those people in the photos were. If Jinx marked them...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Then they're either already dead, or about to be. We need to move fast.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "The enforcer database. I can get us in, but we'll have to be careful. Stanton will be watching for us now.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "The Piltover Enforcer HQ? You sure about this?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "No choice. Those people don't know what's coming for them. And neither does Powder if Silco's pulling her strings.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = $"We'll split up once we're inside. {zaunPlayerName}, you'll need to describe what you remember from those photos to {piltoverPlayerName}, who can search the database.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "I got a good look at them. I can do this.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "And I know my way around the enforcer systems. We'll find them.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Then let's go. Every second counts.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                
                // Transition to Picture Explanation Puzzle
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "The team heads to Piltover Enforcer HQ to identify the four scientists from the marked photos...",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
            });

            // Mark the end of main content - all dialogue is now linear without branches
            // The last main dialogue is "Then let's go. Every second counts."
            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;

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

