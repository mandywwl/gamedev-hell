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

        // Seed the inventory with any designer-assigned starting items
        foreach (var startItem in startingItems)
        {
            if (startItem != null) AddItem(startItem, 1);
        }

        // If nothing was configured in the Inspector, give the player a few starter
        // items so there's something to test the inventory/use-item flow with.
        if (startingItems.Count == 0)
        {
            AddItem(CreateDefaultBandages(), 3);
            AddItem(CreateDefaultCandy(), 4);
        }
    }

    private Item CreateDefaultBandages()
    {
        var item = ScriptableObject.CreateInstance<Item>();
        item.id = 1;
        item.itemName = "Bandages";
        item.category = ItemCategory.Consumables;
        item.type = ItemType.Bandages;
        item.isConsumable = true;
        item.hasDurability = false;
        item.healingAmount = 25f;
        item.hpRestore = 25;
        item.maxStackSize = 20;
        item.weight = 0.2f;
        item.sellPrice = 8;
        item.buyPrice = 25;
        return item;
    }

    private Item CreateDefaultCandy()
    {
        var item = ScriptableObject.CreateInstance<Item>();
        item.id = 2;
        item.itemName = "Candy";
        item.category = ItemCategory.Consumables;
        item.type = ItemType.Candy;
        item.isConsumable = true;
        item.hasDurability = false;
        item.healingAmount = 10f;
        item.hpRestore = 10;
        item.maxStackSize = 20;
        item.weight = 0.05f;
        item.sellPrice = 3;
        item.buyPrice = 10;
        return item;
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

    // Adds up to `quantity` of item, stacking onto existing slots first, then filling empty ones.
    // Returns true if all of it fit.
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        int remaining = quantity;

        for (int i = 0; i < inventorySlots.Count && remaining > 0; i++)
        {
            var slot = inventorySlots[i];
            if (slot != null && slot.item.id == item.id && slot.quantity < item.maxStackSize)
            {
                int space = item.maxStackSize - slot.quantity;
                int toAdd = Mathf.Min(space, remaining);
                slot.AddItems(toAdd);
                remaining -= toAdd;
                OnSlotChanged?.Invoke(i, slot);
            }
        }

        while (remaining > 0)
        {
            int emptyIndex = FindEmptySlot();
            if (emptyIndex == -1) break; // inventory full

            int toAdd = Mathf.Min(item.maxStackSize, remaining);
            var newStack = new ItemStack(item, toAdd);
            inventorySlots[emptyIndex] = newStack;
            remaining -= toAdd;
            OnSlotChanged?.Invoke(emptyIndex, newStack);
        }

        int added = quantity - remaining;
        if (added > 0) OnItemAdded?.Invoke(item, added);

        return remaining == 0;
    }

    // Removes up to `quantity` of item across whichever slots hold it.
    public bool RemoveItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;
        if (!HasItem(item, quantity)) return false;

        int remaining = quantity;
        for (int i = 0; i < inventorySlots.Count && remaining > 0; i++)
        {
            var slot = inventorySlots[i];
            if (slot != null && slot.item.id == item.id)
            {
                int toRemove = Mathf.Min(slot.quantity, remaining);
                slot.RemoveItems(toRemove);
                remaining -= toRemove;

                if (slot.IsEmpty())
                    inventorySlots[i] = null;

                OnSlotChanged?.Invoke(i, inventorySlots[i]);
            }
        }

        return remaining == 0;
    }

    public ItemStack GetSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count) return null;
        return inventorySlots[slotIndex];
    }

    // Indices of every non-empty slot, in slot order - what the list-style UI iterates over.
    public List<int> GetOccupiedSlotIndices()
    {
        var result = new List<int>();
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] != null && !inventorySlots[i].IsEmpty())
                result.Add(i);
        }
        return result;
    }

    // Uses the item in the given slot. Consumables apply their effect via PlayerStats
    // and are removed from the inventory as part of that. Returns success plus a
    // human-readable reason, for the inventory UI's use-result popup.
    public (bool success, string message) UseItem(int slotIndex)
    {
        var stack = GetSlot(slotIndex);
        if (stack == null || stack.IsEmpty()) return (false, "No item to use.");

        Item item = stack.item;

        if (item.isConsumable)
        {
            if (PlayerStats.Instance == null) return (false, "Can't use items right now.");
            return PlayerStats.Instance.UseConsumable(item);
        }

        return (false, $"{item.itemName} can't be used directly from the inventory list.");
    }

}