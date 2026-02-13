using UnityEngine;
using Antymology.Terrain;

public class QueenAgent : AntAgent
{
    [Header("Queen")]
    public float nestBuildIntervalSeconds = 3f;

    private float nestTimer;

    protected override void Start()
    {
        if (dna == null || dna.moveIntervalSeconds <= 0f)
        {
            dna = new AntDNA(
                eat: 0.9f,
                dig: 0.0f,
                moveInterval: 0.6f,
                healthTransfer: 10f
            );
            Debug.Log("QUEEN: Using default DNA");
        }
        else
        {
            // Keep evolved eat/transfer/move but NEVER dig
            dna.digChance = 0f;
            dna.eatMulchChance = Mathf.Max(dna.eatMulchChance, 0.75f);
            Debug.Log($"QUEEN: Using evolved DNA: {dna}");
        }

        base.Start();
        nestTimer = nestBuildIntervalSeconds;
    }

    protected override void Update()
    {
        base.Update();

        nestTimer -= Time.deltaTime;
        if (nestTimer <= 0f)
        {
            nestTimer = nestBuildIntervalSeconds;
            TryPlaceNestBlock();
        }
    }

    private void TryPlaceNestBlock()
    {
        if (WorldManager.Instance == null) return;

        Vector3Int gp = GetGridPos();

        float cost = maxHealth / 3f;
        if (GetHealth() < cost)
        {
            Debug.Log($"QUEEN: Not enough health. Health={GetHealth():F1}, Cost={cost:F1}");
            return;
        }

        bool placed = WorldManager.Instance.TryPlaceNest(gp.x, gp.y - 1, gp.z);
        if (!placed) return;

        AddHealth(-cost);
        Debug.Log($"NEST placed | health={GetHealth():F1} | total={WorldManager.Instance.nestBlockCount}");
    }
}