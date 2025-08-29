using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    public Image backgroundCanvas;      
    public Sprite streetSprite; 
    public Sprite storeSprite;

    public void SetBackgroundToStreet()
    {
        backgroundCanvas.sprite = streetSprite;
    }

    public void SetBackgroundToStore()
    {
        backgroundCanvas.sprite = storeSprite;
    }
}
