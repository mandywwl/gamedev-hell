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
            
            // Create temporary item to get defaults
            Item tempItem = new Item(0, "", "", category, itemType);
            
            // Apply defaults to serialized properties
            ApplyDefaultsToProperty(property, tempItem);
            
            // Mark object as dirty so Unity saves changes
            property.serializedObject.ApplyModifiedProperties();
            
            Debug.Log($"Applied defaults for {itemType}: Damage={tempItem.attackPower}, Ammo={tempItem.requiredAmmoType}, Weight={tempItem.weight}kg");
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