using UnityEngine;
using UnityEngine.SceneManagement;
using static BattleSystem;

public class BossEncounter : MonoBehaviour
{
    [Header("Enemy Configuration")]
    [SerializeField] string uniqueEnemyId = "boss_1"; // <-- GIVE EACH ENEMY A UNIQUE ID IN THE INSPECTOR
    [SerializeField] string combatSceneName = "Combat";
    [SerializeField] int bossIndex = 2;  // which enemy in BattleSystem.enemyPrefabs

    bool triggered;

    void Start()
    {
        // Disable enemy trigger if enemy has already been defeated
        if (GameState.I != null && GameState.I.defeatedEnemies.Contains(uniqueEnemyId))
        {
            if (transform.parent != null)
                transform.parent.gameObject.SetActive(false); // Disable 'Enemy' GameObject
            else
                gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Entered boss trigger!");
        StartCombat(other.transform.position);
    }

    // Shared entry point for anything that spots the player - the trigger collider above
    // (touch) and EnemyPatrol's vision check (seen while patrolling) both funnel here.
    public void StartCombat(Vector3 playerPosition)
    {
        if (triggered) return;
        triggered = true;

        BattleTransfer.enemyId = uniqueEnemyId;

        // pass data to battle
        if (bossIndex == 2)
        {
            BattleTransfer.encounterKind = EncounterKind.BossEnemy;
        }
        else if (bossIndex == 1)
        {
            BattleTransfer.encounterKind = EncounterKind.MutantEnemy;
        }
        else if (bossIndex == 0)
        {
            BattleTransfer.encounterKind = EncounterKind.HumanoidEnemy;
        }
        BattleTransfer.returnSceneName = SceneManager.GetActiveScene().name;
        BattleTransfer.returnPosition = playerPosition;

        SceneTransition.I.LoadBattleScene(combatSceneName);
    }

    public static class BattleTransfer
    {
        public static string returnSceneName;
        public static string enemyId; // which enemy was fought
        // public static int enemyIndex = -1;
        public static Vector3 returnPosition;

        public static BattleSystem.EncounterKind encounterKind = BattleSystem.EncounterKind.HumanoidEnemy;
    }
}
