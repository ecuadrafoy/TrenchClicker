# WWI Clicker/Autobattler Game

## Project Overview
A World War I-themed clicker/autobattler game where players send soldiers across no man's land to capture enemy trenches. Progress is measured in inches of ground gained, with upgrades improving troop efficiency and equipment quality.

## Tech Stack
- **Engine**: Unity (version: [specify your version])
- **Language**: C#
- **Target Platform**: [PC/WebGL/Mobile - specify]

## Project Structure
```
Assets/
├── Scripts/
│   ├── Core/           # Game loop, managers
│   ├── Combat/         # Soldier behavior, combat resolution
│   ├── UI/             # Click handlers, HUD, menus
│   ├── Progression/    # Upgrades, unlocks, stats
│   └── Utils/          # Helper functions, constants
├── Prefabs/
│   ├── Soldiers/
│   └── Environment/
├── Scenes/
└── Resources/
```

## Core Systems

### 1. Clicking System
- Tracks clicks and generates soldiers
- Manages click-to-soldier conversion rate
- Handles click upgrades

### 2. Combat System
- Soldier movement across no man's land
- Health/damage calculations
- Win/loss conditions based on soldier count vs trench defense

### 3. Progression System
- Ground gained measurement (in inches/feet)
- Unlock tiers for equipment upgrades
- Promotion system based on performance

### 4. Upgrade System
Current planned upgrades:
- Soldiers per click
- Soldier equipment (rifles, helmets, etc.)
- Elite troop types
- Combat efficiency modifiers

## Game Design Values
- **Historical Tone**: Somber, reflects WWI's brutal reality
- **Progression Feel**: Early game is grindy (reflects trench warfare), becomes more efficient with upgrades
- **Visual Feedback**: Clear indication of ground gained, casualties, progress

## Coding Conventions
- Use C# naming conventions (PascalCase for public, camelCase for private)
- Comment complex game logic clearly
- Separate game logic from Unity-specific code where possible
- Use ScriptableObjects for upgrade data and soldier stats

## Current Development Phase
**Prototype Stage**: Building first playable version with core click → soldier → combat → result loop

### Immediate Goals
1. Basic click input system
2. Soldier spawning and movement
3. Simple combat resolution (HP-based)
4. Ground gained calculation
5. Basic UI showing progress

## Known Issues / TODOs
- [ ] Decide final mechanics: pure clicker vs autobattler vs hybrid
- [ ] Determine combat resolution method (real-time vs calculated)
- [ ] Design upgrade tree structure
- [ ] Balance: soldiers-per-inch ratio

## Testing Notes
- Test clicking feels responsive and satisfying
- Ensure progression curve isn't too slow early game
- Verify visual clarity of ground gained

## Future Features (Post-Prototype)
- Interactive elements: mines, chemical attack response, aerial bombardment
- Multiple trench lines/levels
- Historical accuracy considerations
- Sound design and period-appropriate audio

## AI Assistant Guidelines
When helping with this project:
- Prioritize clean, maintainable code over clever solutions
- Suggest Unity best practices (object pooling for soldiers, etc.)
- Keep historical WWI context in mind for design decisions
- Focus on prototype functionality first, polish later
- Ask for clarification on game feel vs technical implementation