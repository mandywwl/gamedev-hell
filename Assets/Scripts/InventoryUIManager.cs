using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform slotsContainer;
    public GameObject slotPrefab;

    [Header("Item Details Panel")]
    public GameObject itemDetailsPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemStatsText;
    public Image itemIcon;
    public Button useButton;
    public Button equipButton;
    public Button dropButton;

    [Header("Player Stats Display")]
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI playerAttackText;
    public TextMeshProUGUI playerDefenseText;

    [Header("Category Filters")]
    public Button allItemsButton;
    public Button weaponsButton;
    public Button armorButton;
    public Button consumablesButton;
    public Button keyItemsButton;

    // Private variables
    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private bool isInventoryOpen = false;
    private Item selectedItem = null;
    private ItemCategory currentFilter = ItemCategory.Weapons; // Show all by default

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeUI();
        SetupEventListeners();

        // Initially hide inventory
        inventoryPanel.SetActive(false);
        itemDetailsPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle inventory with "I" key
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        // Close inventory with Escape
        if (Input.GetKeyDown(KeyCode.Escape) && isInventoryOpen)
        {
            CloseInventory();
        }
    }

    void InitializeUI()
    {
        // Create inventory slots
        for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            slotUI.Initialize(i, this);
            slotUIs.Add(slotUI);
        }

        // Setup category filter buttons
        allItemsButton.onClick.AddListener(() => FilterByCategory(ItemCategory.Weapons)); // Use any category as "all"
        weaponsButton.onClick.AddListener(() => FilterByCategory(ItemCategory.Weapons));
        armorButton.onClick.AddListener(() => FilterByCategory(ItemCategory.Armor));
        consumablesButton.onClick.AddListener(() => FilterByCategory(ItemCategory.Consumables));
        keyItemsButton.onClick.AddListener(() => FilterByCategory(ItemCategory.KeyItems));

        // Setup item action buttons
        useButton.onClick.AddListener(UseSelectedItem);
        equipButton.onClick.AddListener(EquipSelectedItem);
        dropButton.onClick.AddListener(DropSelectedItem);
    }

    void SetupEventListeners()
    {
        // Listen to inventory changes
        InventorySystem.Instance.OnSlotChanged += UpdateSlotUI;

        // Listen to player stats changes
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnHPChanged += UpdatePlayerStatsDisplay;
            PlayerStats.Instance.OnAttackPowerChanged += (attack) => UpdatePlayerStatsDisplay(PlayerStats.Instance.GetCurrentHP(), PlayerStats.Instance.maxHP);
            PlayerStats.Instance.OnDefensePowerChanged += (defense) => UpdatePlayerStatsDisplay(PlayerStats.Instance.GetCurrentHP(), PlayerStats.Instance.maxHP);
        }
    }

    public void ToggleInventory()
    {
        if (isInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        isInventoryOpen = true;
        inventoryPanel.SetActive(true);

        // Pause game (optional)
        Time.timeScale = 0f;

        // Update all slots
        RefreshInventoryDisplay();
        UpdatePlayerStatsDisplay(PlayerStats.Instance.GetCurrentHP(), PlayerStats.Instance.maxHP);

        Debug.Log("Inventory opened");
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;
        inventoryPanel.SetActive(false);
        itemDetailsPanel.SetActive(false);

        // Resume game
        Time.timeScale = 1f;

        selectedItem = null;

        Debug.Log("Inventory closed");
    }

    void RefreshInventoryDisplay()
    {
        // Update all slot UIs
        for (int i = 0; i < slotUIs.Count; i++)
        {
            ItemStack itemStack = InventorySystem.Instance.GetItemAtSlot(i);
            slotUIs[i].UpdateSlot(itemStack);
        }
    }

    void UpdateSlotUI(int slotIndex, ItemStack itemStack)
    {
        if (slotIndex >= 0 && slotIndex < slotUIs.Count)
        {
            slotUIs[slotIndex].UpdateSlot(itemStack);
        }
    }

    void FilterByCategory(ItemCategory category)
    {
        currentFilter = category;

        // Show all items for now - you can implement filtering later
        RefreshInventoryDisplay();

        Debug.Log($"Filtering by category: {category}");
    }

    public void OnSlotClicked(int slotIndex)
    {
        ItemStack itemStack = InventorySystem.Instance.GetItemAtSlot(slotIndex);

        if (itemStack != null)
        {
            SelectItem(itemStack.item);
        }
        else
        {
            DeselectItem();
        }
    }

    void SelectItem(Item item)
    {
        selectedItem = item;
        ShowItemDetails(item);

        Debug.Log($"Selected item: {item.itemName}");
    }

    void DeselectItem()
    {
        selectedItem = null;
        itemDetailsPanel.SetActive(false);
    }

    void ShowItemDetails(Item item)
    {
        itemDetailsPanel.SetActive(true);

        // Update item info
        itemNameText.text = item.itemName;
        itemNameText.color = item.GetRarityColor();

        itemDescriptionText.text = item.description;

        // Build stats text
        string statsText = BuildStatsText(item);
        itemStatsText.text = statsText;

        // Update icon if available
        if (item.icon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.gameObject.SetActive(true);
        }
        else
        {
            itemIcon.gameObject.SetActive(false);
        }

        // Update button availability
        UpdateActionButtons(item);
    }

    string BuildStatsText(Item item)
    {
        string stats = "";

        // Basic info
        stats += $"<b>Category:</b> {item.category}\n";
        stats += $"<b>Rarity:</b> {item.rarity}\n";
        stats += $"<b>Weight:</b> {item.weight:F1} kg\n";

        if (item.maxStackSize > 1)
            stats += $"<b>Max Stack:</b> {item.maxStackSize}\n";

        // Combat stats
        if (item.category == ItemCategory.Weapons)
        {
            stats += $"\n<b>WEAPON STATS</b>\n";
            stats += $"<b>Attack Power:</b> {item.attackPower}\n";
            stats += $"<b>Range:</b> {item.range:F1}m\n";
            stats += $"<b>Fire Rate:</b> {item.fireRate:F1}/s\n";

            if (item.requiredAmmoType != ItemType.Misc)
                stats += $"<b>Ammo Type:</b> {item.requiredAmmoType}\n";

            if (item.hasDurability)
                stats += $"<b>Durability:</b> {item.GetDurabilityPercentage():F1}%\n";
        }
        else if (item.category == ItemCategory.Armor)
        {
            stats += $"\n<b>ARMOR STATS</b>\n";
            stats += $"<b>Defense Power:</b> {item.defensePower}\n";

            if (item.hasDurability)
                stats += $"<b>Durability:</b> {item.GetDurabilityPercentage():F1}%\n";
        }
        else if (item.isConsumable)
        {
            stats += $"\n<b>CONSUMABLE EFFECTS</b>\n";
            if (item.healingAmount > 0)
                stats += $"<b>Heals:</b> {item.healingAmount:F1} HP\n";
        }

        // Economic info
        if (item.sellPrice > 0)
        {
            stats += $"\n<b>ECONOMIC</b>\n";
            stats += $"<b>Sell Price:</b> ${item.sellPrice}\n";
            stats += $"<b>Buy Price:</b> ${item.buyPrice}\n";
        }

        return stats;
    }

    void UpdateActionButtons(Item item)
    {
        // Use button
        useButton.gameObject.SetActive(item.isConsumable);

        // Equip button
        bool canEquip = (item.category == ItemCategory.Weapons || item.category == ItemCategory.Armor) && !item.IsBroken();
        equipButton.gameObject.SetActive(canEquip);

        // Drop button - always available
        dropButton.gameObject.SetActive(true);
    }

    void UseSelectedItem()
    {
        if (selectedItem != null && selectedItem.isConsumable)
        {
            if (PlayerStats.Instance.UseConsumable(selectedItem))
            {
                Debug.Log($"Used {selectedItem.itemName}");
                RefreshInventoryDisplay();
                DeselectItem();
            }
        }
    }

    void EquipSelectedItem()
    {
        if (selectedItem != null)
        {
            if (InventorySystem.Instance.EquipItem(selectedItem))
            {
                Debug.Log($"Equipped {selectedItem.itemName}");
                RefreshInventoryDisplay();
                DeselectItem();
            }
        }
    }

    void DropSelectedItem()
    {
        if (selectedItem != null)
        {
            // Remove from inventory (simplified - you might want to create a ground pickup)
            if (InventorySystem.Instance.RemoveItem(selectedItem, 1))
            {
                Debug.Log($"Dropped {selectedItem.itemName}");
                RefreshInventoryDisplay();
                DeselectItem();
            }
        }
    }

    void UpdatePlayerStatsDisplay(float currentHP, float maxHP)
    {
        if (PlayerStats.Instance != null)
        {
            playerHPText.text = $"HP: {currentHP:F0}/{maxHP:F0}";
            playerAttackText.text = $"Attack: {PlayerStats.Instance.GetTotalAttackPower():F1}";
            playerDefenseText.text = $"Defense: {PlayerStats.Instance.GetTotalDefensePower():F1}";
        }
    }
}