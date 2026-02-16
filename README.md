# Trench Clicker

A World War I-themed clicker/autobattler game where players send soldiers across no man's land to capture enemy trenches. Every click generates soldiers, progress is measured in inches of ground gained, and upgrades are purchased with Requisition Points earned on trench capture.

## About

Click to generate soldiers who automatically assault enemy trenches. Capture trenches to earn Requisition Points, then spend them on upgrades to deal more damage, generate soldiers faster, and improve equipment. Break through defensive lines, capture territory, and progress through increasingly challenging trench positions.

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
- **Capture** trenches to earn **Requisition Points** (bonus RP for fast captures)
- **Upgrade** your forces between assaults using RP
- Track your permanent **ground gained** as a progression stat

## Tech Stack

- **Engine:** Unity 6
- **Language:** C#

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # GameManager, WeatherManager, EliteTroopManager
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

- **Game Manager** - Core singleton managing game state, assault timing, combat, and difficulty scaling
- **Weather Manager** - Singleton managing weather state, Markov chain table generation, and weather updates during assaults
- **Elite Troop Manager** - Singleton managing elite troop reserves, deployment, survival calculations, and trench capture rewards
- **Assault System** - 90-second timed assaults with reinforcement mechanics
- **Combat System** - Randomized soldier damage (min-max range), trench HP management
- **Dual-Stat Economy** - Ground gained (permanent progression in inches/feet) + Requisition Points (spendable currency earned on trench capture with speed and efficiency bonuses)
- **Difficulty Scaling** - Enemy trenches increase HP by 20% with each capture, reinforcement rate scales proportionally
- **Upgrade System** - 4 upgrades purchasable with Requisition Points (soldiers per click x2, damage min, damage max), dynamic shop UI
- **Weather System** - Markov chain-based weather that changes during assaults, affecting soldier damage and enemy reinforcement rates (Clear/Partly Cloudy/Overcast/Light Rain/Heavy Rain)
- **Elite Troops (Storm Troopers)** - Deployable elite soldiers dealing automatic per-frame damage during assaults, with survival mechanics and weather resistance
- **Weather Station Upgrade** - 3-level purchasable upgrade that progressively reveals weather forecasts (vague hints → risk categories → exact percentages), housed in a separate Special Shop
- **Special Shop** - Separate shop panel for unique upgrades (Weather Station), with mutual exclusivity against the regular shop
- **UI System** - Click handlers, HUD, weather display with toast notifications and forecast text, assault timer with color warnings, elite troop deploy button with countdown
- **Reinforcement Mechanics** - Enemy trenches receive HP reinforcements after assault timer expires (weather-modified)
- **Debug Overlay** - F1 toggle, runtime game state inspection, sliders/buttons for testing, weather and elite troop controls
