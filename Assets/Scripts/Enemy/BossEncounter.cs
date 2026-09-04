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

        // SceneTransition is a DontDestroyOnLoad singleton created by the first map scene;
        // if it is missing (or already mid-transition) the load below is a no-op, so don't
        // burn the encounter - leave it armed for the next time the player walks into it.
        if (SceneTransition.I == null)
        {
            Debug.LogError("BossEncounter: SceneTransition singleton not found - cannot start combat.");
            return;
        }

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

        // Only latch the encounter once the transition has actually been accepted.
        triggered = SceneTransition.I.LoadBattleScene(combatSceneName);
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
