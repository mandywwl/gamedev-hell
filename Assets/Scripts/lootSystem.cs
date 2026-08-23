using System.Collections.Generic;
using UnityEngine;

public class LootSystem : MonoBehaviour
{
    public static LootSystem Instance { get; private set; }

    [Header("Loot Table")]
    [Tooltip("Possible items a chest can give. One entry is picked at random, weighted by 'weight'.")]
    public List<LootEntry> lootTable = new List<LootEntry>();

    [Tooltip("Relative chance of getting nothing at all, on the same scale as each entry's weight. " +
        "E.g. with 3 items at weight 1 each, set this to 1 for a 1-in-4 chance of an empty chest. Leave at 0 to always give something.")]
    public float nothingWeight = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Rolls one random entry from the loot table and adds it to the player's inventory.
    /// Returns the item and quantity given (item is null if the chest rolled empty).
    /// </summary>
    public (Item item, int quantity) GiveRandomLoot()
    {
        if (!TryPickWeightedEntry(out LootEntry chosen))
        {
            Debug.LogWarning("LootSystem: Loot table is empty and nothingWeight is 0 — nothing to roll.");
            return (null, 0);
        }

        if (chosen == null)
        {
            Debug.Log("Chest rolled empty.");
            return (null, 0);
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("LootSystem: No InventorySystem in scene — cannot give loot.");
            return (null, 0);
        }

        int quantity = Random.Range(chosen.minQuantity, chosen.maxQuantity + 1);
        InventorySystem.Instance.AddItem(chosen.item, quantity);

        Debug.Log($"Gave player {quantity}x {chosen.item.itemName}.");
        return (chosen.item, quantity);
    }

    // Returns false only when there's nothing configured to roll at all (empty table and no
    // nothingWeight). Returns true with result == null when "give nothing" is deliberately rolled.
    private bool TryPickWeightedEntry(out LootEntry result)
    {
        float totalWeight = Mathf.Max(0f, nothingWeight);
        foreach (var entry in lootTable)
            if (entry != null && entry.item != null) totalWeight += Mathf.Max(0f, entry.weight);

        if (totalWeight <= 0f) { result = null; return false; }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = Mathf.Max(0f, nothingWeight);
        if (roll <= cumulative) { result = null; return true; }

        foreach (var entry in lootTable)
        {
            if (entry == null || entry.item == null) continue;
            cumulative += Mathf.Max(0f, entry.weight);
            if (roll <= cumulative) { result = entry; return true; }
        }

        result = null;
        return true;
    }
}

[System.Serializable]
public class LootEntry
{
    public Item item;
    public int minQuantity = 1;
    public int maxQuantity = 1;
    [Tooltip("Relative chance of this item being picked. Higher = more common.")]
    public float weight = 1f;
}
