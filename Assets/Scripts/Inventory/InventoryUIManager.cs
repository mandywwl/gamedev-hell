using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Builds its own list-style inventory UI at runtime - no prefab/Inspector wiring required.
// Just having this component on a GameObject in the scene is enough.
public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    public bool IsOpen => isOpen;

    [Header("Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(560, 640);
    [SerializeField] private Color panelColor = new Color(0.05f, 0.05f, 0.05f, 0.92f);
    [SerializeField] private Color rowNormalColor = new Color(1f, 1f, 1f, 0.05f);
    [SerializeField] private Color rowSelectedColor = new Color(1f, 0.85f, 0.2f, 0.35f);
    [SerializeField] private float rowHeight = 34f;
    [SerializeField] private float quantityColumnWidth = 100f;

    [Header("Navigation")]
    [SerializeField] private KeyCode navigateUpKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode navigateDownKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode selectKey = KeyCode.Return;
    [SerializeField] private KeyCode selectKeyAlt = KeyCode.Space;

    // --- Runtime-built UI references ---
    private GameObject inventoryRoot;
    private RectTransform contentRoot;
    private TextMeshProUGUI emptyStateLabel;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI descriptionText;
    private GameObject confirmDialog;
    private TextMeshProUGUI confirmMessageText;
    private Image confirmYesButtonImage;
    private Image confirmNoButtonImage;
    private bool confirmYesSelected = true;
    private static readonly Color ConfirmButtonNormalColor = new Color(1f, 1f, 1f, 0.12f);

    private GameObject resultDialog;
    private TextMeshProUGUI resultMessageText;
    private bool isResultOpen = false;

    private readonly List<InventoryListRowUI> rowUIs = new();
    private readonly List<int> occupiedSlots = new(); // list position -> InventorySystem slot index
    private int selectedListIndex = 0;
    private bool isOpen = false;
    private bool isConfirmOpen = false;

    // Set while opened via OpenForCombat - lets a battle turn wait for the outcome of one
    // item use instead of the normal free-browsing exploration flow.
    private bool isCombatMode = false;
    private Action<bool, string> combatResultCallback;
    private bool lastUseSucceeded;
    private string lastUseMessage;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        HideLegacyChildren();
        EnsureEventSystem();
        BuildUI();
    }

    // Anything that was already parented under this GameObject before new UI
    // e.g. our old grid-style inventory panel, this component happens to live inside gets hidden.
    private void HideLegacyChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(go);
    }

    private void Update()
    {
        if (!isOpen) return;

        // The inventory is exploration-only outside of OpenForCombat - close immediately if
        // combat starts while it's open some other way.
        if (!isCombatMode && GameState.I != null && GameState.I.CurrentMode == GameState.PlayMode.Combat)
        {
            CloseInventory();
            return;
        }

        if (isResultOpen)
        {
            if (Input.GetKeyDown(selectKey) || Input.GetKeyDown(selectKeyAlt) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape))
                CloseResultPopup();
            return;
        }

        if (isConfirmOpen)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                confirmYesSelected = !confirmYesSelected;
                UpdateConfirmSelectionVisuals();
            }
            else if (Input.GetKeyDown(selectKey) || Input.GetKeyDown(selectKeyAlt) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (confirmYesSelected) ConfirmYes(); else ConfirmNo();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                ConfirmNo();
            }
            return;
        }

        // Escape while just browsing the list (no sub-dialog open) is handled by
        // PauseManager, which closes the inventory - see PauseManager.Update(). Don't also
        // handle it here: PauseManager checks IsOpen in the same frame, so a second handler
        // here would close the inventory first and cause PauseManager to then treat the
        // Escape press as "inventory wasn't open" and open the Pause menu on top of it.

        if (occupiedSlots.Count > 0)
        {
            if (Input.GetKeyDown(navigateUpKey) || Input.GetKeyDown(KeyCode.W)) MoveSelection(-1);
            if (Input.GetKeyDown(navigateDownKey) || Input.GetKeyDown(KeyCode.S)) MoveSelection(1);

            if (Input.GetKeyDown(selectKey) || Input.GetKeyDown(selectKeyAlt) || Input.GetKeyDown(KeyCode.KeypadEnter))
                OpenConfirmForSelected();
        }
    }

    private void MoveSelection(int direction)
    {
        selectedListIndex = (selectedListIndex + direction + occupiedSlots.Count) % occupiedSlots.Count;
        UpdateSelectionVisuals();
    }

    private void OpenConfirmForSelected()
    {
        if (selectedListIndex < 0 || selectedListIndex >= occupiedSlots.Count) return;
        if (InventorySystem.Instance == null) return;

        var stack = InventorySystem.Instance.GetSlot(occupiedSlots[selectedListIndex]);
        if (stack == null) return;

        if (stack.IsEmpty())
        {
            ShowResultPopup($"You don't have any {stack.item.itemName} left.");
            return;
        }

        isConfirmOpen = true;
        confirmYesSelected = true;
        confirmMessageText.text = $"Use {stack.item.itemName}?";
        confirmDialog.SetActive(true);
        UpdateConfirmSelectionVisuals();
    }

    private void UpdateConfirmSelectionVisuals()
    {
        if (confirmYesButtonImage != null)
            confirmYesButtonImage.color = confirmYesSelected ? rowSelectedColor : ConfirmButtonNormalColor;
        if (confirmNoButtonImage != null)
            confirmNoButtonImage.color = !confirmYesSelected ? rowSelectedColor : ConfirmButtonNormalColor;
    }

    private void ConfirmYes()
    {
        isConfirmOpen = false;
        confirmDialog.SetActive(false);

        if (selectedListIndex < 0 || selectedListIndex >= occupiedSlots.Count) return;
        if (InventorySystem.Instance == null) return;

        int inventorySlotIndex = occupiedSlots[selectedListIndex];
        var result = InventorySystem.Instance.UseItem(inventorySlotIndex);
        Refresh();

        lastUseSucceeded = result.success;
        lastUseMessage = result.message;
        ShowResultPopup(result.message);
    }

    private void ConfirmNo()
    {
        isConfirmOpen = false;
        confirmDialog.SetActive(false);
    }

    private void ShowResultPopup(string message)
    {
        isResultOpen = true;
        resultMessageText.text = message;
        resultDialog.SetActive(true);
    }

    private void CloseResultPopup()
    {
        isResultOpen = false;
        resultDialog.SetActive(false);

        // A successful use during combat ends the browsing session here - failed attempts
        // (out of item, can't use directly, etc.) just return to the list to try something else.
        if (isCombatMode && lastUseSucceeded)
        {
            var callback = combatResultCallback;
            var message = lastUseMessage;
            isCombatMode = false;
            combatResultCallback = null;
            CloseInventory();
            callback?.Invoke(true, message);
        }
    }

    // --- UI construction ---
    private void BuildUI()
    {
        var canvasGO = new GameObject("InventoryCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Full-screen backdrop: blocks clicks to whatever's behind (e.g. the combat
        // Attack/Item/Run buttons) while the inventory is open, and closing on a click
        // outside the panel gives mouse users the same "back out" affordance Escape gives
        // keyboard users.
        var backdropGO = new GameObject("Backdrop", typeof(RectTransform));
        backdropGO.transform.SetParent(canvasGO.transform, false);
        var backdropRect = backdropGO.GetComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;
        backdropGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f); // near-invisible but still a raycast target
        var backdropButton = backdropGO.AddComponent<Button>();
        backdropButton.transition = Selectable.Transition.None;
        backdropButton.onClick.AddListener(CloseInventory);

        // --- Root panel ---
        var panelGO = new GameObject("Panel", typeof(RectTransform));
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;

        var panelImage = panelGO.AddComponent<Image>();
        panelImage.color = panelColor;

        var panelLayout = panelGO.AddComponent<VerticalLayoutGroup>();
        panelLayout.padding = new RectOffset(16, 16, 16, 16);
        panelLayout.spacing = 10;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;

        var title = CreateText(panelGO.transform, "Title", TextAlignmentOptions.Center);
        title.text = "Inventory";
        title.fontStyle = FontStyles.Bold;
        title.fontSize = 28;
        title.gameObject.AddComponent<LayoutElement>().preferredHeight = 36;

        // --- Health readout ---
        healthText = CreateText(panelGO.transform, "HealthText", TextAlignmentOptions.Center);
        healthText.fontSize = 22;
        healthText.gameObject.AddComponent<LayoutElement>().preferredHeight = 26;

        // --- Header row (Items | Quantity) ---
        var headerGO = new GameObject("HeaderRow", typeof(RectTransform));
        headerGO.transform.SetParent(panelGO.transform, false);
        headerGO.AddComponent<LayoutElement>().preferredHeight = 30;
        var headerHlg = headerGO.AddComponent<HorizontalLayoutGroup>();
        headerHlg.padding = new RectOffset(10, 10, 0, 4);
        headerHlg.spacing = 8;
        headerHlg.childControlWidth = true;
        headerHlg.childControlHeight = true;
        headerHlg.childForceExpandHeight = true;

        var itemsHeader = CreateText(headerGO.transform, "ItemsHeader", TextAlignmentOptions.MidlineLeft);
        itemsHeader.text = "Items";
        itemsHeader.fontStyle = FontStyles.Bold;
        itemsHeader.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var qtyHeader = CreateText(headerGO.transform, "QuantityHeader", TextAlignmentOptions.MidlineRight);
        qtyHeader.text = "Quantity";
        qtyHeader.fontStyle = FontStyles.Bold;
        qtyHeader.gameObject.AddComponent<LayoutElement>().preferredWidth = quantityColumnWidth;

        // --- Scrollable row list ---
        var scrollGO = new GameObject("ScrollView", typeof(RectTransform));
        scrollGO.transform.SetParent(panelGO.transform, false);
        scrollGO.AddComponent<LayoutElement>().flexibleHeight = 1;
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        var viewportGO = new GameObject("Viewport", typeof(RectTransform));
        viewportGO.transform.SetParent(scrollGO.transform, false);
        var viewportRect = viewportGO.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportGO.AddComponent<RectMask2D>();

        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewportGO.transform, false);
        contentRoot = contentGO.GetComponent<RectTransform>();
        contentRoot.anchorMin = new Vector2(0, 1);
        contentRoot.anchorMax = new Vector2(1, 1);
        contentRoot.pivot = new Vector2(0.5f, 1);
        contentRoot.sizeDelta = new Vector2(0, 0);
        var contentVlg = contentGO.AddComponent<VerticalLayoutGroup>();
        contentVlg.childControlWidth = true;
        contentVlg.childControlHeight = true;
        contentVlg.childForceExpandWidth = true;
        contentVlg.childForceExpandHeight = false;
        contentVlg.spacing = 3;
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRoot;

        // --- Empty state ---
        emptyStateLabel = CreateText(panelGO.transform, "EmptyLabel", TextAlignmentOptions.Center);
        emptyStateLabel.text = "Inventory is empty";
        emptyStateLabel.color = new Color(1, 1, 1, 0.6f);
        emptyStateLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
        emptyStateLabel.gameObject.SetActive(false);

        // --- Selected item description ---
        descriptionText = CreateText(panelGO.transform, "DescriptionText", TextAlignmentOptions.TopLeft);
        descriptionText.fontSize = 18;
        descriptionText.color = new Color(1f, 1f, 1f, 0.75f);
        descriptionText.enableWordWrapping = true;
        descriptionText.gameObject.AddComponent<LayoutElement>().preferredHeight = 60;

        BuildConfirmDialog(canvasGO.transform);
        BuildResultDialog(canvasGO.transform);

        inventoryRoot = canvasGO;
        inventoryRoot.SetActive(false);
    }

    private void BuildResultDialog(Transform canvasParent)
    {
        var overlayGO = new GameObject("ResultOverlay", typeof(RectTransform));
        overlayGO.transform.SetParent(canvasParent, false);
        var overlayRect = overlayGO.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.65f);
        var overlayButton = overlayGO.AddComponent<Button>();
        overlayButton.transition = Selectable.Transition.None;
        overlayButton.onClick.AddListener(CloseResultPopup); // click outside the box dismisses it

        var boxGO = new GameObject("DialogBox", typeof(RectTransform));
        boxGO.transform.SetParent(overlayGO.transform, false);
        var boxRect = boxGO.GetComponent<RectTransform>();
        boxRect.anchorMin = boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.sizeDelta = new Vector2(380, 190);
        boxGO.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f, 0.97f); // blocks clicks from reaching overlay

        var boxLayout = boxGO.AddComponent<VerticalLayoutGroup>();
        boxLayout.padding = new RectOffset(20, 20, 20, 20);
        boxLayout.spacing = 16;
        boxLayout.childControlWidth = true;
        boxLayout.childControlHeight = true;
        boxLayout.childForceExpandHeight = false;

        resultMessageText = CreateText(boxGO.transform, "Message", TextAlignmentOptions.Center);
        resultMessageText.fontSize = 24;
        resultMessageText.enableWordWrapping = true;
        resultMessageText.gameObject.AddComponent<LayoutElement>().preferredHeight = 80;

        var buttonRowGO = new GameObject("ButtonRow", typeof(RectTransform));
        buttonRowGO.transform.SetParent(boxGO.transform, false);
        buttonRowGO.AddComponent<LayoutElement>().preferredHeight = 50;
        var buttonRowHlg = buttonRowGO.AddComponent<HorizontalLayoutGroup>();
        buttonRowHlg.childAlignment = TextAnchor.MiddleCenter;
        buttonRowHlg.childControlWidth = true;
        buttonRowHlg.childControlHeight = true;
        buttonRowHlg.childForceExpandWidth = false;
        buttonRowHlg.childForceExpandHeight = true;

        var okImage = CreateDialogButton(buttonRowGO.transform, "OK", CloseResultPopup);
        okImage.gameObject.AddComponent<LayoutElement>().preferredWidth = 120;

        resultDialog = overlayGO;
        resultDialog.SetActive(false);
    }

    private void BuildConfirmDialog(Transform canvasParent)
    {
        var overlayGO = new GameObject("ConfirmOverlay", typeof(RectTransform));
        overlayGO.transform.SetParent(canvasParent, false);
        var overlayRect = overlayGO.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.65f);
        var overlayButton = overlayGO.AddComponent<Button>();
        overlayButton.transition = Selectable.Transition.None;
        overlayButton.onClick.AddListener(ConfirmNo); // click outside the box cancels

        var boxGO = new GameObject("DialogBox", typeof(RectTransform));
        boxGO.transform.SetParent(overlayGO.transform, false);
        var boxRect = boxGO.GetComponent<RectTransform>();
        boxRect.anchorMin = boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.sizeDelta = new Vector2(380, 190);
        boxGO.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f, 0.97f); // blocks clicks from reaching overlay

        var boxLayout = boxGO.AddComponent<VerticalLayoutGroup>();
        boxLayout.padding = new RectOffset(20, 20, 20, 20);
        boxLayout.spacing = 16;
        boxLayout.childControlWidth = true;
        boxLayout.childControlHeight = true;
        boxLayout.childForceExpandHeight = false;

        confirmMessageText = CreateText(boxGO.transform, "Message", TextAlignmentOptions.Center);
        confirmMessageText.fontSize = 24;
        confirmMessageText.enableWordWrapping = true;
        confirmMessageText.gameObject.AddComponent<LayoutElement>().preferredHeight = 60;

        var buttonRowGO = new GameObject("ButtonRow", typeof(RectTransform));
        buttonRowGO.transform.SetParent(boxGO.transform, false);
        buttonRowGO.AddComponent<LayoutElement>().preferredHeight = 50;
        var buttonRowHlg = buttonRowGO.AddComponent<HorizontalLayoutGroup>();
        buttonRowHlg.spacing = 20;
        buttonRowHlg.childControlWidth = true;
        buttonRowHlg.childControlHeight = true;
        buttonRowHlg.childForceExpandWidth = true;
        buttonRowHlg.childForceExpandHeight = true;

        confirmYesButtonImage = CreateDialogButton(buttonRowGO.transform, "Yes", ConfirmYes);
        confirmNoButtonImage = CreateDialogButton(buttonRowGO.transform, "No", ConfirmNo);

        confirmDialog = overlayGO;
        confirmDialog.SetActive(false);
    }

    private Image CreateDialogButton(Transform parent, string label, Action onClick)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = ConfirmButtonNormalColor;
        var button = go.AddComponent<Button>();
        button.onClick.AddListener(() => onClick());

        var text = CreateText(go.transform, "Label", TextAlignmentOptions.Center);
        text.text = label;
        var textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return image;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.alignment = alignment;
        text.fontSize = 22;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private InventoryListRowUI CreateRow(int index)
    {
        var rowGO = new GameObject($"Row_{index}", typeof(RectTransform));
        rowGO.transform.SetParent(contentRoot, false);
        rowGO.AddComponent<LayoutElement>().preferredHeight = rowHeight;

        var bg = rowGO.AddComponent<Image>();
        bg.color = rowNormalColor;

        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 4, 4);
        hlg.spacing = 8;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        var nameText = CreateText(rowGO.transform, "NameText", TextAlignmentOptions.MidlineLeft);
        nameText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var qtyText = CreateText(rowGO.transform, "QuantityText", TextAlignmentOptions.MidlineRight);
        qtyText.gameObject.AddComponent<LayoutElement>().preferredWidth = quantityColumnWidth;

        var row = rowGO.AddComponent<InventoryListRowUI>();
        row.background = bg;
        row.nameText = nameText;
        row.quantityText = qtyText;
        row.normalColor = rowNormalColor;
        row.selectedColor = rowSelectedColor;
        row.Initialize(index, this);

        return row;
    }

    private void EnsureRowCount(int count)
    {
        while (rowUIs.Count < count)
            rowUIs.Add(CreateRow(rowUIs.Count));

        for (int i = 0; i < rowUIs.Count; i++)
            rowUIs[i].gameObject.SetActive(i < count);
    }

    public void Refresh()
    {
        UpdateHealthText();

        occupiedSlots.Clear();
        if (InventorySystem.Instance != null)
            occupiedSlots.AddRange(InventorySystem.Instance.GetOccupiedSlotIndices());

        if (emptyStateLabel != null) emptyStateLabel.gameObject.SetActive(occupiedSlots.Count == 0);

        if (occupiedSlots.Count == 0)
        {
            selectedListIndex = 0;
            EnsureRowCount(0);
            UpdateSelectionVisuals();
            return;
        }

        selectedListIndex = Mathf.Clamp(selectedListIndex, 0, occupiedSlots.Count - 1);

        EnsureRowCount(occupiedSlots.Count);
        for (int i = 0; i < occupiedSlots.Count; i++)
        {
            var stack = InventorySystem.Instance.GetSlot(occupiedSlots[i]);
            rowUIs[i].SetItem(stack);
        }

        UpdateSelectionVisuals();
    }

    private void UpdateSelectionVisuals()
    {
        for (int i = 0; i < occupiedSlots.Count; i++)
            rowUIs[i].SetSelected(i == selectedListIndex);

        UpdateDescriptionText();
    }

    private void UpdateHealthText()
    {
        if (healthText == null) return;

        if (PlayerStats.Instance != null)
            healthText.text = $"Health: {PlayerStats.Instance.GetCurrentHP():F0} / {PlayerStats.Instance.maxHP:F0}";
        else
            healthText.text = string.Empty;
    }

    private void UpdateDescriptionText()
    {
        if (descriptionText == null) return;

        if (InventorySystem.Instance == null || selectedListIndex < 0 || selectedListIndex >= occupiedSlots.Count)
        {
            descriptionText.text = string.Empty;
            return;
        }

        var stack = InventorySystem.Instance.GetSlot(occupiedSlots[selectedListIndex]);
        if (stack == null)
        {
            descriptionText.text = string.Empty;
            return;
        }

        descriptionText.text = string.IsNullOrEmpty(stack.item.description)
            ? stack.item.itemName
            : stack.item.description;
    }

    // --- Public API ---
    public void ToggleInventory() { if (isOpen) CloseInventory(); else OpenInventory(); }
    public void Toggle() => ToggleInventory();
    public void Open() => OpenInventory();
    public void Close() => CloseInventory();

    public void OpenInventory()
    {
        if (inventoryRoot == null) return;

        if (GameState.I != null && GameState.I.CurrentMode == GameState.PlayMode.Combat)
        {
            Debug.Log("Inventory is only accessible outside of combat.");
            return;
        }

        if (PauseManager.isPaused)
        {
            Debug.Log("Inventory is unavailable while the pause menu is open.");
            return;
        }

        isOpen = true;
        isConfirmOpen = false;
        isResultOpen = false;
        if (confirmDialog != null) confirmDialog.SetActive(false);
        if (resultDialog != null) resultDialog.SetActive(false);
        inventoryRoot.SetActive(true);
        Time.timeScale = 0f; // optional pause
        selectedListIndex = 0;
        Refresh();
    }

    // Opens the inventory during a combat turn. onResult fires exactly once: (true, message)
    // when an item was successfully used, or (false, null) if the player backed out without
    // using anything - the caller (BattleSystem) uses this to decide whether the turn passes.
    public void OpenForCombat(Action<bool, string> onResult)
    {
        if (inventoryRoot == null) return;

        isCombatMode = true;
        combatResultCallback = onResult;
        lastUseSucceeded = false; // reset so a stale result from a previous turn can't leak in

        isOpen = true;
        isConfirmOpen = false;
        isResultOpen = false;
        if (confirmDialog != null) confirmDialog.SetActive(false);
        if (resultDialog != null) resultDialog.SetActive(false);
        inventoryRoot.SetActive(true);
        Time.timeScale = 0f; // pause while the player is deciding, same as exploration
        selectedListIndex = 0;
        Refresh();
    }

    public void CloseInventory()
    {
        if (inventoryRoot == null) return;
        isOpen = false;
        isConfirmOpen = false;
        isResultOpen = false;
        if (confirmDialog != null) confirmDialog.SetActive(false);
        if (resultDialog != null) resultDialog.SetActive(false);
        inventoryRoot.SetActive(false);
        Time.timeScale = 1f;

        // Only reaches here still combat-mode if the player backed out without using an item -
        // a successful use already cleared isCombatMode itself in CloseResultPopup before calling this.
        if (isCombatMode)
        {
            isCombatMode = false;
            var callback = combatResultCallback;
            combatResultCallback = null;
            callback?.Invoke(false, null);
        }
    }

    // Clicking a row: first click highlights it, clicking the already-highlighted row opens the use prompt.
    public void OnSlotClicked(int listIndex)
    {
        if (!isOpen || isConfirmOpen || isResultOpen) return;
        if (listIndex < 0 || listIndex >= occupiedSlots.Count) return;

        if (selectedListIndex == listIndex)
        {
            OpenConfirmForSelected();
        }
        else
        {
            selectedListIndex = listIndex;
            UpdateSelectionVisuals();
        }
    }
}
