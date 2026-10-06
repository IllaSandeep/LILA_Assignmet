<img width="1166" height="718" alt="image" src="https://github.com/user-attachments/assets/a3957bb3-a408-48fb-b968-f5abcb41fba3" /># NEMESIS

### The game learns how you fight.

NEMESIS is a survivor-style action prototype where the enemy adapts to the way you play.

You control a spacecraft, fight increasingly difficult enemies, collect XP, choose temporary upgrades and try to survive. The main twist is the **Nemesis system**: the game observes your playstyle and creates an enemy designed to counter it.

---

## 🎮 Play the Game

**Playable Web Build:**  
https://illasandeep.itch.io/nemesis-prototype

The game runs directly in the browser.

---

## 🕹️ Controls

- **W** → Move forward
- **S** → Move backward
- **A / D** → Rotate
- **Auto-fire** → Your weapon fires continuously in the direction you are facing

There is no manual aiming or target lock. Movement and positioning are the main parts of combat.

---

## 🔥 Core Gameplay

The basic loop is:

**Fight → Collect XP → Upgrade → Build your playstyle → Nemesis appears → Adapt**

Enemies become more challenging as the run progresses. Leveling up gives you temporary abilities that can change how you approach the current run.

The goal is not only to survive, but to experiment with different playstyles and see how the game reacts.

---

## 👁️ The Nemesis System

The main feature of NEMESIS is that the game reacts to the way you play.

During a run, the game tracks player behaviour such as:

- Combat distance
- Movement behaviour
- Ability usage
- General combat style

The game then identifies the player's dominant behaviour and selects a Nemesis designed to counter it.

### Examples

| Player Style | Nemesis |
|---|---|
| Close-range / aggressive | Trapper |
| Ranged / keeps distance | Hunter |
| Mixed playstyle | Balanced |

The idea is simple:

> **You fight the game. Then the game starts fighting the way you play.**

The system is deterministic rather than using a generative AI model in real time. This makes the behaviour easier to test, balance and control.

---

## ⚔️ Enemies

The prototype includes different enemy behaviours to create increasing combat pressure:

- **Chaser** - moves directly towards the player
- **Shooter** - attacks from a distance
- **Flanker** - tries to approach from different angles
- **Nemesis** - adapts to the player's behaviour

Enemy density and variety increase as the player's level increases.

---

## 📈 Progression

### During a Run

- Collect XP
- Level up
- Choose temporary abilities
- Build your current playstyle
- Survive increasing enemy pressure
- Encounter your Nemesis

### Between Runs

Players earn **Scrap**, which can be used for permanent upgrades such as:

- Damage
- Fire Rate
- Movement Speed
- Max Health

This gives players a reason to keep improving while the Nemesis system gives them a reason to experiment with different playstyles.

---

## 💥 Temporary Abilities

Level-ups offer temporary abilities that affect the current run.

Examples include:

- Power
- Rapid Fire
- Speed
- Heavy Shot
- Piercing
- Shield

The combination of abilities changes the way each run can play out.

---

## 🎯 What I Wanted to Explore

Most survivor-like games increase difficulty mainly through more enemies, stronger enemies or faster pacing.

With NEMESIS, I wanted to explore a different type of pressure:

**What if the game could react to the player's behaviour instead of only increasing difficulty?**

The goal was to make players think about changing their strategy between runs.

---

## 🛠️ Built With

- **Unity 6.6**
- **C#**
- **Unity 2D**
- **WebGL / Web Build**
- Unity UI
- Custom gameplay and behaviour systems

---

## 🤖 AI

AI tools were used during development for parts of Unity code generation, debugging, gameplay iteration and some visual/UI asset generation.

The actual Nemesis gameplay system was designed as a deterministic behaviour-classification system rather than using generative AI during gameplay.

---

## 📸 Screenshots

### Gameplay

![Gameplay Screenshot](screenshots/gameplay.png)

### Nemesis Encounter

![Nemesis Screenshot](screenshots/nemesis.png)

### Upgrade Selection

![Upgrade Screenshot](screenshots/upgrades.png)

> Add your screenshots to a `screenshots` folder in the repository and update the filenames above if needed.

---

## 🔗 Links

**Play NEMESIS:**  
https://illasandeep.itch.io/nemesis-prototype

**LinkedIn:**  
[Your LinkedIn Profile](https://www.linkedin.com/in/sandeep-illa0503)

---

## 👨‍💻 Developer

**Sandeep Illa**  
Software Engineer | Game Designer

This prototype was created as part of my Game Designer assessment for LILA Games.

---

## 📄 Assessment

The detailed game design, progression, monetization, testing strategy and design thinking are included in the accompanying assessment document.
