using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Image component that displays the item icon")]
    public Image itemIcon;
    
    [Tooltip("Text component that shows item quantity")]
    public TextMeshProUGUI quantityText;
    
    [Tooltip("Background image of the slot")]
    public Image backgroundImage;
    
    [Tooltip("Button component for slot interaction")]
    public Button slotButton;

    [Header("Visual Settings")]
    [Tooltip("Color when slot is empty")]
    public Color emptySlotColor = Color.gray;
    
    [Tooltip("Color when slot has an item")]
    public Color filledSlotColor = Color.white;
    
    [Tooltip("Color when slot is selected")]
    public Color selectedSlotColor = Color.yellow;

    // Private variables
    private int slotIndex;
    private InventoryUIManager uiManager;
    private ItemStack currentItemStack;
    private bool isSelected = false;

    void Awake()
    {
        // Auto-find components if not assigned
        ValidateComponents();
    }

    // Automatically finds and assigns UI components if they're not already assigned
    private void ValidateComponents()
    {
        // Try to find components if not assigned
        if (itemIcon == null)
        {
            itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (itemIcon == null)
            {
                // Look for any Image component in children
                itemIcon = GetComponentInChildren<Image>();
            }
        }

        if (quantityText == null)
        {
            quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            if (quantityText == null)
            {
                // Look for any TextMeshProUGUI component in children
                quantityText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
        }

        // Log warnings for missing components
        if (itemIcon == null)
            Debug.LogWarning($"ItemIcon not found on {gameObject.name}. Please assign manually or ensure child object named 'ItemIcon' exists.");
        
        if (quantityText == null)
            Debug.LogWarning($"QuantityText not found on {gameObject.name}. Please assign manually or ensure child object named 'QuantityText' exists.");
        
        if (backgroundImage == null)
            Debug.LogWarning($"BackgroundImage not found on {gameObject.name}. Please assign manually.");
        
        if (slotButton == null)
            Debug.LogWarning($"SlotButton not found on {gameObject.name}. Please assign manually.");
    }

    public void Initialize(int index, InventoryUIManager manager)
    {
        slotIndex = index;
        uiManager = manager;

        // Ensure components are valid before setup
        ValidateComponents();

        // Setup button click
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }

        // Initially empty
        UpdateSlot(null);
    }

    public void UpdateSlot(ItemStack itemStack)
    {
        currentItemStack = itemStack;

        if (itemStack == null || itemStack.IsEmpty())
        {
            // Empty slot
            if (itemIcon != null)
                itemIcon.gameObject.SetActive(false);
            
            if (quantityText != null)
                quantityText.gameObject.SetActive(false);
            
            if (backgroundImage != null)
                backgroundImage.color = emptySlotColor;
        }
        else
        {
            // Filled slot
            if (itemIcon != null)
            {
                itemIcon.gameObject.SetActive(true);

                // Set icon if available
                if (itemStack.item.icon != null)
                {
                    itemIcon.sprite = itemStack.item.icon;
                    itemIcon.color = Color.white; // Reset color when using sprite
                }
                else
                {
                    // Use default icon or create a colored square
                    itemIcon.sprite = null; // Clear sprite
                    itemIcon.color = itemStack.item.GetRarityColor();
                }
            }

            // Show quantity if more than 1
            if (quantityText != null)
            {
                if (itemStack.quantity > 1)
                {
                    quantityText.gameObject.SetActive(true);
                    quantityText.text = itemStack.quantity.ToString();
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = filledSlotColor;

                // Show durability if applicable
                if (itemStack.item.hasDurability)
                {
                    float durabilityPercent = itemStack.item.GetDurabilityPercentage();
                    Color durabilityColor = Color.Lerp(Color.red, Color.green, durabilityPercent / 100f);
                    backgroundImage.color = Color.Lerp(filledSlotColor, durabilityColor, 0.3f);
                }
            }
        }

        // Update selection visual
        UpdateSelectionVisual();
    }

    void OnSlotClicked()
    {
        if (uiManager != null)
        {
            uiManager.OnSlotClicked(slotIndex);
            SetSelected(true);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateSelectionVisual();
    }

    void UpdateSelectionVisual()
    {
        if (backgroundImage == null) return;

        if (isSelected && currentItemStack != null)
        {
            backgroundImage.color = selectedSlotColor;
        }
        else if (currentItemStack != null)
        {
            backgroundImage.color = filledSlotColor;
        }
        else
        {
            backgroundImage.color = emptySlotColor;
        }
    }

    public bool HasItem()
    {
        return currentItemStack != null && !currentItemStack.IsEmpty();
    }

    public Item GetItem()
    {
        return currentItemStack?.item;
    }

    // Force refresh component assignments (useful for prefabs)
    [ContextMenu("Refresh Component Assignments")]
    public void RefreshComponents()
    {
        ValidateComponents();
        Debug.Log($"Component assignments refreshed for {gameObject.name}");
    }
}