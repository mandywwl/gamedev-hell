using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }
    public string unitName;

    [Header("Base Stats")]
    public float maxHP = 100f;
    public float baseAttackPower = 10f;
    public float baseDefensePower = 5f;

    [Header("Current Stats")]
    public float currentHP = 100f;

    [Header("Combat Modifiers")]
    [SerializeField] private float equipmentAttackBonus = 0f;
    [SerializeField] private float equipmentDefenseBonus = 0f;

    // Combat-specific temporary stats
    [System.NonSerialized] public float combatHP;
    [System.NonSerialized] public bool isInCombat = false;

    // Events for UI updates
    public System.Action<float, float> OnHPChanged; // current, max
    public System.Action<float> OnAttackPowerChanged;
    public System.Action<float> OnDefensePowerChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Listen to equipment changes to update combat stats
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnEquipmentChanged += OnEquipmentChanged;
        }

        // Calculate initial equipment bonuses
        UpdateEquipmentBonuses();
    }

    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnEquipmentChanged -= OnEquipmentChanged;
        }
    }

    private void InitializeStats()
    {
        currentHP = maxHP;
        combatHP = currentHP;

        // Trigger initial UI updates
        OnHPChanged?.Invoke(currentHP, maxHP);
        OnAttackPowerChanged?.Invoke(GetTotalAttackPower());
        OnDefensePowerChanged?.Invoke(GetTotalDefensePower());
    }

    #region Combat State Management

    /// Call this when combat starts - saves current stats as combat 
    public void StartCombat()
    {
        isInCombat = true;
        combatHP = currentHP;

        Debug.Log($"Combat started! Player HP: {combatHP:F1}, Attack: {GetTotalAttackPower():F1}, Defense: {GetTotalDefensePower():F1}");
    }

    /// Call this when combat ends - saves combat stats back to persistent 
    public void EndCombat()
    {
        if (isInCombat)
        {
            currentHP = combatHP;
            isInCombat = false;

            // Trigger UI updates with final values
            OnHPChanged?.Invoke(currentHP, maxHP);

            Debug.Log($"Combat ended! Final HP: {currentHP:F1}");
        }
    }

    /// Get current HP based on combat state    
    public float GetCurrentHP()
    {
        return isInCombat ? combatHP : currentHP;
    }

    #endregion


    #region Equipment Integration

    private void OnEquipmentChanged(ItemType equipmentType, ItemStack itemStack)
    {
        UpdateEquipmentBonuses();
    }

    private void UpdateEquipmentBonuses()
    {
        equipmentAttackBonus = 0f;
        equipmentDefenseBonus = 0f;

        if (InventorySystem.Instance == null) return;

        // Calculate attack bonus from equipped weapons
        foreach (ItemType weaponType in System.Enum.GetValues(typeof(ItemType)))
        {
            ItemStack weaponStack = InventorySystem.Instance.GetEquippedItem(weaponType);
            if (weaponStack != null && weaponStack.item.category == ItemCategory.Weapons)
            {
                // Only count weapon if it's not broken
                if (!weaponStack.item.IsBroken())
                {
                    equipmentAttackBonus += weaponStack.item.attackPower;
                }
            }
        }

        // Calculate defense bonus from equipped armor
        foreach (ItemType armorType in System.Enum.GetValues(typeof(ItemType)))
        {
            ItemStack armorStack = InventorySystem.Instance.GetEquippedItem(armorType);
            if (armorStack != null && armorStack.item.category == ItemCategory.Armor)
            {
                // Only count armor if it's not broken
                if (!armorStack.item.IsBroken())
                {
                    equipmentDefenseBonus += armorStack.item.defensePower;
                }
            }
        }

        // Trigger UI updates
        OnAttackPowerChanged?.Invoke(GetTotalAttackPower());
        OnDefensePowerChanged?.Invoke(GetTotalDefensePower());

        Debug.Log($"Equipment bonuses updated - Attack: +{equipmentAttackBonus:F1}, Defense: +{equipmentDefenseBonus:F1}");
    }

    #endregion

    #region Combat Stats Calculation

    /// Get total attack power (base + equipment bonuses)    
    public float GetTotalAttackPower()
    {
        return baseAttackPower + equipmentAttackBonus;
    }

    /// Get total defense power (base + equipment bonuses
    public float GetTotalDefensePower()
    {
        return baseDefensePower + equipmentDefenseBonus;
    }

    /// Get attack power for a specific equipped 
    public float GetWeaponAttackPower(ItemType weaponType)
    {
        if (InventorySystem.Instance != null)
        {
            ItemStack weaponStack = InventorySystem.Instance.GetEquippedItem(weaponType);
            if (weaponStack != null && weaponStack.item.category == ItemCategory.Weapons && !weaponStack.item.IsBroken())
            {
                return baseAttackPower + weaponStack.item.attackPower;
            }
        }
        return baseAttackPower; // Return base attack if no weapon equipped
    }

    #endregion

    #region Combat Actions


    /// Take damage during combat - applies defense reduction
    public bool TakeDamage(float damage, bool isInCombat)
    {
        float actualDamage = Mathf.Max(1f, damage - GetTotalDefensePower()); // Minimum 1 damage

        if (isInCombat)
        {
            combatHP = Mathf.Max(0f, combatHP - actualDamage);
            Debug.Log($"Player took {actualDamage:F1} damage (reduced from {damage:F1} by {GetTotalDefensePower():F1} defense). HP: {combatHP:F1}/{maxHP:F1}");
            currentHP = combatHP;
        }
        else
        {
            currentHP = Mathf.Max(0f, currentHP - actualDamage);
            OnHPChanged?.Invoke(currentHP, maxHP);
            Debug.Log($"Player took {actualDamage:F1} damage. HP: {currentHP:F1}/{maxHP:F1}");
        }

        return GetCurrentHP() <= 0f; // Return true if player is dead
    }


    /// Heal HP during combat or outside
    public void Heal(float amount)
    {
        if (isInCombat)
        {
            combatHP = Mathf.Min(maxHP, combatHP + amount);
            Debug.Log($"Player healed {amount:F1} HP during combat. HP: {combatHP:F1}/{maxHP:F1}");
        }
        else
        {
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            OnHPChanged?.Invoke(currentHP, maxHP);
            Debug.Log($"Player healed {amount:F1} HP. HP: {currentHP:F1}/{maxHP:F1}");
        }
    }


    // Use a consumable item and apply its effects
    public bool UseConsumable(Item consumableItem)
    {
        if (!consumableItem.isConsumable) return false;

        if (InventorySystem.Instance != null && InventorySystem.Instance.HasItem(consumableItem, 1))
        {
            // Remove item from inventory
            InventorySystem.Instance.RemoveItem(consumableItem, 1);

            // Apply effects using float values from Item
            if (consumableItem.healingAmount > 0f)
                Heal(consumableItem.healingAmount);

            Debug.Log($"Used {consumableItem.itemName}!");
            return true;
        }

        return false;
    }
    
    #endregion
    
    #region Status Checks
 
    /// Check if player is dead   
    public bool IsDead()
    {
        return GetCurrentHP() <= 0f;
    }

    /// Check if player is at low health
    public bool IsLowHealth(float threshold = 0.25f)
    {
        return GetCurrentHP() / maxHP <= threshold;
    }

    /// Get health percentage
    public float GetHealthPercentage()
    {
        return GetCurrentHP() / maxHP;
    }

    #endregion

    #region Save/Load Support

    [System.Serializable]
    public class PlayerStatsData
    {
        public float currentHP;
        public float maxHP;
        public float baseAttackPower;
        public float baseDefensePower;
    }

    public PlayerStatsData GetSaveData()
    {
        return new PlayerStatsData
        {
            currentHP = this.currentHP,
            maxHP = this.maxHP,
            baseAttackPower = this.baseAttackPower,
            baseDefensePower = this.baseDefensePower
        };
    }

    public void LoadSaveData(PlayerStatsData data)
    {
        currentHP = data.currentHP;
        maxHP = data.maxHP;
        baseAttackPower = data.baseAttackPower;
        baseDefensePower = data.baseDefensePower;

        // Update combat stats if in combat
        if (isInCombat)
        {
            combatHP = currentHP;
        }

        // Update equipment bonuses
        UpdateEquipmentBonuses();

        // Trigger UI updates
        OnHPChanged?.Invoke(currentHP, maxHP);
        OnAttackPowerChanged?.Invoke(GetTotalAttackPower());
        OnDefensePowerChanged?.Invoke(GetTotalDefensePower());
    }
    #endregion
}