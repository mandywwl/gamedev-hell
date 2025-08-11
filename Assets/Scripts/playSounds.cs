using UnityEngine;

public class playSounds : MonoBehaviour
{
    public AudioClip hoverSound;
    public AudioClip selectSound;

    AudioSource sound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sound = GetComponent<AudioSource>();
    }

    public void PlayHoverSound()
    {
        sound.PlayOneShot(hoverSound);
    }

    public void PlaySelectSound()
    {
        sound.PlayOneShot(selectSound);
    }

}
