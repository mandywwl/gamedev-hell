using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I;

    [Header("Mixer routing")]
    public AudioMixer mixer;
    public AudioMixerSnapshot defaultSnapshot;

    [Header("Sources (assign in Inspector)")]
    public AudioSource musicA, musicB;   // 2D → Music
    public AudioSource ambA, ambB;       // 2D → Ambience
    public AudioSource uiSource;         // 2D → UI
    public List<AudioSource> sfxPool;    // 3D → SFX

    AudioSource activeMusic, inactiveMusic, activeAmb, inactiveAmb;

    void Awake() {
        if (I != null) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
        activeMusic = musicA; inactiveMusic = musicB;
        activeAmb   = ambA;   inactiveAmb = ambB;
    }

    public void PlayUI(AudioClip clip, float vol=1f) {
        if (clip) uiSource.PlayOneShot(clip, vol);
    }

    public void PlaySFX(AudioClip clip, Vector3 pos, float vol=1f) {
        if (!clip || sfxPool.Count == 0) return;
        var src = sfxPool.Find(s => !s.isPlaying) ?? sfxPool[0];
        src.transform.position = pos;
        src.PlayOneShot(clip, vol);
    }

    public void PlayMusic(AudioClip clip, float fade=1f, bool loop=true) {
        if (!clip) return; StartCoroutine(Crossfade(activeMusic, inactiveMusic, clip, fade, loop));
        (activeMusic, inactiveMusic) = (inactiveMusic, activeMusic);
    }

    public void PlayAmbience(AudioClip clip, float fade=0.5f, bool loop=true) {
        if (!clip) return; StartCoroutine(Crossfade(activeAmb, inactiveAmb, clip, fade, loop));
        (activeAmb, inactiveAmb) = (inactiveAmb, activeAmb);
    }

    public void GoToSnapshot(AudioMixerSnapshot snap, float t=0.5f) {
        (snap ?? defaultSnapshot)?.TransitionTo(t);
    }

    IEnumerator Crossfade(AudioSource from, AudioSource to, AudioClip next, float t, bool loop) {
        to.clip = next; to.loop = loop; to.volume = 0f; to.Play();
        float a = 0f;
        while (a < t) { a += Time.deltaTime; float k = a/t; from.volume = 1f-k; to.volume = k; yield return null; }
        from.Stop(); from.volume = 1f; to.volume = 1f;
    }
}
