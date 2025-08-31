using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject inventoryPanel;      // Root to show/hide
    public RectTransform slotsContainer;   // Grid container
    public GameObject slotPrefab;          // Prefab with InventorySlotUI

    [Header("Slots")]
    [SerializeField] int initialSlotCount = 8;

    private readonly List<InventorySlotUI> slotUIs = new();
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        BuildSlotsIfNeeded();
    }

    // --- Public API ---
    public void ToggleInventory() { if (isOpen) CloseInventory(); else OpenInventory(); }
    public void OpenInventory()
    {
        if (inventoryPanel == null) return;
        isOpen = true;
        inventoryPanel.SetActive(true);
        Time.timeScale = 0f; // optional pause
        Refresh();
    }
    public void CloseInventory()
    {
        if (inventoryPanel == null) return;
        isOpen = false;
        inventoryPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Legacy aliases
    public void Toggle() => ToggleInventory();
    public void Open()   => OpenInventory();
    public void Close()  => CloseInventory();

    // --- Minimal hooks for other scripts ---
    public void OnSlotClicked(int index) { /* no-op for now */ }
    public void ShowTooltip(ItemStack item, RectTransform anchor) { }
    public void HideTooltip() { }

    // --- Internals ---
    private void BuildSlotsIfNeeded()
    {
        if (slotsContainer == null || slotPrefab == null) return;

        if (slotsContainer.childCount == 0 && initialSlotCount > 0)
        {
            slotUIs.Clear();
            for (int i = 0; i < initialSlotCount; i++)
            {
                var go = Instantiate(slotPrefab, slotsContainer);
                var slot = go.GetComponent<InventorySlotUI>();
                if (slot == null) slot = go.AddComponent<InventorySlotUI>();
                slot.Initialize(i, this);
                slotUIs.Add(slot);
            }
        }
        else
        {
            slotUIs.Clear();
            for (int i = 0; i < slotsContainer.childCount; i++)
            {
                var slot = slotsContainer.GetChild(i).GetComponent<InventorySlotUI>();
                if (slot != null)
                {
                    slot.Initialize(i, this);
                    slotUIs.Add(slot);
                }
            }
        }
    }

    public void Refresh()
    {
        // Later: update icons/counts from your Inventory system
    }
}
