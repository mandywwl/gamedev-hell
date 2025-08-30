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
                // Create a new item with defaults using ScriptableObject.CreateInstance
                Item defaultItem = ScriptableObject.CreateInstance<Item>();
                defaultItem.id = item.id;
                defaultItem.itemName = item.itemName;
                defaultItem.description = item.description;
                defaultItem.category = item.category;
                defaultItem.type = item.type;
                
                // Apply defaults by calling the template method
                ApplyDefaultsToItem(defaultItem);
                
                // Copy defaults back to the original item
                CopyDefaults(defaultItem, item);
                fixedCount++;
                
                // Clean up the temporary item
                DestroyImmediate(defaultItem);
            }
        }
        
        EditorUtility.SetDirty(lootSystem);
        Debug.Log($"Fixed {fixedCount} items with default templates");
    }
    
    void ApplyDefaultsToItem(Item item)
    {
        // Set basic info based on type
        item.itemName = item.type.ToString().Replace("_", " ");
        
        // Set durability for weapons and armor
        if (item.category == ItemCategory.Weapons || item.category == ItemCategory.Armor)
        {
            item.hasDurability = true;
            item.currentDurability = item.maxDurability;
        }

        // Apply the same defaults logic from Item.ApplyDefaultTemplate()
        switch (item.type)
        {
            case ItemType.M4A1:
                item.attackPower = 30;
                item.requiredAmmoType = ItemType.Ammo_556x45_NATO;
                item.magazineSize = 30;
                item.fireRate = 3.5f;
                item.range = 50f;
                item.weight = 4f;
                item.sellPrice = 750;
                item.buyPrice = 1500;
                break;
            case ItemType.FN_SCAR_MK17:
                item.attackPower = 45;
                item.requiredAmmoType = ItemType.Ammo_762x51_NATO;
                item.magazineSize = 20;
                item.fireRate = 2.8f;
                item.range = 60f;
                item.weight = 6f;
                item.sellPrice = 1125;
                item.buyPrice = 2250;
                item.rarity = ItemRarity.ExpeditionGrade;
                item.maxPerRun = 2;
                break;
            case ItemType.CompoundBow:
                item.attackPower = 25;
                item.requiredAmmoType = ItemType.Arrows;
                item.magazineSize = 1;
                item.fireRate = 1.5f;
                item.range = 40f;
                item.weight = 3f;
                item.sellPrice = 625;
                item.buyPrice = 1250;
                item.rarity = ItemRarity.Standard;
                break;
            case ItemType.Bandages:
                item.isConsumable = true;
                item.hpRestore = 25;
                item.sanityRestore = 0;
                item.healingAmount = 25;
                item.maxStackSize = 20;
                item.weight = 0.2f;
                item.sellPrice = 8;
                item.buyPrice = 25;
                break;
            // Add more cases as needed
            default:
                // Keep current values for unknown types
                break;
        }
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