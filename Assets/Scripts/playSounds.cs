using UnityEngine;

public class playSounds : MonoBehaviour
{
    public AudioClip hoverSound;
    public AudioClip selectSound;

    public void PlayHoverSound()
    {
        if (hoverSound != null)
            AudioManager.I.PlayUI(hoverSound);
    }

    public void PlaySelectSound()
    {
        if (selectSound != null)
            AudioManager.I.PlayUI(selectSound);
    }

}
