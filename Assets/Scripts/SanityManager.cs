using UnityEngine;
using System;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    [SerializeField] private float currentSanity = 100f;
    public float sanityDecayRate = 1f; 

    [Header("Thresholds")]
    public float debuffThreshold = 50f;
    public float criticalThreshold = 20f;


    private bool isDebuffed = false;
    private bool isCritical = false;
    private bool hasPlayedLowSanityAudio = false;

    private HallucinationController hallucinationController;

    private PlayerController playerController;

    public Action<float, float> OnSanityChanged; 

    public float CurrentSanity => currentSanity;

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        hallucinationController = FindFirstObjectByType<HallucinationController>();
        currentSanity = maxSanity;

        if (AudioManager.I != null)
            AudioManager.I.StopSanityWarning();

        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    void Update()
    {
        DecaySanityOverTime(Time.deltaTime);
    }

    public void ModifySanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0, maxSanity);
        CheckDebuffStates();
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    public void RestoreSanity(float amount)
    {
        currentSanity = Mathf.Min(currentSanity + amount, maxSanity);
        CheckDebuffStates();
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    public void DecaySanityOverTime(float deltaTime)
    {
        ModifySanity(-sanityDecayRate * deltaTime);
    }

    private void CheckDebuffStates()
    {
        // Debuff state
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

        // Critical state
        if (currentSanity < criticalThreshold && !isCritical)
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
        if (playerController != null)
            playerController.SetSanitySpeedFactor(0.8f);

        if (!hasPlayedLowSanityAudio && AudioManager.I != null)
        {
            AudioManager.I.PlaySanityWarning();
            hasPlayedLowSanityAudio = true;
        }
    }

    private void ClearDebuff()
    {
        if (playerController != null)
            playerController.SetSanitySpeedFactor(1f);

        if (AudioManager.I != null)
            AudioManager.I.StopSanityWarning();

        hasPlayedLowSanityAudio = false;
    }

    private void TriggerCritical()
    {
        if (playerController != null)
            playerController.SetSanitySpeedFactor(0.6f);

        if (hallucinationController != null)
            hallucinationController.enabled = true;
    }

    private void ClearCritical()
    {
        if (playerController != null)
            playerController.SetSanitySpeedFactor(isDebuffed ? 0.8f : 1f);


        if (hallucinationController != null)
            hallucinationController.enabled = false;
    }
}
