# PictureExplanation Role Swap Documentation

This document details the changes made to swap the roles in the PictureExplanation game, where **Zaun players** now describe images and **Piltover players** choose from options (previously it was the opposite).

## Overview

**Before the swap:**
- Piltover players: Described images (describer role)
- Zaun players: Chose from 4 options (guesser role)

**After the swap:**
- Zaun players: Describe images (describer role) 
- Piltover players: Choose from 4 options (guesser role)

## Files Modified

### 1. Components/Pages/PictureExplanation.razor

#### UI Interface Role Conditions
**Lines 196-197:** Changed describer interface from Piltover to Zaun
```diff
- <!-- Piltover (Describer) Interface -->
- @if (playerView.Role == "Piltover")
+ <!-- Zaun (Describer) Interface -->
+ @if (playerView.Role == "Zaunite")
```

**Line 199:** Updated CSS class for describer interface
```diff
- <div class="piltover-interface">
+ <div class="zaunite-interface describer-interface">
```

**Lines 242-243:** Changed guesser interface from Zaun to Piltover
```diff
- <!-- Zaunite (Guesser) Interface -->
- @if (playerView.Role == "Zaunite")
+ <!-- Piltover (Guesser) Interface -->
+ @if (playerView.Role == "Piltover")
```

**Line 245:** Updated CSS class for guesser interface
```diff
- <div class="zaunite-interface">
+ <div class="piltover-interface guesser-interface">
```

#### Instruction Text Updates
**Line 58:** Updated role responsibilities
```diff
- <p><strong>Piltover Agent</strong> accesses the visual records. <strong>Zaun Operative</strong> handles the identification terminal.</p>
+ <p><strong>Zaun Operative</strong> accesses the visual records. <strong>Piltover Agent</strong> handles the identification terminal.</p>
```

**Lines 65:** Updated role descriptions
```diff
- <p><strong>Piltover</strong> describes the person's features from the file photo. <strong>Zaun</strong> identifies them from the database options. Find all 4 marked scientists!</p>
+ <p><strong>Zaun</strong> describes the person's features from the file photo. <strong>Piltover</strong> identifies them from the database options. Find all 4 marked scientists!</p>
```

#### Role Preview Cards Swap
**Lines 72-88:** Completely swapped the role preview cards
```diff
- <div class="role-card piltover-preview">
-     <h4>📋 Caitlyn (Database Access)</h4>
-     <p>You have visual access to the corrupted files:</p>
-     <!-- Piltover preview content -->
- </div>
- <div class="role-card zaunite-preview">
-     <h4>🕵️ Vi (Street Knowledge)</h4>
-     <p>You know these faces from the underground:</p>
-     <!-- Zaun preview content -->
- </div>
+ <div class="role-card zaunite-preview">
+     <h4>🕵️ Vi (Database Access)</h4>
+     <p>You have visual access to the corrupted files:</p>
+     <!-- Zaun preview content -->
+ </div>
+ <div class="role-card piltover-preview">
+     <h4>📋 Caitlyn (Street Knowledge)</h4>
+     <p>You know these faces from the underground:</p>
+     <!-- Piltover preview content -->
+ </div>
```

#### In-Game Text Updates
**Line 222:** Updated partner reference in description prompt
```diff
- <p>This file is corrupting - describe their face, hair, clothing, and any distinguishing features. Vi might recognize them from the underground. Press below when done.</p>
+ <p>This file is corrupting - describe their face, hair, clothing, and any distinguishing features. Caitlyn might recognize them from the underground. Press below when done.</p>
```

**Line 225:** Updated button text
```diff
- 📁 Lock File & Send to Vi
+ 📁 Lock File & Send to Caitlyn
```

**Line 248:** Updated panel header
```diff
- <h4>🕵️ Zaun Database Terminal - Facial Recognition</h4>
+ <h4>📋 Piltover Database Terminal - Facial Recognition</h4>
```

**Line 255:** Updated waiting message
```diff
- <p>Caitlyn is accessing the corrupted file...</p>
+ <p>Vi is accessing the corrupted file...</p>
```

#### CSS Styling Updates
**Lines 1542-1584:** Added new CSS rules for role-specific styling

**Zaun Describer Interface Styles:**
```css
.zaunite-interface.describer-interface .archive-panel {
    background: rgba(0, 200, 200, 0.05);
}

.zaunite-interface.describer-interface .main-image {
    border: 3px solid #00c8c8;
    box-shadow: 0 0 20px rgba(0, 200, 200, 0.3);
}

.zaunite-interface.describer-interface .voice-instructions h5 {
    color: #00c8c8;
}
```

**Piltover Guesser Interface Styles:**
```css
.piltover-interface.guesser-interface .choices-header {
    color: #c89b3c;
}

.piltover-interface.guesser-interface .choice-item {
    border: 2px solid rgba(200, 155, 60, 0.3);
}

.piltover-interface.guesser-interface .choice-label {
    color: #c89b3c;
}
```

### 2. Hubs/GameHub.cs

#### GetPlayerView Method (Lines 5410-5413)
Swapped the role assignments for UI properties:

```diff
- CurrentImageUrl = role == PlayerRole.Piltover && ImageVisible ? (CurrentPicture?.ImageUrl ?? "") : "",
- ChoiceImages = role == PlayerRole.Zaunite && DescriptionFinished ? CurrentChoices : new(),
- CanFinishDescribing = role == PlayerRole.Piltover && ImageVisible && !DescriptionFinished && !RoundComplete,
- CanChoose = role == PlayerRole.Zaunite && DescriptionFinished && !SubmittedChoice.HasValue && !RoundComplete,
+ CurrentImageUrl = role == PlayerRole.Zaunite && ImageVisible ? (CurrentPicture?.ImageUrl ?? "") : "",
+ ChoiceImages = role == PlayerRole.Piltover && DescriptionFinished ? CurrentChoices : new(),
+ CanFinishDescribing = role == PlayerRole.Zaunite && ImageVisible && !DescriptionFinished && !RoundComplete,
+ CanChoose = role == PlayerRole.Piltover && DescriptionFinished && !SubmittedChoice.HasValue && !RoundComplete,
```

#### FinishDescribing Method (Line 5322)
Changed role authorization:

```diff
- if (Players[connectionId] != PlayerRole.Piltover)
-     return new GameActionResult { Success = false, Message = "Only Piltover player can finish describing" };
+ if (Players[connectionId] != PlayerRole.Zaunite)
+     return new GameActionResult { Success = false, Message = "Only Zaunite player can finish describing" };
```

#### SubmitChoice Method (Line 5339)
Changed role authorization:

```diff
- if (Players[connectionId] != PlayerRole.Zaunite)
-     return new GameActionResult { Success = false, Message = "Only Zaunite player can choose" };
+ if (Players[connectionId] != PlayerRole.Piltover)
+     return new GameActionResult { Success = false, Message = "Only Piltover player can choose" };
```

## Key Technical Points

### 1. Three-Layer Architecture
The role swap required changes at three different layers:
- **Frontend UI** (PictureExplanation.razor): Interface display and styling
- **Server Logic** (GameHub.cs): Game state management and permissions
- **Method Authorization** (GameHub.cs): Action validation

### 2. Critical Dependencies
- **UI Display** depends on `CurrentImageUrl`, `CanFinishDescribing`, `ChoiceImages`, and `CanChoose` properties
- **Button Functionality** depends on method-level role checks in `FinishDescribing()` and `SubmitChoice()`
- **CSS Styling** required role-specific classes to maintain visual theming

### 3. Common Pitfall
Initially, only the frontend role conditions were swapped, but the backend still assigned describer properties to Piltover players. This resulted in:
- Correct UI layout but no images/buttons visible
- "Only Piltover player can finish describing" errors when Zaun players pressed buttons

The fix required updating **both** the property assignments in `GetPlayerView()` **and** the method authorization checks.

## Testing Checklist

When implementing similar role swaps:

- [ ] Frontend role conditions (`@if` statements)
- [ ] CSS class assignments for role-specific styling  
- [ ] Instruction text and UI labels
- [ ] Server-side property assignments (`GetPlayerView()`)
- [ ] Method-level authorization checks
- [ ] Error messages reflect new role assignments
- [ ] Visual theming matches new role assignments

## Result

The role swap is now complete and functional:
- **Zaun players** see images, can describe them, and press the "Lock File & Send to Caitlyn" button
- **Piltover players** wait for descriptions, then choose from 4 image options
- All server-side validation and UI styling work correctly with the new role assignments