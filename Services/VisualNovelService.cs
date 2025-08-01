using Arcane_Coop.Models;

namespace Arcane_Coop.Services
{
    public interface IVisualNovelService
    {
        Task<VisualNovelScene> GetSceneAsync(string sceneId);
        Task<List<VisualNovelScene>> GetScenesByThemeAsync(NovelTheme theme);
        Task SaveProgressAsync(string playerId, VisualNovelState state);
        Task<VisualNovelState?> LoadProgressAsync(string playerId);
        VisualNovelScene CreatePiltoverIntroScene();
        VisualNovelScene CreateZaunIntroScene();
        VisualNovelScene CreateEscapeRoomScene(NovelTheme theme, string context);
    }

    public class VisualNovelService : IVisualNovelService
    {
        private readonly Dictionary<string, VisualNovelScene> _scenes = new();
        private readonly Dictionary<string, VisualNovelState> _playerProgress = new();

        public VisualNovelService()
        {
            InitializeDefaultScenes();
        }

        public async Task<VisualNovelScene> GetSceneAsync(string sceneId)
        {
            await Task.Delay(1); // Simulate async operation
            return _scenes.TryGetValue(sceneId, out var scene) ? scene : throw new ArgumentException($"Scene '{sceneId}' not found");
        }

        public async Task<List<VisualNovelScene>> GetScenesByThemeAsync(NovelTheme theme)
        {
            await Task.Delay(1); // Simulate async operation
            return _scenes.Values.Where(s => s.Theme == theme).ToList();
        }

        public async Task SaveProgressAsync(string playerId, VisualNovelState state)
        {
            await Task.Delay(1); // Simulate async operation
            _playerProgress[playerId] = state;
        }

        public async Task<VisualNovelState?> LoadProgressAsync(string playerId)
        {
            await Task.Delay(1); // Simulate async operation
            return _playerProgress.TryGetValue(playerId, out var state) ? state : null;
        }

        public VisualNovelScene CreatePiltoverIntroScene()
        {
            var scene = new VisualNovelScene
            {
                Id = "piltover_intro",
                Name = "Piltover Introduction",
                Layout = SceneLayout.SingleCenter,
                Theme = NovelTheme.Piltover
            };

            var jayce = new VisualNovelCharacter
            {
                Id = "jayce",
                Name = "Jayce",
                DisplayName = "Jayce Talis - The Man of Progress",
                ImagePath = "/images/Jayce.jpeg",
                Position = CharacterPosition.Center,
                ThemeColor = "#c8aa6e"
            };

            scene.Characters.Add(jayce);

            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine
                {
                    CharacterId = "jayce",
                    Text = "Welcome to Piltover, the City of Progress. Here, innovation and ambition drive us toward a brighter future.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40
                },
                new DialogueLine
                {
                    CharacterId = "jayce",
                    Text = "Our Hextech research has opened doorways to possibilities we never imagined. But with great power...",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 45
                },
                new DialogueLine
                {
                    CharacterId = "jayce",
                    Text = "Today, you'll help us uncover a conspiracy that threatens everything we've built. Are you ready to join the investigation?",
                    AnimationType = TextAnimationType.FadeIn
                }
            });

            return scene;
        }

        public VisualNovelScene CreateZaunIntroScene()
        {
            var scene = new VisualNovelScene
            {
                Id = "zaun_intro",
                Name = "Zaun Introduction",
                Layout = SceneLayout.SingleCenter,
                Theme = NovelTheme.Zaun
            };

            var vi = new VisualNovelCharacter
            {
                Id = "vi",
                Name = "Vi",
                DisplayName = "Vi - The Piltover Enforcer",
                ImagePath = "/images/Vi.jpeg",
                Position = CharacterPosition.Center,
                ThemeColor = "#00d4aa"
            };

            scene.Characters.Add(vi);

            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Welcome to the Undercity. Down here, we don't have fancy towers or shiny tech - just grit, determination, and each other.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 35
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "The Chem-Barons think they own these streets, but they're wrong. Today, we're gonna show them what real power looks like.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 30
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Stick with me, and we'll crack this case wide open. Just remember - in Zaun, trust is earned, not given.",
                    AnimationType = TextAnimationType.SlideUp
                }
            });

            return scene;
        }

        public VisualNovelScene CreateEscapeRoomScene(NovelTheme theme, string context)
        {
            var scene = new VisualNovelScene
            {
                Id = $"escape_room_{theme.ToString().ToLower()}_{Guid.NewGuid().ToString()[..8]}",
                Name = $"Escape Room - {theme} Context",
                Layout = SceneLayout.DualCharacters,
                Theme = theme
            };

            if (theme == NovelTheme.Piltover)
            {
                scene.Characters.AddRange(new[]
                {
                    new VisualNovelCharacter
                    {
                        Id = "jayce",
                        Name = "Jayce",
                        DisplayName = "Jayce Talis",
                        ImagePath = "/images/Jayce.jpeg",
                        Position = CharacterPosition.Left,
                        ThemeColor = "#c8aa6e"
                    },
                    new VisualNovelCharacter
                    {
                        Id = "viktor",
                        Name = "Viktor",
                        DisplayName = "Viktor",
                        ImagePath = "/images/Viktor.jpeg",
                        Position = CharacterPosition.Right,
                        ThemeColor = "#0596aa"
                    }
                });

                scene.DialogueLines.Add(new DialogueLine
                {
                    CharacterId = "jayce",
                    Text = $"The situation has escalated. {context} We need to work together to solve this before it's too late.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40
                });
            }
            else
            {
                scene.Characters.AddRange(new[]
                {
                    new VisualNovelCharacter
                    {
                        Id = "vi",
                        Name = "Vi",
                        DisplayName = "Vi",
                        ImagePath = "/images/Vi.jpeg",
                        Position = CharacterPosition.Left,
                        ThemeColor = "#00d4aa"
                    },
                    new VisualNovelCharacter
                    {
                        Id = "caitlyn",
                        Name = "Caitlyn",
                        DisplayName = "Sheriff Caitlyn",
                        ImagePath = "/images/Cait.jpeg",
                        Position = CharacterPosition.Right,
                        ThemeColor = "#ff007f"
                    }
                });

                scene.DialogueLines.Add(new DialogueLine
                {
                    CharacterId = "vi",
                    Text = $"Things just got complicated. {context} We're gonna need to be smart about this.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 35
                });
            }

            return scene;
        }

        private void InitializeDefaultScenes()
        {
            // Add default scenes to the collection
            var piltoverIntro = CreatePiltoverIntroScene();
            var zaunIntro = CreateZaunIntroScene();

            _scenes[piltoverIntro.Id] = piltoverIntro;
            _scenes[zaunIntro.Id] = zaunIntro;

            // Add more default scenes as needed
            var piltoverMidGame = CreatePiltoverMidGameScene();
            var zaunMidGame = CreateZaunMidGameScene();

            _scenes[piltoverMidGame.Id] = piltoverMidGame;
            _scenes[zaunMidGame.Id] = zaunMidGame;
        }

        private VisualNovelScene CreatePiltoverMidGameScene()
        {
            var scene = new VisualNovelScene
            {
                Id = "piltover_midgame",
                Name = "Piltover Investigation",
                Layout = SceneLayout.DualCharacters,
                Theme = NovelTheme.Piltover
            };

            scene.Characters.AddRange(new[]
            {
                new VisualNovelCharacter
                {
                    Id = "jayce",
                    Name = "Jayce",
                    DisplayName = "Jayce Talis",
                    ImagePath = "/images/Jayce.jpeg",
                    Position = CharacterPosition.Left,
                    ThemeColor = "#c8aa6e"
                },
                new VisualNovelCharacter
                {
                    Id = "viktor",
                    Name = "Viktor",
                    DisplayName = "Viktor",
                    ImagePath = "/images/Viktor.jpeg",
                    Position = CharacterPosition.Right,
                    ThemeColor = "#0596aa"
                }
            });

            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine
                {
                    CharacterId = "viktor",
                    Text = "The data patterns suggest someone has been manipulating the Hextech network from within. This is more serious than we thought.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 45
                },
                new DialogueLine
                {
                    CharacterId = "jayce",
                    Text = "An inside job? But who would have access to the core systems? The security protocols should have prevented this.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 40
                },
                new DialogueLine
                {
                    CharacterId = "viktor",
                    Text = "Perhaps the question is not who, but why. What could they hope to gain by destabilizing our progress?",
                    AnimationType = TextAnimationType.FadeIn
                }
            });

            return scene;
        }

        private VisualNovelScene CreateZaunMidGameScene()
        {
            var scene = new VisualNovelScene
            {
                Id = "zaun_midgame",
                Name = "Zaun Investigation",
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
                    ImagePath = "/images/Vi.jpeg",
                    Position = CharacterPosition.Left,
                    ThemeColor = "#00d4aa"
                },
                new VisualNovelCharacter
                {
                    Id = "caitlyn",
                    Name = "Caitlyn",
                    DisplayName = "Sheriff Caitlyn",
                    ImagePath = "/images/Cait.jpeg",
                    Position = CharacterPosition.Right,
                    ThemeColor = "#ff007f"
                }
            });

            scene.DialogueLines.AddRange(new[]
            {
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "The evidence leads back to the old mining tunnels. Someone's been using the abandoned infrastructure for their operations.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 45
                },
                new DialogueLine
                {
                    CharacterId = "vi",
                    Text = "Those tunnels connect to half the Undercity. If they're using them to move product, we're looking at a network bigger than we imagined.",
                    AnimationType = TextAnimationType.Typewriter,
                    TypewriterSpeed = 35
                },
                new DialogueLine
                {
                    CharacterId = "caitlyn",
                    Text = "Then we need to be strategic. One wrong move and they'll scatter like rats. We get one shot at this.",
                    AnimationType = TextAnimationType.SlideUp
                }
            });

            return scene;
        }
    }
}