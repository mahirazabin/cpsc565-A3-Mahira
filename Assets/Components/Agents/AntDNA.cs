using UnityEngine;

[System.Serializable]
public class AntDNA
{
    public float eatMulchChance;
    public float digChance;
    public float moveIntervalSeconds;
    public float healthTransferPerSecond;

    // DEFAULT constructor - NO Random calls here!
    public AntDNA()
    {
        eatMulchChance = 0.5f;
        digChance = 0.15f;
        moveIntervalSeconds = 0.25f;
        healthTransferPerSecond = 10f;
    }

    // Use this to create RANDOM DNA instead
    public static AntDNA CreateRandom()
    {
        AntDNA dna = new AntDNA();
        dna.eatMulchChance = UnityEngine.Random.Range(0f, 1f);
        dna.digChance = UnityEngine.Random.Range(0f, 1f);
        dna.moveIntervalSeconds = UnityEngine.Random.Range(0.1f, 1f);
        dna.healthTransferPerSecond = UnityEngine.Random.Range(0f, 20f);
        return dna;
    }

    public AntDNA(float eat, float dig, float moveInterval, float healthTransfer)
    {
        eatMulchChance = eat;
        digChance = dig;
        moveIntervalSeconds = moveInterval;
        healthTransferPerSecond = healthTransfer;
    }

    public static AntDNA Crossover(AntDNA parent1, AntDNA parent2, float mutationRate = 0.1f)
    {
        AntDNA child = new AntDNA();
        child.eatMulchChance = UnityEngine.Random.value < 0.5f ? parent1.eatMulchChance : parent2.eatMulchChance;
        child.digChance = UnityEngine.Random.value < 0.5f ? parent1.digChance : parent2.digChance;
        child.moveIntervalSeconds = UnityEngine.Random.value < 0.5f ? parent1.moveIntervalSeconds : parent2.moveIntervalSeconds;
        child.healthTransferPerSecond = UnityEngine.Random.value < 0.5f ? parent1.healthTransferPerSecond : parent2.healthTransferPerSecond;
        child.Mutate(mutationRate);
        return child;
    }

    public void Mutate(float mutationRate)
    {
        if (UnityEngine.Random.value < mutationRate)
            eatMulchChance = Mathf.Clamp01(eatMulchChance + UnityEngine.Random.Range(-0.2f, 0.2f));
        if (UnityEngine.Random.value < mutationRate)
            digChance = Mathf.Clamp01(digChance + UnityEngine.Random.Range(-0.2f, 0.2f));
        if (UnityEngine.Random.value < mutationRate)
            moveIntervalSeconds = Mathf.Clamp(moveIntervalSeconds + UnityEngine.Random.Range(-0.2f, 0.2f), 0.1f, 1f);
        if (UnityEngine.Random.value < mutationRate)
            healthTransferPerSecond = Mathf.Clamp(healthTransferPerSecond + UnityEngine.Random.Range(-5f, 5f), 0f, 20f);
    }

    public AntDNA Clone()
    {
        return new AntDNA(eatMulchChance, digChance, moveIntervalSeconds, healthTransferPerSecond);
    }

    public override string ToString()
    {
        return $"DNA[Eat:{eatMulchChance:F2}, Dig:{digChance:F2}, Move:{moveIntervalSeconds:F2}, Transfer:{healthTransferPerSecond:F2}]";
    }
}