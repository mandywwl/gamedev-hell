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

    // Combat/RPG stats
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
}