using UnityEngine;
using System.Collections;
using TMPro;
using static BossEncounter;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, FLED, BUSY}

public class BattleSystem : MonoBehaviour
{
    [Header("Background")]
    public BackgroundManager backgroundManager;
    public TMP_Text turnCounter;
    private bool IsPlayerTurn;

    private GameObject enemyGO;
    private Animator enemyAnim;
    private GameObject chosenEnemyPrefab;

    private GameObject playerGO;
    private Animator playerAnim;

    [Header("Player")]
    public GameObject playerPrefab;
    [Header("Enemy Prefabs")]
    public GameObject HumanoidPrefab;
    public GameObject MutantPrefab;
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

    [Header("Sound")]
    public BattleSound battleSound;

    public BattleState state;

    //choose what type of enemy at runtime (default is random enemy)
    public enum EncounterKind { HumanoidEnemy, MutantEnemy, BossEnemy}
    public EncounterKind encounterKind = EncounterKind.HumanoidEnemy;

    void Start()
    {
        if (BattleTransfer.encounterKind != EncounterKind.HumanoidEnemy)
        {
            encounterKind = BattleTransfer.encounterKind;
        }
        //please use these below for respective triggers!
        //BattleTransfer.encounterKind = EncounterKind.Boss;
        //BattleTransfer.encounterKind = EncounterKind.RandomEnemy;

        state = BattleState.START;
        battleSound.EnemyAppearSound();
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        //load player
        playerGO = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = playerGO.GetComponent<PlayerStats>();

        //choose and load enemy
        if (encounterKind == EncounterKind.HumanoidEnemy)
        {
            chosenEnemyPrefab = HumanoidPrefab;
            backgroundManager.SetBackgroundToStreet();
        }
        else if (encounterKind == EncounterKind.MutantEnemy)
        {
            chosenEnemyPrefab = MutantPrefab;
            backgroundManager.SetBackgroundToStreet();
        }
        else if (encounterKind == EncounterKind.BossEnemy)
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
            if(encounterKind == EncounterKind.BossEnemy)
            {
                PlayerPrefs.SetString("BattleResult", "WON");
                SceneManager.LoadScene("EndGameScrn");
            }
        }
        else if (state == BattleState.LOST)
        {
            dialogueText.text = "You are defeated.";
            PlayerPrefs.SetString("BattleResult", "LOST");
            SceneManager.LoadScene("EndGameScrn");
        }
        else if (state == BattleState.FLED)
        {
            dialogueText.text = "You have fled the battle!";
            if (encounterKind == EncounterKind.BossEnemy)
            {
                SceneManager.LoadScene("NorthEastMap");
            }
            else
            {
                SceneManager.LoadScene("StartMap");
            }
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
        IsPlayerTurn = false;
        UpdateCounterText(IsPlayerTurn);

        dialogueText.text = enemyUnit.unitName + " attacks!";
        enemyAnim.SetTrigger("Attack");
        battleSound.EnemyAttackSound();

        yield return new WaitForSeconds(1f);

        bool isDead = playerUnit.TakeDamage(enemyUnit.damage,true);
        battleSound.PlayerHurtSound();
        playerHUD.SetHP(playerUnit.currentHP);

        yield return new WaitForSeconds(1f);

        if (isDead)
        {
            battleSound.PlayerDieSound();
            state = BattleState.LOST;
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            battleSound.ReloadSound();
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
        IsPlayerTurn = true;
        UpdateCounterText(IsPlayerTurn);
        dialogueText.text = "Choose an action: ";
    }

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN || Time.timeScale == 0f)
        {
            Debug.Log("Not your turn!");
            return;
        }

        state = BattleState.BUSY;
        battleSound.PistolShootSound();
        StartCoroutine(PlayerAttack());
    }

    public void OnItemButton()
    {
        if (state != BattleState.PLAYERTURN || Time.timeScale == 0f)
        {
            Debug.Log("Not your turn!");
            return;
        }
        //go to inventory UI here

    }

    public void OnRunButton()
    {
        if (state != BattleState.PLAYERTURN || Time.timeScale == 0f)
        {
            Debug.Log("Not your turn!");
            return;
        }

        StartCoroutine(PlayerFlee());
    }

    public void UpdateCounterText(bool c)
    {
        if (c)
        {
            turnCounter.text = "Player's Turn";
            turnCounter.color = Color.green;
        }
        else
        {
            turnCounter.text = "Enemy's Turn";
            turnCounter.color = Color.red;
        }
    }
}
