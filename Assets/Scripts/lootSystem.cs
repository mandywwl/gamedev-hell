using System.Collections.Generic;
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

    [Header("Procedural Generation Settings")]
    public int runSeed = 0; // 0 = random seed each run
    public float rarityModifier = 1f; // Higher = more rare items
    public bool enableProceduralModification = true;
    
    // Run-specific tracking
    private Dictionary<int, int> spawnedItemCounts = new Dictionary<int, int>();
    private System.Random runRandom;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeRun();
            CreateSampleItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Initialize each run with unique characteristics
    private void InitializeRun()
    {
        // Set up random seed for this run
        if (runSeed == 0)
        {
            runSeed = Random.Range(1, 999999);
        }
        runRandom = new System.Random(runSeed);
        
        Debug.Log($"Run initialized with seed: {runSeed}");
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
            // Use run-specific random for consistent results
            float roll = (float)runRandom.NextDouble() * 100f;
            if (roll <= drop.dropChance)
            {
                Item item = System.Array.Find(itemDatabase, i => i.id == drop.itemId);
                if (item != null)
                {
                    // Check run limits before spawning
                    if (!CanSpawnItem(item))
                    {
                        Debug.Log($"Skipping {item.itemName} - run limit reached");
                        continue;
                    }

                    int quantity = runRandom.Next(drop.minQuantity, drop.maxQuantity + 1);

                    // For weapons with durability, randomize condition
                    if (item.hasDurability)
                    {
                        Item droppedItem = CreateItemCopy(item);
                        droppedItem.currentDurability = (float)runRandom.NextDouble() * 
                                                       (drop.maxDurability - drop.minDurability) + 
                                                       drop.minDurability;
                        
                        // Apply procedural modifications
                        if (enableProceduralModification)
                        {
                            droppedItem = ApplyProceduralModification(droppedItem);
                        }
                        
                        CreateLootDrop(droppedItem, quantity, dropPosition);
                    }
                    else
                    {
                        CreateLootDrop(item, quantity, dropPosition);
                    }
                    
                    // Track spawned items
                    spawnedItemCounts[item.id] = spawnedItemCounts.GetValueOrDefault(item.id, 0) + quantity;
                }
            }
        }
    }

    // Check if item can spawn based on run limits
    private bool CanSpawnItem(Item item)
    {
        if (item.isUnique && spawnedItemCounts.ContainsKey(item.id))
        {
            return false;
        }
        
        if (item.maxPerRun > 0)
        {
            int currentCount = spawnedItemCounts.GetValueOrDefault(item.id, 0);
            return currentCount < item.maxPerRun;
        }
        
        return true;
    }

    // Apply procedural modifications to make items unique each run
    private Item ApplyProceduralModification(Item originalItem)
    {
        if (!originalItem.canBeModified) return originalItem;
        
        // Generate random stat modifier
        float statModifier = (float)runRandom.NextDouble() * 
                           (originalItem.maxStatModifier - originalItem.minStatModifier) + 
                           originalItem.minStatModifier;
        
        // Create modifier suffix
        string suffix = "";
        if (statModifier > 1.15f)
            suffix = " [Superior]";
        else if (statModifier > 1.05f)
            suffix = " [Enhanced]";
        else if (statModifier < 0.85f)
            suffix = " [Worn]";
        else if (statModifier < 0.95f)
            suffix = " [Damaged]";
        
        return originalItem.CreateModifiedCopy(statModifier, suffix);
    }

    // Generate Rarity based items 
    public Item GenerateQualityItem(ItemCategory category = ItemCategory.Weapons)
    {
        var availableItems = itemDatabase.Where(item => item.category == category).ToArray();
        if (availableItems.Length == 0) return null;
        
        // Roll for rarity
        float rarityRoll = (float)runRandom.NextDouble() * 100f / rarityModifier;
        ItemRarity targetRarity;
        
        if (rarityRoll <= 1f) targetRarity = ItemRarity.Anomalous;
        else if (rarityRoll <= 5f) targetRarity = ItemRarity.Prototype;
        else if (rarityRoll <= 15f) targetRarity = ItemRarity.ExpeditionGrade;
        else if (rarityRoll <= 40f) targetRarity = ItemRarity.Standard;
        else targetRarity = ItemRarity.Makeshift;
        
        // Find item of target rarity or closest
        var rarityItems = availableItems.Where(item => item.rarity == targetRarity).ToArray();
        if (rarityItems.Length == 0)
        {
            // Fallback to any item
            rarityItems = availableItems;
        }
        
        Item selectedItem = rarityItems[runRandom.Next(rarityItems.Length)];
        
        // Apply procedural modifications
        if (enableProceduralModification)
        {
            selectedItem = ApplyProceduralModification(selectedItem);
        }
        
        Debug.Log($"Generated {targetRarity} item: {selectedItem.itemName}");
        return selectedItem;
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
            
            // Color loot drops by rarity
            renderer.color = item.GetRarityColor();
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

        combatTables.Add(new LootTable
        {
            tableName = "ChestDrop",
            lootDrops = new LootDrop[]
    {
        new LootDrop { itemId = 1, dropChance = 40f, minQuantity = 1, maxQuantity = 1, minDurability = 60f, maxDurability = 90f }, // M4A1
        new LootDrop { itemId = 10, dropChance = 80f, minQuantity = 20, maxQuantity = 50 }, // 5.56 Ammo
        new LootDrop { itemId = 31, dropChance = 60f, minQuantity = 2, maxQuantity = 4 }, // Medical Serum
        new LootDrop { itemId = 40, dropChance = 30f, minQuantity = 1, maxQuantity = 1 }  // Keycard
    }
        });

        combatTables.Add(new LootTable
        {
            tableName = "SmallChestDrop",
            lootDrops = new LootDrop[]
            {
        new LootDrop { itemId = 30, dropChance = 90f, minQuantity = 3, maxQuantity = 6 }, // Bandages
        new LootDrop { itemId = 10, dropChance = 70f, minQuantity = 10, maxQuantity = 25 }, // 5.56 Ammo
        new LootDrop { itemId = 32, dropChance = 40f, minQuantity = 1, maxQuantity = 3 }   // Sanity Pills
            }
        });

        combatTables.Add(new LootTable
        {
            tableName = "LargeChestDrop",
            lootDrops = new LootDrop[]
            {
        new LootDrop { itemId = 2, dropChance = 50f, minQuantity = 1, maxQuantity = 1, minDurability = 70f, maxDurability = 95f }, // FN SCAR
        new LootDrop { itemId = 11, dropChance = 85f, minQuantity = 30, maxQuantity = 60 }, // 7.62 Ammo
        new LootDrop { itemId = 31, dropChance = 75f, minQuantity = 3, maxQuantity = 5 }, // Medical Serum
        new LootDrop { itemId = 21, dropChance = 60f, minQuantity = 1, maxQuantity = 1 }  // Bruiser Jacket
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
            float roll = (float)runRandom.NextDouble() * 100f;
            if (roll <= drop.dropChance)
            {
                Item item = System.Array.Find(itemDatabase, i => i.id == drop.itemId);
                if (item != null)
                {
                    int quantity = runRandom.Next(drop.minQuantity, drop.maxQuantity + 1);

                    if (item.hasDurability)
                    {
                        Item rewardItem = CreateItemCopy(item);
                        rewardItem.currentDurability = (float)runRandom.NextDouble() * 
                                                      (drop.maxDurability - drop.minDurability) + 
                                                      drop.minDurability;
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
        // Create a new instance using CreateInstance for ScriptableObjects
        Item copy = ScriptableObject.CreateInstance<Item>();
        
        // Copy all properties manually
        copy.id = original.id;
        copy.itemName = original.itemName;
        copy.description = original.description;
        copy.category = original.category;
        copy.type = original.type;
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
        copy.rarity = original.rarity;
        copy.weight = original.weight;
        copy.isUnique = original.isUnique;
        copy.maxPerRun = original.maxPerRun;
        copy.canBeModified = original.canBeModified;
        copy.minStatModifier = original.minStatModifier;
        copy.maxStatModifier = original.maxStatModifier;

        return copy;
    }

    // Debug method for player to have all items.
    // Replace or create new method during actual gameplay.
    private void CreateSampleItems()
    {
        List<Item> items = new List<Item>();

        // Create Weapons with NEW properties
        items.Add(CreateWeapon(1, "M4A1", "Assault rifle", ItemType.M4A1, 30, ItemType.Ammo_556x45_NATO, 35, 3.5f, 50f, ItemRarity.Makeshift, 4f, false, -1));
        items.Add(CreateWeapon(2, "FN SCAR-MK17", "Heavy Assault rifle", ItemType.FN_SCAR_MK17, 45, ItemType.Ammo_762x51_NATO, 20, 2.8f, 60f, ItemRarity.ExpeditionGrade, 6f, false, 2));
        items.Add(CreateWeapon(3, "Compound Bow", "Silent ranged weapon", ItemType.CompoundBow, 25, ItemType.Arrows, 1, 1.5f, 40f, ItemRarity.Standard, 3f, false, -1));
        items.Add(CreateWeapon(4, "Fire Axe", "Heavy melee weapon", ItemType.Fire_Axe, 40, ItemType.Misc, 0, 1.2f, 2f, ItemRarity.Makeshift, 5f, false, -1));
        items.Add(CreateWeapon(5, "Crowbar", "Versatile melee tool", ItemType.Crowbar, 25, ItemType.Misc, 0, 1.8f, 1.5f, ItemRarity.Makeshift, 2f, false, -1));

        // Create Ammunition
        items.Add(CreateAmmo(10, "5.56x45 NATO", "Standard rifle ammunition", ItemType.Ammo_556x45_NATO, 999, 0.1f));
        items.Add(CreateAmmo(11, "7.62x51 NATO", "Heavy rifle ammunition", ItemType.Ammo_762x51_NATO, 999, 0.15f));
        items.Add(CreateAmmo(12, "Arrows", "Arrows for bows", ItemType.Arrows, 99, 0.05f));
        items.Add(CreateAmmo(13, "Crossbow Bolts", "Bolts for crossbows", ItemType.Crossbow_Bolts, 99, 0.08f));

        // Create Armor
        items.Add(CreateArmor(20, "Survivor Outfit", "Basic protective clothing", ItemType.SurvivorOutfit, 5, 2f));
        items.Add(CreateArmor(21, "Bruiser Jacket", "Reinforced leather jacket", ItemType.BruiserJacket, 12, 4f));
        items.Add(CreateArmor(22, "Anomaly Hoodie", "Strange protective garment", ItemType.AnomalyHoodie, 8, 3f));

        // Create Consumables
        items.Add(CreateConsumable(30, "Bandages", "Basic medical supplies", ItemType.Bandages, 25, 0, 20, 0.2f));
        items.Add(CreateConsumable(31, "Medical Serum", "Advanced healing compound", ItemType.MedicalSerum, 75, 0, 10, 0.3f, true, 5));
        items.Add(CreateConsumable(32, "Sanity Pills", "Helps maintain mental stability", ItemType.SanityPills, 0, 50, 15, 0.1f));
        items.Add(CreateConsumable(33, "Anomaly Pills", "Mysterious mental enhancement", ItemType.AnomalyPills, 10, 25, 8, 0.15f, true, 3));

        // Create Key Items
        items.Add(CreateKeyItem(40, "Keycard", "Electronic access card", true, 1));
        items.Add(CreateKeyItem(41, "Map", "Shows the local area", false, 2));
        items.Add(CreateKeyItem(42, "Expedition Log", "Records of previous explorers", false, 3));

        itemDatabase = items.ToArray();
        Debug.Log($"Created {items.Count} sample items for the database.");
    }

    private Item CreateWeapon(int id, string name, string description, ItemType type, int damage, ItemType ammoType, int magSize, float fireRate, float range, ItemRarity rarity, float weight, bool isUnique, int maxPerRun)
    {
        // Create ScriptableObject instance
        Item weapon = ScriptableObject.CreateInstance<Item>();
        
        // Set basic properties
        weapon.id = id;
        weapon.itemName = name;
        weapon.description = description;
        weapon.category = ItemCategory.Weapons;
        weapon.type = type;
        weapon.attackPower = damage;
        weapon.requiredAmmoType = ammoType;
        weapon.magazineSize = magSize;
        weapon.fireRate = fireRate;
        weapon.range = range;
        weapon.sellPrice = damage * 25;
        weapon.buyPrice = damage * 50;
        weapon.rarity = rarity;
        weapon.weight = weight;
        weapon.isUnique = isUnique;
        weapon.maxPerRun = maxPerRun;
        weapon.hasDurability = true;
        weapon.maxDurability = 100f;
        weapon.currentDurability = 100f;
        
        return weapon;
    }

    private Item CreateAmmo(int id, string name, string description, ItemType type, int maxStack, float weight)
    {
        Item ammo = ScriptableObject.CreateInstance<Item>();
        
        ammo.id = id;
        ammo.itemName = name;
        ammo.description = description;
        ammo.category = ItemCategory.Materials;
        ammo.type = type;
        ammo.maxStackSize = maxStack;
        ammo.sellPrice = 1;
        ammo.buyPrice = 3;
        ammo.weight = weight;
        ammo.rarity = ItemRarity.Standard;
        
        return ammo;
    }

    private Item CreateArmor(int id, string name, string description, ItemType type, int defense, float weight)
    {
        Item armor = ScriptableObject.CreateInstance<Item>();
        
        armor.id = id;
        armor.itemName = name;
        armor.description = description;
        armor.category = ItemCategory.Armor;
        armor.type = type;
        armor.defensePower = defense;
        armor.sellPrice = defense * 20;
        armor.buyPrice = defense * 40;
        armor.weight = weight;
        armor.rarity = ItemRarity.Standard;
        armor.hasDurability = true;
        armor.maxDurability = 100f;
        armor.currentDurability = 100f;
        
        return armor;
    }

    private Item CreateConsumable(int id, string name, string description, ItemType type, int hp, int sanity, int maxStack, float weight, bool isUnique = false, int maxPerRun = -1)
    {
        Item consumable = ScriptableObject.CreateInstance<Item>();
        
        consumable.id = id;
        consumable.itemName = name;
        consumable.description = description;
        consumable.category = ItemCategory.Consumables;
        consumable.type = type;
        consumable.isConsumable = true;
        consumable.hpRestore = hp;
        consumable.sanityRestore = sanity;
        consumable.healingAmount = hp;
        consumable.sanityAmount = sanity;
        consumable.maxStackSize = maxStack;
        consumable.sellPrice = (hp + sanity) / 3;
        consumable.buyPrice = (hp + sanity);
        consumable.weight = weight;
        consumable.isUnique = isUnique;
        consumable.maxPerRun = maxPerRun;
        consumable.rarity = isUnique ? ItemRarity.ExpeditionGrade : ItemRarity.Makeshift;
        
        return consumable;
    }

    private Item CreateKeyItem(int id, string name, string description, bool isUnique = true, int maxPerRun = 1)
    {
        Item keyItem = ScriptableObject.CreateInstance<Item>();
        
        keyItem.id = id;
        keyItem.itemName = name;
        keyItem.description = description;
        keyItem.category = ItemCategory.KeyItems;
        keyItem.type = ItemType.Misc;
        keyItem.sellPrice = 0; // Key items usually can't be sold
        keyItem.weight = 0.1f;
        keyItem.isUnique = isUnique;
        keyItem.maxPerRun = maxPerRun;
        keyItem.rarity = ItemRarity.Standard;
        
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
            // Check if player can pick up item before attempting
            if (InventorySystem.Instance.CanPickupItem(item, quantity))
            {
                if (InventorySystem.Instance.AddItem(item, quantity))
                {
                    string durabilityText = item.hasDurability ? $" (Durability: {item.GetDurabilityPercentage():F1}%)" : "";
                    Debug.Log($"Picked up {quantity}x {item.itemName}!{durabilityText}");
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log($"Cannot pick up {item.itemName} - restrictions apply!");
            }
        }
    }
}