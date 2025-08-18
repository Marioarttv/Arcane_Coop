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
        VisualNovelScene CreateShimmerFactoryEntranceScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateEmptyCellsScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateTracerCompleteScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateFollowingJinxTrailScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateJayceWorkshopArrivalScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateBombDiscoveryScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateBombDefusedScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateGauntletsCompleteScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateWarehouseApproachScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateTruthRevealedScene(string squadName, Act1MultiplayerGame game);
        VisualNovelScene CreateFinalResolutionScene(string squadName, Act1MultiplayerGame game);

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
                        { CharacterExpression.Default, "/images/Characters/Caitlyn/cait_default.png" },
                        { CharacterExpression.Serious, "/images/Characters/Caitlyn/cait_serious.png" },
                        { CharacterExpression.Worried, "/images/Characters/Caitlyn/cait_worried.png" },
                        { CharacterExpression.Surprised, "/images/Characters/Caitlyn/cait_surprised.png" },
                        { CharacterExpression.Determined, "/images/Characters/Caitlyn/cait_determined.png" }
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

        public VisualNovelScene CreateShimmerFactoryEntranceScene(string squadName, Act1MultiplayerGame game)
        {
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));

            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Player A";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Player B";

            var scene = new VisualNovelScene
            {
                Id = "shimmer_factory_entrance",
                Name = "The Shimmer Factory",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Zaun
            };

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
                        { CharacterExpression.Serious, "/images/cait.jpeg" },
                        { CharacterExpression.Worried, "/images/cait.jpeg" },
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
                        { CharacterExpression.Determined, "/images/zaun_friend.png" }
                    }
                },
                new VisualNovelCharacter
                {
                    Id = "narrator",
                    Name = "Narrator",
                    DisplayName = "Narrator",
                    ImagePath = "",
                    Position = CharacterPosition.Center,
                    ThemeColor = "#888888",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "" }
                    }
                }
            });

            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine { CharacterId = "narrator", Text = "Act 2: The Shimmer Factory", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Arriving at the Factory", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "The explosion came from the upper levels. But the structure's still standing.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "caitlyn", Text = "For now. This whole place could come down.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "playerB", Text = "Purple smoke everywhere. That's shimmer residue. We shouldn't breathe too much of it.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "playerA", Text = "Look! Over there by the entrance!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "narrator", Text = "They rush over to find a hand-drawn map weighted down by a rock", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "caitlyn", Text = "It's hand-drawn. Recently too - the ink's still fresh in places.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "vi", Text = "This is Renni's handwriting. She mapped the place out.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "playerB", Text = "She marked locations... 'Main processing,' 'Storage tanks,' and... 'Holding cells - Level 3.'", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "playerA", Text = "Holding cells. That's where she thought the scientists were.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "narrator", Text = "Planning the Approach", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "Then that's where we go.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "caitlyn", Text = "Wait. We can't all go barging in. The place is probably full of Silco's goons.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerA", Text = "According to this, there are multiple routes through the factory. Maintenance tunnels, catwalks...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerB", Text = "I could scout ahead, but I don't know the layout.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "vi", Text = $"{zaunPlayerName}, you go in first. You're quicker, quieter. {piltoverPlayerName}, you guide them using Renni's map.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "caitlyn", Text = "Vi and I will create a distraction if needed. Draw any guards away.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "narrator", Text = "Setting Up Navigation", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "playerA", Text = "Okay, I've got the map. There's an entrance through a maintenance hatch about twenty meters to your left.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerB", Text = "I see it. Looks clear.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "vi", Text = "Be careful in there. And if you see Jinx...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "playerB", Text = "I'll signal you. I won't engage alone.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "caitlyn", Text = "Guide them carefully. One wrong turn in there and they could walk right into a patrol.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerA", Text = "According to the map, once you're inside, there should be a corridor straight ahead.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerB", Text = "I'm in. It's dark. Pipes everywhere. Which way?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried }
            });

            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
            Console.WriteLine($"[Act1StoryEngine] Shimmer Factory Entrance scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

        public VisualNovelScene CreateEmptyCellsScene(string squadName, Act1MultiplayerGame game)
        {
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));

            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Player A";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Player B";

            var scene = new VisualNovelScene
            {
                Id = "empty_cells",
                Name = "The Empty Holding Cells",
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
                    Position = CharacterPosition.Left,
                    ThemeColor = "#00d4aa",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "/images/vi.jpeg" },
                        { CharacterExpression.Worried, "/images/vi.jpeg" },
                        { CharacterExpression.Determined, "/images/vi.jpeg" },
                        { CharacterExpression.Serious, "/images/vi.jpeg" }
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
                        { CharacterExpression.Worried, "/images/cait.jpeg" }
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
                        { CharacterExpression.Serious, "/images/enforcer.png" }
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
                        { CharacterExpression.Determined, "/images/zaun_friend.png" }
                    }
                },
                new VisualNovelCharacter
                {
                    Id = "narrator",
                    Name = "Narrator",
                    DisplayName = "Narrator",
                    ImagePath = "",
                    Position = CharacterPosition.Center,
                    ThemeColor = "#888888",
                    ExpressionPaths = new Dictionary<CharacterExpression, string>
                    {
                        { CharacterExpression.Default, "" }
                    }
                }
            });

            // Dialogue based on Scene8.md
            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine { CharacterId = "narrator", Text = "The Empty Holding Cells", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "playerB", Text = "I'm here but... the cells are empty. Just chains and... is that blood?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "playerA", Text = "Empty? But there are signs people were there?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "playerB", Text = "Food scraps, water bowls... and someone's glasses, broken on the floor.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "vi", Text = "They were moved. Recently by the sounds of it.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "caitlyn", Text = "Or taken somewhere else. Can you see anything else?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerB", Text = "Wait, there's a room adjoining the cells. Looks like... a laboratory?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Surprised },
                new DialogueLine { CharacterId = "narrator", Text = "The Laboratory Discovery", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "playerB", Text = "This place is full of equipment. Beakers, burners, and... tons of papers everywhere.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerA", Text = "What kind of papers?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerB", Text = "Recipes. Chemical formulas. Most of it's about shimmer refinement, but...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerB", Text = "Here's something different. 'Shimmer Residue Tracer - For tracking subjects exposed to concentrated shimmer.'", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "vi", Text = "A tracking potion? If Jinx has been using shimmer...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Surprised },
                new DialogueLine { CharacterId = "caitlyn", Text = "We could track her. Find where she's been, where she's going.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "playerA", Text = "Can you make it? Is the equipment there working?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "playerB", Text = "The equipment's functional. Silco's crew must have been using this recently. But I don't know chemistry.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "narrator", Text = "Setting Up Alchemy Lab", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "playerA", Text = "I can read the recipe to you. Tell you what to mix.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "playerB", Text = "Are you sure? One wrong mixture and this could explode.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "vi", Text = "You can do this. Both of you. We need that tracer.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "caitlyn", Text = "But be precise. Shimmer's volatile even in small amounts.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerA", Text = "Okay, first ingredient: Two parts diluted shimmer base. Do you see anything labeled like that?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerB", Text = "Yes! Purple liquid in a sealed container. Got it.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "vi", Text = "Work fast. We don't know when Jinx or Silco's people might come back.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                // Transition line to Alchemy Lab puzzle
                new DialogueLine { CharacterId = "narrator", Text = "The team begins brewing the tracer in the Alchemy Lab...", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 }
            });

            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
            Console.WriteLine($"[Act1StoryEngine] Empty Cells scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

        public VisualNovelScene CreateTracerCompleteScene(string squadName, Act1MultiplayerGame game)
        {
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));

            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Player A";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Player B";

            var scene = new VisualNovelScene
            {
                Id = "tracer_complete",
                Name = "The Tracer Revealed",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Zaun
            };

            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter { Id = "vi", Name = "Vi", DisplayName = "Vi", ImagePath = "/images/vi.jpeg", Position = CharacterPosition.Left, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "caitlyn", Name = "Caitlyn", DisplayName = "Caitlyn", ImagePath = "/images/cait.jpeg", Position = CharacterPosition.Right, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerA", Name = piltoverPlayerName, DisplayName = piltoverPlayerName, ImagePath = "/images/enforcer.png", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerB", Name = zaunPlayerName, DisplayName = zaunPlayerName, ImagePath = "/images/zaun_friend.png", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "narrator", Name = "Narrator", DisplayName = "Narrator", ImagePath = "", Position = CharacterPosition.Center, ThemeColor = "#888888" }
            });

            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine { CharacterId = "narrator", Text = "Post-Alchemy Lab Dialogue", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "The Tracer Revealed", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "playerB", Text = "It worked! The potion's glowing bright blue!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Happy },
                new DialogueLine { CharacterId = "playerA", Text = "Perfect! Now splash some on the ground. See if it reveals anything.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "narrator", Text = "The tracer splashes across the concrete...", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "playerB", Text = "Oh my god... footprints! Glowing purple footprints everywhere!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Surprised },
                new DialogueLine { CharacterId = "vi", Text = "Jinx. She's been micro-dosing shimmer.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "caitlyn", Text = "That's dangerous. Shimmer addiction can cause permanent damage.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "playerB", Text = "The footprints are recent. They lead out of here and... up?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Confused },
                new DialogueLine { CharacterId = "narrator", Text = "Following the Trail", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "Good work. Both of you.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Happy },
                new DialogueLine { CharacterId = "caitlyn", Text = "These are fresh. Maybe a few hours old at most.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerA", Text = "So Jinx was here after Renni?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "vi", Text = "Or at the same time. Come on, let's follow the trail.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "playerB", Text = "It leads to that stairwell. Goes up several floors.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "narrator", Text = "Discovering the Path", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "caitlyn", Text = "Stay alert. We're tracking her, but she could be anywhere.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "vi", Text = "After all these years... we can finally find her.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Happy },
                new DialogueLine { CharacterId = "playerA", Text = "The trail's getting stronger. More shimmer concentration.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerB", Text = "She's using more and more. That can't be good for her.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "vi", Text = "Powder, what have you done to yourself?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Sad },
                new DialogueLine { CharacterId = "caitlyn", Text = "We'll find her, Vi. We'll help her.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "narrator", Text = "Trail Leads Out", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "playerB", Text = "The trail leads outside. Across the district towards...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "playerA", Text = "That's towards the old arcade district. The abandoned sections.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "vi", Text = "She'd go somewhere familiar. Somewhere that meant something.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Serious },
                new DialogueLine { CharacterId = "caitlyn", Text = "Then that's where we go. We're close now.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "playerB", Text = "What if it's a trap? What if she knows we're following?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "vi", Text = "Then we spring it. I'm not losing her again.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "narrator", Text = "Transition: Following the shimmer trail to Jinx's hideout...", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 }
            });

            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
            Console.WriteLine($"[Act1StoryEngine] Tracer Complete scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

		public VisualNovelScene CreateFollowingJinxTrailScene(string squadName, Act1MultiplayerGame game)
		{
			var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
			var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));

			var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Player A";
			var zaunPlayerName = zaunPlayer?.PlayerName ?? "Player B";

			var scene = new VisualNovelScene
			{
				Id = "following_jinx_trail",
				Name = "Following the Trail",
				Layout = SceneLayout.DualCharacters,
				Theme = NovelTheme.Zaun
			};

			scene.Characters.AddRange(new[]
			{
				new VisualNovelCharacter { Id = "vi", Name = "Vi", DisplayName = "Vi", ImagePath = "/images/vi.jpeg", Position = CharacterPosition.Left, ThemeColor = "#00d4aa" },
				new VisualNovelCharacter { Id = "caitlyn", Name = "Caitlyn", DisplayName = "Caitlyn", ImagePath = "/images/cait.jpeg", Position = CharacterPosition.Right, ThemeColor = "#c8aa6e" },
				new VisualNovelCharacter { Id = "playerA", Name = piltoverPlayerName, DisplayName = piltoverPlayerName, ImagePath = "/images/enforcer.png", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
				new VisualNovelCharacter { Id = "playerB", Name = zaunPlayerName, DisplayName = zaunPlayerName, ImagePath = "/images/zaun_friend.png", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" },
				new VisualNovelCharacter { Id = "narrator", Name = "Narrator", DisplayName = "Narrator", ImagePath = "", Position = CharacterPosition.Center, ThemeColor = "#888888" }
			});

			scene.DialogueLines.AddRange(new[]
			{
				new DialogueLine { CharacterId = "narrator", Text = "Scene 10: Following the Trail", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "narrator", Text = "Approaching the Hideout — Abandoned Arcade District", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "vi", Text = "I know this place. We used to come here as kids. Before everything went wrong.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "It's completely abandoned now. Been that way for years.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "The footprints are everywhere here. She comes and goes frequently.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerA", Text = "Look — they converge on that old arcade. 'Jericho's Games.'", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "Powder loved that place. She'd spend hours trying to beat the high scores.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "Entering the Arcade", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "playerB", Text = "It's quiet. Too quiet.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "She's not here. But this is definitely her space.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "My god... look at this place.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "Discovering the Workshop", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "playerA", Text = "The walls... they're covered in drawings. Equations. Schematics.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "And these mannequins... they're dressed like people. Is that one supposed to be you, Vi?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "She made a whole fake family. Look — there's Mylo, Claggor... even Vander.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "Vi, I'm sorry. This must be—", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "Look at the workbench. Those are bombs. Lots of them.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "Finding the Evidence", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "playerA", Text = "Guys, you need to see this. There are photos here.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "The four scientists. All marked with 'MURDERERS' in red paint.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "And these documents... 'Proof of Hextech cores stored in Vander's warehouse.' But these are forgeries.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "Silco. He made her believe the explosion wasn't her fault.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "The False Evidence", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "playerA", Text = "Listen to this: 'The scientists stored illegal Hextech cores in civilian buildings. When exposed to crude explosives, caused catastrophic chain reaction.'", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "But that's not what happened, right?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "No. It was Powder's bomb. The hex crystals she put in it from Jayce's apartment. They caused the explosion.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "Silco convinced her otherwise. Made her think these scientists were responsible for Vander's death.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "Finding the Plans", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "playerA", Text = "There's more. Look at these schematics.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "She's planning something big. Combining old Hextech cores with... 'J's stabilization matrix?'", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "Jayce's stabilization notes. She's planning to steal from his workshop.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "Here — 'Topsider working alone now - partner sick - perfect timing.' She knows Viktor is ill.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "The Timeline", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "playerA", Text = "According to this, she's planning the raid... tonight? No, wait — tomorrow night.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "That doesn't give us much time.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "We need to warn Jayce immediately.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "Look at this drawing. It's all of us — me, Powder, Mylo, Claggor, Vander. But Vander's face is circled in red.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "The Painful Truth", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "playerB", Text = "'I'll make them pay for you.' She really believes she's avenging him.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "She doesn't know. After all these years, she doesn't know the truth.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "Silco protected her from the guilt. In his twisted way, he was trying to help her.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerA", Text = "There's a journal here. Listen: 'Silco says I'm perfect. Says the voices are just trying to make me weak. But they won't stop screaming.'", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "Voices? She's hearing voices?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "'Mylo says I'm still a jinx. Claggor says Vi will never forgive me. But Silco says they're lying. Says Vi abandoned me because she was weak.'", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "No... no, I didn't abandon her. I was taken. Marcus arrested me that night.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "She doesn't know that either. She thinks you chose to leave.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerA", Text = "This entry is from yesterday: 'Saw Vi on the bridge with an enforcer. She replaced me. Found a new sister. Silco was right — she never loved me.'", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "We have to find her. I have to explain—", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "The Urgency", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "caitlyn", Text = "First we stop her from stealing Jayce's notes. If she builds this weapon...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "It would be devastating. Combine the instability of old cores with modern stabilization?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerA", Text = "She could level a city block. Maybe more.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "Then we go to Jayce. Now. Before she has everything she needs.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "Leaving the Lair", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "vi", Text = "She kept this.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "narrator", Text = "[Vi holds up a small bunny toy]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "vi", Text = "I gave this to her when she was five. Told her it would protect her from monsters.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "She still has it. That means something, Vi.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "We should go. If Jinx comes back...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerA", Text = "Right. Jayce's workshop. We need to move.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "We'll save you, Powder. I promise. This time I won't fail you.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
			});

			scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
			Console.WriteLine($"[Act1StoryEngine] Following Jinx Trail scene created with {scene.DialogueLines.Count} dialogues");
			return scene;
		}

		public VisualNovelScene CreateJayceWorkshopArrivalScene(string squadName, Act1MultiplayerGame game)
		{
			var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
			var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));

			var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Player A";
			var zaunPlayerName = zaunPlayer?.PlayerName ?? "Player B";

			var scene = new VisualNovelScene
			{
				Id = "jayce_workshop_arrival",
				Name = "The Workshop Entrance",
				Layout = SceneLayout.DualCharacters,
				Theme = NovelTheme.Piltover
			};

			scene.Characters.AddRange(new[]
			{
				new VisualNovelCharacter { Id = "vi", Name = "Vi", DisplayName = "Vi", ImagePath = "/images/vi.jpeg", Position = CharacterPosition.Left, ThemeColor = "#00d4aa" },
				new VisualNovelCharacter { Id = "caitlyn", Name = "Caitlyn", DisplayName = "Caitlyn", ImagePath = "/images/cait.jpeg", Position = CharacterPosition.Right, ThemeColor = "#c8aa6e" },
				new VisualNovelCharacter { Id = "jayce", Name = "Jayce", DisplayName = "Jayce", ImagePath = "/images/Jayce.jpeg", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
				new VisualNovelCharacter { Id = "playerA", Name = piltoverPlayerName, DisplayName = piltoverPlayerName, ImagePath = "/images/enforcer.png", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
				new VisualNovelCharacter { Id = "playerB", Name = zaunPlayerName, DisplayName = zaunPlayerName, ImagePath = "/images/zaun_friend.png", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" },
				new VisualNovelCharacter { Id = "narrator", Name = "Narrator", DisplayName = "Narrator", ImagePath = "", Position = CharacterPosition.Center, ThemeColor = "#888888" }
			});

			scene.DialogueLines.AddRange(new[]
			{
				new DialogueLine { CharacterId = "narrator", Text = "Scene 11: The Workshop Entrance — Academy District, Jayce's Workshop", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "caitlyn", Text = "The Academy should be just ahead!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "The lights are still on in Jayce's workshop. Good, he's there.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerA", Text = "But it's too quiet. The Academy's usually bustling even at night.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "The door to the workshop... it's ajar.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "No. We're too late.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "Entering the Workshop", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "vi", Text = "Jayce! JAYCE!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "jayce", Text = "Vi? Caitlyn? Thank the gods you're here!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "caitlyn", Text = "You're alive! We thought—", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "jayce", Text = "She was here! The blue-haired girl! She took everything!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "The Theft", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "playerA", Text = "The place is ransacked.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "jayce", Text = "My stabilization notes, the frequency calibrations, even the prototype crystals - all gone!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "How long ago?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "jayce", Text = "Maybe an hour? I was in the back room when I heard the explosion. By the time I got here...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "vi", Text = "The footprints. They're fresh. She's been all over this room.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "The Discovery", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "caitlyn", Text = "Jayce, where's Viktor? Shouldn't he be here?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "jayce", Text = "He's been bedridden for days. His illness... it's getting worse. I've been trying to finish our work alone.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerA", Text = "That's why Jinx struck now. She knew you'd be alone.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "jayce", Text = "Wait... look at the floor. What's that glow?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "playerB", Text = "The shimmer trail! It leads... behind the main forge?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

				new DialogueLine { CharacterId = "narrator", Text = "Finding the Bomb", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "vi", Text = "Everyone stay back. Let me check—", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "jayce", Text = "Oh gods. Is that...?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "narrator", Text = "[Behind the forge - a complex device with Jinx's monkey symbol]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
				new DialogueLine { CharacterId = "caitlyn", Text = "It's a bomb. Everyone out, NOW!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
				new DialogueLine { CharacterId = "jayce", Text = "No! Wait! If that detonates here...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
			});

			scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
			Console.WriteLine($"[Act1StoryEngine] Jayce Workshop Arrival scene created with {scene.DialogueLines.Count} dialogues");
			return scene;
		}

        public VisualNovelScene CreateBombDiscoveryScene(string squadName, Act1MultiplayerGame game)
        {
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));

            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Player A";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Player B";

            var scene = new VisualNovelScene
            {
                Id = "bomb_discovery",
                Name = "Discovering the Bomb",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Piltover
            };

            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter { Id = "vi", Name = "Vi", DisplayName = "Vi", ImagePath = "/images/vi.jpeg", Position = CharacterPosition.Left, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "caitlyn", Name = "Caitlyn", DisplayName = "Caitlyn", ImagePath = "/images/cait.jpeg", Position = CharacterPosition.Right, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "jayce", Name = "Jayce", DisplayName = "Jayce", ImagePath = "/images/Jayce.jpeg", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerA", Name = piltoverPlayerName, DisplayName = piltoverPlayerName, ImagePath = "/images/enforcer.png", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerB", Name = zaunPlayerName, DisplayName = zaunPlayerName, ImagePath = "/images/zaun_friend.png", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "narrator", Name = "Narrator", DisplayName = "Narrator", ImagePath = "", Position = CharacterPosition.Center, ThemeColor = "#888888" }
            });

            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine { CharacterId = "narrator", Text = "Scene 12: Discovering the Bomb — Jayce's Workshop", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "caitlyn", Text = "Everyone needs to evacuate. Now!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "No! You don't understand - if it detonates here, it'll trigger all the Hextech crystals in storage!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "How many crystals?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "Enough to level half the Academy district. Maybe more.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "Look at this thing. Eight mechanical levers, all interconnected.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "There's writing etched on the casing. Rules or... instructions?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                new DialogueLine { CharacterId = "narrator", Text = "Understanding the Threat", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "caitlyn", Text = "She didn't set a timer. Why leave it without a timer?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "It's a message. She wants us to know she could destroy everything... but is choosing not to. Yet.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "Can we disarm it? There must be a way!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "I can see some rules on this side. 'Levers 1 and 5 must match positions.'", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "And I've got different rules over here. 'Exactly 5 levers must be UP.'", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                new DialogueLine { CharacterId = "narrator", Text = "Setting Up Defusal", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "It's a puzzle. Jinx always loved her puzzles.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "We need to find the configuration that satisfies all the rules.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "One wrong move could trigger it. We need to be absolutely certain.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "I'll read out my rules. We work through this logically.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "And I'll track which combinations satisfy both sets. We can do this.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "Work fast but careful. Powder... Jinx... she's testing us.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 }
            });

            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
            Console.WriteLine($"[Act1StoryEngine] Bomb Discovery scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

        public VisualNovelScene CreateBombDefusedScene(string squadName, Act1MultiplayerGame game)
        {
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));
            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Player A";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Player B";

            var scene = new VisualNovelScene
            {
                Id = "bomb_defused",
                Name = "After the Crisis",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Piltover
            };

            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter { Id = "jayce", Name = "Jayce", DisplayName = "Jayce", ImagePath = "/images/Jayce.jpeg", Position = CharacterPosition.Left, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "caitlyn", Name = "Caitlyn", DisplayName = "Caitlyn", ImagePath = "/images/Cait.jpeg", Position = CharacterPosition.Right, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "vi", Name = "Vi", DisplayName = "Vi", ImagePath = "/images/Vi.jpeg", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "playerA", Name = piltoverPlayerName, DisplayName = piltoverPlayerName, ImagePath = "/images/enforcer.png", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerB", Name = zaunPlayerName, DisplayName = zaunPlayerName, ImagePath = "/images/zaun_friend.png", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" }
            });

            scene.DialogueLines.AddRange(new List<DialogueLine>
            {
                // Relief and Realization
                new DialogueLine { CharacterId = "narrator", Text = "Scene 13: After the Crisis", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "[The bomb powers down with a mechanical whir]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "jayce", Text = "Oh thank the gods. I thought we were all dead.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "She could have killed us all. But she didn't.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "This was a demonstration. She's showing us what she's capable of.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "That was too close.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "Is it really safe now?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "The mechanism's completely inactive. We did it.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // The Next Problem
                new DialogueLine { CharacterId = "narrator", Text = "The Next Problem", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "But she still has your notes. She can build her weapon.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "With my stabilization formulas and the old unstable cores? She could create something devastating.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "Then we need to be ready to counter it.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
            
                // The Gauntlets
                new DialogueLine { CharacterId = "narrator", Text = "The Gauntlets", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "jayce", Text = "Wait... Viktor and I were developing something. The Atlas Gauntlets.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "Gauntlets?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "They can absorb and redirect Hextech energy. In theory, they could counter Jinx's weapon.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "Then let's use them!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "They're not calibrated! Viktor was supposed to help with the resonance frequencies, but he's too ill.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // Vi's Decision
                new DialogueLine { CharacterId = "narrator", Text = "Vi's Decision", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "I'll wear them.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "caitlyn", Text = "Vi, untested Hextech could be dangerous…", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Worried },
                new DialogueLine { CharacterId = "vi", Text = "I don't care. If it gives me a chance to stop her... to save her... I'll take the risk.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "The calibration process is complex. I'll need help. I can't do it alone.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "Tell us what to do.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "We've come this far. We're not stopping now.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // Setting Up the Forge
                new DialogueLine { CharacterId = "narrator", Text = "Setting Up the Forge", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "jayce", Text = "The gauntlets are here, in the prototype vault. They're almost complete, just need final calibration.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "[He retrieves massive metal gauntlets, humming with dormant energy]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "They're beautiful.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "But deadly if miscalibrated. We need to input the exact forging sequence.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "You two have been incredible so far. Can you handle this?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "Just tell me what to look for.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "I'm ready at the interface.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // The Calibration Process
                new DialogueLine { CharacterId = "narrator", Text = "The Calibration Process", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "jayce", Text = $"{piltoverPlayerName}, I'll give you the engineering specifications. You'll need to relay them.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = $"{zaunPlayerName}, you'll input the commands at the forge based on those specifications.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "How long will this take?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "Not long if we do it right. Too long if we do it wrong and have to start over.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "You can do this. Both of you. I've seen what you're capable of.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                new DialogueLine { CharacterId = "narrator", Text = "Word Forge Puzzle", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 }
            });

            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
            Console.WriteLine($"[Act1StoryEngine] Bomb Defused scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

        public VisualNovelScene CreateGauntletsCompleteScene(string squadName, Act1MultiplayerGame game)
        {
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));
            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Player A";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Player B";

            var scene = new VisualNovelScene
            {
                Id = "gauntlets_complete",
                Name = "The Atlas Gauntlets Complete",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Piltover
            };

            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter { Id = "jayce", Name = "Jayce", DisplayName = "Jayce", ImagePath = "/images/Jayce.jpeg", Position = CharacterPosition.Left, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "caitlyn", Name = "Caitlyn", DisplayName = "Caitlyn", ImagePath = "/images/Cait.jpeg", Position = CharacterPosition.Right, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "vi", Name = "Vi", DisplayName = "Vi", ImagePath = "/images/Vi.jpeg", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "playerA", Name = piltoverPlayerName, DisplayName = piltoverPlayerName, ImagePath = "/images/enforcer.png", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerB", Name = zaunPlayerName, DisplayName = zaunPlayerName, ImagePath = "/images/zaun_friend.png", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "narrator", Name = "Narrator", DisplayName = "Narrator", ImagePath = "", Position = CharacterPosition.Center, ThemeColor = "#888888" }
            });

            scene.DialogueLines.AddRange(new List<DialogueLine>
            {
                // Success
                new DialogueLine { CharacterId = "narrator", Text = "Scene 14: The Atlas Gauntlets Complete", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "[The gauntlets pulse with blue Hextech energy]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "jayce", Text = "It worked! The calibration is perfect!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "They're... incredible. I can feel the power.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "[Vi flexes her fingers, the gauntlets responding instantly]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "caitlyn", Text = "The way they move... it's like they're part of you.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "We did it. We actually did it.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "Now we have a fighting chance against Jinx's weapon.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // The Moment of Truth
                new DialogueLine { CharacterId = "narrator", Text = "The Moment of Truth", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "jayce", Text = "These gauntlets can absorb Hextech energy and redirect it. But Vi, you need to be careful.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "They feel right. Like they were meant for me.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "Viktor would say... use them to protect, not just to punish. I hope he recovers in time to see them in action.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "I know what I have to do. Protect the family. Protect Powder, even from herself.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // New Information
                new DialogueLine { CharacterId = "narrator", Text = "[Player A's radio crackles urgently]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Radio Voice:", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "\"All units... disturbance at warehouse district... reports of... Silco's men moving... large gathering...\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "caitlyn", Text = "The warehouse district? That's where—", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "Where Vander died. Where it all went wrong.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "They're saying something about a meeting. Silco and... someone else.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "You think it's Jinx?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // The Final Push
                new DialogueLine { CharacterId = "narrator", Text = "The Final Push", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "It has to be. She's going back to where it all started.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40, SpeakerExpression = CharacterExpression.Determined },
                new DialogueLine { CharacterId = "caitlyn", Text = "This could be our chance. If we can get there before—", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "Before she does something she can't take back. Let's go.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jayce", Text = "Be careful! The gauntlets are powerful but they're not invincible!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "We're with you, Vi.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "All the way. Let's finish this.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "Hold on, Powder. I'm coming. This time, I won't let you down.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // Transition
                new DialogueLine { CharacterId = "narrator", Text = "Transition: Racing to the warehouse district", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 }
            });

            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
            Console.WriteLine($"[Act1StoryEngine] Gauntlets Complete scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

        public VisualNovelScene CreateWarehouseApproachScene(string squadName, Act1MultiplayerGame game)
        {
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));
            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Player A";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Player B";

            var scene = new VisualNovelScene
            {
                Id = "warehouse_approach",
                Name = "Approaching the Warehouse",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Zaun
            };

            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter { Id = "vi", Name = "Vi", DisplayName = "Vi", ImagePath = "/images/Vi.jpeg", Position = CharacterPosition.Left, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "caitlyn", Name = "Caitlyn", DisplayName = "Caitlyn", ImagePath = "/images/Cait.jpeg", Position = CharacterPosition.Right, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerA", Name = piltoverPlayerName, DisplayName = piltoverPlayerName, ImagePath = "/images/enforcer.png", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerB", Name = zaunPlayerName, DisplayName = zaunPlayerName, ImagePath = "/images/zaun_friend.png", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "narrator", Name = "Narrator", DisplayName = "Narrator", ImagePath = "", Position = CharacterPosition.Center, ThemeColor = "#888888" }
            });

            scene.DialogueLines.AddRange(new List<DialogueLine>
            {
                new DialogueLine { CharacterId = "narrator", Text = "Scene 15: Approaching the Warehouse", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Location: Abandoned Warehouse District", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },

                // Return to the Past
                new DialogueLine { CharacterId = "vi", Text = "This is it. Where everything went wrong seven years ago.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "Are you sure you're ready for this?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "I have to be. For Powder.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "The place looks different. More run down, but... there are lights inside.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "Fresh shimmer tracks everywhere. Multiple sets. Jinx has been here a lot.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // Voices Inside
                new DialogueLine { CharacterId = "narrator", Text = "[They creep closer, hearing voices through broken windows]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx's Voice (Muffled but audible): \"It's ready, Silco! The weapon that'll make them pay for what they did to Vander!\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Silco's Voice (Calm): \"Excellent work, Jinx. With the scientists' cooperation, we finally have all the pieces.\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "Scientists' cooperation? But she thinks they're dead...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "Listen...", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // The Fatal Slip
                new DialogueLine { CharacterId = "narrator", Text = "Jinx's Voice (Confused): \"Cooperation? But... you said they fled. You said they were gone.\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Silco's Voice (Trying to recover): \"I meant their notes, their research—\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx's Voice (Rising): \"You said they DISAPPEARED. That they were too scared to face justice. But cooperation means... where are they, Silco? WHERE ARE THEY?\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "playerA", Text = "She's figuring it out.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "We need to get in there.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // Entering the Warehouse
                new DialogueLine { CharacterId = "narrator", Text = "[Vi kicks open the door]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "Powder!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "[Inside: Jinx spins around, weapon in hand. Silco stands near a table covered in papers]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Shocked, then angry): \"Vi?! What are you doing here? And with... with THEM?\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Silco (Composed but tense): \"Well. The prodigal sister returns.\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },

                // The Standoff
                new DialogueLine { CharacterId = "caitlyn", Text = "Put the weapon down, Jinx.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Laughing manically): \"Jinx? JINX?! Even you're calling me that now, Vi?\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "Powder, please. You need to listen—", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Screaming): \"DON'T call me that! Powder's DEAD! You left her to die!\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "I didn't leave you! I was taken! Marcus arrested me that night!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // The Truth Begins
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Faltering): \"No... no, you abandoned me. You said I was a jinx and you LEFT!\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Silco (Interjecting): \"Jinx, don't listen to her. She's trying to confuse you—\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "playerA", Text = "We have proof! Evidence about what really happened that night!", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "About the warehouse, about the explosion. The real truth.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Conflicted): \"Shut up! SHUT UP! Silco told me the truth! The scientists stored Hextech cores! They killed Vander!\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },

                // Setting Up the Revelation
                new DialogueLine { CharacterId = "vi", Text = "That's a lie, Powder. And we can prove it.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "Show her. Show her everything we found.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "We've collected evidence from all over the city.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "Police reports, witness accounts, scientific records.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Wavering): \"I don't... I don't want to hear this...\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "You need to. Please, Powder. Just look at the evidence.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                new DialogueLine { CharacterId = "narrator", Text = "Detective Puzzle Setup", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 }
            });

            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
            Console.WriteLine($"[Act1StoryEngine] Warehouse Approach scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

        public VisualNovelScene CreateTruthRevealedScene(string squadName, Act1MultiplayerGame game)
        {
            var piltoverPlayerName = game.Players.FirstOrDefault(p => p.PlayerRole == "piltover")?.PlayerName ?? "Piltover Agent";
            var zaunPlayerName = game.Players.FirstOrDefault(p => p.PlayerRole == "zaun")?.PlayerName ?? "Zaun Operative";

            var scene = new VisualNovelScene
            {
                Id = "truth_revealed",
                Name = "The Truth Revealed",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Zaun
            };

            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter { Id = "vi", Name = "Vi", DisplayName = "Vi", ImagePath = "/images/Vi.jpeg", Position = CharacterPosition.Left, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "jinx", Name = "Jinx", DisplayName = "Jinx", ImagePath = "/images/jinx.png", Position = CharacterPosition.Center, ThemeColor = "#ff00ff" },
                new VisualNovelCharacter { Id = "caitlyn", Name = "Caitlyn", DisplayName = "Caitlyn", ImagePath = "/images/Cait.jpeg", Position = CharacterPosition.Right, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerA", Name = piltoverPlayerName, DisplayName = piltoverPlayerName, ImagePath = "/images/enforcer.png", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerB", Name = zaunPlayerName, DisplayName = zaunPlayerName, ImagePath = "/images/zaun_operative.png", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "silco", Name = "Silco", DisplayName = "Silco", ImagePath = "/images/silco.png", Position = CharacterPosition.Center, ThemeColor = "#800080" },
                new VisualNovelCharacter { Id = "narrator", Name = "Narrator", DisplayName = "", ImagePath = "", Position = CharacterPosition.Center, ThemeColor = "#888888" }
            });

            scene.DialogueLines.AddRange(new[]
            {
                // After completing the Final Puzzle
                new DialogueLine { CharacterId = "narrator", Text = "Evidence Revealed", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "[The truth has been laid bare through the evidence you've assembled]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                
                // Jinx's Realization
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Shaking, holding the evidence): \"No... no, this can't be...\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "Powder, your bomb worked. The hex crystals you put in it... they were enough.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Breaking down): \"The crystals weren't strong enough... Silco said they weren't enough...\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Silco (Desperate): \"Jinx, they're lying. Your bomb was small, the crystals were just—\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                
                // The Complete Breakdown
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Dropping weapon, clutching head): \"The voices said... Mylo said it wasn't... but it WAS! IT WAS ME!\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "[She sees hallucinations intensifying]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Screaming): \"I put the crystals in! I made it work! I MADE IT WORK TOO WELL!\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "Powder—", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Hysterical): \"All this time... all these people... for nothing... for a LIE!\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                
                // Confronting Silco
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Whirling on Silco): \"You KNEW! You let me believe—\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Silco (Finally honest): \"I was protecting you! The guilt would have destroyed you!\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Rage and pain): \"So you let me destroy THEM instead?!\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "playerA", Text = "The scientists are still alive, Jinx. You can still save them.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "It's not too late to make this right.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                
                // The Choice
                new DialogueLine { CharacterId = "vi", Text = "Powder, I know the truth hurts. But you're not alone. You never were.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Sobbing): \"I killed them... Mylo, Claggor, Vander... I killed them all...\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Silco (Soft): \"You're perfect, Jinx. Even now—\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Screaming): \"STOP LYING TO ME!\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "[She grabs her weapon, pointing it wildly at everyone]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "caitlyn", Text = "We need to help her. What do we do?", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                
                // Player Choice Moment
                new DialogueLine 
                { 
                    CharacterId = "narrator", 
                    Text = "Critical Decision", 
                    AnimationType = TextAnimationType.FadeIn, 
                    TypewriterSpeed = 30,
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice 
                        { 
                            Id = "help_silco", 
                            Text = "Help Silco escape - he knows where the scientists are", 
                            RequiredRole = "piltover"
                        },
                        new DialogueChoice 
                        { 
                            Id = "calm_jinx", 
                            Text = "Try to calm Jinx down with words", 
                            RequiredRole = "piltover"
                        },
                        new DialogueChoice 
                        { 
                            Id = "protect_vi", 
                            Text = "Get between Vi and Jinx to protect them both", 
                            RequiredRole = "zaun"
                        },
                        new DialogueChoice 
                        { 
                            Id = "find_scientists", 
                            Text = "Find and free the scientists while everyone's distracted", 
                            RequiredRole = "zaun"
                        }
                    }
                },
                
                // Resolution (varies based on choices)
                new DialogueLine { CharacterId = "vi", Text = "Powder, please. Whatever happens next, know that I never stopped loving you. I never stopped looking for you.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (Broken but listening): \"Vi... I... I don't know who I am anymore...\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "[The warehouse falls silent except for Jinx's quiet sobs. The truth has finally been revealed, but at what cost?]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                
                new DialogueLine { CharacterId = "narrator", Text = "To Be Continued...", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 }
            });

            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
            Console.WriteLine($"[Act1StoryEngine] Truth Revealed scene created with {scene.DialogueLines.Count} dialogues");
            return scene;
        }

        public VisualNovelScene CreateFinalResolutionScene(string squadName, Act1MultiplayerGame game)
        {
            var piltoverPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("piltover", StringComparison.OrdinalIgnoreCase));
            var zaunPlayer = game.Players.FirstOrDefault(p => p.PlayerRole.Equals("zaun", StringComparison.OrdinalIgnoreCase));
            var piltoverPlayerName = piltoverPlayer?.PlayerName ?? "Player A";
            var zaunPlayerName = zaunPlayer?.PlayerName ?? "Player B";

            var scene = new VisualNovelScene
            {
                Id = "final_resolution",
                Name = "Resolution",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Zaun
            };

            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter { Id = "vi", Name = "Vi", DisplayName = "Vi", ImagePath = "/images/Vi.jpeg", Position = CharacterPosition.Left, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "caitlyn", Name = "Caitlyn", DisplayName = "Caitlyn", ImagePath = "/images/Cait.jpeg", Position = CharacterPosition.Right, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerA", Name = piltoverPlayerName, DisplayName = piltoverPlayerName, ImagePath = "/images/enforcer.png", Position = CharacterPosition.Center, ThemeColor = "#c8aa6e" },
                new VisualNovelCharacter { Id = "playerB", Name = zaunPlayerName, DisplayName = zaunPlayerName, ImagePath = "/images/zaun_friend.png", Position = CharacterPosition.Center, ThemeColor = "#00d4aa" },
                new VisualNovelCharacter { Id = "narrator", Name = "Narrator", DisplayName = "Narrator", ImagePath = "", Position = CharacterPosition.Center, ThemeColor = "#888888" }
            });

            scene.DialogueLines.AddRange(new List<DialogueLine>
            {
                new DialogueLine { CharacterId = "narrator", Text = "Scene 17: Resolution — Warehouse, After the Confrontation", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },

                // Immediate Aftermath – Path A/B merged into coherent flow
                new DialogueLine { CharacterId = "narrator", Text = "[The dust settles. The truth hangs heavy in the air]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "No. You can't undo what's done. But you can choose what happens next.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "The scientists are still alive. Tell us where they are.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (weakly): \"Shimmer factory... underwater level... Silco's private cells...\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },

                // The Scientists' Location
                new DialogueLine { CharacterId = "vi", Text = "Can you get to them? Caitlyn and I will handle things here.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "We're on it.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "Those people have been through enough. Let's go.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "Be careful. There might still be guards.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // Intercut while players are gone
                new DialogueLine { CharacterId = "vi", Text = "Powder... Jinx... whoever you are now. I'm sorry.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (bitter laugh): \"You're sorry? I killed them, Vi. I killed our family.\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = "And I wasn't there to help you through it. We both carry that weight.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // Players return
                new DialogueLine { CharacterId = "playerA", Text = "We found them! The scientists — they're alive but weak.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "Medical teams are on the way. They're going to be okay.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "narrator", Text = "Jinx (surprised): \"They're... alive? I didn't... I didn't kill them too?\"", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "caitlyn", Text = "No. You scared them, hurt them maybe, but they're alive.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // The Decision
                new DialogueLine { CharacterId = "vi", Text = "You have a choice now. Come with us. Face what you've done. Try to heal.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "Or get help. Real help. The things you've been through... no one should face that alone.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "The scientists want to speak for you. They know you were manipulated.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "Renni especially. She says you're a victim too.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // Silco's fate (light touch)
                new DialogueLine { CharacterId = "narrator", Text = "[Enforcers close in on Silco — whether he is arrested or slips away depends on earlier choices]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },

                // The Goodbye
                new DialogueLine { CharacterId = "jinx", Text = "I can't go with you. Not yet. Maybe... maybe not ever.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "I'm not giving up on you. Not ever.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "jinx", Text = "I'm not Powder anymore. But... maybe I'm not just Jinx either.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // Epilogue — The Last Drop
                new DialogueLine { CharacterId = "narrator", Text = "[A few days later, at the Last Drop]", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "vi", Text = $"I wanted to thank you both. Without you, we never would have uncovered the truth.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "You two make quite a team. Piltover and Zaun, working together.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerA", Text = "We just did what needed to be done.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "playerB", Text = "Besides, someone had to keep you two from killing each other at the start.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "The Council approved real aid for Zaun. The scientists are pardoned.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "caitlyn", Text = "We'll keep working — together — and keep an eye out if a certain blue-haired girl needs help.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },
                new DialogueLine { CharacterId = "vi", Text = "She'll come back when she's ready. And when she does... we'll be ready to help her, not fight her.", AnimationType = TextAnimationType.Typewriter, TypewriterSpeed = 40 },

                // Closing narration
                new DialogueLine { CharacterId = "narrator", Text = "The truth has a way of finding the light, even in the darkest corners of our cities.", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "Sometimes the hardest battles aren't against our enemies, but against the lies we tell ourselves.", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 },
                new DialogueLine { CharacterId = "narrator", Text = "With courage and trust, even the deepest wounds can begin to heal.", AnimationType = TextAnimationType.FadeIn, TypewriterSpeed = 30 }
            });

            scene.MainContentEndIndex = scene.DialogueLines.Count - 1;
            Console.WriteLine($"[Act1StoryEngine] Final Resolution scene created with {scene.DialogueLines.Count} dialogues");
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

                    case "shimmer_factory_entrance":
                        // Move to Scene 7 (Shimmer Factory Entrance)
                        game.CurrentScene = CreateShimmerFactoryEntranceScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Shimmer Factory Entrance scene");
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

                    case "empty_cells":
                        // Move to Scene 8 (Empty Holding Cells)
                        game.CurrentScene = CreateEmptyCellsScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Empty Cells scene");
                        break;

                    case "alchemy_lab_transition":
                        // Transition to Alchemy Lab puzzle (Shimmer Tracer)
                        game.Status = Act1GameStatus.SceneTransition;
                        game.ShowTransition = true;
                        game.NextGameName = "Alchemy Lab";

                        result.TransitionStarted = true;
                        result.NextGameName = game.NextGameName;

                        // Build per-player redirect URLs to Alchemy Lab
                        var alchemyUrls = new Dictionary<string, string>();
                        foreach (var p in game.Players)
                        {
                            var parameters =
                                $"role={p.PlayerRole}&avatar={p.PlayerAvatar}&name={Uri.EscapeDataString(p.PlayerName)}&squad={Uri.EscapeDataString(p.OriginalSquadName)}&story=true&transition=FromEmptyCells";
                            alchemyUrls[p.PlayerId] = $"/alchemy-lab?{parameters}";
                        }
                        result.RedirectUrlsByPlayerId = alchemyUrls;
                        Console.WriteLine($"[Act1StoryEngine] Initiating transition to Alchemy Lab");
                        break;

                    case "tracer_complete":
                        // Move to Scene 9 (Tracer Revealed)
                        game.CurrentScene = CreateTracerCompleteScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Tracer Complete scene");
                        break;

                    case "following_jinx_trail":
                        // Move to Scene 10 (Following the Trail)
                        game.CurrentScene = CreateFollowingJinxTrailScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Following Jinx Trail scene");
                        break;

                    case "jayce_workshop_arrival":
                        // Move to Scene 11 (Workshop Entrance)
                        game.CurrentScene = CreateJayceWorkshopArrivalScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Jayce Workshop Arrival scene");
                        break;

                    case "bomb_discovery":
                        // Move to Scene 12 (Discovering the Bomb)
                        game.CurrentScene = CreateBombDiscoveryScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Bomb Discovery scene");
                        break;

                    case "rune_protocol_transition":
                        // Transition to Rune Protocol puzzle (Bomb Defusal)
                        game.Status = Act1GameStatus.SceneTransition;
                        game.ShowTransition = true;
                        game.NextGameName = "Rune Protocol";

                        result.TransitionStarted = true;
                        result.NextGameName = game.NextGameName;

                        // Build per-player redirect URLs to Rune Protocol
                        var runeUrls = new Dictionary<string, string>();
                        foreach (var p in game.Players)
                        {
                            var parameters =
                                $"role={p.PlayerRole}&avatar={p.PlayerAvatar}&name={Uri.EscapeDataString(p.PlayerName)}&squad={Uri.EscapeDataString(p.OriginalSquadName)}&story=true&transition=FromJayceWorkshop";
                            runeUrls[p.PlayerId] = $"/rune-protocol?{parameters}";
                        }
                        result.RedirectUrlsByPlayerId = runeUrls;
                        Console.WriteLine($"[Act1StoryEngine] Initiating transition to Rune Protocol");
                        break;

                    case "bomb_defused":
                        // Move to Scene 13 (After the Crisis)
                        game.CurrentScene = CreateBombDefusedScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Bomb Defused scene");
                        break;

                    case "word_forge_transition":
                        // Transition to Word Forge puzzle (Gauntlet Calibration)
                        game.Status = Act1GameStatus.SceneTransition;
                        game.ShowTransition = true;
                        game.NextGameName = "Word Forge";

                        result.TransitionStarted = true;
                        result.NextGameName = game.NextGameName;

                        // Build per-player redirect URLs to Word Forge
                        var wordForgeUrls = new Dictionary<string, string>();
                        foreach (var p in game.Players)
                        {
                            var parameters =
                                $"role={p.PlayerRole}&avatar={p.PlayerAvatar}&name={Uri.EscapeDataString(p.PlayerName)}&squad={Uri.EscapeDataString(p.OriginalSquadName)}&story=true&transition=FromBombDefused";
                            wordForgeUrls[p.PlayerId] = $"/word-forge?{parameters}";
                        }
                        result.RedirectUrlsByPlayerId = wordForgeUrls;
                        Console.WriteLine($"[Act1StoryEngine] Initiating transition to Word Forge");
                        break;

                    case "gauntlets_complete":
                        // Move to Scene 14 (Gauntlets Complete)
                        game.CurrentScene = CreateGauntletsCompleteScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Gauntlets Complete scene");
                        break;

                    case "warehouse_approach":
                        // Move to Scene 15 (Approaching the Warehouse)
                        game.CurrentScene = CreateWarehouseApproachScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Warehouse Approach scene");
                        break;

                    case "final_puzzle_transition":
                        // Transition to Final Puzzle - Truth Echo
                        game.Status = Act1GameStatus.SceneTransition;
                        game.ShowTransition = true;
                        game.NextGameName = "Truth Echo - Final Confrontation";
                        
                        result.TransitionStarted = true;
                        result.NextGameName = game.NextGameName;
                        
                        var finalPuzzleUrls = new Dictionary<string, string>();
                        foreach (var p in game.Players)
                        {
                            var parameters = $"role={p.PlayerRole}&avatar={p.PlayerAvatar}&name={Uri.EscapeDataString(p.PlayerName)}&squad={Uri.EscapeDataString(p.OriginalSquadName)}&story=true&transition=FromWarehouse";
                            finalPuzzleUrls[p.PlayerId] = $"/finalpuzzle?{parameters}";
                        }
                        result.RedirectUrlsByPlayerId = finalPuzzleUrls;
                        
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Transitioning to Final Puzzle");
                        break;

                    case "truth_revealed":
                        // Move to Scene 16 (Truth Revealed)
                        game.CurrentScene = CreateTruthRevealedScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Truth Revealed scene");
                        break;
                    case "final_resolution":
                        // Move to Scene 17 (Resolution)
                        game.CurrentScene = CreateFinalResolutionScene(originalSquadName, game);
                        game.GameState.CurrentSceneId = game.CurrentScene.Id;
                        game.GameState.CurrentDialogueIndex = 0;
                        game.GameState.IsTextFullyDisplayed = false;
                        game.IsTextAnimating = true;
                        game.TextAnimationStartTime = DateTime.UtcNow;
                        game.RecordAction();
                        Console.WriteLine($"[Act1StoryEngine] Progressed to Final Resolution scene");
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

