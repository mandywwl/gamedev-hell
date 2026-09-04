using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Fades the assigned roof tilemap(s) out while the player is inside this trigger area, and
// back in when they leave, so roofs don't permanently block the view of what's underneath.
// Place on a hand-drawn trigger collider covering the walkable area under a roof section.
public class RoofVisibility : MonoBehaviour
{
    public List<TilemapRenderer> roofRenderers;

    [Header("Fade Settings")]
    [Range(0f, 1f)] public float hiddenAlpha = 0f;
    public float fadeDuration = 0.25f;

    private int occupantCount = 0;
    private Coroutine fadeCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        occupantCount++;
        if (occupantCount == 1) StartFade(hiddenAlpha);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        occupantCount = Mathf.Max(0, occupantCount - 1);
        if (occupantCount == 0) StartFade(1f);
    }

    private void StartFade(float targetAlpha)
    {
        if (!isActiveAndEnabled)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        var tilemaps = new List<Tilemap>();
        var startAlphas = new List<float>();
        foreach (var renderer in roofRenderers)
        {
            if (renderer == null) continue;
            var tilemap = renderer.GetComponent<Tilemap>();
            if (tilemap == null) continue;
            tilemaps.Add(tilemap);
            startAlphas.Add(tilemap.color.a);
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            for (int i = 0; i < tilemaps.Count; i++)
            {
                var c = tilemaps[i].color;
                c.a = Mathf.Lerp(startAlphas[i], targetAlpha, t);
                tilemaps[i].color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var tilemap in tilemaps)
        {
            var c = tilemap.color;
            c.a = targetAlpha;
            tilemap.color = c;
        }

        fadeCoroutine = null;
    }
}
