using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Inventory Settings")]
    public int maxSlots = 30;

    [Header("Starting Items")]
    public List<Item> startingItems = new List<Item>();

    // Dictionary for organizing items by category (efficient lookup and filtering)
    private Dictionary<ItemCategory, List<ItemStack>> itemsByCategory;

    // List for visual slot representation (what player sees in grid)
    private List<ItemStack> inventorySlots;

    // Equipment slots (separate from main inventory)
    private Dictionary<ItemType, ItemStack> equippedItems;

    // Events for UI updates
    public System.Action<int, ItemStack> OnSlotChanged;
    public System.Action<ItemCategory> OnCategoryChanged;
    public System.Action<ItemType, ItemStack> OnEquipmentChanged;

    public event Action<Item,int> OnItemAdded;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;  // Will work only on a GameObject!
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
        }
    }

    void Start()
    {
        // Add starting items
        foreach (var item in startingItems)
        {
            AddItem(item, 1);
        }
    }

    private void InitializeInventory()
    {
        // Initialize visual slots list
        inventorySlots = new List<ItemStack>(maxSlots);
        for (int i = 0; i < maxSlots; i++)
        {
            inventorySlots.Add(null);
        }

        // Initialize category dictionary
        itemsByCategory = new Dictionary<ItemCategory, List<ItemStack>>();
        foreach (ItemCategory category in System.Enum.GetValues(typeof(ItemCategory)))
        {
            itemsByCategory[category] = new List<ItemStack>();
        }

        // Initialize equipment slots
        equippedItems = new Dictionary<ItemType, ItemStack>();
    }

    public bool AddItem(Item item, int quantity = 1)
    {
        OnItemAdded?.Invoke(item, quantity);
        // For weapons with durability, each one is unique - don't stack
        if (item.hasDurability)
        {
            for (int i = 0; i < quantity; i++)
            {
                if (!AddSingleItem(item, 1))
                {
                    Debug.Log($"Could only add {i} out of {quantity} {item.itemName}(s). Inventory full!");
                    return i > 0; // Return true if we added at least one
                }
            }
            return true;
        }

        // Try to stack with existing items first
        ItemStack existingStack = FindStackableItem(item);
        if (existingStack != null && existingStack.CanAddItems(quantity))
        {
            existingStack.AddItems(quantity);
            OnSlotChanged?.Invoke(inventorySlots.IndexOf(existingStack), existingStack);
            return true;
        }

        // If partial stacking is possible
        if (existingStack != null)
        {
            int canAdd = item.maxStackSize - existingStack.quantity;
            existingStack.AddItems(canAdd);
            OnSlotChanged?.Invoke(inventorySlots.IndexOf(existingStack), existingStack);

            // Try to add remaining quantity in new slot
            return AddItem(item, quantity - canAdd);
        }

        return AddSingleItem(item, quantity);
    }

    private bool AddSingleItem(Item item, int quantity)
    {
        int emptySlotIndex = FindEmptySlot();
        if (emptySlotIndex != -1)
        {
            ItemStack newStack = new ItemStack(item, quantity);
            inventorySlots[emptySlotIndex] = newStack;
            itemsByCategory[item.category].Add(newStack);
            OnSlotChanged?.Invoke(emptySlotIndex, newStack);
            OnCategoryChanged?.Invoke(item.category);
            return true;
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public bool RemoveItem(Item item, int quantity = 1)
    {
        ItemStack stack = FindItemStack(item);
        if (stack != null && stack.quantity >= quantity)
        {
            stack.RemoveItems(quantity);
            int slotIndex = inventorySlots.IndexOf(stack);

            if (stack.IsEmpty())
            {
                inventorySlots[slotIndex] = null;
                itemsByCategory[item.category].Remove(stack);
            }

            OnSlotChanged?.Invoke(slotIndex, stack.IsEmpty() ? null : stack);
            OnCategoryChanged?.Invoke(item.category);
            return true;
        }
        return false;
    }

    public bool EquipItem(Item item)
    {
        if (item.category != ItemCategory.Weapons && item.category != ItemCategory.Armor)
        {
            Debug.Log("This item cannot be equipped!");
            return false;
        }

        if (!HasItem(item, 1))
        {
            Debug.Log("You don't have this item!");
            return false;
        }

        // Check if weapon is broken
        if (item.hasDurability && item.IsBroken())
        {
            Debug.Log("This item is broken and cannot be equipped!");
            return false;
        }

        // Unequip current item in slot if exists
        if (equippedItems.ContainsKey(item.type) && equippedItems[item.type] != null)
        {
            UnequipItem(item.type);
        }

        // Remove from inventory and equip
        RemoveItem(item, 1);
        equippedItems[item.type] = new ItemStack(item, 1);
        OnEquipmentChanged?.Invoke(item.type, equippedItems[item.type]);

        Debug.Log($"Equipped {item.itemName} (Durability: {item.GetDurabilityPercentage():F1}%)");
        return true;
    }

    public bool UnequipItem(ItemType equipmentSlot)
    {
        if (equippedItems.ContainsKey(equipmentSlot) && equippedItems[equipmentSlot] != null)
        {
            ItemStack equippedStack = equippedItems[equipmentSlot];

            if (AddItem(equippedStack.item, equippedStack.quantity))
            {
                equippedItems[equipmentSlot] = null;
                OnEquipmentChanged?.Invoke(equipmentSlot, null);
                Debug.Log($"Unequipped {equippedStack.item.itemName}");
                return true;
            }
            else
            {
                Debug.Log("Inventory is full! Cannot unequip item.");
                return false;
            }
        }
        return false;
    }

    public bool UseConsumable(Item item)
    {
        if (!item.isConsumable)
        {
            Debug.Log("This item is not consumable!");
            return false;
        }

        if (HasItem(item, 1))
        {
            RemoveItem(item, 1);
            Debug.Log($"Used {item.itemName}! Restored {item.hpRestore} HP and {item.sanityRestore} sanity");

            // Apply the item effects to player stats -- not tested yet. commenting out for now
            // PlayerStats.Instance.Heal(item.hpRestore);
            // PlayerStats.Instance.RestoreSanity(item.sanityRestore);

            return true;
        }

        Debug.Log("You don't have this item!");
        return false;
    }

    public bool UseWeapon(ItemType weaponType, float durabilityLoss = 1f)
    {
        if (equippedItems.ContainsKey(weaponType) && equippedItems[weaponType] != null)
        {
            Item weapon = equippedItems[weaponType].item;

            if (weapon.IsBroken())
            {
                Debug.Log($"{weapon.itemName} is broken and cannot be used!");
                return false;
            }

            weapon.UseDurability(durabilityLoss);
            Debug.Log($"Used {weapon.itemName}. Durability: {weapon.GetDurabilityPercentage():F1}%");

            if (weapon.IsBroken())
            {
                Debug.Log($"{weapon.itemName} has broken!");
            }

            OnEquipmentChanged?.Invoke(weaponType, equippedItems[weaponType]);
            return true;
        }

        Debug.Log("No weapon equipped in this slot!");
        return false;
    }

    public bool HasAmmoForWeapon(ItemType weaponType)
    {
        if (equippedItems.ContainsKey(weaponType) && equippedItems[weaponType] != null)
        {
            Item weapon = equippedItems[weaponType].item;
            ItemType requiredAmmo = weapon.requiredAmmoType;

            if (requiredAmmo == ItemType.Misc) return true; // Melee weapons don't need ammo

            return GetAmmoCount(requiredAmmo) > 0;
        }
        return false;
    }

    public bool ConsumeAmmo(ItemType ammoType, int amount = 1)
    {
        var ammoStacks = inventorySlots.Where(slot =>
            slot != null && slot.item.type == ammoType).ToList();

        int totalAmmo = ammoStacks.Sum(stack => stack.quantity);

        if (totalAmmo >= amount)
        {
            int remaining = amount;
            foreach (var stack in ammoStacks)
            {
                if (remaining <= 0) break;

                int toRemove = Mathf.Min(remaining, stack.quantity);
                stack.RemoveItems(toRemove);
                remaining -= toRemove;

                int slotIndex = inventorySlots.IndexOf(stack);
                if (stack.IsEmpty())
                {
                    inventorySlots[slotIndex] = null;
                    itemsByCategory[stack.item.category].Remove(stack);
                }

                OnSlotChanged?.Invoke(slotIndex, stack.IsEmpty() ? null : stack);
            }
            return true;
        }

        return false;
    }
    public bool CanPickupItem(Item item, int quantity = 1)
    {
        // Check if we have enough inventory slots
        if (item.hasDurability)
        {
            // Weapons with durability don't stack - need one slot per item
            int emptySlots = GetEmptySlotCount();
            if (emptySlots < quantity)
            {
                Debug.Log($"Cannot pick up {item.itemName} - need {quantity} empty slots, only have {emptySlots}");
                return false;
            }
        }
        else
        {
            // Check if we can stack or need new slots
            ItemStack existingStack = FindStackableItem(item);
            if (existingStack == null || !existingStack.CanAddItems(quantity))
            {
                // Need at least one empty slot
                if (GetEmptySlotCount() == 0)
                {
                    Debug.Log($"Cannot pick up {item.itemName} - inventory full");
                    return false;
                }
            }
        }

        return true;
    }

    // Helper method to count empty slots
    private int GetEmptySlotCount()
    {
        int count = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot == null) count++;
        }
        return count;
    }

    public int GetAmmoCount(ItemType ammoType)
    {
        return inventorySlots
            .Where(slot => slot != null && slot.item.type == ammoType)
            .Sum(slot => slot.quantity);
    }

    public List<ItemStack> GetItemsByCategory(ItemCategory category)
    {
        return itemsByCategory[category].Where(stack => !stack.IsEmpty()).ToList();
    }

    public List<ItemStack> GetAllItems()
    {
        return inventorySlots.Where(slot => slot != null && !slot.IsEmpty()).ToList();
    }

    public ItemStack GetItemAtSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
        {
            return inventorySlots[slotIndex];
        }
        return null;
    }

    public ItemStack GetEquippedItem(ItemType equipmentSlot)
    {
        if (equippedItems.ContainsKey(equipmentSlot))
        {
            return equippedItems[equipmentSlot];
        }
        return null;
    }

    public int GetItemCount(Item item)
    {
        return inventorySlots
            .Where(slot => slot != null && slot.item.id == item.id)
            .Sum(slot => slot.quantity);
    }

    public bool HasItem(Item item, int requiredQuantity = 1)
    {
        return GetItemCount(item) >= requiredQuantity;
    }

    private ItemStack FindStackableItem(Item item)
    {
        // Don't stack items with durability - each one is unique
        if (item.hasDurability) return null;

        return inventorySlots.FirstOrDefault(slot =>
            slot != null &&
            slot.item.id == item.id &&
            slot.quantity < item.maxStackSize);
    }

    private ItemStack FindItemStack(Item item)
    {
        return inventorySlots.FirstOrDefault(slot =>
            slot != null && slot.item.id == item.id);
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] == null)
                return i;
        }
        return -1;
    }

    public void SortInventory()
    {
        var sortedItems = GetAllItems()
            .OrderBy(stack => stack.item.category)
            .ThenBy(stack => stack.item.type)
            .ThenBy(stack => stack.item.itemName)
            .ToList();

        // Clear current slots
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i] = null;
        }

        // Rebuild category dictionary
        foreach (var categoryList in itemsByCategory.Values)
        {
            categoryList.Clear();
        }

        // Place sorted items back
        for (int i = 0; i < sortedItems.Count && i < maxSlots; i++)
        {
            inventorySlots[i] = sortedItems[i];
            itemsByCategory[sortedItems[i].item.category].Add(sortedItems[i]);
            OnSlotChanged?.Invoke(i, sortedItems[i]);
        }

        Debug.Log("Inventory sorted!");
    }

    // Turn-based combat ready weapon usage
    public bool UseWeaponInTurnBasedCombat(ItemType weaponType, float durabilityLoss = 1f)
    {
        // This method will be called by the combat system during player's turn
        if (equippedItems.ContainsKey(weaponType) && equippedItems[weaponType] != null)
        {
            Item weapon = equippedItems[weaponType].item;

            if (weapon.IsBroken())
            {
                Debug.Log($"{weapon.itemName} is broken and cannot be used!");
                return false;
            }

            // Check if weapon needs ammo and consume it
            if (weapon.requiredAmmoType != ItemType.Misc)
            {
                if (!HasAmmoForWeapon(weaponType))
                {
                    Debug.Log($"No ammo for {weapon.itemName}!");
                    return false;
                }

                // Consume exactly 1 ammo per shot
                if (!ConsumeAmmo(weapon.requiredAmmoType, 1))
                {
                    Debug.Log($"Failed to consume ammo for {weapon.itemName}!");
                    return false;
                }

                Debug.Log($"Consumed 1x {weapon.requiredAmmoType} ammo");
            }

            // Use weapon durability
            weapon.UseDurability(durabilityLoss);
            Debug.Log($"Used {weapon.itemName}. Durability: {weapon.GetDurabilityPercentage():F1}%");

            if (weapon.IsBroken())
            {
                Debug.Log($"{weapon.itemName} has broken!");
            }

            OnEquipmentChanged?.Invoke(weaponType, equippedItems[weaponType]);
            return true;
        }

        Debug.Log("No weapon equipped in this slot!");
        return false;
    }

    // Turn-based combat ready consumable usage
    public bool UseConsumableInTurnBasedCombat(Item item)
    {
        // This method will be called by the combat system during player's turn
        if (!item.isConsumable)
        {
            Debug.Log("This item is not consumable!");
            return false;
        }

        if (HasItem(item, 1))
        {
            RemoveItem(item, 1);
            Debug.Log($"Used {item.itemName}! Restored {item.hpRestore} HP and {item.sanityRestore} sanity");

            // Return the healing values so combat system can apply them
            // The combat system will handle applying these values to player stats
            return true;
        }

        Debug.Log("You don't have this item!");
        return false;
    }

    // Helper method for combat system to get item effects
    public (int hpRestore, int sanityRestore) GetItemEffects(Item item)
    {
        if (item.isConsumable)
        {
            return (item.hpRestore, item.sanityRestore);
        }
        return (0, 0);
    }

    // Helper method for combat system to get weapon damage
    public int GetWeaponDamage(ItemType weaponType)
    {
        if (equippedItems.ContainsKey(weaponType) && equippedItems[weaponType] != null)
        {
            return equippedItems[weaponType].item.attackPower;
        }
        return 0;
    }

    // Method for combat system to check if action is possible
    public bool CanUseWeaponInCombat(ItemType weaponType)
    {
        if (!equippedItems.ContainsKey(weaponType) || equippedItems[weaponType] == null)
            return false;

        Item weapon = equippedItems[weaponType].item;

        // Check if weapon is broken
        if (weapon.IsBroken())
            return false;

        // Check ammo if needed
        if (weapon.requiredAmmoType != ItemType.Misc)
        {
            return HasAmmoForWeapon(weaponType);
        }

        return true;
    }

    public bool CanUseConsumableInCombat(Item item)
    {
        return item.isConsumable && HasItem(item, 1);
    }
}