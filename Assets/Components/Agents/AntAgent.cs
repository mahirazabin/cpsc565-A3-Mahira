using UnityEngine;
using Antymology.Terrain;

public class AntAgent : MonoBehaviour
{
    public float GetHealth() => health;
    private float birthTime;
    public float survivalTime { get; private set; }

    public void AddHealth(float delta)
    {
        health = Mathf.Clamp(health + delta, 0f, maxHealth);
    }

    [Header("DNA-Controlled Behavior")]
    public AntDNA dna;

    [Header("Health")]
    public float maxHealth = 100f;
    public float baseHealthDecayPerSecond = 1f;

    [Header("Movement")]
    public int maxStepHeight = 2;

    private float health;
    private float moveTimer;

    protected virtual void Start()
    {
        health = maxHealth;
        birthTime = Time.time;
        
        // If DNA is null OR has zero values (not initialized), create random
        if (dna == null || dna.moveIntervalSeconds == 0f)
        {
            dna = AntDNA.CreateRandom();
        }
        
        moveTimer = UnityEngine.Random.Range(0f, dna.moveIntervalSeconds);
        Debug.Log($"Ant created with {dna}");
    }

    /// <summary>
    /// Applies DNA values to ant behavior
    /// </summary>
    protected virtual void ApplyDNA()
    {
        // DNA controls these behaviors directly
        Debug.Log($"Ant created with {dna}");
    }

    private void TryShareHealth()
    {
        if (WorldManager.Instance == null) return;
        // Find other ants extremely close to us (same tile-ish)
        Collider[] hits = Physics.OverlapBox(transform.position, Vector3.one * 0.1f);

        foreach (var h in hits)
        {
            if (h.gameObject == gameObject) continue;

            AntAgent other = h.GetComponent<AntAgent>();
            if (other == null) continue;

            // Make sure we only process the pair once (avoid double transfer)
            if (GetInstanceID() > other.GetInstanceID()) continue;

            // Must be on the exact same grid tile
            Vector3Int a = GetGridPos();
            Vector3Int b = other.GetGridPos();
            if (a != b) continue;

            float total = health + other.health;
            float target = total / 2f;

            // Transfer towards equalization at a limited rate
            float diff = health - other.health;
            if (Mathf.Abs(diff) < 0.001f) continue;

            // Use DNA-controlled transfer rate
            float maxDelta = dna.healthTransferPerSecond * Time.deltaTime;

            if (diff > 0f)
            {
                // we have more health, give some
                float give = Mathf.Min(maxDelta, diff / 2f);
                health -= give;
                other.health += give;
            }
            else
            {
                // other has more health, they give some to us
                float give = Mathf.Min(maxDelta, (-diff) / 2f);
                other.health -= give;
                health += give;
            }

            // Clamp both
            health = Mathf.Clamp(health, 0f, maxHealth);
            other.health = Mathf.Clamp(other.health, 0f, other.maxHealth);
        }
    }

    protected virtual void Update()
    {
        // Guard against WorldManager not being ready yet
        if (WorldManager.Instance == null) return;

        // 1) Health decay (acid doubles it)
        float decay = baseHealthDecayPerSecond;
        if (IsStandingOnAcid()) decay *= 2f;

        health -= decay * Time.deltaTime;
        if (health <= 0f)
        {
            survivalTime = Time.time - birthTime;
            Debug.Log($"ANT DIED: survived {survivalTime:F1}s | {dna}");
            Destroy(gameObject);
            return;
        }

        TryShareHealth();

        // 2) Do actions on a timer
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            moveTimer = dna.moveIntervalSeconds;

            if (UnityEngine.Random.value < dna.eatMulchChance && TryEatMulch()) return;
            if (UnityEngine.Random.value < dna.digChance && TryDigDown()) return;
            TryRandomMove();
        }
    }



    // ----------------------------
    // Grid helpers
    // ----------------------------

    protected Vector3Int GetGridPos()
    {
        Vector3 p = transform.position;
        return new Vector3Int(
            Mathf.RoundToInt(p.x),
            Mathf.RoundToInt(p.y),
            Mathf.RoundToInt(p.z)
        );
    }

    protected void SetGridPos(Vector3Int gp)
    {
        transform.position = new Vector3(gp.x, gp.y, gp.z);
    }

    private int SurfaceYAt(int x, int z)
    {
        for (int y = 256; y >= 0; y--)
        {
            var b = WorldManager.Instance.GetBlock(x, y, z);
            if (b is AirBlock) continue;
            return y;
        }
        return 0;
    }

    // ----------------------------
    // Behaviors
    // ----------------------------

    private bool IsStandingOnAcid()
    {
        // Guard against null WorldManager
        if (WorldManager.Instance == null) return false;

        Vector3Int gp = GetGridPos();
        var below = WorldManager.Instance.GetBlock(gp.x, gp.y - 1, gp.z);
        return (below is AcidicBlock);
    }

    private bool TryEatMulch()
    {
        if (WorldManager.Instance == null) return false;

        Vector3Int gp = GetGridPos();
        var below = WorldManager.Instance.GetBlock(gp.x, gp.y - 1, gp.z);

        if (!(below is MulchBlock)) return false;

        // Check exclusivity
        Collider[] hits = Physics.OverlapBox(transform.position, Vector3.one * 0.1f);
        foreach (var h in hits)
        {
            if (h.gameObject != gameObject && h.GetComponent<AntAgent>() != null)
            {
                return false;
            }
        }

        // Consume mulch
        WorldManager.Instance.SetBlock(gp.x, gp.y - 1, gp.z, new AirBlock());
        health = Mathf.Min(maxHealth, health + 40f);
        return true;
    }

    private bool TryDigDown()
    {
        if (WorldManager.Instance == null) return false;

        Vector3Int gp = GetGridPos();
        var below = WorldManager.Instance.GetBlock(gp.x, gp.y - 1, gp.z);

        if (below is AirBlock) return false;
        if (below is ContainerBlock) return false;

        WorldManager.Instance.SetBlock(gp.x, gp.y - 1, gp.z, new AirBlock());
        SetGridPos(new Vector3Int(gp.x, gp.y - 1, gp.z));
        return true;
    }

    private bool TryRandomMove()
    {
        if (WorldManager.Instance == null) return false;

        Vector3Int gp = GetGridPos();

        Vector3Int[] dirs = new Vector3Int[]
        {
            new Vector3Int(1,0,0),
            new Vector3Int(-1,0,0),
            new Vector3Int(0,0,1),
            new Vector3Int(0,0,-1)
        };

        Vector3Int dir = dirs[UnityEngine.Random.Range(0, dirs.Length)];
        int nx = gp.x + dir.x;
        int nz = gp.z + dir.z;

        int curSurface = SurfaceYAt(gp.x, gp.z);
        int nextSurface = SurfaceYAt(nx, nz);

        int heightDiff = Mathf.Abs(nextSurface - curSurface);
        if (heightDiff > maxStepHeight) return false;

        SetGridPos(new Vector3Int(nx, nextSurface + 1, nz));
        return true;
    }
}
