using UnityEditor;
using UnityEngine;

// One-click way to get real Item assets to drag into LootSystem.lootTable / InventorySystem.startingItems
// without hand-filling every stat field - each one is built from Item's own default template.
public static class ItemAssetCreator
{
    private const string OutputFolder = "Assets/Items";

    private static readonly ItemType[] StarterConsumables =
    {
        ItemType.Bandages,
        ItemType.Candy,
        ItemType.MedicalSerum,
        ItemType.SanityPills,
        ItemType.AnomalyPills,
    };

    [MenuItem("Tools/Loot/Create Starter Item Assets")]
    private static void CreateStarterItems()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets", "Items");

        int created = 0;
        foreach (var type in StarterConsumables)
        {
            if (CreateItemAsset(type, ItemCategory.Consumables))
                created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"ItemAssetCreator: created {created} item asset(s) in {OutputFolder} (existing ones were left untouched).");
    }

    private static bool CreateItemAsset(ItemType type, ItemCategory category)
    {
        string path = $"{OutputFolder}/{type}.asset";
        if (AssetDatabase.LoadAssetAtPath<Item>(path) != null) return false; // don't overwrite existing edits

        var item = ScriptableObject.CreateInstance<Item>();
        item.category = category;
        item.type = type;
        item.ApplyDefaultTemplate();

        AssetDatabase.CreateAsset(item, path);
        return true;
    }
}
