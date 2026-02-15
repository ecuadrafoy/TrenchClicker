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

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # GameManager (game loop, state, combat, weather)
│   ├── Combat/         # Combat-related scripts (future)
│   ├── UI/             # UIManager, ClickButton, UIShop, UISpecialShop, DebugOverlay
│   ├── Progression/    # UpgradeManager, WeatherStationManager
│   └── Data/           # ScriptableObjects (UpgradeData), WeatherData
├── Prefabs/
│   └── UI/             # UpgradeItemPrefab
├── Scenes/
└── Settings/
```

## What's Been Built

- **Game Manager** - Core singleton managing game state, assault timing, combat, and weather
- **Assault System** - 90-second timed assaults with reinforcement mechanics
- **Combat System** - Randomized soldier damage (min-max range), trench HP management
- **Ground Progression** - Tracks ground gained in inches/feet per engagement
- **Difficulty Scaling** - Enemy trenches increase HP by 20% with each capture, reinforcement rate scales proportionally
- **Upgrade System** - 4 upgrades purchasable with ground gained (soldiers per click x2, damage min, damage max), dynamic shop UI
- **Weather System** - Markov chain-based weather that changes during assaults, affecting soldier damage and enemy reinforcement rates (Clear/Partly Cloudy/Overcast/Light Rain/Heavy Rain)
- **Weather Station Upgrade** - 3-level purchasable upgrade that progressively reveals weather forecasts (vague hints → risk categories → exact percentages), housed in a separate Special Shop
- **Special Shop** - Separate shop panel for unique upgrades (Weather Station), with mutual exclusivity against the regular shop
- **UI System** - Click handlers, HUD, weather display with toast notifications and forecast text, assault timer with color warnings
- **Reinforcement Mechanics** - Enemy trenches receive HP reinforcements after assault timer expires (weather-modified)
- **Debug Overlay** - F1 toggle, runtime game state inspection, sliders/buttons for testing, weather controls
