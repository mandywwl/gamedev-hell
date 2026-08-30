using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Renders the sanity bar (intended to sit directly below the HP bar). Shows the
// current value/percent, tints by state color, prints the state label, and pulses
// briefly when the player crosses a threshold(IF THERES TIME TO IMPLEMENT THIS)
public class SanityHUD : MonoBehaviour
{
    [Header("UI References")]
    public Slider sanitySlider;
    public TMP_Text valueText;   // e.g. "72%"
    public TMP_Text stateLabel;  // e.g. "Shaken"
    public Image fillImage;      // optional: tinted by state color

    [Header("Threshold Transition Feedback")]
    [SerializeField] private float pulseDuration = 0.5f;
    [SerializeField] private Vector3 pulseScale = new Vector3(1.12f, 1.12f, 1f);

    private SanityController controller;
    private Coroutine pulseRoutine;
    private Vector3 baseScale;

    void Start()
    {
        controller = SanityController.Instance;
        if (controller == null)
        {
            Debug.LogWarning("SanityHUD: no SanityController in scene.");
            return;
        }

        baseScale = transform.localScale;

        controller.OnSanityChanged += Refresh;
        controller.OnSanityStateChanged += OnStateChanged;

        Refresh(controller.GetSanityCurrent(), controller.GetSanityMax());
    }

    void OnDestroy()
    {
        if (controller == null) return;
        controller.OnSanityChanged -= Refresh;
        controller.OnSanityStateChanged -= OnStateChanged;
    }

    private void Refresh(float current, float max)
    {
        if (sanitySlider != null)
        {
            sanitySlider.maxValue = max;
            sanitySlider.value = current;
        }

        if (valueText != null)
        {
            float percent = max > 0f ? current / max * 100f : 0f;
            valueText.text = $"{percent:0}%";
        }

        ApplyStateVisuals(controller.CurrentState);
    }

    private void ApplyStateVisuals(SanityState state)
    {
        SanityConfig config = controller.GetConfig();
        if (config == null) return;

        if (stateLabel != null)
            stateLabel.text = config.GetStateLabel(state);

        if (fillImage != null)
            fillImage.color = config.GetStateColor(state);
    }

    private void OnStateChanged(SanityState oldState, SanityState newState)
    {
        ApplyStateVisuals(newState);
        Pulse();
    }

    private void Pulse()
    {
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pulseDuration);
            float envelope = Mathf.Sin(t * Mathf.PI); // 0 -> 1 -> 0
            transform.localScale = Vector3.Lerp(baseScale, pulseScale, envelope);
            yield return null;
        }
        transform.localScale = baseScale;
        pulseRoutine = null;
    }
}
