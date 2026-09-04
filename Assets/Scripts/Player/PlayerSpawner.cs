using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] Transform player; // optional; will auto-find if null
    [SerializeField] Transform defaultSpawn; // optional fallback

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find the player in the newly loaded scene if needed
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            var found = FindPlayerTransform();
            if (found != null) player = found;
        }
        if (player == null) return; // no player found; nothing to place


        if (GameState.I == null)
        {
            if (defaultSpawn != null) player.position = defaultSpawn.position;
            return;
        }

        // Use requested SpawnPoint
        if (!string.IsNullOrEmpty(GameState.I.NextSpawnId))
        {
            var target = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None)
                         .FirstOrDefault(s => s.Id == GameState.I.NextSpawnId);
            if (target != null)
            {
                player.position = target.transform.position;
                GameState.I.NextSpawnId = null; // consume it
                return;
            }
        }

        // Else resume last position in this scene (if remembered)
        if (GameState.I.SceneMem.TryGetValue(scene.name, out var mem) && mem.hasLastPosition)
        {
            player.position = mem.lastPosition;
            return;
        }

        // Else Fallback
        if (defaultSpawn != null)
            player.position = defaultSpawn.position;
    }

    // A scene can contain more than one object tagged "Player" (e.g. a trigger volume that was
    // mis-tagged in the editor), and FindGameObjectWithTag returns an arbitrary one of them -
    // which would teleport that object to the spawn point instead of the player. Prefer the
    // one that actually carries a PlayerController.
    private Transform FindPlayerTransform()
    {
        var controller = FindFirstObjectByType<PlayerController>();
        if (controller != null) return controller.transform;

        var tagged = GameObject.FindGameObjectWithTag("Player");
        return tagged != null ? tagged.transform : null;
    }
}
