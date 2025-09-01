using UnityEngine;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity = 100f;
    public float sanityDecayRate = 0.1667f; // Decays to 0 in 10 minutes

    [Header("Audio")]
    public AudioSource lowSanityAudio;
    public AudioSource sanityLossSFX;

    [Header("Thresholds")]
    public float debuffThreshold = 50f;
    public float criticalThreshold = 20f;

    private bool isDebuffed = false;
    private bool isCritical = false;

    private PlayerController playerController;
    private CombatSystem combatSystem;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        combatSystem = FindObjectOfType<CombatSystem>();
        currentSanity = maxSanity;
    }

    void Update()
    {
        DecaySanityOverTime(Time.deltaTime);
    }

    public void ModifySanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0, maxSanity);

        if (amount < 0 && sanityLossSFX != null && Mathf.Abs(amount) >= 5f)
            sanityLossSFX.Play();

        CheckDebuffStates();
    }

    public void DecaySanityOverTime(float deltaTime)
    {
        ModifySanity(-sanityDecayRate * deltaTime);
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
            playerController.SetSanitySpeedFactor(0.8f); // Reduce movement speed

        if (lowSanityAudio != null && !lowSanityAudio.isPlaying)
            lowSanityAudio.Play();
    }

    private void ClearDebuff()
    {
        if (playerController != null)
            playerController.SetSanitySpeedFactor(1f); // Recover movement speed

        if (lowSanityAudio != null && lowSanityAudio.isPlaying)
            lowSanityAudio.Stop();
    }

    private void TriggerCritical()
    {
        if (playerController != null)
            playerController.SetSanitySpeedFactor(0.6f); // Further reduce movement speed

      

    private void ClearCritical()
    {
        if (playerController != null)
            playerController.SetSanitySpeedFactor(isDebuffed ? 0.8f : 1f); // Restore based on debuff state

        e
    }
}
