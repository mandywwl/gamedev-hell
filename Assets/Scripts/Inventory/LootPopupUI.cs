using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Small "You got X!" popup shown after opening loot containers (e.g. chests).
// Builds its own UI at runtime - just having this component on a GameObject in the scene is enough.
public class LootPopupUI : MonoBehaviour
{
    public static LootPopupUI Instance { get; private set; }

    private GameObject popupRoot;
    private TextMeshProUGUI messageText;
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        EnsureEventSystem();
        BuildUI();
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePopup();
        }
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(go);
    }

    private void BuildUI()
    {
        var canvasGO = new GameObject("LootPopupCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60; // above the inventory panel (sortingOrder 50)

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        var overlayGO = new GameObject("Overlay", typeof(RectTransform));
        overlayGO.transform.SetParent(canvasGO.transform, false);
        var overlayRect = overlayGO.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.65f);
        var overlayButton = overlayGO.AddComponent<Button>();
        overlayButton.transition = Selectable.Transition.None;
        overlayButton.onClick.AddListener(ClosePopup); // click outside the box dismisses it

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

        messageText = CreateText(boxGO.transform, "Message");
        messageText.fontSize = 24;
        messageText.enableWordWrapping = true;
        messageText.gameObject.AddComponent<LayoutElement>().preferredHeight = 80;

        var buttonRowGO = new GameObject("ButtonRow", typeof(RectTransform));
        buttonRowGO.transform.SetParent(boxGO.transform, false);
        buttonRowGO.AddComponent<LayoutElement>().preferredHeight = 50;
        var buttonRowHlg = buttonRowGO.AddComponent<HorizontalLayoutGroup>();
        buttonRowHlg.childAlignment = TextAnchor.MiddleCenter;
        buttonRowHlg.childControlWidth = true;
        buttonRowHlg.childControlHeight = true;
        buttonRowHlg.childForceExpandWidth = false;
        buttonRowHlg.childForceExpandHeight = true;

        var okImage = CreateButton(buttonRowGO.transform, "OK", ClosePopup);
        okImage.gameObject.AddComponent<LayoutElement>().preferredWidth = 120;

        popupRoot = canvasGO;
        popupRoot.SetActive(false);
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 22;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static Image CreateButton(Transform parent, string label, System.Action onClick)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.12f);
        var button = go.AddComponent<Button>();
        button.onClick.AddListener(() => onClick());

        var text = CreateText(go.transform, "Label");
        text.text = label;
        var textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return image;
    }

    // --- Public API ---

    public void ShowLoot(Item item, int quantity)
    {
        if (popupRoot == null) return;

        messageText.text = (item != null && quantity > 0)
            ? $"You got {quantity}x {item.itemName}!"
            : "The chest was empty...";

        isOpen = true;
        popupRoot.SetActive(true);
    }

    public void ClosePopup()
    {
        isOpen = false;
        if (popupRoot != null) popupRoot.SetActive(false);
    }
}
