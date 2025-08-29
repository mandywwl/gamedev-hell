using UnityEngine;
using System.Collections;
using TMPro;
using static BossEncounter;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, FLED, BUSY}

public class BattleSystem : MonoBehaviour
{
    [Header("Background")]
    public BackgroundManager backgroundManager;

    private GameObject enemyGO;
    private Animator enemyAnim;
    private GameObject chosenEnemyPrefab;

    private GameObject playerGO;
    private Animator playerAnim;

    [Header("Player")]
    public GameObject playerPrefab;
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;
    [Header("Boss Prefab")]
    public GameObject bossPrefab;

    [Header("Spawn Areas")]
    public Transform playerBattleStation;
    public Transform enemyBattleStation;

    PlayerStats playerUnit;
    Unit enemyUnit;

    [Header("UI")]
    public TMP_Text dialogueText;
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;

    public BattleState state;

    //choose what type of enemy at runtime (default is random enemy)
    public enum EncounterKind { RandomEnemy, Boss }
    public EncounterKind encounterKind = EncounterKind.RandomEnemy;

    void Start()
    {
        if (BattleTransfer.encounterKind != EncounterKind.RandomEnemy)
        {
            encounterKind = BattleTransfer.encounterKind;
        }
        //please use these below for respecitve triggers!
        //BattleTransfer.encounterKind = EncounterKind.Boss;
        //BattleTransfer.encounterKind = EncounterKind.RandomEnemy;

        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        //load player
        playerGO = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = playerGO.GetComponent<PlayerStats>();

        //choose and load enemy
        if (encounterKind == EncounterKind.RandomEnemy)
        {
            // Pick one randomly
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            chosenEnemyPrefab = enemyPrefabs[randomIndex];
            backgroundManager.SetBackgroundToStreet();
        }
        else if (encounterKind == EncounterKind.Boss)
        {
            chosenEnemyPrefab = bossPrefab;
            backgroundManager.SetBackgroundToStore();
        }
        enemyGO = Instantiate(chosenEnemyPrefab, enemyBattleStation);
        enemyUnit = enemyGO.GetComponent<Unit>();

        dialogueText.text = "Encountered " + enemyUnit.unitName;

        playerHUD.SetPlayerHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);

        playerGO.transform.localScale = new Vector3(5, 5, 1);
        enemyGO.transform.localScale = new Vector3(5, 5, 1);

        enemyAnim = enemyGO.GetComponent<Animator>();
        enemyAnim.Play("Idle");

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void EndBattle()
    {
        if (state == BattleState.WON)
        {
            dialogueText.text = "You won!";
        }
        else if (state == BattleState.LOST)
        {
            dialogueText.text = "You are defeated.";
        }
        else if (state == BattleState.FLED)
        {
            dialogueText.text = "You have fled the battle!";
        }
    }

    IEnumerator PlayerAttack()
    {
        //Deal damage
        bool isDead = enemyUnit.TakeDamage(playerUnit.GetTotalAttackPower());
        enemyAnim.SetTrigger("Hurt");

        enemyHUD.SetHP(enemyUnit.currentHP);
        dialogueText.text = "Attack successful!";

        yield return new WaitForSeconds(2f);

        //check if enemy dead
        if (isDead)
        {
            //end battle
            enemyAnim.ResetTrigger("Attack");
            enemyAnim.ResetTrigger("Hurt");
            enemyAnim.SetTrigger("Die");
            yield return new WaitForSeconds(enemyAnim.GetCurrentAnimatorStateInfo(0).length);
            Destroy(enemyGO);

            state = BattleState.WON;
            EndBattle();
        }
        else
        {
            //enemy turn
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
        
    }

    IEnumerator EnemyTurn()
    {
        dialogueText.text = enemyUnit.unitName + " attacks!";
        enemyAnim.SetTrigger("Attack");

        yield return new WaitForSeconds(1f);

        bool isDead = playerUnit.TakeDamage(enemyUnit.damage,true);

        playerHUD.SetHP(playerUnit.currentHP);

        yield return new WaitForSeconds(1f);

        if (isDead)
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    IEnumerator PlayerFlee()
    {
        dialogueText.text = "You chose to flee!";

        yield return new WaitForSeconds(2f);

        state = BattleState.FLED;
        EndBattle();
    }

    void PlayerTurn()
    {
        dialogueText.text = "Choose an action: ";
    }

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            Debug.Log("Not your turn!");
            return;
        }

        state = BattleState.BUSY;
        StartCoroutine(PlayerAttack());
    }

    public void OnItemButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            Debug.Log("Not your turn!");
            return;
        }
        //go to inventory UI here

    }

    public void OnRunButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            Debug.Log("Not your turn!");
            return;
        }

        StartCoroutine(PlayerFlee());
    }
}
