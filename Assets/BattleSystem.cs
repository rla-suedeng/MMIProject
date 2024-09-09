using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab1;
    public GameObject enemyPrefab2;
    public GameObject enemyPrefab3;

    public Transform playerBattleStation;
    public Transform enemyBattleStation;

    Unit playerUnit;
    Unit enemyUnit;

    public Text dialogueText;

    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;

    public Button maxButton;
    public Button attackButton;
    public Button healButton;
    public Button voiceAttackButton;
    public Button voiceHealButton;

    private bool attackVoiceCommandReceived = false;
    private bool healVoiceCommandReceived = false;
    private bool isListening = true;

    public BattleState state;

    private Animator enemyAnimator;
    private Animator playerAnimator;

    private int currentEnemyIndex = 0;

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());

        maxButton.onClick.AddListener(OnMaxButton);
        attackButton.onClick.AddListener(OnAttackButton);
        healButton.onClick.AddListener(OnHealButton);

        voiceAttackButton.onClick.AddListener(OnVoiceAttackButton);
        voiceHealButton.onClick.AddListener(OnVoiceHealButton);

        // Subscribe to the speech recognition event
        //googleSTTService.OnRecognizeSpeech += OnRecognizeSpeech;
    }

    IEnumerator SetupBattle()
    {
        GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = playerGO.GetComponent<Unit>();
        playerAnimator = playerGO.GetComponent<Animator>();
        playerHUD.SetHUD(playerUnit);
        SpawnNextEnemy();

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void SpawnNextEnemy()
    {
        GameObject enemyGO;

        if (currentEnemyIndex == 0)
        {
            enemyGO = Instantiate(enemyPrefab1, enemyBattleStation);
        }
        else if (currentEnemyIndex == 1)
        {
            enemyGO = Instantiate(enemyPrefab2, enemyBattleStation);
        }
        else
        {
            enemyGO = Instantiate(enemyPrefab3, enemyBattleStation);
        }

        enemyUnit = enemyGO.GetComponent<Unit>();
        enemyAnimator = enemyGO.GetComponent<Animator>();
        dialogueText.text = "A wild " + enemyUnit.unitName + " approaches...";
        enemyHUD.SetHUD(enemyUnit);
    }

    IEnumerator PlayerAttack(float additionalDamage = 0f)
    {
        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);

        playerAnimator.SetTrigger("Attack");

        enemyHUD.SetHP(enemyUnit.currentHP);
        if (additionalDamage > 0)
        {
            dialogueText.text = "Attacked the vital point!";
        }
        else
        {
            dialogueText.text = "The attack was successful!";
        }

        yield return new WaitForSeconds(2f);

        if (isDead)
        {
            currentEnemyIndex++;

            if (currentEnemyIndex < 3)
            {
                Destroy(enemyUnit.gameObject);
                SpawnNextEnemy();
                yield return new WaitForSeconds(2f);
                state = BattleState.PLAYERTURN;
                PlayerTurn();
            }
            else
            {
                state = BattleState.WON;
                EndBattle();
            }
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
        attackButton.interactable = true;
        healButton.interactable = true;
        isListening = true; // 공격이 끝난 후 다시 음성 인식 가능
    }

    IEnumerator EnemyTurn()
    {
        enemyAnimator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f);

        dialogueText.text = enemyUnit.unitName + " attacks!";

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

    void EndBattle()
    {
        if (state == BattleState.WON)
        {
            dialogueText.text = "You beat the whole TU-Enemies!";
        }
        else if (state == BattleState.LOST)
        {
            dialogueText.text = "You were defeated.";
        }
    }

    void PlayerTurn()
    {
        dialogueText.text = "Choose an action:";
        EnableButtons(); // 턴이 시작될 때 버튼을 활성화
    }

    IEnumerator PlayerHeal()
    {
        playerUnit.Heal(5);

        playerHUD.SetHP(playerUnit.currentHP);
        dialogueText.text = "You feel renewed strength!";

        yield return new WaitForSeconds(2f);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());

        EnableButtons(); // 치유가 끝난 후 버튼 다시 활성화
        isListening = true; // 치유가 끝난 후 다시 음성 인식 가능
    }

    private void OnVoiceAttackButton()
    {
        attackVoiceCommandReceived = true;
        CheckModalityFusion();
    }

    private void OnVoiceHealButton()
    {
        healVoiceCommandReceived = true;
        CheckModalityFusion();
    }

    private void CheckModalityFusion()
    {
        if (attackVoiceCommandReceived && healVoiceCommandReceived)
        {
            ResolveModalityFission();
        }
        else if (attackVoiceCommandReceived)
        {
            StartCoroutine(PlayerAttack());
            attackVoiceCommandReceived = false;
        }
        else if (healVoiceCommandReceived)
        {
            StartCoroutine(PlayerHeal());
            healVoiceCommandReceived = false;
        }
    }

    private void ResolveModalityFission()
    {
        Debug.Log("Conflicting commands received. Resolving modality fission...");
        attackVoiceCommandReceived = false;
        healVoiceCommandReceived = false;
    }

    private void DisableButtons()
    {
        attackButton.interactable = false;
        healButton.interactable = false;
        voiceAttackButton.interactable = false;
        voiceHealButton.interactable = false;
    }

    private void EnableButtons()
    {
        attackButton.interactable = true;
        healButton.interactable = true;
        voiceAttackButton.interactable = true;
        voiceHealButton.interactable = true;
    }

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerAttack());
    }

    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerHeal());
    }

    public void OnMaxButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;


        StartCoroutine(PlayerAttack(10f));
    }

    // 음성 명령을 통해 공격 실행
    public void VoiceAttack()
    {
        if (state == BattleState.PLAYERTURN && isListening)
        {
            attackButton.interactable = false;
            healButton.interactable = false;
            isListening = false; // 공격 중에는 추가 음성 명령을 무시
            StartCoroutine(PlayerAttack());
        }
    }

    // 음성 인식 결과를 처리하는 메서드
    public void VoiceCommandReceived()
    {
        if (state == BattleState.PLAYERTURN && isListening)
        {
            VoiceAttack();
        }
    }
}
