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

    [Header("Sanity Audio")]
    [SerializeField] private AudioSource sanityWarningSource; // Optional dedicated source
    [SerializeField] private AudioClip sanityWarningClip; // Clip to play when sanity drops

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup ambienceGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;

    void OnValidate()
    {
        if (musicSource && musicGroup)     musicSource.outputAudioMixerGroup = musicGroup;
        if (ambienceSource && ambienceGroup) ambienceSource.outputAudioMixerGroup = ambienceGroup;
        if (uiSource && uiGroup)           uiSource.outputAudioMixerGroup = uiGroup;
        if (sfxSources != null && sfxGroup)
            foreach (var s in sfxSources) if (s) s.outputAudioMixerGroup = sfxGroup;
    }

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
        }
        else
        {
            I = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    // Method for SFX
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        foreach (var source in sfxSources)
        {
            if (!source.isPlaying)
            {
                source.PlayOneShot(clip);
                return;
            }
        }

        if (sfxSources.Length > 0)
        {
            sfxSources[0].PlayOneShot(clip);
        }
    }

    // Method for UI sounds
    public void PlayUI(AudioClip clip)
    {
        if (clip != null)
        {
            uiSource.PlayOneShot(clip);
        }
    }

    // Method for music
    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    // Sanity Warning Playback
    public void PlaySanityWarning()
    {
        if (sanityWarningSource != null && !sanityWarningSource.isPlaying)
        {
            sanityWarningSource.Play();
        }
        else if (sanityWarningClip != null)
        {
            PlaySFX(sanityWarningClip); // fallback to SFX pool
        }
    }

    public void StopSanityWarning()
    {
        if (sanityWarningSource != null && sanityWarningSource.isPlaying)
        {
            sanityWarningSource.Stop();
        }
    }
}
