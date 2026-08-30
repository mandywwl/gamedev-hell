using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition I { get; private set; }

    [Header("UI")]
    [SerializeField] private Image fadeImage;

    [Header("URP Volume")]
    [SerializeField] private Volume transitionVolume;

    [Header("Transition Settings")]
    [SerializeField] private float distortionDuration = 0.9f;
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Distortion")]
    [SerializeField] private float maxLensDistortion = -0.4f;
    [SerializeField] private float maxChromaticAberration = 0.4f;

    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;

    private bool transitioning;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        SetupVolume();

        SetFade(0f);
        ResetDistortion();
    }

    private void SetupVolume()
    {
        if (transitionVolume == null)
        {
            Debug.LogError("SceneTransition: Transition Volume is not assigned.");
            return;
        }

        if (transitionVolume.profile == null)
        {
            Debug.LogError("SceneTransition: Volume has no Profile.");
            return;
        }

        transitionVolume.profile.TryGet(out lensDistortion);
        transitionVolume.profile.TryGet(out chromaticAberration);

        if (lensDistortion == null)
            Debug.LogError("SceneTransition: Lens Distortion override not found.");

        if (chromaticAberration == null)
            Debug.LogError("SceneTransition: Chromatic Aberration override not found.");
    }

    public void LoadBattleScene(string sceneName)
    {
        if (transitioning)
            return;

        StartCoroutine(BattleTransition(sceneName));
    }

    private IEnumerator BattleTransition(string sceneName)
    {
        transitioning = true;

        float elapsed = 0f;

        // --------------------------------
        // DISTORTION PHASE
        // --------------------------------

        while (elapsed < distortionDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / distortionDuration
            );

            // Smooth progression
            float intensity = Mathf.SmoothStep(0f, 1f, t);

            // Lens distortion
            if (lensDistortion != null)
            {
                lensDistortion.intensity.value =
                    Mathf.Lerp(
                        0f,
                        maxLensDistortion,
                        intensity
                    );
            }

            // Chromatic aberration
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value =
                    Mathf.Lerp(
                        0f,
                        maxChromaticAberration,
                        intensity
                    );
            }

            yield return null;
        }

        // --------------------------------
        // FADE TO BLACK
        // --------------------------------

        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / fadeDuration
            );

            SetFade(t);

            yield return null;
        }

        SetFade(1f);

        // Load combat scene while screen is black
        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        StartCoroutine(FadeIntoScene());
    }

    private IEnumerator FadeIntoScene()
    {
        // Reset distortion before revealing the new scene
        ResetDistortion();

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / fadeDuration
            );

            SetFade(1f - t);

            yield return null;
        }

        SetFade(0f);

        transitioning = false;
    }

    private void SetFade(float alpha)
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    private void ResetDistortion()
    {
        if (lensDistortion != null)
            lensDistortion.intensity.value = 0f;

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0f;
    }

    public void ReturnFromBattle(string sceneName)
    {
        if (transitioning)
            return;

        StartCoroutine(ReturnBattleTransition(sceneName));
    }

    private IEnumerator ReturnBattleTransition(string sceneName)
    {
        transitioning = true;

        float elapsed = 0f;

        // -----------------------------
        // DISTORTION
        // -----------------------------

        while (elapsed < distortionDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / distortionDuration
            );

            float intensity =
                Mathf.SmoothStep(0f, 1f, t);

            if (lensDistortion != null)
            {
                lensDistortion.intensity.value =
                    Mathf.Lerp(
                        0f,
                        maxLensDistortion,
                        intensity
                    );
            }

            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value =
                    Mathf.Lerp(
                        0f,
                        maxChromaticAberration,
                        intensity
                    );
            }

            yield return null;
        }

        // -----------------------------
        // FADE TO BLACK
        // -----------------------------

        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / fadeDuration
            );

            SetFade(t);

            yield return null;
        }

        SetFade(1f);

        // -----------------------------
        // LOAD PREVIOUS SCENE
        // -----------------------------

        SceneManager.sceneLoaded += OnReturnSceneLoaded;

        SceneManager.LoadScene(sceneName);
    }

    private void OnReturnSceneLoaded(
    Scene scene,
    LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnReturnSceneLoaded;

        StartCoroutine(FadeIntoReturnScene());
    }

    private IEnumerator FadeIntoReturnScene()
    {
        ResetDistortion();

        yield return null;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / fadeDuration
            );

            SetFade(1f - t);

            yield return null;
        }

        SetFade(0f);

        transitioning = false;
    }
}
