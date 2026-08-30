using UnityEngine;

// Sanity states, from the top being most stable while 0 is breakdown.  The int values will match up with 
// SanityConfig.stateModifiers array (Stable = 0 ... Breakdown = 4).
public enum SanityState
{
    Stable = 0,     // 76-100
    Shaken = 1,     // 51-75
    Disturbed = 2,  // 26-50
    Unstable = 3,   // 1-25
    Breakdown = 4   // 0
}

// Tunable debuffs applied per sanity state. These are read by
// SanityController.GetCurrentCombatModifiers() so combat/aim systems will act dynamically
[System.Serializable]
public class SanityCombatModifiers
{
    [Tooltip("Extra aim spread / recoil (0 = none).")]
    public float aimInstability = 0f;

    [Tooltip("Reload time multiplier (1 = normal, >1 = slower).")]
    public float reloadMultiplier = 1f;

    [Tooltip("Extra stamina drain per second.")]
    public float staminaDrain = 0f;

    [Tooltip("Incoming critical-hit vulnerability (0 = none, 0.25 = +25% chance).")]
    public float incomingCritVulnerability = 0f;

    [Tooltip("Brief attack lockout in seconds (0 = none).")]
    public float panicLockoutDuration = 0f;
}

[CreateAssetMenu(fileName = "SanityConfig", menuName = "Scriptable Objects/SanityConfig")]
public class SanityConfig : ScriptableObject
{
    [Header("Core")]
    [Tooltip("Maximum sanity. Current sanity is clamped to [0, maxSanity].")]
    public float maxSanity = 100f;

    [Header("State Thresholds (minimum value for each state)")]
    [Tooltip("Sanity at or above this value is Stable.")]
    public float stableMin = 76f;
    [Tooltip("Sanity at or above this value is Shaken.")]
    public float shakenMin = 51f;
    [Tooltip("Sanity at or above this value is Disturbed.")]
    public float disturbedMin = 26f;
    [Tooltip("Sanity at or above this value is Unstable (0 is Breakdown).")]
    public float unstableMin = 1f;

    [Header("State Colors")]
    public Color stableColor = new Color(0.36f, 0.83f, 0.36f);
    public Color shakenColor = new Color(0.95f, 0.77f, 0.06f);
    public Color disturbedColor = new Color(0.98f, 0.55f, 0.10f);
    public Color unstableColor = new Color(0.91f, 0.24f, 0.16f);
    public Color breakdownColor = new Color(0.55f, 0.05f, 0.05f);

    [Header("Loss Sources")]
    [Tooltip("Sanity lost per point of HP damage taken.")]
    public float damageSanityLossPerHP = 0.3f;
    [Tooltip("Clamp range for a single damage-triggered sanity loss.")]
    public float minDamageSanityLoss = 3f;
    public float maxDamageSanityLoss = 8f;

    [Tooltip("Sanity lost when the player survives a battle (win or flee).")]
    public float battleEndSanityLoss = 10f;

    [Tooltip("Sanity drained per tick while in a hostile zone / darkness.")]
    public float darknessDrainPerTick = 1f;
    [Tooltip("Seconds between passive drain ticks while hostile.")]
    public float darknessDrainInterval = 3f;

    [Header("Gain Sources")]
    [Tooltip("Sanity recovered per tick while safe / out of combat.")]
    public float safeZoneRecoveryPerTick = 2f;
    [Tooltip("Seconds between passive recovery ticks while safe.")]
    public float safeZoneRecoveryInterval = 3f;

    [Header("Combat Modifiers (indexed by SanityState)")]
    [Tooltip("Order: Stable, Shaken, Disturbed, Unstable, Breakdown.")]
    public SanityCombatModifiers[] stateModifiers;

    // Pre-populate the modifier array with reasonable default values when the asset is first
    // created, so a designer opens a ready-to-tune list instead of an empty one.
    private void Reset()
    {
        int count = System.Enum.GetValues(typeof(SanityState)).Length;
        stateModifiers = new SanityCombatModifiers[count];
        for (int i = 0; i < count; i++)
            stateModifiers[i] = DefaultModifiers((SanityState)i);
    }

    public SanityCombatModifiers GetModifiers(SanityState state)
    {
        int index = (int)state;
        if (stateModifiers != null && index >= 0 && index < stateModifiers.Length && stateModifiers[index] != null)
            return stateModifiers[index];
        return DefaultModifiers(state); // graceful fallback if the array is unassigned/short
    }

    public Color GetStateColor(SanityState state)
    {
        switch (state)
        {
            case SanityState.Shaken: return shakenColor;
            case SanityState.Disturbed: return disturbedColor;
            case SanityState.Unstable: return unstableColor;
            case SanityState.Breakdown: return breakdownColor;
            default: return stableColor;
        }
    }

    public string GetStateLabel(SanityState state) => state.ToString();

    private static SanityCombatModifiers DefaultModifiers(SanityState state)
    {
        var m = new SanityCombatModifiers();
        switch (state)
        {
            case SanityState.Shaken:
                m.aimInstability = 0.15f;
                m.staminaDrain = 2f;
                break;
            case SanityState.Disturbed:
                m.aimInstability = 0.30f;
                m.reloadMultiplier = 1.25f;
                m.staminaDrain = 3f;
                break;
            case SanityState.Unstable:
                m.aimInstability = 0.50f;
                m.reloadMultiplier = 1.5f;
                m.staminaDrain = 5f;
                m.incomingCritVulnerability = 0.25f;
                break;
            case SanityState.Breakdown:
                m.aimInstability = 0.75f;
                m.reloadMultiplier = 2f;
                m.staminaDrain = 8f;
                m.incomingCritVulnerability = 0.4f;
                m.panicLockoutDuration = 1.5f;
                break;
            // Stable: all defaults (no penalties).
        }
        return m;
    }
}
