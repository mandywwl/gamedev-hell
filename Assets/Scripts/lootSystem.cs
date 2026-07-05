using UnityEngine;

public class LootSystem : MonoBehaviour
{
    public static LootSystem Instance { get; private set; }

    [Header("Only thing we care about")]
    [Tooltip("Assign your Medkit (Medical Serum) Item asset here")]
    public Item medkit;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Give the player N medkits and update the on-screen counter (if present).
    /// </summary>
    public void GiveMedkitToPlayer(int quantity = 1)
    {
        if (medkit == null)
        {
            Debug.LogWarning("LootSystem: No medkit Item assigned.");
            return;
        }

        // Update the HUD counter if the script exists in scene
        var counter = FindObjectOfType<MedkitCounterUI>();
        if (counter != null) counter.AddMedkit(quantity);

        Debug.Log($"Gave player {quantity} medkit(s).");
    }
}
