using UnityEngine;
using UnityEngine.UI;
using System;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity;
    public float sanityDecayRate = 1f;

    [Header("UI Elements")]
    public Slider sanitySlider;
    public Image sanityOverlay;

    [Header("Audio")]
    public AudioSource lowSanityAudio;
    public AudioSource sanityLossSFX;

    [Header("Thresholds")]
    public float debuffThreshold = 50f;
    public float criticalThreshold = 20f;

    private bool isDebuffed = false;
    private bool isCritical = false;

    private PlayerController playerController;
    private Camera mainCam;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        mainCam = Camera.main;
        currentSanity = maxSanity;
        UpdateUI();
    }

    public void ModifySanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0, maxSanity);

        if (amount < 0 && sanityLossSFX != null && Mathf.Abs(amount) >= 5f)
            sanityLossSFX.Play();

        UpdateUI();
        CheckDebuffStates();
    }

    public void DecaySanityOverTime(float deltaTime)
    {
        ModifySanity(-sanityDecayRate * deltaTime);
    }
    
    private void UpdateUI()
    {
        if (sanitySlider != null)
            sanitySlider.value = currentSanity / maxSanity;

        if (sanityOverlay != null)
        {
            float alpha = Mathf.InverseLerp(maxSanity, 0, currentSanity);
            Color c = sanityOverlay.color;
            c.a = Mathf.Lerp(0f, 0.6f, alpha);
            sanityOverlay.color = c;
        }
    }

    private void CheckDebuffStates()
    {
        if (currentSanity < debuffThreshold && !isDebuffed)
        {
            TriggerDebuff();
            isDebuffed = true;
        }
        else if (currentSanity >= debuffThreshold && isDebuffed)
        {
            ClearDebuff();
            isDebuffed = false;
        }
        if (currentSanity < criticalThreshold && isCritical)
        {
            TriggerCritical();
            isCritical = true;
        }
        else if (currentSanity >= criticalThreshold && isCritical)
        {
            ClearCritical();
            isCritical = false;
        }
    }

    private void TriggerDebuff()
    {
        
    }

    private void ClearDebuff()
    {

    }

    private void TriggerCritical()
    {
        
    }

    private void ClearCritical()
    {
        
    }
}

