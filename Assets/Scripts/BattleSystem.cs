using UnityEngine;
using System.Collections;
using TMPro;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, FLED}

public class BattleSystem : MonoBehaviour
{

    private GameObject enemyGO;
    private Animator enemyAnim;

    private GameObject playerGO;
    private Animator playerAnim;

    public GameObject playerPrefab;
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    public Transform playerBattleStation;
    public Transform enemyBattleStation;

    Unit playerUnit;
    Unit enemyUnit;

    public TMP_Text dialogueText;

    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;

    public BattleState state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        // Pick one randomly
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject chosenEnemyPrefab = enemyPrefabs[randomIndex];

        playerGO = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = playerGO.GetComponent<Unit>();

        enemyGO = Instantiate(chosenEnemyPrefab, enemyBattleStation);
        enemyUnit = enemyGO.GetComponent<Unit>();

        dialogueText.text = "Encountered " + enemyUnit.unitName;

        playerHUD.SetHUD(playerUnit);
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
        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
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

        bool isDead = playerUnit.TakeDamage(enemyUnit.damage);

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
            return;

        StartCoroutine(PlayerAttack());
    }

    public void OnItemButton()
    { 
        if (state != BattleState.PLAYERTURN)
            return;

        //go to inventory UI here
    }

    public void OnRunButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerFlee());
    }
}
