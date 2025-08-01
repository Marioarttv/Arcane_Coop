# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **real-time multiplayer cooperative escape room** built with **ASP.NET Core Blazor Server** and **SignalR**, themed around the Arcane universe. Players must work together as representatives from Zaun and Piltover to solve puzzles where each player sees different clues requiring communication and cooperation.

## 🚀 Current Status

**✅ COMPLETED FEATURES:**
- ✅ Real-time multiplayer architecture with SignalR
- ✅ Room creation and joining system
- ✅ Automatic city assignment (Zaun/Piltover)
- ✅ Player state management and persistence
- ✅ Cooperative puzzle engine with asymmetric information
- ✅ Real-time chat system
- ✅ Waiting room with shareable room codes
- ✅ Copy-to-clipboard functionality
- ✅ Beautiful Arcane-themed UI

**🔄 NEXT PRIORITIES:**
1. **More Cooperative Puzzles**: Design and implement additional puzzles with different mechanics
2. **Puzzle Categories**: Create puzzle types like Symbol Matching, Code Breaking, Pattern Recognition
3. **Progress System**: Visual progress tracking and completion celebrations
4. **Room Persistence**: Save/resume games functionality
5. **Audio Integration**: Sound effects and ambient music
6. **Mobile Optimization**: Touch-friendly UI improvements

## Development Commands

### Running the Application
```bash
dotnet run
```
Application runs on: **http://localhost:5055**

### Database Commands
```bash
dotnet ef migrations add [MigrationName]
dotnet ef database update
```

### Build Commands
```bash
dotnet build
dotnet clean
dotnet restore
```

## 🏗️ Architecture Overview

### Core Technologies
- **ASP.NET Core 9.0** with **Blazor Server**
- **SignalR** for real-time communication
- **Entity Framework Core** with **SQLite**
- **Interactive Server Components**

### Key Components

#### **1. Backend Services**
- **`GameRoomService`**: Manages room lifecycle, player joining/leaving
- **`PuzzleEngine`**: Handles cooperative puzzle logic and validation
- **`StateManager`**: Manages player-specific vs shared game state
- **`GameHub`**: SignalR hub for real-time communication

#### **2. Database Models**
- **`GameRoom`**: Room information and player list
- **`Player`**: Player data with city assignment and connection info
- **`GameState`**: JSON-serialized shared and player-specific state

#### **3. UI Components**
- **`LandingPage.razor`**: Entry point with Arcane theming
- **`RoomLobby.razor`**: Room creation interface
- **`GameRoom.razor`**: Main game interface with waiting room and puzzle display
- **`TestSignalR.razor`**: Development testing tool

### Project Structure
```
Arcane_Coop/
├── Components/
│   ├── Pages/
│   │   ├── LandingPage.razor (Entry point)
│   │   ├── RoomLobby.razor (Room creation)
│   │   ├── GameRoom.razor (Main game interface)
│   │   └── TestSignalR.razor (Development tool)
│   └── Layout/MainLayout.razor
├── Services/
│   ├── GameRoomService.cs (Room management)
│   ├── PuzzleEngine.cs (Puzzle logic)
│   └── StateManager.cs (State management)
├── Hubs/GameHub.cs (SignalR communication)
├── Models/GameRoom.cs (Data models)
├── Data/GameDbContext.cs (Database context)
└── wwwroot/ (Static assets)
```

## 🎮 Game Workflow Tutorial

### **Player Experience Flow**

#### **1. Room Creation**
1. Player 1 visits `/lobby`
2. Enters room name, their name, and selects city (Zaun/Piltover)
3. Clicks "Create Room" → **Automatically redirected to waiting room**
4. Sees room code (e.g., "A3B7F2") and copy button
5. Waiting screen shows: *"🕐 Waiting for your partner to join..."*

#### **2. Room Joining**
1. Player 2 visits `/lobby`
2. Enters the 6-digit room code and their name
3. Clicks "Join Room" → **Automatically assigned opposite city**
4. Both players now see each other in the game room

#### **3. Cooperative Gameplay**
1. **Available puzzles** appear when both players are connected
2. Either player clicks "Load Puzzle X" 
3. **Each player sees different clues** based on their city:
   - **Zaun player** sees underground/industrial clues
   - **Piltover player** sees upper city/technological clues
4. Players **communicate via chat** to share information
5. **Server validates combined answers** from both players
6. **Success triggers** puzzle completion and unlocks next challenges

### **Technical Workflow**

#### **SignalR Communication Pattern**
```csharp
// Room Creation
CreateRoom(roomName, playerName, city) 
  → GameRoomService.CreateRoomAsync()
  → JoinRoomAsync() with chosen city
  → SendAsync("JoinedRoom", room, puzzles)

// Room Joining  
JoinRoom(roomId, playerName)
  → Auto-detect opposite city from existing player
  → JoinRoomAsync() with assigned city
  → Notify all: SendAsync("PlayerJoined", player)

// Puzzle Flow
RequestPuzzle(roomId, puzzleId)
  → PuzzleEngine.GetPuzzleForPlayerAsync() 
  → Returns city-specific clues
  → SendAsync("PuzzleData", puzzleData)

SubmitAnswer(roomId, puzzleId, answer)
  → Store answer in StateManager
  → Check if both players submitted
  → Validate combined answer
  → SendAsync("AnswerResult" / "PuzzleCompleted")
```

## 🧩 Cooperative Puzzle System

### **Current Puzzle Examples**

#### **Puzzle 1: "The Bridge Connection"**
- **Zaun View**: "Ancient hextech conduits, symbols: ⚡🔧⚙️"
- **Piltover View**: "Golden bridge structure, symbols: 🔩⚡🛠️" 
- **Solution**: Both players must identify the common symbol (⚡) 
- **Answer Format**: "LIGHTNING|LIGHTNING"

#### **Puzzle 2: "Synchronized Crystals"**
- **Zaun View**: "Purple crystal with odd numbers: 3, 7, 9"
- **Piltover View**: "Blue crystal with even numbers: 2, 4, 8"
- **Solution**: Continue each sequence (11 for odd, 16 for even)
- **Answer Format**: "11|16"

### **Puzzle Creation Pattern**
```csharp
new CooperativePuzzle
{
    Id = X,
    Title = "Puzzle Name",
    RequiresCooperation = true,
    ZaunDescription = "What Zaun player sees...",
    PiltoverDescription = "What Piltover player sees...", 
    ZaunClues = ["Clue 1", "Clue 2"],
    PiltoverClues = ["Different clue 1", "Different clue 2"],
    SharedClues = ["Information both players see"],
    AcceptedAnswers = ["ZAUN_ANSWER|PILTOVER_ANSWER"]
}
```

## 🎯 Next Development Tasks

### **1. Immediate Priorities**
- **More Puzzle Types**: Create 5-10 additional cooperative puzzles
- **Puzzle Categories**: Symbol matching, math sequences, word puzzles, image analysis
- **Visual Enhancements**: Puzzle-specific images and animations
- **Progress Tracking**: Completion percentage and achievements

### **2. Enhanced Features**  
- **Hint System**: Players can request hints after failed attempts
- **Time Challenges**: Optional timed puzzle modes
- **Leaderboards**: Track completion times and scores
- **Room History**: Save and replay completed games

### **3. Technical Improvements**
- **Connection Recovery**: Handle network disconnections gracefully
- **Performance**: Optimize for larger player counts
- **Security**: Input validation and anti-cheat measures
- **Monitoring**: Add logging and analytics

## 🛠️ Development Guidelines

### **Adding New Puzzles**
1. Add puzzle definition to `PuzzleEngine.InitializePuzzles()`
2. Test with different city perspectives
3. Ensure answers require cooperation
4. Add appropriate validation logic

### **Database Changes**
1. Create migration: `dotnet ef migrations add [Name]`
2. Update database: `dotnet ef database update`
3. Test with existing data

### **SignalR Methods**
1. Add method to `GameHub.cs`
2. Add client handler in relevant Razor component
3. Test real-time communication
4. Add error handling

### **UI Updates**
1. Follow existing Arcane visual theme
2. Use component-scoped CSS
3. Ensure mobile responsiveness
4. Test with multiple screen sizes

## 🚨 Important Notes

- **No character selection**: Removed for simplified workflow
- **Automatic city assignment**: Second player gets opposite city
- **Real-time sync**: All game state changes broadcast immediately  
- **Cooperative focus**: Puzzles require communication between players
- **Room codes**: 6-character alphanumeric codes for easy sharing
- **Database**: SQLite for development, easily changeable for production

## 🔧 Troubleshooting

### **Common Issues**
- **Build errors**: Check for missing `using` statements in `_Imports.razor`
- **SignalR connection**: Verify hub registration in `Program.cs`
- **Database issues**: Run `dotnet ef database update`
- **JSON serialization**: Check for circular references in models

### **Testing Workflow**
1. Run `dotnet run`
2. Open two browser windows/tabs
3. Create room in first tab
4. Join with room code in second tab
5. Test puzzle solving with both perspectives