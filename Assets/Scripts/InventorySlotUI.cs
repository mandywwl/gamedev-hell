using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    public Image backgroundImage;
    public Button slotButton;

    [Header("Visual Settings")]
    public Color emptySlotColor = Color.gray;
    public Color filledSlotColor = Color.white;
    public Color selectedSlotColor = Color.yellow;

    private int slotIndex;
    private InventoryUIManager uiManager;
    private ItemStack currentItemStack;
    private bool isSelected = false;

    public void Initialize(int index, InventoryUIManager manager)
    {
        slotIndex = index;
        uiManager = manager;

        // Setup button click
        slotButton.onClick.AddListener(OnSlotClicked);

        // Initially empty
        UpdateSlot(null);
    }

    public void UpdateSlot(ItemStack itemStack)
    {
        currentItemStack = itemStack;

        if (itemStack == null || itemStack.IsEmpty())
        {
            // Empty slot
            itemIcon.gameObject.SetActive(false);
            quantityText.gameObject.SetActive(false);
            backgroundImage.color = emptySlotColor;
        }
        else
        {
            // Filled slot
            itemIcon.gameObject.SetActive(true);

            // Set icon if available
            if (itemStack.item.icon != null)
            {
                itemIcon.sprite = itemStack.item.icon;
            }
            else
            {
                // Use default icon or create a colored square
                itemIcon.color = itemStack.item.GetRarityColor();
            }

            // Show quantity if more than 1
            if (itemStack.quantity > 1)
            {
                quantityText.gameObject.SetActive(true);
                quantityText.text = itemStack.quantity.ToString();
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }

            backgroundImage.color = filledSlotColor;

            // Show durability if applicable
            if (itemStack.item.hasDurability)
            {
                float durabilityPercent = itemStack.item.GetDurabilityPercentage();
                Color durabilityColor = Color.Lerp(Color.red, Color.green, durabilityPercent / 100f);
                backgroundImage.color = Color.Lerp(filledSlotColor, durabilityColor, 0.3f);
            }
        }

        // Update selection visual
        UpdateSelectionVisual();
    }

    void OnSlotClicked()
    {
        uiManager.OnSlotClicked(slotIndex);
        SetSelected(true);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateSelectionVisual();
    }

    void UpdateSelectionVisual()
    {
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
}