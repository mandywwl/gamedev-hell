using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    public Image backgroundCanvas;
    public Sprite streetSprite;
    public Sprite storeSprite;

    // Optional: a world-space background behind the player/enemy sprites. The UI Canvas
    // Image above draws in a later render pass than world-space SpriteRenderers, so it can
    // visually cover the combat sprites. Leave unassigned to keep current behavior unchanged;
    // assign a SpriteRenderer positioned/scaled behind the battle stations to fix that.
    public SpriteRenderer worldBackgroundRenderer;

    public void SetBackgroundToStreet()
    {
        backgroundCanvas.sprite = streetSprite;
        if (worldBackgroundRenderer != null) worldBackgroundRenderer.sprite = streetSprite;
    }

    public void SetBackgroundToStore()
    {
        backgroundCanvas.sprite = storeSprite;
        if (worldBackgroundRenderer != null) worldBackgroundRenderer.sprite = storeSprite;
    }
}
