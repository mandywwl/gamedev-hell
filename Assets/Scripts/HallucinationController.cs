using UnityEngine;

public class HallucinationController : MonoBehaviour
{
    [Header("Ghost Settings")]
    [Tooltip("Prefab to spawn for hallucinations (visual only).")]
    public GameObject ghostPrefab;

    [Tooltip("How far from the player the ghost should appear.")]
    public float spawnDistance = 5f;

    [Tooltip("How long the ghost stays before disappearing.")]
    public float ghostLifetime = 3f;

    [Tooltip("Minimum time between hallucinations.")]
    public float minSpawnInterval = 5f;

    [Tooltip("Maximum time between hallucinations.")]
    public float maxSpawnInterval = 10f;

    [Header("References")]
    public Transform player;

    private float nextSpawnTime;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        ScheduleNextSpawn();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnGhost();
            ScheduleNextSpawn();
        }
    }

    void SpawnGhost()
    {
        if (ghostPrefab == null || player == null) return;

        // Pick a random direction around player
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0; 
        randomDirection.Normalize();

        Vector3 spawnPos = player.position + randomDirection * spawnDistance;

        // Spawn ghost
        GameObject ghost = Instantiate(ghostPrefab, spawnPos, Quaternion.LookRotation(-randomDirection));

        
        RemoveCombatScripts(ghost);

        // Destroy after lifetime
        Destroy(ghost, ghostLifetime);

        Debug.Log($"[HallucinationController] Spawned ghost at {spawnPos}");
    }

    void RemoveCombatScripts(GameObject ghost)
    {
        var unwanted = ghost.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in unwanted)
        {
            string typeName = script.GetType().Name;
            if (typeName.Contains("Unit") || typeName.Contains("AI") || typeName.Contains("BossEncounter"))
            {
                Destroy(script);
            }
        }

        // Remove colliders
        foreach (var col in ghost.GetComponentsInChildren<Collider2D>())
        {
            Destroy(col);
        }
    }

    void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}
