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
            transform.parent.gameObject.SetActive(false); // Disable 'Enemy' GameObject
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player")) 
        {   
            Debug.Log("Entered boss trigger!"); 
        }

        if (triggered) return;
        if (!other.CompareTag("Player")) return;

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
        BattleTransfer.returnPosition = other.transform.position;

        SceneManager.LoadScene(combatSceneName);
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
