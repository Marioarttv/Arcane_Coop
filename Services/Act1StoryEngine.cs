using Arcane_Coop.Models;
using System.Linq;

namespace Arcane_Coop.Services
{
    public interface IAct1StoryEngine
    {
        // Content builders
        VisualNovelScene CreateEmergencyBriefingScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateDatabaseRevelationScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateRadioDecodedScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateRenniApartmentScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateCodeDecodedScene(string squadName, Act1MultiplayerGame game);

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

        public VisualNovelScene CreateDatabaseRevelationScene(string squadName, Act1MultiplayerGame game)
        {
            // Get player names for dynamic character naming
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));
            
            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Piltover Agent";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Zaun Operative";
            
            var scene = new VisualNovelScene
            {
                Id = "database_revelation",
                Name = "Enforcer Database - After Identification",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Piltover
            };

            // Characters - reusing existing character definitions
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
                        { CharacterExpression.Confused, "/images/vi.jpeg" },
                        { CharacterExpression.Angry, "/images/vi.jpeg" },
                        { CharacterExpression.Surprised, "/images/vi.jpeg" },
                        { CharacterExpression.Determined, "/images/vi.jpeg" },
                        { CharacterExpression.Worried, "/images/vi.jpeg" }
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
                        { CharacterExpression.Worried, "/images/cait.jpeg" },
                        { CharacterExpression.Surprised, "/images/cait.jpeg" },
                        { CharacterExpression.Serious, "/images/cait.jpeg" },
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
                        { CharacterExpression.Surprised, "/images/enforcer.png" },
                        { CharacterExpression.Serious, "/images/enforcer.png" },
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
                        { CharacterExpression.Confused, "/images/zaun_friend.png" },
                        { CharacterExpression.Determined, "/images/zaun_friend.png" }
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

            // Dialogue following Scene 3 markdown content
            scene.DialogueLines.AddRange(new[]
            {
                // Discovery Complete - Scene 3 Start
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "Piltover Enforcer HQ Records Room - Discovery Complete",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "Got them all. Four matches in the system... but this doesn't make sense.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "What do you mean? Who are they?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "Dr. Werner Steinberg, Dr. Renni Stiltner, Professor Albus Ferros, and Dr. Corin Reveck. All of them worked on something called... Project Safeguard?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Never heard of it. What's Project Safeguard?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Confused
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Wait... I recognize these names. My mother mentioned them once at a Council dinner. They're all former apprentices of Heimerdinger.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "Heimerdinger? The Council guy with the fur?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "These aren't random targets... they all worked on early Hextech prototypes before the technology was regulated. Before Jayce's breakthrough.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "But why would Powder... why would Jinx care about old scientists?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "Look at this - their project was shut down seven years ago. Right around the time of...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "The warehouse. The night Vander died.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                
                // The Revelation section
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "According to these records, Project Safeguard was an attempt to create stable Hextech cores for industrial use. But it was deemed too dangerous and shut down.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "So what, Jinx thinks these people had something to do with that night?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Confused
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "That's impossible. The explosion was... it was Powder's bomb. The hex crystals she put in it.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Vi... what if someone told her otherwise? What if someone made her believe—",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Silco. That bastard. He's been lying to her.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                
                // Radio Interruption section
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "The enforcer radio from earlier suddenly crackles to life...",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "...incident at... street... blue hair seen fleeing...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 25
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "The radio! It's picking up enforcer chatter!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Quick, we need to hear this. But the signal's breaking up.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "I can barely make it out. Something about an explosion?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Confused
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "We need to decode this. If it's about Jinx, we need to know what happened.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                
                // Setting Up Signal Decoder section
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "The audio's fragmented but I can hear pieces. If we had the written patrol logs...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "Wait! I saw a dispatch terminal over there. It shows transcripts but with missing words - probably corrupted data.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = $"Perfect. {piltoverPlayerName}, you listen to the audio transmission. {zaunPlayerName}, you read what's on the transcript.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Work together - fill in the gaps. We need to know what's happening out there.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "I'm getting clearer audio now. Ready when you are.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "Transcript's up. There are blanks everywhere but I can see the structure.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Whatever you're about to hear... it might tell us where these scientists are. Or if we're already too late.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Focus on any mention of locations, names, or Deputy Stanton. He's covering something up and we need to know what.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                
                // Transition to Signal Decoder Puzzle
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "The team prepares to decode the critical enforcer transmissions that could reveal Werner's attack and Renni's disappearance...",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                }
            });

            // Mark the end of main content
            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;

            Console.WriteLine($"[Act1StoryEngine] Database Revelation scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

        public VisualNovelScene CreateRadioDecodedScene(string squadName, Act1MultiplayerGame game)
        {
            // Get player names for dynamic character naming
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));
            
            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Piltover Agent";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Zaun Operative";
            
            var scene = new VisualNovelScene
            {
                Id = "radio_decoded",
                Name = "After Decoding the Messages",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Piltover
            };

            // Characters - reusing existing character definitions
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
                        { CharacterExpression.Serious, "/images/vi.jpeg" },
                        { CharacterExpression.Angry, "/images/vi.jpeg" },
                        { CharacterExpression.Determined, "/images/vi.jpeg" },
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
                        { CharacterExpression.Angry, "/images/cait.jpeg" },
                        { CharacterExpression.Determined, "/images/cait.jpeg" },
                        { CharacterExpression.Worried, "/images/cait.jpeg" },
                        { CharacterExpression.Serious, "/images/cait.jpeg" }
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
                        { CharacterExpression.Serious, "/images/enforcer.png" },
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
                        { CharacterExpression.Surprised, "/images/zaun_friend.png" },
                        { CharacterExpression.Worried, "/images/zaun_friend.png" },
                        { CharacterExpression.Determined, "/images/zaun_friend.png" }
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

            // Dialogue following Scene 4 markdown content
            scene.DialogueLines.AddRange(new[]
            {
                // Message Decoded section
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "After Decoding the Messages",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "Werner's workshop on Fifth Street... an explosion...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "And Dr. Renni Stiltner failed to report for protective custody. They knew these people were in danger!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Protective custody? Stanton knew about this threat and didn't warn anyone?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Worse. He's actively avoiding the warehouse district tonight for 'evidence disposal.' He's cleaning up Marcus's mess.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "So Werner's already been hit. That means...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Jinx already got to him. She's hunting them down one by one.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "But Renni's still out there. She didn't trust Stanton's protection - smart woman.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "If she's hiding on her own, she'd go somewhere she knows. Somewhere safe.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Her home. Or someone she trusts. We need to find her before Jinx does.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },

                // Planning Next Move section
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "Planning Next Move",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "According to this, Renni has an apartment above a chem-tech repair shop in Zaun. And... she has a sister. Kira.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "I know that area. It's not far from where I live. Rough neighborhood though.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Then that's where we go. Maybe the sister knows something.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "What if Jinx is already there?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Then we better hurry. Powder... Jinx... she won't hesitate. Not anymore.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "We should prepare for trouble. Stanton's enforcers might be watching, and if Jinx shows up...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "I can get us there through the back alleys. Avoid enforcer patrols.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "And I'll keep monitoring the radio. Any more chatter about attacks, we'll know.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Renni might be our only chance to understand what Silco's told Jinx. What lies he's fed her about that night.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "We'll find her, Vi. And we'll find the truth.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Let's move. Every second we waste is another second Jinx gets closer.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },

                // Transition to Renni's Apartment
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "The team heads to Zaun to find Renni's apartment and discover Renni's coded message...",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                }
            });

            // Mark the end of main content
            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;

            Console.WriteLine($"[Act1StoryEngine] Radio Decoded scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

        public VisualNovelScene CreateRenniApartmentScene(string squadName, Act1MultiplayerGame game)
        {
            // Get player names for dynamic character naming
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));
            
            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Piltover Agent";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Zaun Operative";
            
            var scene = new VisualNovelScene
            {
                Id = "renni_apartment",
                Name = "Renni's Apartment - Finding the Clue",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Zaun
            };

            // Characters - reusing existing character definitions
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
                        { CharacterExpression.Serious, "/images/vi.jpeg" },
                        { CharacterExpression.Angry, "/images/vi.jpeg" },
                        { CharacterExpression.Determined, "/images/vi.jpeg" },
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
                        { CharacterExpression.Angry, "/images/cait.jpeg" },
                        { CharacterExpression.Determined, "/images/cait.jpeg" },
                        { CharacterExpression.Worried, "/images/cait.jpeg" },
                        { CharacterExpression.Serious, "/images/cait.jpeg" }
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
                        { CharacterExpression.Serious, "/images/enforcer.png" },
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
                        { CharacterExpression.Surprised, "/images/zaun_friend.png" },
                        { CharacterExpression.Worried, "/images/zaun_friend.png" },
                        { CharacterExpression.Determined, "/images/zaun_friend.png" }
                    }
                },
                new VisualNovelCharacter
                {
                    Id = "kira",
                    Name = "Kira",
                    DisplayName = "Kira",
                    ImagePath = "/images/kira.png",
                    Position = CharacterPosition.Center,
                    ThemeColor = "#00d4aa",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "/images/kira.png" },
                        { CharacterExpression.Worried, "/images/kira.png" },
                        { CharacterExpression.Determined, "/images/kira.png" }
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

            // Full Scene 5 dialogue following documentation
            scene.DialogueLines.AddRange(new[]
            {
                // Arrival
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "This is it. Kepler's Chem-Tech Repairs. Renni lives in the apartment above.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Place looks abandoned. Shop's been closed for days by the look of it.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "The lock's been forced. Recently. Someone's been here.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "You think it was Jinx?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Only one way to find out.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "They climb the stairs to the apartment. Vi knocks firmly",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Hello? Anyone home? We're looking for Renni Stiltner!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "Footsteps inside. A young woman opens the door - eyes red from crying",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },

                // Meeting Kira
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "Who are you? What do you want?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "I'm Caitlyn Kiramman. We're here about your sister.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "You're too late. She's gone. Left three days ago after someone torched her workshop.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Sad
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Three days? Where did she go?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "If I knew that, would I be standing here? First Werner gets attacked, then the enforcers show up offering 'protection,' and Renni just... vanishes.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "You didn't trust the enforcers either.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "Trust them? Deputy Stanton couldn't even look me in the eye. Kept talking about 'containing the situation.' Renni was right not to go with them.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },

                // The Discovery
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "We're trying to help. We think your sister might be in danger.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "In danger? She's probably already dead! Just like Werner, just like the others!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Sad
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Hey. Look at me. We're not with Stanton. We're trying to stop whoever's hunting them.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "You're from down here. I can tell. Why do you care?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Confused
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Because the person doing this... she's my sister. And I need to stop her before she does something she can't take back.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Sad
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "Your sister? I... I'm sorry.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Kira, did Renni leave anything? Any clue about where she might go?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "She... wait. She was paranoid, but smart. Said if anything happened, she'd leave me a way to find her.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "Kira retrieves a crumpled paper from a drawer",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "Found this under my door yesterday. Just looks like her old study notes - definitions, word games we used to play as kids. Made no sense to me.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Confused
                },

                // Discovering the Wall
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "These are... definitions? Synonyms? Like a vocabulary test?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Confused
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "That's what I thought! But why would she—",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Confused
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "Guys! You need to see this!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "They rush to Renni's bedroom",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "The wall... it's covered in graffiti.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Not graffiti. It's a message. Look - words with missing letters.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "The words on the wall have blanks...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "And this paper has definitions! She left us a code!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Classic Renni. Even her hiding spots have puzzles.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Smug
                },

                // Setting Up Code Cracker
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "You can solve it? You can find her?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = $"We'll figure it out. {zaunPlayerName}, you have the definitions. {piltoverPlayerName}, you can see the wall clearly.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "I'll read out the definitions and synonyms.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "And I'll match them to the words on the wall, fill in the blanks.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Work fast. If Renni went to all this trouble, it means she knew someone would come looking.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "Please... just find her. She's all I have left.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "We will. I promise.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },

                // Transition to Code Cracker
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "The team prepares to decode Renni's coded message using the vocabulary definitions and the wall puzzle...",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                }
            });

            // Mark the end of main content
            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;

            Console.WriteLine($"[Act1StoryEngine] Renni Apartment scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

        public VisualNovelScene CreateCodeDecodedScene(string squadName, Act1MultiplayerGame game)
        {
            // Get player names for dynamic character naming
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));
            
            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Piltover Agent";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Zaun Operative";
            
            var scene = new VisualNovelScene
            {
                Id = "code_decoded",
                Name = "After Decoding the Message",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Zaun
            };

            // Characters - reusing existing character definitions
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
                        { CharacterExpression.Serious, "/images/vi.jpeg" },
                        { CharacterExpression.Angry, "/images/vi.jpeg" },
                        { CharacterExpression.Determined, "/images/vi.jpeg" },
                        { CharacterExpression.Worried, "/images/vi.jpeg" },
                        { CharacterExpression.Surprised, "/images/vi.jpeg" },
                        { CharacterExpression.Sad, "/images/vi.jpeg" },
                        { CharacterExpression.Happy, "/images/vi.jpeg" },
                        { CharacterExpression.Confused, "/images/vi.jpeg" },
                        { CharacterExpression.Smug, "/images/vi.jpeg" }
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
                        { CharacterExpression.Determined, "/images/cait.jpeg" },
                        { CharacterExpression.Happy, "/images/cait.jpeg" },
                        { CharacterExpression.Surprised, "/images/cait.jpeg" },
                        { CharacterExpression.Angry, "/images/cait.jpeg" }
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
                        { CharacterExpression.Serious, "/images/enforcer.png" },
                        { CharacterExpression.Worried, "/images/enforcer.png" },
                        { CharacterExpression.Determined, "/images/enforcer.png" },
                        { CharacterExpression.Happy, "/images/enforcer.png" }
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
                        { CharacterExpression.Surprised, "/images/zaun_friend.png" },
                        { CharacterExpression.Determined, "/images/zaun_friend.png" },
                        { CharacterExpression.Worried, "/images/zaun_friend.png" },
                        { CharacterExpression.Happy, "/images/zaun_friend.png" },
                        { CharacterExpression.Serious, "/images/zaun_friend.png" },
                        { CharacterExpression.Angry, "/images/zaun_friend.png" }
                    }
                },
                new VisualNovelCharacter
                {
                    Id = "kira",
                    Name = "Kira",
                    DisplayName = "Kira",
                    ImagePath = "/images/kira.png",
                    Position = CharacterPosition.Center,
                    ThemeColor = "#00d4aa",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "/images/kira.png" },
                        { CharacterExpression.Confused, "/images/kira.png" },
                        { CharacterExpression.Surprised, "/images/kira.png" },
                        { CharacterExpression.Sad, "/images/kira.png" },
                        { CharacterExpression.Serious, "/images/kira.png" },
                        { CharacterExpression.Worried, "/images/kira.png" },
                        { CharacterExpression.Determined, "/images/kira.png" }
                    }
                },
                new VisualNovelCharacter
                {
                    Id = "radio",
                    Name = "Radio",
                    DisplayName = "Radio",
                    ImagePath = "",
                    Position = CharacterPosition.Center,
                    ThemeColor = "#888888",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "" }
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

            // Dialogue following Scene 6 markdown content
            scene.DialogueLines.AddRange(new[]
            {
                // Message Revealed
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "SHIMMER... FACTORY... LEVEL... THREE. That's the message!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "The old shimmer refinement facility? That place has been abandoned for years!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "Why would she go there?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Confused
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "She wasn't leaving you a hideout location...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "She was telling you where she was going. She went to investigate.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "Wait, that's... she wouldn't. That message wasn't FOR me to follow. It's where she was heading!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "You think she went to rescue the other scientists?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "She figured out where the others were taken and she... oh no, Renni, you brave fool.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Sad
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "She went alone? When?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "The note appeared yesterday morning, so... she's been gone at least a day.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },

                // The Explosion
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "A distant explosion rattles the windows. Orange glow on the horizon",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "What was that?!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Surprised
                },
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "Player A's radio crackles to life",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "radio",
                    Text = "Explosion at old Shimmer refinement facility, level three. Blue-haired suspect seen entering...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 35
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Jinx. She's there!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "If Renni's there too...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "We go. NOW!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "Wait! Take me with you!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "No. It's too dangerous. Stay here, lock the doors.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Serious
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "We'll find her. We'll bring her back.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "kira",
                    Text = "Please... save her. Please.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Sad
                },

                // Racing to the Factory
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "The factory's not far! I know a shortcut through the old industrial district!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Jinx is there. After all this time... she's right there.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Vi, remember - she's not the Powder you knew. She's dangerous now.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "I know. But she's still my sister. I have to try.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Sad
                },
                new DialogueLine
                {
                    CharacterId = "playerA",
                    Text = "What if the scientists are already...?",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "They're not. They can't be. Silco needs them alive for something.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Angry
                },
                new DialogueLine
                {
                    CharacterId = "playerB",
                    Text = "There! The factory! I can see smoke!",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "We need to be smart about this. If Jinx set off explosions, the place could be unstable.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Determined
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "I don't care. Renni might be in there. The other scientists too. And Jinx...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40,
                    SpeakerExpression = CharacterExpression.Worried
                },

                // Transition to Shimmer Factory
                new DialogueLine
                {
                    CharacterId = "narrator",
                    Text = "The team arrives at the abandoned shimmer factory. Next: Infiltrating the factory using Renni's map - Navigation Maze puzzle where Player A guides Player B through the factory.",
                    AnimationType = TextAnimationType.FadeIn,
                    TypewriterSpeed = 30
                }
            });

            // Mark the end of main content
            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;

            Console.WriteLine($"[Act1StoryEngine] Code Decoded scene created with {scene.DialogueLines.Count} dialogues");
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
            Console.WriteLine($"[Act1StoryEngine] ProgressToNextScene: Current index={game.CurrentSceneIndex}, StoryProgression=[{string.Join(", ", game.StoryProgression)}]");

            game.CurrentSceneIndex++;
            Console.WriteLine($"[Act1StoryEngine] After increment: New index={game.CurrentSceneIndex}");

            if (game.CurrentSceneIndex < game.StoryProgression.Count)
            {
                var nextPhase = game.StoryProgression[game.CurrentSceneIndex];
                Console.WriteLine($"[Act1StoryEngine] Next phase at index {game.CurrentSceneIndex}: '{nextPhase}'");
                var originalSquadName = game.Players.FirstOrDefault()?.OriginalSquadName ?? "";

                switch (nextPhase)
                {
                    case "database_revelation":
                        // Move to Scene 3 (Database Revelation)
                        game.CurrentScene = CreateDatabaseRevelationScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Database Revelation scene");
                        break;

                    case "signal_decoder_transition":
                        // Transition to Signal Decoder puzzle
                        game.Status = Act1GameStatus.SceneTransition;
                        game.ShowTransition = true;
                        game.NextGameName = "Signal Decoder Analysis";

                        result.TransitionStarted = true;
                        result.NextGameName = game.NextGameName;

                        // Build per-player redirect URLs to Signal Decoder
                        var signalDecoderUrls = new Dictionary<string, string>();
                        foreach (var p in game.Players)
                        {
                            var parameters =
                                $"role={p.PlayerRole}&avatar={p.PlayerAvatar}&name={Uri.EscapeDataString(p.PlayerName)}&squad={Uri.EscapeDataString(p.OriginalSquadName)}&story=true&transition=FromScene3";
                            signalDecoderUrls[p.PlayerId] = $"/signal-decoder?{parameters}";
                        }
                        result.RedirectUrlsByPlayerId = signalDecoderUrls;
                        Console.WriteLine($"[Act1StoryEngine] Initiating transition to Signal Decoder");
                        break;

                    case "radio_decoded":
                        // Move to Scene 4 (Radio Decoded)
                        game.CurrentScene = CreateRadioDecodedScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Radio Decoded scene");
                        break;

                    case "renni_apartment":
                        // Move to Scene 5 (Renni's Apartment)
                        game.CurrentScene = CreateRenniApartmentScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Renni Apartment scene");
                        break;

                    case "code_cracker_transition":
                        // Transition to Code Cracker puzzle
                        game.Status = Act1GameStatus.SceneTransition;
                        game.ShowTransition = true;
                        game.NextGameName = "Code Cracker Analysis";

                        result.TransitionStarted = true;
                        result.NextGameName = game.NextGameName;

                        // Build per-player redirect URLs to Code Cracker
                        var codeCrackerUrls = new Dictionary<string, string>();
                        foreach (var p in game.Players)
                        {
                            var parameters =
                                $"role={p.PlayerRole}&avatar={p.PlayerAvatar}&name={Uri.EscapeDataString(p.PlayerName)}&squad={Uri.EscapeDataString(p.OriginalSquadName)}&story=true&transition=FromRenniApartment";
                            codeCrackerUrls[p.PlayerId] = $"/code-cracker?{parameters}";
                        }
                        result.RedirectUrlsByPlayerId = codeCrackerUrls;
                        Console.WriteLine($"[Act1StoryEngine] Initiating transition to Code Cracker");
                        break;

                    case "code_decoded":
                        // Move to Scene 6 (Code Decoded)
                        game.CurrentScene = CreateCodeDecodedScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Code Decoded scene");
                        break;

                    case "navigation_maze_transition":
                        // Transition to Navigation Maze puzzle
                        game.Status = Act1GameStatus.SceneTransition;
                        game.ShowTransition = true;
                        game.NextGameName = "Navigation Maze";

                        result.TransitionStarted = true;
                        result.NextGameName = game.NextGameName;

                        // Build per-player redirect URLs to Navigation Maze
                        var navigationMazeUrls = new Dictionary<string, string>();
                        foreach (var p in game.Players)
                        {
                            var parameters =
                                $"role={p.PlayerRole}&avatar={p.PlayerAvatar}&name={Uri.EscapeDataString(p.PlayerName)}&squad={Uri.EscapeDataString(p.OriginalSquadName)}&story=true&transition=FromCodeDecoded";
                            navigationMazeUrls[p.PlayerId] = $"/navigation-maze?{parameters}";
                        }
                        result.RedirectUrlsByPlayerId = navigationMazeUrls;
                        Console.WriteLine($"[Act1StoryEngine] Initiating transition to Navigation Maze");
                        break;

                    case "picture_explanation_transition":
                        // Legacy transition to Picture Explanation puzzle (if needed)
                        game.Status = Act1GameStatus.SceneTransition;
                        game.ShowTransition = true;
                        game.NextGameName = "Visual Intelligence Analysis";

                        result.TransitionStarted = true;
                        result.NextGameName = game.NextGameName;

                        // Build per-player redirect URLs to Picture Explanation
                        var pictureUrls = new Dictionary<string, string>();
                        foreach (var p in game.Players)
                        {
                            var parameters =
                                $"role={p.PlayerRole}&avatar={p.PlayerAvatar}&name={Uri.EscapeDataString(p.PlayerName)}&squad={Uri.EscapeDataString(p.OriginalSquadName)}&story=true&transition=FromScene1and2";
                            pictureUrls[p.PlayerId] = $"/picture-explanation?{parameters}";
                        }
                        result.RedirectUrlsByPlayerId = pictureUrls;
                        Console.WriteLine($"[Act1StoryEngine] Initiating transition to Picture Explanation");
                        break;
                }
            }
            else
            {
                game.Status = Act1GameStatus.Completed;
                result.StoryCompleted = true;
                Console.WriteLine($"[Act1StoryEngine] Act 1 story completed");
            }

            return result;
        }
    }
}

