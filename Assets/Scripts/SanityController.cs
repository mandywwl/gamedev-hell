using UnityEngine;

// Core sanity stat: holds the 0..100 value, clamps it, and drives loss/gain from
// damage, hostile zones, safe zones, and items. Exposes events so the HUD / audio /
public class SanityController : MonoBehaviour
{
    public static SanityController Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private SanityConfig config;

    [Header("Runtime State")]
    [SerializeField] private float sanityCurrent = 100f;

    // Events for UI / audio / VFX.
    public System.Action<float, float> OnSanityChanged;                 // current, max
    public System.Action<SanityState, SanityState> OnSanityStateChanged; // old state , new state

    public SanityState CurrentState { get; private set; } = SanityState.Stable;

    // Basedd on the zone/lighting triggers will be combined with isInCombat to decide sanity passive drain.
    public bool InHostileZone { get; set; }

    // Passive tick timers (drain while hostile, recover while safe).
    private float drainTimer;
    private float recoveryTimer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSanity();
        }
        else
        {
            // Duplicate (e.g. per scene) - keep the canon/actual instance only.
            Destroy(this);
        }
    }

    void Start()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnDamageTaken += OnPlayerDamageTaken;
    }

    void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnDamageTaken -= OnPlayerDamageTaken;
    }

    private void InitializeSanity()
    {
        if (config == null)
        {
            Debug.LogWarning("SanityController: no SanityConfig assigned - sanity system is inert.");
            return;
        }

        sanityCurrent = config.maxSanity;
        CurrentState = GetSanityState(sanityCurrent);
        OnSanityChanged?.Invoke(sanityCurrent, config.maxSanity);
    }

    void Update()
    {
        if (config == null) return;
        TickPassiveSanity();
    }

    // Passive drain / recovery
    // Define areas as safe zone or hostile zone if theres time

    private void TickPassiveSanity()
    {
        bool hostile = InHostileZone || (PlayerStats.Instance != null && PlayerStats.Instance.isInCombat);

        if (hostile)
        {
            recoveryTimer = 0f;
            drainTimer += Time.deltaTime;
            if (drainTimer >= config.darknessDrainInterval)
            {
                drainTimer = 0f;
                ModifySanity(-config.darknessDrainPerTick, "Hostile zone");
            }
        }
        else
        {
            drainTimer = 0f;
            recoveryTimer += Time.deltaTime;
            if (recoveryTimer >= config.safeZoneRecoveryInterval)
            {
                recoveryTimer = 0f;
                ModifySanity(config.safeZoneRecoveryPerTick, "Safe zone");
            }
        }
    }

    // --- Event hooks ---

    private void OnPlayerDamageTaken(float damage)
    {
        if (config == null) return;
        float loss = Mathf.Clamp(damage * config.damageSanityLossPerHP,
            config.minDamageSanityLoss, config.maxDamageSanityLoss);
        ModifySanity(-loss, "Took damage");
    }

    // --- Public API ---

    public void ModifySanity(float amount, string reason = "")
    {
        SetSanity(sanityCurrent + amount, reason);
    }

    public void SetSanity(float value, string reason = "")
    {
        if (config == null) return;

        float newValue = Mathf.Clamp(value, 0f, config.maxSanity);
        if (Mathf.Approximately(newValue, sanityCurrent)) return;

        sanityCurrent = newValue;
        OnSanityChanged?.Invoke(sanityCurrent, config.maxSanity);

        SanityState newState = GetSanityState(sanityCurrent);
        if (newState != CurrentState)
        {
            SanityState oldState = CurrentState;
            CurrentState = newState;
            OnSanityStateChanged?.Invoke(oldState, newState);

            string suffix = string.IsNullOrEmpty(reason) ? "" : $" [{reason}]";
            Debug.Log($"Sanity state changed: {oldState} -> {newState} ({sanityCurrent:F0}/{config.maxSanity:F0}){suffix}");
        }
    }

    // Convenience for item consumption (called by PlayerStats.UseConsumable).
    public void RestoreSanity(float amount)
    {
        ModifySanity(amount, "Item");
    }

    // Flat sanity cost for surviving a battle (win or flee) - the mental toll of combat.
    public void ApplyBattleEndSanityLoss()
    {
        if (config == null) return;
        ModifySanity(-config.battleEndSanityLoss, "Battle ended");
    }

    public float GetSanityCurrent() => sanityCurrent;

    public float GetSanityMax() => config != null ? config.maxSanity : 0f;

    public float GetSanityPercent()
    {
        if (config == null || config.maxSanity <= 0f) return 1f;
        return sanityCurrent / config.maxSanity;
    }

    public SanityState GetSanityState(float value)
    {
        if (config == null) return SanityState.Stable;

        if (value >= config.stableMin) return SanityState.Stable;
        if (value >= config.shakenMin) return SanityState.Shaken;
        if (value >= config.disturbedMin) return SanityState.Disturbed;
        if (value >= config.unstableMin) return SanityState.Unstable;
        return SanityState.Breakdown;
    }

    public SanityConfig GetConfig() => config;

    public SanityCombatModifiers GetCurrentCombatModifiers()
    {
        return config != null ? config.GetModifiers(CurrentState) : new SanityCombatModifiers();
    }
}
