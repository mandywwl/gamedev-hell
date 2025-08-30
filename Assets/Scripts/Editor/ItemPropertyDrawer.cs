#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Item))]
public class ItemPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // Calculate rects
        Rect buttonRect = new Rect(position.x, position.y, position.width, 20);
        Rect propertyRect = new Rect(position.x, position.y + 25, position.width, position.height - 25);
        
        // Add "Apply Defaults" button
        if (GUI.Button(buttonRect, "Apply Default Template"))
        {
            ApplyItemDefaults(property);
        }
        
        // Draw default property field
        EditorGUI.PropertyField(propertyRect, property, label, true);
        
        EditorGUI.EndProperty();
    }
    
    private void ApplyItemDefaults(SerializedProperty property)
    {
        // Get the ItemType from the property
        SerializedProperty typeProperty = property.FindPropertyRelative("type");
        SerializedProperty categoryProperty = property.FindPropertyRelative("category");
        
        if (typeProperty != null && categoryProperty != null)
        {
            ItemType itemType = (ItemType)typeProperty.enumValueIndex;
            ItemCategory category = (ItemCategory)categoryProperty.enumValueIndex;
            
            // Create temporary item using ScriptableObject.CreateInstance
            Item tempItem = ScriptableObject.CreateInstance<Item>();
            tempItem.id = 0;
            tempItem.itemName = "";
            tempItem.description = "";
            tempItem.category = category;
            tempItem.type = itemType;
            
            // Apply the default template (this calls the same logic as Item.ApplyDefaultTemplate())
            ApplyDefaultTemplateToItem(tempItem);
            
            // Apply defaults to serialized properties
            ApplyDefaultsToProperty(property, tempItem);
            
            // Mark object as dirty so Unity saves changes
            property.serializedObject.ApplyModifiedProperties();
            
            Debug.Log($"Applied defaults for {itemType}: Damage={tempItem.attackPower}, Ammo={tempItem.requiredAmmoType}, Weight={tempItem.weight}kg");
        }
    }
    
    private void ApplyDefaultTemplateToItem(Item item)
    {
        // Set basic info based on type
        item.itemName = item.type.ToString().Replace("_", " ");
        
        // Set durability for weapons and armor
        if (item.category == ItemCategory.Weapons || item.category == ItemCategory.Armor)
        {
            item.hasDurability = true;
            item.currentDurability = item.maxDurability;
        }

        switch (item.type)
        {
            // === ASSAULT RIFLES ===
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

            case ItemType.M16:
                item.attackPower = 28;
                item.requiredAmmoType = ItemType.Ammo_556x45_NATO;
                item.magazineSize = 30;
                item.fireRate = 4f;
                item.range = 55f;
                item.weight = 3.8f;
                item.sellPrice = 700;
                item.buyPrice = 1400;
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

            // === CONSUMABLES ===
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

            case ItemType.MedicalSerum:
                item.isConsumable = true;
                item.hpRestore = 75;
                item.sanityRestore = 0;
                item.healingAmount = 75;
                item.maxStackSize = 10;
                item.weight = 0.3f;
                item.sellPrice = 25;
                item.buyPrice = 75;
                item.isUnique = true;
                item.maxPerRun = 5;
                item.rarity = ItemRarity.ExpeditionGrade;
                break;

            // === AMMUNITION ===
            case ItemType.Ammo_556x45_NATO:
                item.maxStackSize = 999;
                item.weight = 0.1f;
                item.sellPrice = 1;
                item.buyPrice = 3;
                item.rarity = ItemRarity.Standard;
                break;

            case ItemType.Ammo_762x51_NATO:
                item.maxStackSize = 999;
                item.weight = 0.15f;
                item.sellPrice = 2;
                item.buyPrice = 4;
                item.rarity = ItemRarity.Standard;
                break;

            case ItemType.Arrows:
                item.maxStackSize = 99;
                item.weight = 0.05f;
                item.sellPrice = 1;
                item.buyPrice = 2;
                item.rarity = ItemRarity.Standard;
                break;

            // Add more item types as needed...

            default:
                // Keep current values for unknown types
                break;
        }
    }
    
    private void ApplyDefaultsToProperty(SerializedProperty property, Item defaultItem)
    {
        // Basic properties
        property.FindPropertyRelative("itemName").stringValue = defaultItem.itemName;
        property.FindPropertyRelative("maxStackSize").intValue = defaultItem.maxStackSize;
        property.FindPropertyRelative("sellPrice").intValue = defaultItem.sellPrice;
        property.FindPropertyRelative("buyPrice").intValue = defaultItem.buyPrice;
        property.FindPropertyRelative("isConsumable").boolValue = defaultItem.isConsumable;
        
        // Combat stats
        property.FindPropertyRelative("attackPower").intValue = defaultItem.attackPower;
        property.FindPropertyRelative("defensePower").intValue = defaultItem.defensePower;
        property.FindPropertyRelative("hpRestore").intValue = defaultItem.hpRestore;
        property.FindPropertyRelative("sanityRestore").intValue = defaultItem.sanityRestore;
        
        // Durability
        property.FindPropertyRelative("hasDurability").boolValue = defaultItem.hasDurability;
        property.FindPropertyRelative("maxDurability").floatValue = defaultItem.maxDurability;
        property.FindPropertyRelative("currentDurability").floatValue = defaultItem.currentDurability;
        
        // Weapon properties
        property.FindPropertyRelative("requiredAmmoType").enumValueIndex = (int)defaultItem.requiredAmmoType;
        property.FindPropertyRelative("magazineSize").intValue = defaultItem.magazineSize;
        property.FindPropertyRelative("fireRate").floatValue = defaultItem.fireRate;
        property.FindPropertyRelative("range").floatValue = defaultItem.range;
        
        // Consumable properties
        property.FindPropertyRelative("healingAmount").floatValue = defaultItem.healingAmount;
        property.FindPropertyRelative("sanityAmount").floatValue = defaultItem.sanityAmount;
        property.FindPropertyRelative("isInstantUse").boolValue = defaultItem.isInstantUse;
        
        // Procedural properties
        property.FindPropertyRelative("rarity").enumValueIndex = (int)defaultItem.rarity;
        property.FindPropertyRelative("weight").floatValue = defaultItem.weight;
        property.FindPropertyRelative("isUnique").boolValue = defaultItem.isUnique;
        property.FindPropertyRelative("maxPerRun").intValue = defaultItem.maxPerRun;
        
        // Run variation
        property.FindPropertyRelative("canBeModified").boolValue = defaultItem.canBeModified;
        property.FindPropertyRelative("minStatModifier").floatValue = defaultItem.minStatModifier;
        property.FindPropertyRelative("maxStatModifier").floatValue = defaultItem.maxStatModifier;
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true) + 25;
    }
}
#endif