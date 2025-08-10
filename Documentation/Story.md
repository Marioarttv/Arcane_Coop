# Arcane Coop - Story Structure

## Setting & Timeline
**When:** ~6 months after Vi's release from Stillwater Prison, before Silco's full consolidation of power in Zaun
**Where:** The border districts between Piltover and Zaun - the contested zones where both cities' influence overlaps
**Context:** Tensions are rising. Vander's death left a power vacuum. Silco is quietly building his shimmer empire but hasn't yet revealed himself publicly. Piltover is conducting more aggressive "peacekeeping" operations in response to increased crime.

## Core Premise
A series of mysterious technological thefts have occurred in both cities - someone is stealing hextech components from Piltover AND chemtech equipment from Zaun. Both sides blame each other, threatening to shatter the fragile peace. Vi and Caitlyn must work together to uncover the truth before war erupts.

## The Mystery
**The True Villain:** A rogue inventor named **Maris** - a former Piltover engineer who was exiled after their experiments with combining hextech and chemtech were deemed too dangerous. They believe the only way to prevent war is to create a "deterrent" weapon so powerful that neither side would dare attack the other. They're wrong - their device would actually poison both cities' air supplies.

---

## Act 1: The Investigation Begins
*Location: Caitlyn's Office in Piltover / Vi's contacts in Zaun*

### Scene 1: Emergency Briefing
- **Setup:** Players are briefed on the thefts. Caitlyn represents Piltover's interests, Vi represents Zaun's perspective
- **Choice Point:** Players choose their initial investigation approach:
  - A) Start with the Piltover hextech theft sites (leads to CodeCracker puzzle)
  - B) Start with Zaun's chemtech black markets (leads to AlchemyLab puzzle)
- **Narrative Impact:** Choice determines which faction trusts you more initially

### Scene 2A: Hextech Investigation Path
- **Puzzle Integration:** **CodeCracker** - Decrypt intercepted messages between the thief and their suppliers
- **Story Beat:** Messages reveal the thief needs both hextech crystals AND Zaun chemicals
- **Discovery:** Find coordinates to a neutral meeting point

### Scene 2B: Chemtech Investigation Path  
- **Puzzle Integration:** **AlchemyLab** - Analyze residue samples to identify what was stolen
- **Story Beat:** The stolen chemicals can only be useful when combined with hextech energy
- **Discovery:** Underground supplier mentions strange orders going to the border zones

---

## Act 2: Following the Trail
*Location: Border Districts - The Gray Zone*

### Scene 3: Emergency Transmission
- **Puzzle Integration:** **SignalDecoder** - Intercept and decode emergency transmissions
- **Story Beat:** Distress signals reveal accidents at theft sites - the thief is getting desperate
- **Choice Point:** 
  - A) Rush to help the injured (gain moral standing, lose time)
  - B) Use the chaos to investigate (gain evidence, lose reputation)
- **Consequence:** This choice affects which NPCs help you later

### Scene 4: The Warehouse District
- **Puzzle Integration:** **NavigationMaze** - Navigate through dangerous warehouse district
- **Story Beat:** Vi guides Caitlyn (or vice versa based on player roles) through gang territories
- **Discovery:** Find the thief's abandoned workshop with partial blueprints
- **Revelation:** The device combines both technologies in a way that could be catastrophic

---

## Act 3: Race Against Time
*Location: The Depths - Abandoned Mining Facility*

### Scene 5: The Hidden Laboratory
- **Puzzle Integration:** **PictureExplanation** - Describe complex blueprint pieces to partner
- **Story Beat:** Players must reconstruct the full device schematic from scattered pieces
- **Choice Point:**
  - A) Alert both Piltover and Zaun authorities (diplomatic but slow)
  - B) Go in alone to stop Maris (dangerous but fast)
- **Impact:** Determines available resources for final confrontation

### Scene 6: The Logic Lock
- **Puzzle Integration:** **RuneProtocol** - Disable Maris's security system
- **Story Beat:** The security system reflects Maris's paranoid mindset - complex conditional logic
- **Discovery:** Find evidence of Maris's tragic backstory - lost family in previous Piltover-Zaun conflict
- **Moral Complexity:** Maris genuinely believes they're preventing war

---

## Act 4: The Confrontation
*Location: The Convergence Point - Where Piltover meets Zaun*

### Scene 7: The Final Device
- **Dynamic Puzzle Selection:** Based on previous choices, face 2-3 puzzles in sequence:
  - If diplomatic path: PictureExplanation + CodeCracker (coordinate with authorities)
  - If solo path: AlchemyLab + RuneProtocol (disable device directly)
  - If balanced: NavigationMaze + SignalDecoder (evacuate civilians while disabling)

### Scene 8: Resolution Branches

#### Ending A: Unity Through Crisis
*Requirements: Made mostly cooperative choices, saved civilians*
- Maris is captured alive and their research is split between cities for peaceful purposes
- Vi and Caitlyn are recognized as cross-city peacekeepers
- Sets up potential for future cooperation

#### Ending B: Uneasy Truce
*Requirements: Mixed choices, some casualties*
- Device is destroyed but Maris escapes
- Both cities agree to joint patrols in border zones
- Tensions remain but war is averted

#### Ending C: Pyrrhic Victory
*Requirements: Made mostly aggressive choices*
- Maris dies trying to activate the device
- The threat is ended but both cities blame each other for the crisis
- Vi and Caitlyn's partnership is strained but intact

---

## Puzzle Integration Strategy

### Mandatory Puzzles (Story Critical)
1. **CodeCracker** OR **AlchemyLab** (Act 1 - Investigation)
2. **SignalDecoder** (Act 2 - Emergency)
3. **NavigationMaze** (Act 2 - Pursuit)
4. **RuneProtocol** (Act 3 - Security)

### Choice-Dependent Puzzles
- **PictureExplanation** - Available if players choose cooperative/communication paths
- Additional **CodeCracker** - If players need to decrypt more messages
- Additional **AlchemyLab** - If players choose to analyze the device

### Adaptive Difficulty
- Player performance in early puzzles affects later puzzle complexity
- Failed puzzles don't end the game but affect story outcomes (more casualties, less trust, harder final confrontation)

---

## Character Development Arcs

### Vi's Arc
- Starts distrustful of Piltover authority (except Caitlyn)
- Must choose between Zaun loyalty and greater good
- Learns to work within the system while maintaining her identity

### Caitlyn's Arc  
- Starts naive about Zaun's legitimate grievances
- Confronts Piltover's role in creating the crisis
- Develops understanding of systemic inequality

### Maris's Arc (Antagonist)
- Brilliant but traumatized by past violence
- Genuinely believes in their solution
- Can potentially be redeemed based on player choices

---

## Thematic Elements

### Core Themes
1. **Cooperation vs Competition** - Mirrored in both story and gameplay
2. **Perspective and Truth** - Each side has valid points
3. **Cycles of Violence** - How fear perpetuates conflict
4. **Technology's Double Edge** - Progress can heal or harm

### Visual Novel Integration
- Use VisualNovel system for major story beats
- Character expressions change based on trust levels
- Environmental descriptions reflect rising tensions

---

## Technical Implementation Notes

### Story State Tracking
```csharp
public class StoryState
{
    public int ActNumber { get; set; }
    public int SceneNumber { get; set; }
    public Dictionary<string, bool> PlayerChoices { get; set; }
    public int PiltoverTrust { get; set; }  // -100 to 100
    public int ZaunTrust { get; set; }      // -100 to 100
    public List<string> CompletedPuzzles { get; set; }
    public EndingType CurrentTrajectory { get; set; }
}
```

### Choice Impact System
- Each choice modifies trust scores with both factions
- Puzzle performance affects story pacing (success = more time for later events)
- Failed puzzles trigger alternate story branches, not game over

### Dynamic Puzzle Selection
- Story engine checks player history before presenting puzzles
- Some puzzles can appear multiple times with different contexts
- Final act adapts based on accumulated choices

---

## Implementation Priority

### Phase 1: Core Story Path
1. Implement Act 1 with both investigation branches
2. Create linear Act 2 with SignalDecoder and NavigationMaze
3. Basic Act 3 with RuneProtocol
4. Single ending for testing

### Phase 2: Branching Narrative
1. Add choice consequences
2. Implement trust system
3. Create alternate puzzle paths
4. Add multiple endings

### Phase 3: Polish
1. Full VisualNovel integration
2. Character expression system
3. Environmental storytelling
4. Achievement system for different paths

---

## Notes for Future Development

### Potential DLC/Expansion Ideas
- Prequel showing Maris's backstory
- Epilogue showing consequences of player choices
- Side stories focusing on secondary characters
- Additional puzzle types for alternate paths

### Multiplayer Considerations
- Players can disagree on choices - implement voting or alternating decision system
- Some puzzles could have asymmetric story information
- Trust scores could be individual, creating inter-player tension

### Educational Goals Alignment
- Vocabulary building through investigation documents
- Critical thinking through moral choices
- Teamwork emphasized through puzzle cooperation
- Cultural understanding through Piltover/Zaun perspectives