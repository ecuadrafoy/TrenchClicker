# Trench Clicker

A World War I-themed clicker/autobattler game where players send soldiers across no man's land to capture enemy trenches. Every click generates soldiers, and progress is measured in inches of ground gained.

## About

Click to generate soldiers who automatically assault enemy trenches. Upgrade your forces to deal more damage, generate soldiers faster, and improve equipment. Break through defensive lines, capture territory, and progress through increasingly challenging trench positions.

## Getting Started

### Requirements

- Unity 6
- C# .NET

### Setup

1. Clone the repository
2. Open the project folder in Unity
3. Open a scene from `Assets/Scenes/` and press Play

## How to Play

- **Click** to generate soldiers
- Soldiers automatically **attack** the enemy trench
- **Upgrade** your forces between assaults
- **Capture** trenches to gain ground

## Tech Stack

- **Engine:** Unity 6
- **Language:** C#
- **Input System:** Unity Input System
- **UI:** TextMesh Pro

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # Game managers, core game loop
│   ├── Combat/         # Soldier behavior, combat mechanics
│   ├── UI/             # Click handlers, HUD
│   ├── Progression/    # Upgrades, progression
│   └── Utils/          # Helper functions
├── Prefabs/
├── Scenes/
└── Settings/
```

## What's Been Built

- **Game Manager** - Core singleton managing game state, assault timing, and combat
- **Assault System** - 90-second timed assaults with reinforcement mechanics
- **Combat System** - Soldier damage calculation and trench health management
- **Ground Progression** - Tracks ground gained in inches per engagement
- **Difficulty Scaling** - Enemy trenches increase in difficulty with each capture
- **UI System** - Click handlers and HUD updates for real-time feedback
- **Reinforcement Mechanics** - Enemy trenches receive HP reinforcements after assault time expires


