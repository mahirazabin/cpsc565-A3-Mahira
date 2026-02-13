using UnityEngine;
using System.Collections.Generic;
using Antymology.Terrain;

public class EvolutionManager : MonoBehaviour
{
    [Header("Evolution Settings")]
    public int populationSize = 10;
    public int generationDuration = 60;
    public float mutationRate = 0.15f;
    public int eliteCount = 2;
    public int maxGenerations = 5;

    [Header("Current Generation Info")]
    public int currentGeneration = 0;
    public int bestFitness = 0;
    public int currentFitness = 0;
    public bool simulationComplete = false;

    // Tracks fitness each generation for README documentation
    private List<int> generationFitnessLog = new List<int>();
    private AntDNA bestDNAEver;

    private struct DNAFitnessPair
    {
        public AntDNA dna;
        public int fitness;
        public DNAFitnessPair(AntDNA dna, int fitness)
        {
            this.dna = dna;
            this.fitness = fitness;
        }
    }

    private List<AntDNA> currentPopulation;
    private List<int> fitnessScores;
    private float generationTimer;
    private bool generationRunning = false;

    void Start()
    {
        InitializeFirstGeneration();
        StartGeneration();
    }

    void Update()
    {
        if (!generationRunning) return;
        if (simulationComplete) return;
        if (WorldManager.Instance == null) return;

        generationTimer -= Time.deltaTime;
        currentFitness = WorldManager.Instance.nestBlockCount;

        if (generationTimer <= 0)
        {
            EndGeneration();
        }
    }

    void InitializeFirstGeneration()
    {
        currentPopulation = new List<AntDNA>();
        fitnessScores = new List<int>();

        for (int i = 0; i < populationSize; i++)
        {
            currentPopulation.Add(AntDNA.CreateRandom());
            fitnessScores.Add(0);
        }

        currentGeneration = 1;
        Debug.Log($"=== GENERATION 1 INITIALIZED ===");
        Debug.Log($"Population size: {populationSize}");
        Debug.Log($"Max generations: {maxGenerations}");
    }

    void StartGeneration()
    {
        generationTimer = generationDuration;
        generationRunning = true;
        Debug.Log($"=== GENERATION {currentGeneration}/{maxGenerations} STARTED ===");
        Debug.Log($"Using DNA: {GetCurrentDNA()}");
    }

    void EndGeneration()
    {
        GameObject[] survivors = GameObject.FindGameObjectsWithTag("Ant");
        Debug.Log($"=== GENERATION {currentGeneration} SURVIVORS: {survivors.Length}/{WorldManager.Instance.numberOfWorkers} ===");
        foreach (var s in survivors)
        {
            AntAgent agent = s.GetComponent<AntAgent>();
            if (agent != null)
                Debug.Log($"  Survivor: {agent.dna} | Alive for: {agent.survivalTime:F1}s");
        }
        generationRunning = false;

        int fitness = WorldManager.Instance != null ?
                      WorldManager.Instance.nestBlockCount : 0;

        generationFitnessLog.Add(fitness);

        Debug.Log($"=== GENERATION {currentGeneration}/{maxGenerations} ENDED ===");
        Debug.Log($"Fitness (Nest Blocks): {fitness}");

        if (fitness > bestFitness)
        {
            bestFitness = fitness;
            bestDNAEver = GetCurrentDNA().Clone();
            Debug.Log($"NEW BEST FITNESS: {bestFitness}");
            Debug.Log($"BEST DNA: {bestDNAEver}");
        }

        if (fitnessScores.Count > 0)
            fitnessScores[0] = fitness;

        // Check if we've reached max generations
        if (currentGeneration >= maxGenerations)
        {
            EndSimulation();
            return;
        }

        // Evolve population
        EvolvePopulation();
        currentGeneration++;

        // Full world reset for next generation
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.ResetNestCount();
            WorldManager.Instance.RestoreTerrain();
            WorldManager.Instance.RegenerateMulch();
            WorldManager.Instance.RespawnAnts(GetCurrentDNA());
        }

        StartGeneration();
    }

    void EndSimulation()
    {
        simulationComplete = true;
        generationRunning = false;

        Debug.Log("╔══════════════════════════════╗");
        Debug.Log("║   SIMULATION COMPLETE!       ║");
        Debug.Log("╚══════════════════════════════╝");
        Debug.Log($"Total Generations Run: {maxGenerations}");
        Debug.Log($"Best Fitness Ever: {bestFitness} nests");
        if (bestDNAEver != null)
            Debug.Log($"Best DNA Ever: {bestDNAEver}");
        Debug.Log("--- Fitness Per Generation ---");
        for (int i = 0; i < generationFitnessLog.Count; i++)
        {
            Debug.Log($"  Generation {i + 1}: {generationFitnessLog[i]} nests");
        }
    }

    void EvolvePopulation()
    {
        List<DNAFitnessPair> sortedPairs = new List<DNAFitnessPair>();
        for (int i = 0; i < currentPopulation.Count; i++)
        {
            int score = i < fitnessScores.Count ? fitnessScores[i] : 0;
            sortedPairs.Add(new DNAFitnessPair(currentPopulation[i], score));
        }

        sortedPairs.Sort((a, b) => b.fitness.CompareTo(a.fitness));

        List<AntDNA> newPopulation = new List<AntDNA>();

        Debug.Log("=== EVOLUTION ===");

        // Elite selection - best performers survive unchanged
        for (int i = 0; i < eliteCount && i < sortedPairs.Count; i++)
        {
            newPopulation.Add(sortedPairs[i].dna.Clone());
            Debug.Log($"Elite {i + 1}: Fitness {sortedPairs[i].fitness} - {sortedPairs[i].dna}");
        }

        // Fill rest with offspring via crossover + mutation
        while (newPopulation.Count < populationSize)
        {
            AntDNA parent1 = TournamentSelection(sortedPairs, 3);
            AntDNA parent2 = TournamentSelection(sortedPairs, 3);
            AntDNA child = AntDNA.Crossover(parent1, parent2, mutationRate);
            newPopulation.Add(child);
        }

        currentPopulation = newPopulation;
        Debug.Log($"New population created: {newPopulation.Count} individuals");
        Debug.Log($"Best DNA this generation: {sortedPairs[0].dna}");
    }

    AntDNA TournamentSelection(List<DNAFitnessPair> pairs, int tournamentSize)
    {
        AntDNA best = null;
        int bestScore = -1;

        for (int i = 0; i < tournamentSize; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, pairs.Count);
            DNAFitnessPair candidate = pairs[randomIndex];

            if (candidate.fitness > bestScore)
            {
                bestScore = candidate.fitness;
                best = candidate.dna;
            }
        }

        if (best == null) best = AntDNA.CreateRandom();
        return best;
    }

    public AntDNA GetCurrentDNA()
    {
        if (currentPopulation == null || currentPopulation.Count == 0)
            return AntDNA.CreateRandom();

        return currentPopulation[0];
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.yellow;

        // Show completion screen
        if (simulationComplete)
        {
            GUIStyle doneStyle = new GUIStyle();
            doneStyle.fontSize = 28;
            doneStyle.normal.textColor = Color.green;
            doneStyle.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(10, 100, 600, 40), "SIMULATION COMPLETE!", doneStyle);
            GUI.Label(new Rect(10, 145, 500, 30), $"Total Generations: {maxGenerations}", style);
            GUI.Label(new Rect(10, 175, 500, 30), $"Best Fitness Ever: {bestFitness} nests", style);
            if (bestDNAEver != null)
                GUI.Label(new Rect(10, 205, 700, 30), $"Best DNA: {bestDNAEver}", style);
            GUI.Label(new Rect(10, 235, 500, 30), "Check Console for full results!", style);

            // Show per-generation results
            for (int i = 0; i < generationFitnessLog.Count; i++)
            {
                GUI.Label(new Rect(10, 265 + (i * 30), 400, 30),
                    $"  Gen {i + 1}: {generationFitnessLog[i]} nests", style);
            }
            return;
        }

        // Show live stats during simulation
        GUI.Label(new Rect(10, 100, 400, 30), $"Generation: {currentGeneration}/{maxGenerations}", style);
        GUI.Label(new Rect(10, 130, 400, 30), $"Time Left: {generationTimer:F1}s", style);
        GUI.Label(new Rect(10, 160, 400, 30), $"Current Fitness: {currentFitness}", style);
        GUI.Label(new Rect(10, 190, 400, 30), $"Best Fitness Ever: {bestFitness}", style);
    }
}