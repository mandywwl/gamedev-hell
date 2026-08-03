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

}