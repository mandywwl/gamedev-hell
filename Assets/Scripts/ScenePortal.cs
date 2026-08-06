using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ScenePortal : MonoBehaviour

{
    [Header("Destination")]
    public string TargetSceneName;       
    public string TargetSpawnId;         
    
    [Header("Optional: for clarity only")]
    public string ThisPortalId; // e.g. "toForestGate"

    [Header("References")]
    public Transform player; // drag player here / find via tag

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to the Player
        if (!other.CompareTag("Player")) return;

        if (GameState.I == null)
        {
            Debug.LogError("ScenePortal: GameState not found. Cannot transition scene.");
            return;
        }

        // Save current scene position
        var sceneName = SceneManager.GetActiveScene().name;
        if (!GameState.I.SceneMem.ContainsKey(sceneName))
            GameState.I.SceneMem[sceneName] = new GameState.SceneMemory();

        GameState.I.SceneMem[sceneName].lastPosition = other.transform.position;
        GameState.I.SceneMem[sceneName].hasLastPosition = true;

        // Tell next scene which SpawnPoint to use
        GameState.I.NextSpawnId = TargetSpawnId;

        // Load next scene
        SceneManager.LoadScene(TargetSceneName);
    }
}
