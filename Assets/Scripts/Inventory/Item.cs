using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public int id;
    public string itemName;
    public string description;
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
    public int sanityRestore = 0;

    // Icon for UI representation
    [Header("Visual")]
    public Sprite icon;

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
    public float weight = 1f;
    public bool isUnique = false;
    public int maxPerRun = -1;

    // Run variation properties
    [Header("Run Variation")]
    public bool canBeModified = true;
    public float minStatModifier = 0.8f;
    public float maxStatModifier = 1.3f;

    [SerializeField, HideInInspector] private ItemType lastAppliedType;

    void OnEnable()
    {
        // Apply default templates when the asset is created/loaded
        if (string.IsNullOrEmpty(itemName))
        {
            ApplyDefaultTemplate();
            lastAppliedType = type;
        }
    }

    // Re-apply the template whenever `type` is changed in the Inspector, so picking a new
    // type from the dropdown is enough to get that type's name/stats - OnEnable's empty-name
    // check only catches the very first assignment, not later changes.
    void OnValidate()
    {
        if (type != lastAppliedType)
        {
            ApplyDefaultTemplate();
            lastAppliedType = type;
        }
    }

    [ContextMenu("Apply Default Template")]
    private void ApplyDefaultTemplateFromMenu()
    {
        ApplyDefaultTemplate();
        lastAppliedType = type;
    }

    // Apply realistic defaults for each weapon/item type
    public void ApplyDefaultTemplate()
    {
        // id is derived from type so every item gets a unique id automatically - the
        // inventory stacks/matches items by id, so two items sharing one id get merged.
        id = (int)type;

        // Set basic info based on type
        itemName = type.ToString().Replace("_", " ");

        // Set durability for weapons and armor
        if (category == ItemCategory.Weapons || category == ItemCategory.Armor)
        {
            hasDurability = true;
            currentDurability = maxDurability;
        }

        switch (type)
        {
            // === ASSAULT RIFLES ===
            case ItemType.M4A1:
                attackPower = 30;
                requiredAmmoType = ItemType.Ammo_556x45_NATO;
                magazineSize = 30;
                fireRate = 3.5f;
                range = 50f;
                weight = 4f;
                sellPrice = 750;
                buyPrice = 1500;
                description = "A reliable assault rifle chambered in 5.56x45mm NATO.";
                break;

            case ItemType.FN_SCAR_MK17:
                attackPower = 45;
                requiredAmmoType = ItemType.Ammo_762x51_NATO;
                magazineSize = 20;
                fireRate = 2.8f;
                range = 60f;
                weight = 6f;
                sellPrice = 1125;
                buyPrice = 2250;
                rarity = ItemRarity.ExpeditionGrade;
                maxPerRun = 2;
                description = "A hard-hitting battle rifle chambered in 7.62x51mm NATO.";
                break;

            case ItemType.M16:
                attackPower = 28;
                requiredAmmoType = ItemType.Ammo_556x45_NATO;
                magazineSize = 30;
                fireRate = 4f;
                range = 55f;
                weight = 3.8f;
                sellPrice = 700;
                buyPrice = 1400;
                description = "A lightweight assault rifle with a high rate of fire.";
                break;

            case ItemType.G36_HK:
                attackPower = 32;
                requiredAmmoType = ItemType.Ammo_556x45_NATO;
                magazineSize = 30;
                fireRate = 3.2f;
                range = 48f;
                weight = 4.2f;
                sellPrice = 800;
                buyPrice = 1600;
                rarity = ItemRarity.Standard;
                description = "A rugged assault rifle favored for its reliability.";
                break;

            case ItemType.F1_Famas:
                attackPower = 26;
                requiredAmmoType = ItemType.Ammo_556x45_NATO;
                magazineSize = 25;
                fireRate = 4.5f;
                range = 45f;
                weight = 3.5f;
                sellPrice = 650;
                buyPrice = 1300;
                description = "A bullpup assault rifle with a blistering fire rate.";
                break;

            // === BOW WEAPONS ===
            case ItemType.CompoundBow:
                attackPower = 25;
                requiredAmmoType = ItemType.Arrows;
                magazineSize = 1;
                fireRate = 1.5f;
                range = 40f;
                weight = 3f;
                sellPrice = 625;
                buyPrice = 1250;
                rarity = ItemRarity.Standard;
                description = "A modern compound bow. Silent, but slow to reload.";
                break;

            case ItemType.FlatBow:
                attackPower = 20;
                requiredAmmoType = ItemType.Arrows;
                magazineSize = 1;
                fireRate = 1.8f;
                range = 35f;
                weight = 2.5f;
                sellPrice = 500;
                buyPrice = 1000;
                description = "A simple flatbow. Easy to make, easy to use.";
                break;

            case ItemType.CompoundCrossbow:
                attackPower = 35;
                requiredAmmoType = ItemType.Crossbow_Bolts;
                magazineSize = 1;
                fireRate = 1.2f;
                range = 45f;
                weight = 4.5f;
                sellPrice = 875;
                buyPrice = 1750;
                rarity = ItemRarity.Standard;
                description = "A compound crossbow with serious stopping power.";
                break;

            case ItemType.PistolCrossbow:
                attackPower = 22;
                requiredAmmoType = ItemType.Crossbow_Bolts;
                magazineSize = 1;
                fireRate = 2f;
                range = 25f;
                weight = 2f;
                sellPrice = 550;
                buyPrice = 1100;
                description = "A compact crossbow, easy to carry but weak.";
                break;

            // === MELEE WEAPONS ===
            case ItemType.Fire_Axe:
                attackPower = 40;
                requiredAmmoType = ItemType.Misc; // No ammo needed
                magazineSize = 0;
                fireRate = 1.2f;
                range = 2f;
                weight = 5f;
                sellPrice = 1000;
                buyPrice = 2000;
                description = "A heavy fire axe. Slow but devastating.";
                break;

            case ItemType.Crowbar:
                attackPower = 25;
                requiredAmmoType = ItemType.Misc;
                magazineSize = 0;
                fireRate = 1.8f;
                range = 1.5f;
                weight = 2f;
                sellPrice = 625;
                buyPrice = 1250;
                description = "A trusty crowbar. Doubles as a lockpick in a pinch.";
                break;

            case ItemType.Sledgehammer:
                attackPower = 50;
                requiredAmmoType = ItemType.Misc;
                magazineSize = 0;
                fireRate = 0.8f;
                range = 2.2f;
                weight = 7f;
                sellPrice = 1250;
                buyPrice = 2500;
                rarity = ItemRarity.Standard;
                description = "A massive sledgehammer. Crushes anything it hits.";
                break;

            case ItemType.Spiked_Baseball_Bat:
                attackPower = 30;
                requiredAmmoType = ItemType.Misc;
                magazineSize = 0;
                fireRate = 1.5f;
                range = 1.8f;
                weight = 3f;
                sellPrice = 750;
                buyPrice = 1500;
                description = "A baseball bat wrapped in rusty spikes.";
                break;

            // === AMMUNITION ===
            case ItemType.Ammo_556x45_NATO:
                maxStackSize = 999;
                weight = 0.1f;
                sellPrice = 1;
                buyPrice = 3;
                rarity = ItemRarity.Standard;
                description = "Standard 5.56x45mm NATO rounds.";
                break;

            case ItemType.Ammo_762x51_NATO:
                maxStackSize = 999;
                weight = 0.15f;
                sellPrice = 2;
                buyPrice = 4;
                rarity = ItemRarity.Standard;
                description = "Heavy 7.62x51mm NATO rounds.";
                break;

            case ItemType.Arrows:
                maxStackSize = 99;
                weight = 0.05f;
                sellPrice = 1;
                buyPrice = 2;
                rarity = ItemRarity.Standard;
                description = "Arrows for bow-type weapons.";
                break;

            case ItemType.Crossbow_Bolts:
                maxStackSize = 99;
                weight = 0.08f;
                sellPrice = 2;
                buyPrice = 3;
                rarity = ItemRarity.Standard;
                description = "Bolts for crossbow-type weapons.";
                break;

            // === ARMOR ===
            case ItemType.SurvivorOutfit:
                defensePower = 5;
                weight = 2f;
                sellPrice = 100;
                buyPrice = 200;
                description = "Basic survivor gear. Better than nothing.";
                break;

            case ItemType.BruiserJacket:
                defensePower = 12;
                weight = 4f;
                sellPrice = 240;
                buyPrice = 480;
                rarity = ItemRarity.Standard;
                description = "A reinforced jacket built to take a beating.";
                break;

            case ItemType.AnomalyHoodie:
                defensePower = 8;
                weight = 3f;
                sellPrice = 160;
                buyPrice = 320;
                rarity = ItemRarity.Standard;
                description = "A hoodie treated to resist anomalous exposure.";
                break;

            case ItemType.DreadedWearSuit:
                defensePower = 15;
                weight = 5f;
                sellPrice = 300;
                buyPrice = 600;
                rarity = ItemRarity.ExpeditionGrade;
                description = "A heavily armored suit built for the worst of it.";
                break;

            // === CONSUMABLES ===
            case ItemType.Bandages:
                isConsumable = true;
                hpRestore = 25;
                sanityRestore = 0;
                healingAmount = 25;
                maxStackSize = 20;
                weight = 0.2f;
                sellPrice = 8;
                buyPrice = 25;
                description = "Basic first aid. Restores 25 HP.";
                break;

            case ItemType.MedicalSerum:
                isConsumable = true;
                hpRestore = 75;
                sanityRestore = 0;
                healingAmount = 75;
                maxStackSize = 10;
                weight = 0.3f;
                sellPrice = 25;
                buyPrice = 75;
                isUnique = true;
                maxPerRun = 5;
                rarity = ItemRarity.ExpeditionGrade;
                description = "A potent medical serum. Restores 75 HP.";
                break;

            case ItemType.SanityPills:
                isConsumable = true;
                hpRestore = 0;
                sanityRestore = 50;
                sanityAmount = 50;
                maxStackSize = 15;
                weight = 0.1f;
                sellPrice = 16;
                buyPrice = 50;
                description = "Calms the nerves. Restores 50 sanity.";
                break;

            case ItemType.AnomalyPills:
                isConsumable = true;
                hpRestore = 10;
                sanityRestore = 25;
                healingAmount = 10;
                sanityAmount = 25;
                maxStackSize = 8;
                weight = 0.15f;
                sellPrice = 11;
                buyPrice = 35;
                isUnique = true;
                maxPerRun = 3;
                rarity = ItemRarity.ExpeditionGrade;
                description = "Experimental pills that steady both body and mind. Restores 10 HP and 25 sanity.";
                break;

            case ItemType.Candy:
                isConsumable = true;
                hpRestore = 10;
                sanityRestore = 0;
                healingAmount = 10;
                maxStackSize = 20;
                weight = 0.05f;
                sellPrice = 3;
                buyPrice = 10;
                description = "A sugary snack. Restores 10 HP.";
                break;

            // === KEY ITEMS ===
            case ItemType.Keycard:
                weight = 0.1f;
                sellPrice = 0; // Can't sell key items
                buyPrice = 0;
                isUnique = true;
                maxPerRun = 1;
                rarity = ItemRarity.Standard;
                description = "Grants access to restricted areas.";
                break;

            case ItemType.Map:
                weight = 0.1f;
                sellPrice = 0;
                buyPrice = 0;
                maxPerRun = 2;
                rarity = ItemRarity.Standard;
                description = "A map of the surrounding area.";
                break;

            case ItemType.Key:
                weight = 0.05f;
                sellPrice = 0;
                buyPrice = 0;
                isUnique = true;
                maxPerRun = 1;
                rarity = ItemRarity.Standard;
                description = "Opens a specific lock somewhere nearby.";
                break;

            case ItemType.Passport:
                weight = 0.05f;
                sellPrice = 0;
                buyPrice = 0;
                isUnique = true;
                maxPerRun = 1;
                rarity = ItemRarity.Standard;
                description = "An old passport. Might prove useful for identification.";
                break;

            case ItemType.ID_Card:
                weight = 0.05f;
                sellPrice = 0;
                buyPrice = 0;
                isUnique = true;
                maxPerRun = 1;
                rarity = ItemRarity.Standard;
                description = "An identification card belonging to someone else.";
                break;

            // === LORE & CRAFTING ===
            case ItemType.ExpeditionLog:
                weight = 0.1f;
                sellPrice = 0;
                buyPrice = 0;
                maxPerRun = 3;
                rarity = ItemRarity.Standard;
                description = "A logbook detailing a previous expedition.";
                break;

            case ItemType.AbandonedChecklist:
                weight = 0.05f;
                sellPrice = 0;
                buyPrice = 0;
                maxPerRun = 2;
                rarity = ItemRarity.Standard;
                description = "A checklist left behind by whoever was here before.";
                break;

            case ItemType.CraftingMaterial:
                maxStackSize = 50;
                weight = 0.3f;
                sellPrice = 2;
                buyPrice = 6;
                rarity = ItemRarity.Standard;
                description = "Raw material used for crafting.";
                break;

            case ItemType.Misc:
                weight = 0.5f;
                sellPrice = 1;
                buyPrice = 2;
                description = "An unidentified item.";
                break;

            // === DEFAULT ===
            default:
                // Keep current values for unknown types
                break;
        }
    }

    // Validation method to check if item setup is correct
    public bool ValidateSetup()
    {
        // Check weapon ammo compatibility
        if (category == ItemCategory.Weapons && requiredAmmoType == ItemType.Misc &&
            (type == ItemType.M4A1 || type == ItemType.FN_SCAR_MK17 || type == ItemType.CompoundBow))
        {
            Debug.LogWarning($"{itemName} is a ranged weapon but has no ammo type set!");
            return false;
        }

        // Check if consumables have effects
        if (isConsumable && hpRestore == 0 && sanityRestore == 0)
        {
            Debug.LogWarning($" {itemName} is consumable but has no effects!");
            return false;
        }

        return true;
    }

    // Rest of your existing methods remain the same...
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

    public bool IsUsableInCombat()
    {
        return isConsumable || (category == ItemCategory.Weapons && !IsBroken());
    }


    public Item CreateModifiedCopy(float statModifier = 1f, string suffix = "")
    {
        if (!canBeModified) return this;

        // Create a new instance (not asset) for runtime modifications
        Item modifiedItem = CreateInstance<Item>();

        // Copy all base properties
        modifiedItem.id = id;
        modifiedItem.itemName = itemName + suffix;
        modifiedItem.description = description;
        modifiedItem.category = category;
        modifiedItem.type = type;
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
    Candy,

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

        // Example: 10x Bandages = 0.2f � 10 = 2.0f total weight
    }
}