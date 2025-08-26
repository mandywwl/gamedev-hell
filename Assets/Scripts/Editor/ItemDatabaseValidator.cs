#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ItemDatabaseValidator : EditorWindow
{
    private LootSystem lootSystem;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Item Database Validator")]
    public static void ShowWindow()
    {
        GetWindow<ItemDatabaseValidator>("Item Database Validator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Item Database Validation", EditorStyles.boldLabel);
        
        lootSystem = (LootSystem)EditorGUILayout.ObjectField("Loot System", lootSystem, typeof(LootSystem), true);
        
        if (lootSystem == null)
        {
            EditorGUILayout.HelpBox("Please assign a LootSystem from your scene to validate the item database.", MessageType.Info);
            return;
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Validate All Items"))
        {
            ValidateItemDatabase();
        }
        
        if (GUILayout.Button("Fix All Items with Defaults"))
        {
            FixAllItemsWithDefaults();
        }
        
        EditorGUILayout.Space();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (lootSystem.itemDatabase != null)
        {
            foreach (Item item in lootSystem.itemDatabase)
            {
                if (item != null)
                {
                    DrawItemValidation(item);
                }
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    void DrawItemValidation(Item item)
    {
        EditorGUILayout.BeginVertical("box");
        
        bool isValid = ValidateItem(item);
        Color originalColor = GUI.color;
        GUI.color = isValid ? Color.green : Color.red;
        
        EditorGUILayout.LabelField($"{item.itemName} ({item.type})", EditorStyles.boldLabel);
        GUI.color = originalColor;
        
        // Show key properties
        EditorGUILayout.LabelField($"Category: {item.category} | Rarity: {item.rarity}");
        
        if (item.category == ItemCategory.Weapons)
        {
            EditorGUILayout.LabelField($"Damage: {item.attackPower} | Ammo: {item.requiredAmmoType} | Weight: {item.weight}kg");
            
            if (item.requiredAmmoType == ItemType.Misc && IsRangedWeapon(item.type))
            {
                EditorGUILayout.HelpBox("Ranged weapon has no ammo type!", MessageType.Warning);
            }
        }
        else if (item.category == ItemCategory.Consumables)
        {
            EditorGUILayout.LabelField($"HP: +{item.hpRestore} | Sanity: +{item.sanityRestore} | Weight: {item.weight}kg");
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    bool ValidateItem(Item item)
    {
        // Check weapon ammo compatibility
        if (item.category == ItemCategory.Weapons && item.requiredAmmoType == ItemType.Misc && IsRangedWeapon(item.type))
        {
            return false;
        }
        
        // Check if consumables have effects
        if (item.isConsumable && item.hpRestore == 0 && item.sanityRestore == 0)
        {
            return false;
        }
        
        // Check if attack power is set for weapons
        if (item.category == ItemCategory.Weapons && item.attackPower == 0)
        {
            return false;
        }
        
        return true;
    }
    
    bool IsRangedWeapon(ItemType type)
    {
        return type == ItemType.M4A1 || type == ItemType.FN_SCAR_MK17 || type == ItemType.CompoundBow || 
               type == ItemType.M16 || type == ItemType.G36_HK || type == ItemType.F1_Famas ||
               type == ItemType.FlatBow || type == ItemType.CompoundCrossbow || type == ItemType.PistolCrossbow;
    }
    
    void ValidateItemDatabase()
    {
        if (lootSystem.itemDatabase == null) return;
        
        int validItems = 0;
        int totalItems = lootSystem.itemDatabase.Length;
        
        foreach (Item item in lootSystem.itemDatabase)
        {
            if (item != null && ValidateItem(item))
            {
                validItems++;
            }
        }
        
        Debug.Log($" Validation Complete: {validItems}/{totalItems} items are properly configured");
        
        if (validItems == totalItems)
        {
            Debug.Log("All items passed validation!");
        }
        else
        {
            Debug.LogWarning($"{totalItems - validItems} items need attention. Check the validator window for details.");
        }
    }
    
    void FixAllItemsWithDefaults()
    {
        if (lootSystem.itemDatabase == null) return;
        
        int fixedCount = 0;
        
        foreach (Item item in lootSystem.itemDatabase)
        {
            if (item != null && !ValidateItem(item))
            {
                // Create a new item with defaults and copy properties
                Item defaultItem = new Item(item.id, item.itemName, item.description, item.category, item.type);
                CopyDefaults(defaultItem, item);
                fixedCount++;
            }
        }
        
        EditorUtility.SetDirty(lootSystem);
        Debug.Log($"Fixed {fixedCount} items with default templates");
    }
    
    void CopyDefaults(Item source, Item target)
    {
        target.attackPower = source.attackPower;
        target.requiredAmmoType = source.requiredAmmoType;
        target.magazineSize = source.magazineSize;
        target.fireRate = source.fireRate;
        target.range = source.range;
        target.weight = source.weight;
        target.sellPrice = source.sellPrice;
        target.buyPrice = source.buyPrice;
        target.rarity = source.rarity;
        target.isUnique = source.isUnique;
        target.maxPerRun = source.maxPerRun;
        target.hpRestore = source.hpRestore;
        target.sanityRestore = source.sanityRestore;
        target.healingAmount = source.healingAmount;
        target.sanityAmount = source.sanityAmount;
        target.maxStackSize = source.maxStackSize;
    }
}
#endif