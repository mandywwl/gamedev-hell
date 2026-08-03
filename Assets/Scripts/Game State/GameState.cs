using UnityEngine;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public static GameState I;
    
    [System.Serializable]
    public class SceneMemory
    {
        public Vector3 lastPosition;
        public bool hasLastPosition;
        public string lastFacing; // optional
    }

    // sceneName -> memory; for storing player position and state
    public Dictionary<string, SceneMemory> SceneMem = new();

    // Set by portal used to enter the next scene
    [HideInInspector]
    public string NextSpawnId = null;

    // Track defeated enemies
    public HashSet<string> defeatedEnemies = new();

    // Track current play mode
    public enum PlayMode { Exploration, Combat }
    public PlayMode CurrentMode = PlayMode.Exploration;

    
    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }
}
