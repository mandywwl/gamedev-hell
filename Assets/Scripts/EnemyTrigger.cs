using UnityEngine;
using UnityEngine.SceneManagement;
using static BattleSystem;

public class BossEncounter : MonoBehaviour
{
    [SerializeField] string combatSceneName = "Combat";
    [SerializeField] int bossIndex = 2;  // which enemy in BattleSystem.enemyPrefabs

    bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player")) 
        {   
            Debug.Log("Entered boss trigger!"); 
        }

        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

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
        public static int enemyIndex = -1;
        public static Vector3 returnPosition;

        public static BattleSystem.EncounterKind encounterKind = BattleSystem.EncounterKind.HumanoidEnemy;
    }
}
