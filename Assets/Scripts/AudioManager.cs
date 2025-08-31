using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource uiSource; // for all UI sounds

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    void Awake()
    {
        // Standard singleton setup
        if (I != null && I != this)
        {
            Destroy(gameObject);
        }
        else
        {
            I = this;
            DontDestroyOnLoad(gameObject); // persist across scenes
        }
    }

    // Method to call for playing UI sounds
    public void PlayUI(AudioClip clip)
    {
        if (clip != null)
        {
            uiSource.PlayOneShot(clip); // Use for overlapping UI sounds like hovers and clicks
        }
    }

    // Template for playing music // TODO: Expand?
    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }
}