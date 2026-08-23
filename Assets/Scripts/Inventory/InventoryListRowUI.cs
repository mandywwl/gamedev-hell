using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A single row in the runtime-built inventory list: item name (left) + quantity (right).
public class InventoryListRowUI : MonoBehaviour, IPointerClickHandler
{
    public Image background;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;

    public Color normalColor = new Color(1f, 1f, 1f, 0.05f);
    public Color selectedColor = new Color(1f, 0.85f, 0.2f, 0.35f);

    private int listIndex;
    private InventoryUIManager manager;

    public void Initialize(int index, InventoryUIManager owner)
    {
        listIndex = index;
        manager = owner;
    }

    public void SetItem(ItemStack stack)
    {
        // Only a truly unused slot (null) blanks the row - a stack at quantity 0 still shows,
        // just with "0", since the player has held this item before and may find more later.
        if (stack == null)
        {
            nameText.text = string.Empty;
            quantityText.text = string.Empty;
            return;
        }

        nameText.text = stack.item.itemName;
        quantityText.text = stack.quantity.ToString();
    }

    public void SetSelected(bool selected)
    {
        if (background != null)
            background.color = selected ? selectedColor : normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        manager?.OnSlotClicked(listIndex);
    }
}
