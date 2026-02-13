# Antymology – Evolutionary Ant Colony Simulation
### CPSC 565 – Assignment 3 | Winter 2026

---

## Table of Contents
1. [Project Overview](#1-project-overview)
2. [The World & Visual Guide](#2-the-world--visual-guide)
3. [User Interface Explained](#3-user-interface-explained)
4. [How to Run](#4-how-to-run)
5. [Generation Walkthrough](#5-generation-walkthrough)
6. [Ant & Queen Parameters](#6-ant--queen-parameters)
7. [Evolutionary Algorithm Overview](#7-evolutionary-algorithm-overview)
8. [Console Output Guide](#8-console-output-guide)
9. [How the Algorithm Selects the Best DNA](#9-how-the-algorithm-selects-the-best-dna)
10. [Between Generations: What Happens](#10-between-generations-what-happens)
11. [Fitness Calculations & Equations](#11-fitness-calculations--equations)
12. [Evolution in Depth](#12-evolution-in-depth)
13. [End of Simulation & Expected Patterns](#13-end-of-simulation--expected-patterns)
14. [Custom File Reference](#14-custom-file-reference)
15. [Parameter Impact Guide](#15-parameter-impact-guide)
16. [Limitations & Future Work](#16-limitations--future-work)

---

## 1. Project Overview

This project is a 3D ant colony simulation built in **Unity 6** that uses a **Genetic Algorithm (GA)** to evolve ant behaviour over multiple generations. The core goal is to demonstrate how evolutionary pressure — survival of the fittest — can guide a population of agents toward better collective performance over time.

The simulation measures success by how many **nest blocks** the queen ant produces. Worker ants must find food (mulch), survive long enough to share health with the queen, and enable the queen to accumulate enough health to place nest blocks. The more nests placed in a generation, the better that generation's DNA performed.

Over successive generations, the algorithm preserves the best-performing DNA, combines it through crossover, and introduces small mutations to explore new behavioural strategies — with the expectation that, over time, the colony becomes increasingly efficient at nest production.

---

## 2. The World & Visual Guide

### The Terrain

The world is a procedurally generated 3D voxel terrain made up of multiple block types, each with a distinct visual appearance and gameplay role:

| Block Type | Appearance | Role |
|------------|------------|------|
| **StoneBlock** | Grey cube | Structural underground layer; can be dug through |
| **GrassBlock** | Green cube | Surface layer; appears on top of stone |
| **MulchBlock** | Dark brown/olive cube | **Food source** for ants; consumed to restore health |
| **AcidicBlock** | Red/orange cube | Hazardous zone; doubles ant health decay rate |
| **ContainerBlock** | Dark grey/black cube | Indestructible boundary; cannot be dug or placed on |
| **NestBlock** | Purple cube | Placed by the queen; represents colony progress |
| **AirBlock** | Invisible | Empty space; result of digging or natural gaps |

### The Agents

**Worker Ants** appear as small **red cubes** scattered across the terrain surface. There are 10 worker ants per generation. They move randomly across the surface, eating mulch to survive, occasionally digging down into the terrain, and sharing health with nearby ants including the queen.

**The Queen Ant** appears as a larger **blue cube**, approximately 1.5× the scale of worker ants. There is exactly one queen per generation. She is visually distinct and behaviourally distinct — her primary purpose is to place nest blocks (purple cubes) at her current location, at the cost of her own health.

---

## 3. User Interface Explained

### Top-Left HUD (OnGUI overlay)

During simulation, four labels are displayed in yellow text at the top-left of the game view:

```
Generation: 2/5
Time Left: 44.3s
Current Fitness: 7
Best Fitness Ever: 10
```

| Label | Meaning |
|-------|---------|
| **Generation: X/Y** | Current generation number out of maximum generations (e.g. 2 out of 5) |
| **Time Left** | Seconds remaining in the current generation's evaluation window (counts down from 60s) |
| **Current Fitness** | The number of nest blocks placed so far in this generation. Updates in real time as the queen places nests |
| **Best Fitness Ever** | The highest nest count achieved across ALL generations so far. This number can only increase — it represents the best the algorithm has found |

**What these numbers mean together:** If Generation 3 shows `Current Fitness: 8` and `Best Fitness Ever: 10`, it means Generation 2 (or 1) produced 10 nests, which is the best result so far, and the current generation has produced 8 nests with time still remaining.

### Top-Left Canvas (TextMeshPro)

```
nest blocks: 9
```

This counter shows the raw nest block count for the current generation, updated live. It directly reflects how many times the queen successfully placed a nest block and had enough health to do so.

### End of Simulation Screen

When all generations complete, the screen displays:

```
SIMULATION COMPLETE!
Total Generations: 5
Best Fitness Ever: 13 nests
Best DNA: DNA[Eat:0.72, Dig:0.53, Move:0.87, Transfer:11.23]
Check Console for full results!
  Gen 1: 10 nests
  Gen 2: 9 nests
  Gen 3: 12 nests
  Gen 4: 13 nests
  Gen 5: 10 nests
```

This summarises the entire simulation run, showing per-generation performance and the single best DNA configuration ever discovered.

---

## 4. How to Run

1. Open the project in **Unity 6000.x**
2. Load the **SampleScene**
3. Ensure the following are assigned in the **WorldManager** Inspector:
   - `Ant Prefab` → Ant.prefab
   - `Queen Prefab` → Queen.prefab
   - `Block Material` → terrain material
   - `Nest Count UI Text` → NestCountText (Canvas)
   - `Evolution Manager` → EvolutionManager GameObject
4. Ensure the **EvolutionManager** GameObject exists in the scene hierarchy
5. Press **Play**
6. Use **WASD** + **middle mouse drag** to navigate the camera (FlyCamera)
7. Press **1–5** keys to place blocks manually in edit mode (UITerrainEditor)

The simulation will automatically run through all generations and display results on screen and in the Console window.

---

## 5. Generation Walkthrough

### What to Expect in Generation 1

When Play is pressed, the following sequence occurs:

1. The world terrain is procedurally generated using Simplex noise
2. Stone, Grass, Mulch, Acidic, and Container blocks are placed across the 3D grid
3. The EvolutionManager initialises a population of 10 DNA configurations, with the first 4 seeded with known-good starting values (high eat chance, low dig chance) and the remaining 6 fully random
4. **1 Queen** (blue, larger cube) spawns at the world centre surface
5. **10 Worker Ants** (red, small cubes) spawn randomly within ±5 blocks of centre
6. All ants receive the same DNA configuration for this generation
7. The 60-second evaluation timer begins

**What you observe:**
- Worker ants move randomly across the terrain in all 4 cardinal directions
- Ants that land on mulch blocks consume them (mulch block disappears, ant health increases)
- Ants occasionally dig downward, removing the block below them
- Ants standing on red acid regions lose health twice as fast — you may see them die quickly in those zones
- The queen moves slowly, ignores digging entirely, and periodically attempts to place a purple nest block beneath her
- When the queen places a nest, the nest count in the top-left increases
- As the timer runs, ants with poor DNA (e.g. low eat chance) will die first — visible as their GameObject disappearing from the Hierarchy
- When time runs out, evolution occurs and Generation 2 begins

### Expected Ant Behaviour Each Generation

| Behaviour | Description |
|-----------|-------------|
| Random movement | Ant picks one of 4 directions each move cycle; can only move to surfaces within 2 blocks height difference |
| Eat mulch | If standing on mulch and no other ant is on same tile, consumes it: +40 HP, mulch block → air |
| Dig down | Removes block directly below ant, ant drops one level |
| Share health | If two ants occupy the same grid tile, they equalise health at a controlled rate |
| Acid damage | Health decay doubles when standing on an AcidicBlock |
| Die | When health reaches 0, ant is destroyed and removed from hierarchy |

### Expected Queen Behaviour Each Generation

The queen inherits worker DNA but with dig chance forced to 0 (she never digs, to prevent her from trapping herself underground). Her behaviour:

1. Moves slowly across the surface seeking mulch
2. Eats mulch aggressively (eat chance ≥ 0.75) to maintain health
3. Every 3–4 seconds, attempts to place a nest block at her current location
4. If she has less than 1/3 of her max health, she cannot place a nest — she must eat first
5. Each successful nest placement costs exactly 1/3 of her maximum health

---

## 6. Ant & Queen Parameters

### Worker Ant Parameters (AntAgent.cs)

| Parameter | Inspector Field | Default | Description |
|-----------|----------------|---------|-------------|
| Max Health | `maxHealth` | 100 | Maximum health value. Ant dies when health reaches 0 |
| Base Health Decay | `baseHealthDecayPerSecond` | 5.0 | HP lost per second. Higher = ants die faster without food |
| Max Step Height | `maxStepHeight` | 2 | Maximum block height difference ant can move between |
| DNA | `dna` (AntDNA) | Assigned by EvolutionManager | Controls eat, dig, move speed, and health transfer behaviour |

### Queen Ant Parameters (QueenAgent.cs, extends AntAgent)

| Parameter | Inspector Field | Default | Description |
|-----------|----------------|---------|-------------|
| Max Health | `maxHealth` | 100 | Same system as workers — queen dies at 0 |
| Base Health Decay | `baseHealthDecayPerSecond` | 0.5 | Lower decay than workers — queen survives longer |
| Nest Build Interval | `nestBuildIntervalSeconds` | 3–4 | How often (seconds) queen attempts to place a nest |
| Dig Chance | Forced to 0 in code | 0 | Queen never digs regardless of evolved DNA |
| Eat Chance | Minimum 0.75 in code | ≥0.75 | Queen always prioritises eating to stay alive |

### Health System Equations

**Health decay per second:**
```
decay = baseHealthDecayPerSecond × (IsOnAcid ? 2.0 : 1.0)
health -= decay × Time.deltaTime
```

**Eating mulch:**
```
health = Min(maxHealth, health + 40)
```
One mulch block restores 40 HP, capped at max health. The mulch block is permanently removed.

**Health sharing (zero-sum):**
```
diff = healthA - healthB
transfer = Min(dna.healthTransferPerSecond × deltaTime, diff / 2)
healthA -= transfer
healthB += transfer
```
Health is redistributed between two ants on the same tile, equalising toward a balanced midpoint. No health is created or destroyed.

**Nest placement cost:**
```
cost = maxHealth / 3
if (currentHealth >= cost):
    placeNest()
    health -= cost
```
Each nest block costs exactly one-third of the queen's maximum health. With max health of 100, each nest costs ~33.3 HP. The queen must accumulate this health through eating before she can place a nest.

---

## 7. Evolutionary Algorithm Overview

The simulation uses a **Generational Genetic Algorithm** to evolve ant behaviour over 5 generations. The core idea is:

> Ants whose DNA leads to better colony performance (more nests) should have their genes preserved and passed to the next generation. Ants with poor DNA should die off and not contribute.

### What is DNA in this context?

Each ant has a DNA object (`AntDNA.cs`) containing 4 floating-point genes that directly control its behaviour:

| Gene | Range | Effect |
|------|-------|--------|
| `eatMulchChance` | 0.0 – 1.0 | Probability per action cycle of attempting to eat mulch |
| `digChance` | 0.0 – 1.0 | Probability per action cycle of digging downward |
| `moveIntervalSeconds` | 0.1 – 1.0 | Time between action cycles. Lower = faster, more active ant |
| `healthTransferPerSecond` | 0.0 – 20.0 | Rate at which health is equalised with nearby ants |

These 4 genes together define an ant's entire behavioural strategy. A DNA with high `eatMulchChance` and low `digChance` produces an ant that focuses on staying alive by eating rather than exploring underground. A DNA with low `eatMulchChance` means the ant rarely eats and dies quickly.

### Why this approach?

This approach was chosen because it creates a direct, observable link between DNA and behaviour — you can literally read the DNA values and predict how that ant will behave. This makes the evolution transparent and easy to document and explain.

---

## 8. Console Output Guide

The Unity Console window provides a real-time log of everything happening in the simulation. Here is what each message means:

### Initialisation
```
=== GENERATION 1 INITIALIZED (Seeded) ===
Population size: 10
Max generations: 5
```
The algorithm has created 10 DNA configurations. The first 4 are seeded with good starting values to give the algorithm a head start. The remaining 6 are fully random to maintain diversity.

### Generation Start
```
=== GENERATION 2/5 STARTED ===
Worker DNA: DNA[Eat:0.82, Dig:0.19, Move:0.35, Transfer:16.77]
```
Shows which generation is starting and which DNA configuration will be used for all worker ants this generation.

### Ant Creation
```
Ant created with DNA[Eat:0.82, Dig:0.19, Move:0.35, Transfer:16.77]
QUEEN: Using evolved DNA: DNA[Eat:0.82, Dig:0.00, Move:0.35, Transfer:16.77]
```
Each ant logs its DNA upon creation. The DNA is tracked so we can trace which behavioural configuration was active during any generation. Note the queen's dig chance is always shown as 0.00 regardless of the evolved value.

### Ant Deaths
```
ANT DIED: survived 12.4s | DNA[Eat:0.23, Dig:0.67, Move:0.91, Transfer:3.2]
ANT DIED: survived 38.7s | DNA[Eat:0.82, Dig:0.19, Move:0.35, Transfer:16.77]
```
When an ant's health reaches 0, it logs how long it survived and what DNA it had. This directly shows natural selection — ants with high eat chance (0.82) tend to survive longer than those with low eat chance (0.23).

### Queen Activity
```
QUEEN: Not enough health to place nest. Health=45.2, Cost=33.3
NEST placed at 64,18,67 | health=12.1 | total=4
```
Tracks the queen's nest-building attempts. When she lacks health, she logs it and waits. When she successfully places a nest, the coordinates, remaining health, and running total are logged.

### Generation End & Evolution
```
=== GENERATION 1/5 ENDED ===
Fitness (Nest Blocks): 10
NEW BEST FITNESS: 10
BEST DNA: DNA[Eat:0.82, Dig:0.19, Move:0.35, Transfer:16.77]
Survivors: 3/10
=== EVOLUTION ===
Elite 1: Fitness 10 - DNA[Eat:0.82, Dig:0.19, Move:0.35, Transfer:16.77]
Elite 2: Fitness 0  - DNA[Eat:0.26, Dig:0.35, Move:0.12, Transfer:12.34]
New population: 10 individuals
Best DNA this generation: DNA[Eat:0.82, ...]
Nest count reset to 0
Restored 138078 dug blocks
Regenerated 14539 mulch blocks for new generation
Killed 3 ants and 1 queens
New queen spawned with DNA: DNA[Eat:0.82, ...]
Spawned 1 queen + 10 workers with evolved DNA
=== GENERATION 2/5 STARTED ===
```

---

## 9. How the Algorithm Selects the Best DNA

### Fitness Measurement

At the end of each generation's 60-second window, the algorithm records:
```
fitness = WorldManager.nestBlockCount
```
This is the total number of nest blocks placed by the queen during that generation. It is a direct proxy for colony success — more nests means the queen survived longer and workers supported her better.

### Why nest count as fitness?

Nest count captures the entire colony's cooperative performance in a single number:
- Workers must eat enough to survive → share health with queen → queen builds nests
- If workers have poor DNA (die fast), the queen gets less health support → fewer nests
- If the queen has poor DNA (doesn't eat), she starves → fewer nests
- The metric naturally rewards the full cooperation chain

### Elite Selection

After recording fitness, the algorithm sorts all 10 DNA configurations by their fitness score. The top 2 performers (elites) are **copied unchanged** into the next generation's population. This guarantees the best solution found is never lost, no matter what mutations occur.

```
Sort population by fitness (descending)
elite[0] = population[0].Clone()  // best DNA, exact copy
elite[1] = population[1].Clone()  // second best, exact copy
```

### Tournament Selection

For the remaining 8 slots in the new population, the algorithm uses **Tournament Selection**:

```
For each new individual needed:
    Pick 3 random DNA from current population
    The one with highest fitness becomes a parent
    Repeat to get parent2
    Create child via crossover(parent1, parent2)
    Apply mutation to child
```

Tournament selection is used rather than pure fitness-proportionate selection because it:
- Maintains selection pressure (good DNA wins more often)
- Preserves diversity (weaker DNA can still occasionally win a tournament)
- Is resistant to domination by a single super-individual

---

## 10. Between Generations: What Happens

When a generation's timer reaches zero, the following sequence executes automatically:

### Step 1: Record Fitness
```
fitness = nestBlockCount  // snapshot current nest count
generationFitnessLog.Add(fitness)
```

### Step 2: Update Best Fitness
```
if (fitness > bestFitness):
    bestFitness = fitness
    bestDNAEver = currentDNA.Clone()
```
The `Best Fitness Ever` UI label can only increase. If this generation scored higher than all previous, it becomes the new best.

### Step 3: Evolve Population
Elite selection + tournament selection + crossover + mutation produces a new population of 10 DNA configurations (see Section 9 and 12 for detail).

### Step 4: Reset World
```
WorldManager.ResetNestCount()     // set nestBlockCount = 0
WorldManager.RestoreTerrain()     // fill dug holes with GrassBlock/StoneBlock
WorldManager.RegenerateMulch()    // place 2 layers of mulch on all surfaces
WorldManager.RespawnAnts(newDNA)  // destroy old ants, spawn fresh with evolved DNA
```

This world reset ensures each generation starts from a comparable state — same terrain structure, same food availability — so that differences in nest count across generations more accurately reflect DNA quality rather than world state.

### What you see in Unity:
- All ant and queen GameObjects disappear from the Hierarchy instantly
- The terrain visually updates as dug holes are filled
- New mulch blocks appear on all surfaces
- New Queen (blue) and 10 new Ant (red) GameObjects appear in the Hierarchy
- Console shows the new generation's starting DNA
- UI labels reset: `Current Fitness: 0`, timer resets to 60s, generation counter increments

---

## 11. Fitness Calculations & Equations

### Per-Generation Fitness
```
F(g) = nestBlockCount at end of generation g
```

### Best Fitness Update
```
bestFitness = Max(bestFitness, F(g))  for all g
```
This means `Best Fitness Ever` is a running maximum and can never decrease.

### Current Fitness (Live)
```
currentFitness = WorldManager.nestBlockCount  // updated every frame
```
This reflects nest count in real time as the queen places blocks. It feeds directly into the UI label.

### Crossover (Child DNA Creation)
For each gene in the child:
```
child.gene = (Random.value < 0.5) ? parent1.gene : parent2.gene
```
Each gene is independently inherited from either parent with 50/50 probability.

### Mutation
For each gene, with probability `mutationRate` (default 0.08):
```
gene += Random.Range(-delta, +delta)
gene = Clamp(gene, min, max)
```
Where delta values are:
- eatMulchChance, digChance: ±0.2
- moveIntervalSeconds: ±0.2 (clamped to 0.1–1.0)
- healthTransferPerSecond: ±5.0 (clamped to 0–20)

### Why 0.08 mutation rate?

A lower mutation rate (0.08 vs default 0.15) means good genes are more likely to be preserved between generations. Too high a mutation rate would randomise DNA too quickly, destroying good solutions. Too low would prevent exploration of new strategies. 0.08 is a balance that allows gradual improvement while preserving elite behaviour.

---

## 12. Evolution in Depth

### The Full Evolutionary Loop

```
Generation starts
    → Ants spawn with current population's DNA[0]
    → Ants live, eat, die, share health
    → Queen builds nests
    → Timer reaches 0
Generation ends
    → fitness = nestCount recorded
    → bestFitness updated if improved
    → Sort population by fitness
    → Keep top 2 elites unchanged
    → Generate 8 new offspring:
        → Tournament select 2 parents
        → Crossover genes
        → Mutate genes
    → New population replaces old
World resets
    → Terrain restored
    → Mulch regenerated
    → Old ants destroyed
    → New ants spawned with population[0] DNA
Next generation starts
```

### What the Algorithm is Trying to Demonstrate

The core evolutionary hypothesis is:

> **DNA configurations that cause ants to eat more and dig less will result in longer survival, more health sharing with the queen, and therefore more nest production.**

The algorithm is designed to converge toward DNA where:
- `eatMulchChance` → high (ants eat aggressively to survive)
- `digChance` → low (ants don't waste time/position digging)
- `moveIntervalSeconds` → moderate (active enough to find food, not so fast they never eat)
- `healthTransferPerSecond` → moderate-high (efficient queen support)

### Why All Ants Share One DNA Per Generation

In this implementation, all ants in a generation receive the same DNA configuration (population[0]). This is a simplification that treats the colony as a single organism being evaluated collectively. The fitness of that DNA is the colony's nest output. This is analogous to evaluating a strategy rather than individual agents.

A more sophisticated implementation would give each ant individual DNA and track survival time per individual, but the current approach is sufficient to demonstrate the core evolutionary concept within the assignment constraints.

### The Selection Pressure Chain

```
High eatMulchChance
    → Ant eats mulch frequently
    → Ant health stays high
    → Ant survives longer (seen: fewer "ANT DIED" logs)
    → More ants alive = more health sharing
    → Queen receives more health transfers
    → Queen builds more nests
    → Higher fitness score
    → This DNA preserved via elite selection
    → Next generation inherits high eatMulchChance
```

This chain is directly visible in the console: ants with high eat chance show "survived 40+ seconds" while ants with low eat chance show "survived 5–10 seconds."

---

## 13. End of Simulation & Expected Patterns

After 5 generations complete, the screen displays `SIMULATION COMPLETE!` with the full results summary.

### Expected Pattern

The algorithm is designed to show a generally improving trend in nest count across generations:

```
Gen 1: ~8–10 nests   (seeded DNA, reasonable starting point)
Gen 2: ~9–11 nests   (elite DNA preserved, crossover begins)
Gen 3: ~10–13 nests  (stronger selection pressure)
Gen 4: ~11–14 nests  (DNA converging toward high eat chance)
Gen 5: ~10–13 nests  (may vary due to randomness)
Best: 13 nests
```

### The Vision and Greater Picture

The simulation demonstrates a microcosm of evolutionary biology — a population of agents adapting their behaviour through inherited, varied, and selected traits. The ant colony serves as the fitness landscape: survival requires eating, cooperation enables the queen, and nest building is the measurable output of successful evolution.

Over generations, the algorithm explores behavioural space (the 4-dimensional DNA space) and converges toward strategies that balance survival and cooperation. The process mirrors how real evolutionary systems work: random variation, environmental pressure, selection, and inheritance combine to produce adapted behaviour without any explicit programming of what "good" behaviour looks like.

The result is a system that discovers, on its own, that eating is more important than digging — a conclusion reached through evolutionary pressure rather than human instruction.

---

## 14. Custom File Reference

The following files were created custom for this assignment:

### `AntDNA.cs` — Assets/Components/Agents/
**Purpose:** Defines the genetic encoding for ant behaviour.

Contains 4 float genes: `eatMulchChance`, `digChance`, `moveIntervalSeconds`, `healthTransferPerSecond`. Provides:
- `AntDNA()` — default constructor with safe starting values (no Random calls, safe for Unity serialisation)
- `CreateRandom()` — static factory method for random DNA (called at runtime, not during serialisation)
- `Crossover(parent1, parent2, mutationRate)` — creates child DNA by mixing two parents gene by gene with 50/50 probability, then mutating
- `Mutate(rate)` — randomly adjusts each gene with given probability
- `Clone()` — creates exact copy for elite preservation
- `ToString()` — formatted string for console logging

### `AntAgent.cs` — Assets/Components/Agents/
**Purpose:** Controls all worker ant behaviour.

Key methods:
- `Start()` — initialises health, birth time, and DNA. If no DNA assigned or DNA has zero values, creates random DNA
- `Update()` — health decay, acid check, action timer, death detection
- `TryEatMulch()` — checks block below, exclusivity, consumes mulch
- `TryDigDown()` — removes block below, moves ant down (cannot dig ContainerBlock)
- `TryRandomMove()` — moves to adjacent surface within height limit
- `TryShareHealth()` — zero-sum health equalisation with colocated ants
- `IsStandingOnAcid()` — checks if block below is AcidicBlock
- `survivalTime` property — tracks how long ant has been alive for logging

### `QueenAgent.cs` — Assets/Components/Agents/
**Purpose:** Extends AntAgent with nest-building behaviour.

Overrides `Start()` to force `digChance = 0` and ensure minimum eat chance. Adds a nest timer that attempts `TryPlaceNestBlock()` at regular intervals. Nest placement costs `maxHealth / 3` health, satisfying the assignment requirement.

### `EvolutionManager.cs` — Assets/Components/Configuration/
**Purpose:** Manages the generational evolutionary algorithm.

Maintains:
- Population of 10 `AntDNA` objects
- Per-generation fitness log
- Best fitness and best DNA trackers
- Generation timer and state machine

Implements: initialisation (seeded + random), `StartGeneration()`, `EndGeneration()`, `EvolvePopulation()` (elite selection + tournament + crossover + mutation), `EndSimulation()` (final report), and `OnGUI()` (live HUD).

### `NestCountUI.cs` — Assets/Components/UI/
**Purpose:** Simple UI component that updates a TextMeshPro label with the current nest block count every frame.

---

## 15. Parameter Impact Guide

The following describes how changing key parameters affects simulation behaviour:

### Changing Queen Max Health

| Change | Effect |
|--------|--------|
| Increase (e.g. 500) | Each nest costs more health (500/3 = 166). Queen can only build ~3 nests before running low. Fitness scores decrease. |
| Decrease (e.g. 50) | Each nest costs less health (50/3 = 17). Queen builds more nests but dies faster from decay. Net effect depends on eat rate. |
| **Recommended** | 100 — balanced between affordable nest cost (33 HP) and survivable decay |

### Changing Base Health Decay (Ant)

| Change | Effect |
|--------|--------|
| Increase (e.g. 10/s) | Ants die within seconds unless near mulch constantly. Natural selection is extreme — only very high eat-chance DNA survives. |
| Decrease (e.g. 0.5/s) | Ants live almost indefinitely. No natural selection pressure. Evolution cannot distinguish good from bad DNA. |
| **Recommended** | 5/s — forces ants to eat regularly, creates meaningful selection pressure |

### Changing Mutation Rate

| Change | Effect |
|--------|--------|
| Increase (e.g. 0.5) | DNA changes dramatically each generation. Good genes lost quickly. Evolution becomes essentially random. |
| Decrease (e.g. 0.01) | DNA barely changes. Algorithm exploits found solution but cannot explore new strategies. May plateau early. |
| **Recommended** | 0.08 — allows gradual improvement while preserving good genes |

### Changing Number of Acidic Regions

| Change | Effect |
|--------|--------|
| Increase (e.g. 20) | More hazardous terrain. Ants die faster. Queen has less health support. Fitness scores lower overall. |
| Decrease (e.g. 2) | Safer terrain. Ants survive longer. Queen gets more support. Higher overall fitness scores. |

### Changing Nest Build Interval

| Change | Effect |
|--------|--------|
| Decrease (e.g. 1s) | Queen attempts nests more frequently. More likely to attempt when lacking health. More "Not enough health" logs. |
| Increase (e.g. 8s) | Queen attempts nests less frequently. Builds fewer nests per generation even if healthy. Lower fitness ceiling. |
| **Recommended** | 3–4s — allows queen time to eat between nest attempts |

### Changing Generation Duration

| Change | Effect |
|--------|--------|
| Increase (e.g. 120s) | More time for queen to build nests. Higher raw fitness scores. More world depletion between resets. |
| Decrease (e.g. 30s) | Less time, fewer nests, faster iteration, less world depletion. Better for showing trends across generations. |

---

## 16. Limitations & Future Work

### Known Limitations

**Single DNA per generation:** All ants in a generation share the same DNA. This means the fitness score reflects one DNA configuration's performance, not the result of competition between multiple strategies. A richer implementation would give each ant individual DNA and evaluate survival time per individual.

**Fitness attribution problem:** Because all ants share DNA, the fitness score is attributed to population[0]'s DNA, even though other DNA variants in the population never actually ran. This means most of the population evolves without real fitness data, relying heavily on the elites.

**World state variation:** Despite terrain restoration between generations, the world is not perfectly identical each generation. Acid regions remain, container spheres remain, and deep structural differences from digging may persist. This introduces environmental noise into fitness comparisons.

**Short run:** 5 generations with 10 individuals each explores only a tiny fraction of the 4-dimensional DNA space. Real evolutionary algorithms typically require hundreds or thousands of generations to show reliable convergence trends.

**Queen health coupling:** Because the queen's nest output depends on worker health sharing, changes to worker DNA affect queen performance indirectly. This coupling is intentional but makes it difficult to isolate which DNA gene is driving fitness improvements.

### Ways to Improve

**Individual DNA per ant:** Give each ant unique DNA. Track each ant's survival time. Use survival time as individual fitness. Select parents based on who lived longest. This creates true natural selection at the individual level.

**Separate queen DNA evolution:** Evolve queen DNA separately from worker DNA. Queen genes could control nest interval and movement pattern; worker genes control eating and sharing. Co-evolution of two specialised roles.

**Pheromone trails:** As suggested in the assignment, implement pheromone deposits (already commented in AirBlock.cs). Evolve genes that control sensitivity to pheromone concentrations. This would enable emergent path-finding and collective foraging behaviour.

**Parallel world evaluation:** Run multiple worlds simultaneously, one per DNA variant, evaluate them in parallel, and use results for a proper population-level fitness comparison.

**Longer runs with graphing:** Run 50–100 generations and plot fitness over time as a graph. The evolutionary trend would be much clearer over longer runs, showing the characteristic S-curve of evolutionary optimisation.

**Neural network controller:** Replace the 4-gene DNA with a small neural network. Evolve network weights. This would allow much richer and more adaptive behaviour but at the cost of interpretability.

**True generational reset:** Implement full terrain regeneration from the original seed between generations, rather than partial restoration. This would ensure perfectly comparable conditions for each generation's fitness evaluation.

---

*README authored for CPSC 565 Assignment 3 — Antymology Evolutionary Ant Simulation*
*Unity 6 | C# | Genetic Algorithm | Winter 2026*
