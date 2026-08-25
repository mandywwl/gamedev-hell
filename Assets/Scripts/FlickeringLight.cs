using UnityEngine;

// Fakes a flickering light by randomly varying a SpriteRenderer's alpha over time - no
// dynamic lighting involved, so it works with this project's unlit sprites as-is.
[RequireComponent(typeof(SpriteRenderer))]
public class FlickeringLight : MonoBehaviour
{
    [Header("Brightness Range")]
    [Range(0f, 1f)] public float minAlpha = 0.3f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    [Header("Timing")]
    [Tooltip("How often (seconds) the flicker picks a new target brightness.")]
    public float minInterval = 0.05f;
    public float maxInterval = 0.3f;
    [Tooltip("How quickly it fades toward each new target - higher is snappier/more erratic.")]
    public float fadeSpeed = 15f;

    [Header("Blackout")]
    [Tooltip("Chance, each time a new target is picked, that it drops to fully off instead of the normal range.")]
    [Range(0f, 1f)] public float blackoutChance = 0.1f;

    private SpriteRenderer sr;
    private float targetAlpha;
    private float nextChangeTime;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        targetAlpha = maxAlpha;
    }

    void Update()
    {
        if (Time.time >= nextChangeTime)
        {
            targetAlpha = (Random.value < blackoutChance) ? 0f : Random.Range(minAlpha, maxAlpha);
            nextChangeTime = Time.time + Random.Range(minInterval, maxInterval);
        }

        var c = sr.color;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        sr.color = c;
    }
}
