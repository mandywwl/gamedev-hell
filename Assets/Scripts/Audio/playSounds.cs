using UnityEngine;

public class playSounds : MonoBehaviour
{
    public AudioClip uiHover;
    public AudioClip uiClick;

    public void PlayHoverSound()
    {
        if (uiHover) AudioManager.I.PlayUI(uiHover);
    }

    public void PlaySelectSound()
    {
        if (uiClick) AudioManager.I.PlayUI(uiClick);
    }

}
