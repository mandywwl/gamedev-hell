using UnityEngine;

[System.Serializable]
public class Item
{
    public int id;
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemCategory category;
    public ItemType type;
    public int maxStackSize = 1;
    public int sellPrice;
    public int buyPrice;
    public bool isConsumable;

    // Combat stats
    public int attackPower = 0;
    public int defensePower = 0;
    public int hpRestore = 0;
    public int sanityRestore = 0; // For SanityPills/AnomalyPills

    // Durability system for weapons/armor
    [Header("Durability")]
    public bool hasDurability = false;
    public float maxDurability = 100f;
    public float currentDurability = 100f;

    // Weapon-specific properties
    [Header("Weapon Properties")]
    public ItemType requiredAmmoType = ItemType.Misc;
    public int magazineSize = 0;
    public float fireRate = 1f;
    public float range = 10f;

    // Consumable properties
    [Header("Consumable Properties")]
    public float healingAmount = 0f;
    public float sanityAmount = 0f;
    public bool isInstantUse = true;

    // Procedural Generation Properties
    [Header("Procedural Properties")]
    public ItemRarity rarity = ItemRarity.Makeshift;
    public float weight = 1f; // Weight for inventory limits
    public bool isUnique = false; // Only one can exist per run
    public int maxPerRun = -1; // -1 = unlimited, otherwise max amount per run
    
    // Run variation properties
    [Header("Run Variation")]
    public bool canBeModified = true; // Can stats be randomly modified?
    public float minStatModifier = 0.8f; // Minimum stat multiplier
    public float maxStatModifier = 1.3f; // Maximum stat multiplier

    public Item(int id, string name, string description, ItemCategory category, ItemType type = ItemType.Misc)
    {
        this.id = id;
        this.itemName = name;
        this.description = description;
        this.category = category;
        this.type = type;

        // Set durability for weapons and armor
        if (category == ItemCategory.Weapons || category == ItemCategory.Armor)
        {
            hasDurability = true;
            currentDurability = maxDurability;
        }
    }

    // Durability methods
    public float GetDurabilityPercentage()
    {
        if (!hasDurability) return 100f;
        return (currentDurability / maxDurability) * 100f;
    }

    public void UseDurability(float amount)
    {
        if (hasDurability)
        {
            currentDurability = Mathf.Max(0f, currentDurability - amount);
        }
    }

    public void RepairDurability(float amount)
    {
        if (hasDurability)
        {
            currentDurability = Mathf.Min(maxDurability, currentDurability + amount);
        }
    }

    public bool IsBroken()
    {
        return hasDurability && currentDurability <= 0f;
    }

    public bool CanUseAmmo(ItemType ammoType)
    {
        return requiredAmmoType == ammoType;
    }

    // Combat integration helpers
    public bool IsUsableInCombat()
    {
        return isConsumable || (category == ItemCategory.Weapons && !IsBroken());
    }

    public string GetCombatDescription()
    {
        if (isConsumable)
        {
            return $"Restores {hpRestore} HP, {sanityRestore} Sanity";
        }
        else if (category == ItemCategory.Weapons)
        {
            string ammoInfo = requiredAmmoType != ItemType.Misc ? $" (Uses {requiredAmmoType})" : "";
            return $"Damage: {attackPower}{ammoInfo}, Durability: {GetDurabilityPercentage():F1}%";
        }
        return description;
    }

    // Helper for turn-based combat action cost
    public int GetActionCost()
    {
        // This can be expanded later when combat system defines action costs
        if (category == ItemCategory.Weapons)
            return 1; // Attacking costs 1 action
        else if (isConsumable)
            return 1; // Using items costs 1 action

        return 0;
    }

    // Procedural modification methods
    public Item CreateModifiedCopy(float statModifier = 1f, string suffix = "")
    {
        if (!canBeModified) return this;

        Item modifiedItem = new Item(id, itemName + suffix, description, category, type);
        
        // Copy all base properties
        modifiedItem.icon = icon;
        modifiedItem.maxStackSize = maxStackSize;
        modifiedItem.sellPrice = Mathf.RoundToInt(sellPrice * statModifier);
        modifiedItem.buyPrice = Mathf.RoundToInt(buyPrice * statModifier);
        modifiedItem.isConsumable = isConsumable;
        modifiedItem.rarity = rarity;
        modifiedItem.weight = weight;
        modifiedItem.isUnique = isUnique;
        modifiedItem.maxPerRun = maxPerRun;
        
        // Apply stat modifications
        modifiedItem.attackPower = Mathf.RoundToInt(attackPower * statModifier);
        modifiedItem.defensePower = Mathf.RoundToInt(defensePower * statModifier);
        modifiedItem.hpRestore = Mathf.RoundToInt(hpRestore * statModifier);
        modifiedItem.sanityRestore = Mathf.RoundToInt(sanityRestore * statModifier);
        
        modifiedItem.hasDurability = hasDurability;
        modifiedItem.maxDurability = maxDurability * statModifier;
        modifiedItem.currentDurability = modifiedItem.maxDurability;
        
        modifiedItem.requiredAmmoType = requiredAmmoType;
        modifiedItem.magazineSize = Mathf.RoundToInt(magazineSize * statModifier);
        modifiedItem.fireRate = fireRate * statModifier;
        modifiedItem.range = range * statModifier;
        
        modifiedItem.healingAmount = healingAmount * statModifier;
        modifiedItem.sanityAmount = sanityAmount * statModifier;
        modifiedItem.isInstantUse = isInstantUse;
        
        return modifiedItem;
    }

    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Makeshift: return Color.white;
            case ItemRarity.Standard: return Color.green;
            case ItemRarity.ExpeditionGrade: return Color.blue;
            case ItemRarity.Prototype: return Color.magenta;
            case ItemRarity.Anomalous: return Color.yellow;
            default: return Color.white;
        }
    }
}

public enum ItemCategory
{
    Weapons,
    Armor,
    Consumables,
    KeyItems,
    Materials,
    Accessories
}

// Item Rarity System
public enum ItemRarity
{
    Makeshift,     // 60% chance
    Standard,   // 25% chance  
    ExpeditionGrade,       // 10% chance
    Prototype,       // 4% chance
    Anomalous   // 1% chance
}

public enum ItemType
{
    // Use underscore for special characters


    // -- Ranged Weapons --
    // Assault Rifles
    M4A1,
    M16,
    FN_SCAR_MK17, 
    G36_HK,
    F1_Famas,

    // Ranged Weapons
    CompoundBow,
    FlatBow,
    CompoundCrossbow,
    PistolCrossbow,

    //Melee Weapons
    Spiked_Baseball_Bat,
    Crowbar,
    Fire_Axe,
    Sledgehammer,

    // -- Ammunition --
    // Standardized ammo makes inventory management more strategic.
    // Players have to match ammo to the right weapon.
    // Might change ammo type to be more generic later?
    Ammo_556x45_NATO,   // For M4A1, M16, G36, Famas
    Ammo_762x51_NATO,   // For FN SCAR-MK17
    Arrows,             // For Bows
    Crossbow_Bolts,     // For Crossbows

    // -- Armor / Outfits --
    // (These could provide status bonuses later. Now its just generic)
    SurvivorOutfit,     // Default
    DreadedWearSuit,
    AnomalyHoodie,
    BruiserJacket,

    // -- Consumables --
    Bandages,
    MedicalSerum,
    SanityPills,
    AnomalyPills,

    // -- Key & Quest Items --
    Keycard,
    Key,
    Passport,
    ID_Card,
    Map,

    // -- Lore & Crafting --
    ExpeditionLog,
    AbandonedChecklist,
    CraftingMaterial,
    Misc // Generic catch-all for other items
}

[System.Serializable]
public class ItemStack
{
    public Item item;
    public int quantity;

    public ItemStack(Item item, int quantity = 1)
    {
        this.item = item;
        this.quantity = Mathf.Clamp(quantity, 0, item.maxStackSize);
    }

    public bool CanAddItems(int amount)
    {
        return quantity + amount <= item.maxStackSize;
    }

    public void AddItems(int amount)
    {
        quantity = Mathf.Clamp(quantity + amount, 0, item.maxStackSize);
    }

    public void RemoveItems(int amount)
    {
        quantity = Mathf.Max(0, quantity - amount);
    }

    public bool IsEmpty()
    {
        return quantity <= 0;
    }

    // Weight calculation
    public float GetTotalWeight()
    {
        return item.weight * quantity;

        // Example: 10x Bandages = 0.2f × 10 = 2.0f total weight
    }
}