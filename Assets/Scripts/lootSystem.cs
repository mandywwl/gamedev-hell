using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class LootSystem : MonoBehaviour
{
    public static LootSystem Instance { get; private set; }

    [Header("Loot Settings")]
    public LootTable[] lootTables;

    [Header("Item Database")]
    public Item[] itemDatabase;

    [Header("Loot Drop Settings")]
    public GameObject lootDropPrefab; // Assign a simple GameObject with SpriteRenderer
    public float lootPickupRange = 2f;
    public LayerMask playerLayer = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CreateSampleItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DropLoot(string lootTableName, Vector3 dropPosition)
    {
        LootTable table = System.Array.Find(lootTables, lt => lt.tableName == lootTableName);
        if (table == null)
        {
            Debug.LogWarning($"Loot table '{lootTableName}' not found!");
            return;
        }

        foreach (var drop in table.lootDrops)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= drop.dropChance)
            {
                Item item = System.Array.Find(itemDatabase, i => i.id == drop.itemId);
                if (item != null)
                {
                    int quantity = Random.Range(drop.minQuantity, drop.maxQuantity + 1);

                    // For weapons with durability, randomize condition
                    if (item.hasDurability)
                    {
                        Item droppedItem = CreateItemCopy(item);
                        droppedItem.currentDurability = Random.Range(drop.minDurability, drop.maxDurability);
                        CreateLootDrop(droppedItem, quantity, dropPosition);
                    }
                    else
                    {
                        CreateLootDrop(item, quantity, dropPosition);
                    }
                }
            }
        }
    }

    public void DropSpecificItem(Item item, int quantity, Vector3 dropPosition, float durabilityPercentage = 100f)
    {
        if (item.hasDurability)
        {
            Item droppedItem = CreateItemCopy(item);
            droppedItem.currentDurability = (durabilityPercentage / 100f) * item.maxDurability;
            CreateLootDrop(droppedItem, quantity, dropPosition);
        }
        else
        {
            CreateLootDrop(item, quantity, dropPosition);
        }
    }

    private void CreateLootDrop(Item item, int quantity, Vector3 position)
    {
        GameObject lootDrop;

        if (lootDropPrefab != null)
        {
            lootDrop = Instantiate(lootDropPrefab, position, Quaternion.identity);
        }
        else
        {
            // Create basic loot drop if no prefab assigned
            lootDrop = new GameObject($"Loot_{item.itemName}");
            lootDrop.transform.position = position;

            SpriteRenderer renderer = lootDrop.AddComponent<SpriteRenderer>();
            renderer.sprite = item.icon;
            renderer.sortingOrder = 10;
        }

        // Use 3D collider instead of 2D for isometric
        if (lootDrop.GetComponent<Collider>() == null)
        {
            SphereCollider collider = lootDrop.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = lootPickupRange;
        }

        LootPickup pickup = lootDrop.GetComponent<LootPickup>();
        if (pickup == null)
        {
            pickup = lootDrop.AddComponent<LootPickup>();
        }

        pickup.item = item;
        pickup.quantity = quantity;

        Debug.Log($"Dropped {quantity}x {item.itemName} at {position}" +
                 (item.hasDurability ? $" (Durability: {item.GetDurabilityPercentage():F1}%)" : ""));
    }

    // Add these methods to your existing LootSystem class:

    // Method for combat system to reward items after combat
    public void GiveCombatRewards(string[] lootTableNames)
    {
        Debug.Log("Distributing combat rewards...");

        foreach (string tableName in lootTableNames)
        {
            GiveLootToPlayer(tableName);
        }
    }

    // Method to get item by ID (useful for combat system integration)
    public Item GetItemById(int itemId)
    {
        return System.Array.Find(itemDatabase, item => item.id == itemId);
    }

    // Method to get item by type (useful for combat system integration)
    public Item GetItemByType(ItemType itemType)
    {
        return System.Array.Find(itemDatabase, item => item.type == itemType);
    }

    // Method for combat system to check if specific items exist
    public bool ItemExistsInDatabase(int itemId)
    {
        return GetItemById(itemId) != null;
    }

    // Method to create a damaged weapon for combat rewards
    public void DropDamagedWeapon(ItemType weaponType, Vector3 dropPosition, float durabilityPercentage)
    {
        Item weapon = GetItemByType(weaponType);
        if (weapon != null && weapon.category == ItemCategory.Weapons)
        {
            DropSpecificItem(weapon, 1, dropPosition, durabilityPercentage);
            Debug.Log($"Dropped damaged {weapon.itemName} ({durabilityPercentage:F1}% durability)");
        }
    }

    // Combat loot tables for different enemy types
    private void CreateCombatLootTables()
    {
        // This method can be called to set up combat-specific loot tables
        // These table names in the combat system

        List<LootTable> combatTables = new List<LootTable>();

        // Example: Zombie loot table
        combatTables.Add(new LootTable
        {
            tableName = "ZombieDrop",
            lootDrops = new LootDrop[]
            {
            new LootDrop { itemId = 30, dropChance = 60f, minQuantity = 1, maxQuantity = 3 }, // Bandages
            new LootDrop { itemId = 10, dropChance = 20f, minQuantity = 5, maxQuantity = 15 }, // 5.56 Ammo
            new LootDrop { itemId = 40, dropChance = 10f, minQuantity = 1, maxQuantity = 1 }  // Keycard
            }
        });

        // Example: Military zombie type loot table
        combatTables.Add(new LootTable
        {
            tableName = "MilitaryDrop",
            lootDrops = new LootDrop[]
            {
            new LootDrop { itemId = 1, dropChance = 30f, minQuantity = 1, maxQuantity = 1, minDurability = 40f, maxDurability = 80f }, // M4A1
            new LootDrop { itemId = 10, dropChance = 80f, minQuantity = 15, maxQuantity = 30 }, // 5.56 Ammo
            new LootDrop { itemId = 31, dropChance = 25f, minQuantity = 1, maxQuantity = 2 }   // Medical Serum
            }
        });

        // Add to existing loot tables
        var currentTables = lootTables?.ToList() ?? new List<LootTable>();
        currentTables.AddRange(combatTables);
        lootTables = currentTables.ToArray();

        Debug.Log($"Created {combatTables.Count} combat loot tables");
    }

    // Call this in Start() to set up combat loot tables
    void Start()
    {
        CreateCombatLootTables();
    }
    public void GiveLootToPlayer(string lootTableName)
    {
        LootTable table = System.Array.Find(lootTables, lt => lt.tableName == lootTableName);
        if (table == null) return;

        foreach (var drop in table.lootDrops)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= drop.dropChance)
            {
                Item item = System.Array.Find(itemDatabase, i => i.id == drop.itemId);
                if (item != null)
                {
                    int quantity = Random.Range(drop.minQuantity, drop.maxQuantity + 1);

                    if (item.hasDurability)
                    {
                        Item rewardItem = CreateItemCopy(item);
                        rewardItem.currentDurability = Random.Range(drop.minDurability, drop.maxDurability);
                        InventorySystem.Instance.AddItem(rewardItem, quantity);
                    }
                    else
                    {
                        InventorySystem.Instance.AddItem(item, quantity);
                    }

                    Debug.Log($"Player received {quantity}x {item.itemName}!");
                }
            }
        }
    }

    private Item CreateItemCopy(Item original)
    {
        Item copy = new Item(original.id, original.itemName, original.description,
                           original.category, original.type);

        // Copy all properties
        copy.icon = original.icon;
        copy.maxStackSize = original.maxStackSize;
        copy.sellPrice = original.sellPrice;
        copy.buyPrice = original.buyPrice;
        copy.isConsumable = original.isConsumable;
        copy.attackPower = original.attackPower;
        copy.defensePower = original.defensePower;
        copy.hpRestore = original.hpRestore;
        copy.sanityRestore = original.sanityRestore;
        copy.hasDurability = original.hasDurability;
        copy.maxDurability = original.maxDurability;
        copy.currentDurability = original.currentDurability;
        copy.requiredAmmoType = original.requiredAmmoType;
        copy.magazineSize = original.magazineSize;
        copy.fireRate = original.fireRate;
        copy.range = original.range;
        copy.healingAmount = original.healingAmount;
        copy.sanityAmount = original.sanityAmount;
        copy.isInstantUse = original.isInstantUse;

        return copy;
    }

    // Debug method for player to have all items.
    // Replace or create new method during actual gameplay.
    private void CreateSampleItems()
    {
        List<Item> items = new List<Item>();

        // Create Weapons
        items.Add(CreateWeapon(1, "M4A1", "Assault rifle", ItemType.M4A1, 30, ItemType.Ammo_556x45_NATO, 35, 3.5f, 50f));
        items.Add(CreateWeapon(2, "FN SCAR-MK17", "Heavy Assault rifle", ItemType.FN_SCAR_MK17, 45, ItemType.Ammo_762x51_NATO, 20, 2.8f, 60f));
        items.Add(CreateWeapon(3, "Compound Bow", "Silent ranged weapon", ItemType.CompoundBow, 25, ItemType.Arrows, 1, 1.5f, 40f));
        items.Add(CreateWeapon(4, "Fire Axe", "Heavy melee weapon", ItemType.Fire_Axe, 40, ItemType.Misc, 0, 1.2f, 2f));
        items.Add(CreateWeapon(5, "Crowbar", "Versatile melee tool", ItemType.Crowbar, 25, ItemType.Misc, 0, 1.8f, 1.5f));

        // Create Ammunition
        items.Add(CreateAmmo(10, "5.56x45 NATO", "Standard rifle ammunition", ItemType.Ammo_556x45_NATO, 999));
        items.Add(CreateAmmo(11, "7.62x51 NATO", "Heavy rifle ammunition", ItemType.Ammo_762x51_NATO, 999));
        items.Add(CreateAmmo(12, "Arrows", "Arrows for bows", ItemType.Arrows, 99));
        items.Add(CreateAmmo(13, "Crossbow Bolts", "Bolts for crossbows", ItemType.Crossbow_Bolts, 99));

        // Create Armor
        items.Add(CreateArmor(20, "Survivor Outfit", "Basic protective clothing", ItemType.SurvivorOutfit, 5));
        items.Add(CreateArmor(21, "Bruiser Jacket", "Reinforced leather jacket", ItemType.BruiserJacket, 12));
        items.Add(CreateArmor(22, "Anomaly Hoodie", "Strange protective garment", ItemType.AnomalyHoodie, 8));

        // Create Consumables
        items.Add(CreateConsumable(30, "Bandages", "Basic medical supplies", ItemType.Bandages, 25, 0, 20));
        items.Add(CreateConsumable(31, "Medical Serum", "Advanced healing compound", ItemType.MedicalSerum, 75, 0, 10));
        items.Add(CreateConsumable(32, "Sanity Pills", "Helps maintain mental stability", ItemType.SanityPills, 0, 50, 15));
        items.Add(CreateConsumable(33, "Anomaly Pills", "Mysterious mental enhancement", ItemType.AnomalyPills, 10, 25, 8));

        // Create Key Items
        items.Add(CreateKeyItem(40, "Keycard", "Electronic access card"));
        items.Add(CreateKeyItem(41, "Map", "Shows the local area"));
        items.Add(CreateKeyItem(42, "Expedition Log", "Records of previous explorers"));

        itemDatabase = items.ToArray();
        Debug.Log($"Created {items.Count} sample items for the database.");
    }

    private Item CreateWeapon(int id, string name, string description, ItemType type, int damage, ItemType ammoType, int magSize, float fireRate, float range)
    {
        Item weapon = new Item(id, name, description, ItemCategory.Weapons, type);
        weapon.attackPower = damage;
        weapon.requiredAmmoType = ammoType;
        weapon.magazineSize = magSize;
        weapon.fireRate = fireRate;
        weapon.range = range;
        weapon.sellPrice = damage * 25;
        weapon.buyPrice = damage * 50;
        return weapon;
    }

    private Item CreateAmmo(int id, string name, string description, ItemType type, int maxStack)
    {
        Item ammo = new Item(id, name, description, ItemCategory.Materials, type);
        ammo.maxStackSize = maxStack;
        ammo.sellPrice = 1;
        ammo.buyPrice = 3;
        return ammo;
    }

    private Item CreateArmor(int id, string name, string description, ItemType type, int defense)
    {
        Item armor = new Item(id, name, description, ItemCategory.Armor, type);
        armor.defensePower = defense;
        armor.sellPrice = defense * 20;
        armor.buyPrice = defense * 40;
        return armor;
    }

    private Item CreateConsumable(int id, string name, string description, ItemType type, int hp, int sanity, int maxStack)
    {
        Item consumable = new Item(id, name, description, ItemCategory.Consumables, type);
        consumable.isConsumable = true;
        consumable.hpRestore = hp;
        consumable.sanityRestore = sanity;
        consumable.healingAmount = hp;
        consumable.sanityAmount = sanity;
        consumable.maxStackSize = maxStack;
        consumable.sellPrice = (hp + sanity) / 3;
        consumable.buyPrice = (hp + sanity);
        return consumable;
    }

    private Item CreateKeyItem(int id, string name, string description)
    {
        Item keyItem = new Item(id, name, description, ItemCategory.KeyItems, ItemType.Misc);
        keyItem.sellPrice = 0; // Key items usually can't be sold
        return keyItem;
    }
}

[System.Serializable]
public class LootTable
{
    public string tableName;
    public LootDrop[] lootDrops;
}

[System.Serializable]
public class LootDrop
{
    public int itemId;
    public float dropChance; // 0-100%
    public int minQuantity = 1;
    public int maxQuantity = 1;

    [Header("Durability Range (for weapons/armor)")]
    [Range(0f, 100f)]
    public float minDurability = 20f;
    [Range(0f, 100f)]
    public float maxDurability = 80f;
}

public class LootPickup : MonoBehaviour
{
    public Item item;
    public int quantity;

    [Header("Pickup Settings")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.1f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Make loot bob up and down for visibility
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (InventorySystem.Instance.AddItem(item, quantity))
            {
                string durabilityText = item.hasDurability ? $" (Durability: {item.GetDurabilityPercentage():F1}%)" : "";
                Debug.Log($"Picked up {quantity}x {item.itemName}!{durabilityText}");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory is full!");
            }
        }
    }
}