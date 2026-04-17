<div align="center">

  <img src="assets/logo.png" alt="logo" width="200" height="auto" />
  <h1>🐱 Nine Lives: Echo Alley</h1>
  
  <p>
    A puzzle-stealth game where each life becomes a ghost that helps solve the level.
  </p>

<p>
  <a href="https://github.com/nancicardenas/FinalGroupProject/graphs/contributors">
    <img src="https://img.shields.io/github/contributors/nancicardenas/FinalGroupProject" alt="contributors" />
  </a>
  <a href="https://github.com/nancicardenas/FinalGroupProject">
    <img src="https://img.shields.io/github/last-commit/nancicardenas/FinalGroupProject" alt="last update" />
  </a>
</p>

</div>

---

# Table of Contents

- [Repository](#-repository)
- [Game Name](#-game-name)
- [Game Description](#-game-description)
- [X-Factors](#-x-factors-core-innovations)
- [Screenshots](#-screenshots)
- [Group Organization & Responsibilities](#-group-organization--responsibilities)
- [Project Status](#-project-status-midway-review)
- [Anticipated Challenges](#-anticipated-challenges--assistance-needed)
- [Project Structure](#-project-structure)
- [Contact](#-contact)

---

# Repository
https://github.com/nancicardenas/FinalGroupProject

---

# Game Name
**Nine Lives: Echo Alley**

---

# Game Description

**Nine Lives: Echo Alley** is a puzzle-stealth game where the player controls a cat navigating a city environment using all nine of its lives as a core gameplay mechanic. Each time the player dies or manually resets, that attempt is recorded and replayed as a ghost in the next run. These ghost cats repeat previous actions exactly and can be used to distract enemies, activate switches, and solve environmental puzzles.

The gameplay emphasizes planning and coordination across multiple lives rather than traditional combat or simple avoidance. Players build a solution over multiple attempts, turning failure into a strategic advantage. This creates emergent gameplay situations where timing, positioning, and interaction between ghosts, enemies, and the environment are key to progression.

This project incorporates course concepts including **state machines, AI behavior, interaction systems, animation control, UI design, and structured code organization**. The ghost replay mechanic and NPC systems combine to create dynamic, non-linear gameplay.

---

# X-Factors (Core Innovations)

- **Ghost Replay System**
  - Each life becomes a replayable “ghost”  
  - Enables multi-agent puzzle solving  

- **Emergent Gameplay**
  - Player + ghosts + enemies interact dynamically  
  - No fixed solutions  

- **NPC State Machine AI**
  - Enemies operate using state machines (patrol, chase, alert)  
  - React to player, environment, and ghost echoes  

- **Multi-Life Strategy System**
  - Failure becomes part of the solution  
  - Encourages planning across multiple runs  

---

# Screenshots

<div align="center"> 
  <img src="https://placehold.co/600x400?text=Title+Screen" alt="screenshot" />
  <img src="https://placehold.co/600x400?text=Tutorial+Level" alt="screenshot" />
</div>

---

# Group Organization & Responsibilities

**Group Name:** [Insert Group Name]

Each member is responsible for their system and must understand how it integrates into the full game.

---

## Jordan Spencer – Ghost Mechanics & Lives

- [ ] Life system (9 lives)
- [ ] Death/reset system
- [ ] Record player actions
- [ ] Ghost replay system
- [ ] Multi-ghost support (2–5, up to 8 max)
- [ ] Interaction replay
- [ ] Reset clearing logic

---

## Noah – Gameplay & Interaction

- [ ] Movement (WASD)
- [ ] Run (Shift)
- [ ] Jump (Space)
- [ ] Idle animations (2 states)
- [ ] Interaction (Left Click)
- [ ] Manual reset (Right Click)
- [ ] Key system
- [ ] Gate system
- [ ] Win/lose conditions

---

## Dylan Rambo – Enemies & AI

- [ ] NPC state machine system
- [ ] Dog AI (patrol, chase)
- [ ] Human AI (alert, detection)
- [ ] Detection systems
- [ ] Interaction with ghosts
- [ ] Balance enemy behavior

---

## Nanci Cardenas – Level Building & UI

- [ ] Title screen (Play, Options, Exit)
- [ ] Options menu (sound slider)
- [ ] Cat selection
- [ ] Tutorial level
- [ ] Key, gate, trap placement
- [ ] UI prompts
- [ ] Scene transitions
- [ ] Sound effects (no music)

---

# Project Status (Midway Review)

**Current Phase:** Prototype

### Completed
- [x] Repository created  
- [x] Roles assigned  
- [x] Game concept finalized  

### In Progress
- [ ] Unity project skeleton  
- [ ] UI flow  
- [ ] Player controller  
- [ ] Interaction system  
- [ ] Ghost system
- [ ] Key, gate, trap placement for level 1
- [ ] Sound effects

---

# Anticipated Challenges / Assistance Needed

- Ghost replay synchronization  
- Interaction consistency across runs  
- Reset reliability  
- NPC state machine implementation  
- Animation integration  

---

# Project Structure

```txt
Assets/
├── Scenes/
├── Scripts/
├── Prefabs/
├── Audio/
└── Art/
```

---

# Contact

- Jordan Spencer: https://github.com/[your-username]

- Dylan Rambo: https://github.com/[your-username]

- Nanci Cardenas: https://github.com/[your-username]

- Noah: https://github.com/[your-username]
