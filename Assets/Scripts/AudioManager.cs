using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource uiSource; // for all UI sounds
    [SerializeField] private AudioSource[] sfxSources; // for all non-UI sounds

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

    // Method for SFX
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        // Find available AudioSource in the pool that isn't currently playing
        foreach (var source in sfxSources)
        {
            if (!source.isPlaying)
            {
                source.PlayOneShot(clip);
                return;
            }
        }
        
        // Optional fallback: If all sources busy, play on the first one anyway.
        if (sfxSources.Length > 0)
        {
            sfxSources[0].PlayOneShot(clip);
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