using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoBattleController : MonoBehaviour
{
    private enum PendingDamageFxKind
    {
        None,
        Normal,
        TechniqueDamage
    }

    [Header("Core")]
    [SerializeField] private BattleRules rules;
    [SerializeField] private CombatStage combatStage;

    [Header("Fighters")]
    [SerializeField] private FighterCombatStats playerStats;
    [SerializeField] private FighterCombatStats enemyStats;
    [SerializeField] private FighterAnimator playerAnimator;
    [SerializeField] private FighterAnimator enemyAnimator;
    [SerializeField] private FighterMover playerMover;
    [SerializeField] private FighterMover enemyMover;
    [SerializeField] private Transform playerLookTarget;
    [SerializeField] private Transform enemyLookTarget;

    [Header("UI Text")]
    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private TMP_Text enemyHpText;
    [SerializeField] private TMP_Text playerQiText;
    [SerializeField] private TMP_Text enemyQiText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text playerQiSpendPopupText;
    [SerializeField] private TMP_Text enemyQiSpendPopupText;

    [Header("HP / Qi Bars (optional)")]
    [SerializeField] private Image playerHpFillImage;
    [SerializeField] private Image enemyHpFillImage;
    [SerializeField] private Image playerQiFillImage;
    [SerializeField] private Image enemyQiFillImage;
    [SerializeField] private TMP_Text playerHpBarText;
    [SerializeField] private TMP_Text enemyHpBarText;
    [SerializeField] private TMP_Text playerQiBarText;
    [SerializeField] private TMP_Text enemyQiBarText;

    [Header("Floating Damage (optional)")]
    [SerializeField] private RectTransform floatingDamageRoot;
    [SerializeField] private FloatingCombatText floatingDamagePrefab;
    [SerializeField] private Transform playerDamageAnchor;
    [SerializeField] private Transform enemyDamageAnchor;
    [SerializeField] private Vector2 floatingDamageScreenOffset = new Vector2(0f, 60f);

    [Header("Floating Exchange Message (optional)")]
    [SerializeField] private Vector2 floatingExchangeMessageScreenOffset = new Vector2(0f, 120f);
    [SerializeField] [TextArea(2, 4)] private string equalExchangeMessage = "Оба мастера прочитали движение друг друга\nРавный обмен";

    [Header("UI Slots")]
    [SerializeField] private MoveSlotView[] playerSlots;
    [SerializeField] private MoveSlotView[] enemySlots;

    [Header("Buttons")]
    [SerializeField] private Button handButton;
    [SerializeField] private Button legButton;
    [SerializeField] private Button swordButton;
    [SerializeField] private Button palmButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button fightButton;
    [SerializeField] private Button rerollEnemyButton;

    [Header("UI SFX")]
    [SerializeField] private AudioClip uiClickSfx;
    [SerializeField] private AudioClip uiConfirmSfx;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultInfoText;
    [SerializeField] private Button restartButton;

    [Header("Normal VFX Prefabs")]
    [SerializeField] private GameObject blockContactFxPrefab;
    [SerializeField] private GameObject blockCenterBurstFxPrefab;

    [Header("Normal Source VFX Per Move")]
    [SerializeField] private GameObject handSourceFxPrefab;
    [SerializeField] private GameObject legSourceFxPrefab;
    [SerializeField] private GameObject swordSourceFxPrefab;
    [SerializeField] private GameObject palmSourceFxPrefab;
    [SerializeField] private GameObject hitSourceFxPrefab; // legacy fallback

    [Header("Normal Damage VFX")]
    [SerializeField] private GameObject hitTargetFxPrefab;

    [Header("Technique Cast VFX")]
    [SerializeField] private GameObject dragonFistCastFxPrefab;
    [SerializeField] private GameObject craneKickCastFxPrefab;
    [SerializeField] private GameObject moonSlashCastFxPrefab;
    [SerializeField] private GameObject voidPalmCastFxPrefab;

    [Header("Technique Hit VFX")]
    [SerializeField] private GameObject dragonFistHitFxPrefab;
    [SerializeField] private GameObject craneKickHitFxPrefab;
    [SerializeField] private GameObject moonSlashHitFxPrefab;
    [SerializeField] private GameObject voidPalmHitFxPrefab;

    [Header("Technique Common VFX")]
    [SerializeField] private GameObject techniqueDamageFxPrefab;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Move Swing SFX")]
    [SerializeField] private AudioClip handSwingSfx;
    [SerializeField] private AudioClip legSwingSfx;
    [SerializeField] private AudioClip swordSwingSfx;
    [SerializeField] private AudioClip palmSwingSfx;

    [Header("Normal Result SFX")]
    [SerializeField] private AudioClip bodyHitSfx;
    [SerializeField] private AudioClip swordHitSfx;
    [SerializeField] private AudioClip partialHitImpactSfx;
    [SerializeField] private AudioClip normalBlockSfx;
    [SerializeField] private AudioClip normalClashSfx;
    [SerializeField] private AudioClip fullHitImpactSfx; // legacy fallback
    [SerializeField] private AudioClip blockImpactSfx;   // legacy fallback

    [Header("Technique Release SFX")]
    [SerializeField] private AudioClip dragonFistReleaseSfx;
    [SerializeField] private AudioClip craneKickReleaseSfx;
    [SerializeField] private AudioClip moonSlashReleaseSfx;
    [SerializeField] private AudioClip voidPalmReleaseSfx;
    [SerializeField] private AudioClip techniqueCastSfx; // legacy fallback

    [Header("Technique Result SFX")]
    [SerializeField] private AudioClip techniqueHitLightSfx;
    [SerializeField] private AudioClip techniqueHitHeavySfx;
    [SerializeField] private AudioClip techniqueBlockSfx;
    [SerializeField] private AudioClip techniqueHitSfx; // legacy fallback

    [Header("Reaction SFX")]
    [SerializeField] private AudioClip damageReactSfx;
    [SerializeField] private AudioClip techniqueDamageReactSfx;

    [Header("Movement SFX")]
    [SerializeField] private AudioClip stepForwardSfx;
    [SerializeField] private AudioClip stepBackSfx;
    [SerializeField] private AudioClip dashInSfx;
    [SerializeField] private AudioClip landingBackSfx;

    [Header("End SFX")]
    [SerializeField] private AudioClip victorySfx;
    [SerializeField] private AudioClip defeatSfx;

    [Header("Camera Shake")]
    [SerializeField] private Transform cameraShakeTarget;

    [Header("Qi Feedback")]
    [SerializeField] private Color qiFlashColor = new Color(0.4f, 1f, 1f, 1f);
    [SerializeField] private float qiFlashDuration = 0.22f;
    [SerializeField] private float qiPopupDuration = 0.55f;
    [SerializeField] private float qiPopupRise = 28f;

    private readonly List<MoveType> playerQueue = new List<MoveType>();
    private readonly List<MoveType> enemyQueue = new List<MoveType>();

    private bool isBusy;
    private bool battleFinished;

    private SlotResolution currentSlot;
    private bool currentSlotActive;
    private bool currentImpactResolved;
    private bool currentBlockCenterSpawned;
    private PendingDamageFxKind playerPendingDamageFx;
    private PendingDamageFxKind enemyPendingDamageFx;

    private Color playerQiBaseColor = Color.white;
    private Color enemyQiBaseColor = Color.white;
    private Vector2 playerQiPopupBasePos;
    private Vector2 enemyQiPopupBasePos;
    private Coroutine playerQiPopupRoutine;
    private Coroutine enemyQiPopupRoutine;
    private Coroutine playerQiFlashRoutine;
    private Coroutine enemyQiFlashRoutine;
    private Coroutine shakeRoutine;
    private Coroutine hitStopRoutine;
    private Vector3 cameraBaseLocalPos;

    private void Awake()
    {
        if (playerQiText != null) playerQiBaseColor = playerQiText.color;
        if (enemyQiText != null) enemyQiBaseColor = enemyQiText.color;
        if (playerQiSpendPopupText != null)
        {
            RectTransform rt = playerQiSpendPopupText.rectTransform;
            playerQiPopupBasePos = rt.anchoredPosition;
            playerQiSpendPopupText.gameObject.SetActive(false);
        }
        if (enemyQiSpendPopupText != null)
        {
            RectTransform rt = enemyQiSpendPopupText.rectTransform;
            enemyQiPopupBasePos = rt.anchoredPosition;
            enemyQiSpendPopupText.gameObject.SetActive(false);
        }
        if (cameraShakeTarget != null)
            cameraBaseLocalPos = cameraShakeTarget.localPosition;
    }

    private void Start()
    {
        BindButtons();
        ResetBattle();
    }

    private void OnDestroy()
    {
        UnbindButtons();
        if (Time.timeScale != 1f)
            Time.timeScale = 1f;
    }

    private void BindButtons()
    {
        if (handButton != null) handButton.onClick.AddListener(() => AddPlayerMove(MoveType.Hand));
        if (legButton != null) legButton.onClick.AddListener(() => AddPlayerMove(MoveType.Leg));
        if (swordButton != null) swordButton.onClick.AddListener(() => AddPlayerMove(MoveType.Sword));
        if (palmButton != null) palmButton.onClick.AddListener(() => AddPlayerMove(MoveType.Palm));
        if (clearButton != null) clearButton.onClick.AddListener(ClearPlayerQueue);
        if (fightButton != null) fightButton.onClick.AddListener(TryStartBattle);
        if (rerollEnemyButton != null) rerollEnemyButton.onClick.AddListener(RerollEnemy);
        if (restartButton != null) restartButton.onClick.AddListener(ResetBattle);
    }

    private void UnbindButtons()
    {
        if (handButton != null) handButton.onClick.RemoveAllListeners();
        if (legButton != null) legButton.onClick.RemoveAllListeners();
        if (swordButton != null) swordButton.onClick.RemoveAllListeners();
        if (palmButton != null) palmButton.onClick.RemoveAllListeners();
        if (clearButton != null) clearButton.onClick.RemoveAllListeners();
        if (fightButton != null) fightButton.onClick.RemoveAllListeners();
        if (rerollEnemyButton != null) rerollEnemyButton.onClick.RemoveAllListeners();
        if (restartButton != null) restartButton.onClick.RemoveAllListeners();
    }

    private void ResetBattle()
    {
        StopAllCoroutines();
        isBusy = false;
        battleFinished = false;
        currentSlotActive = false;
        currentImpactResolved = false;
        currentBlockCenterSpawned = false;
        playerPendingDamageFx = PendingDamageFxKind.None;
        enemyPendingDamageFx = PendingDamageFxKind.None;
        Time.timeScale = 1f;

        playerQueue.Clear();
        GenerateEnemyQueue();

        if (playerStats != null) playerStats.ResetForBattle();
        if (enemyStats != null) enemyStats.ResetForBattle();

        if (resultPanel != null) resultPanel.SetActive(false);
        if (resultTitleText != null) resultTitleText.text = string.Empty;
        if (resultInfoText != null) resultInfoText.text = string.Empty;

        if (playerMover != null && combatStage != null && combatStage.playerStart != null)
            playerMover.SnapTo(combatStage.playerStart);
        if (enemyMover != null && combatStage != null && combatStage.enemyStart != null)
            enemyMover.SnapTo(combatStage.enemyStart);

        if (cameraShakeTarget != null)
            cameraShakeTarget.localPosition = cameraBaseLocalPos;

        if (playerQiSpendPopupText != null)
        {
            playerQiSpendPopupText.rectTransform.anchoredPosition = playerQiPopupBasePos;
            playerQiSpendPopupText.gameObject.SetActive(false);
        }
        if (enemyQiSpendPopupText != null)
        {
            enemyQiSpendPopupText.rectTransform.anchoredPosition = enemyQiPopupBasePos;
            enemyQiSpendPopupText.gameObject.SetActive(false);
        }
        if (playerQiText != null) playerQiText.color = playerQiBaseColor;
        if (enemyQiText != null) enemyQiText.color = enemyQiBaseColor;

        FaceEachOther();
        if (playerAnimator != null) playerAnimator.PlayIdle();
        if (enemyAnimator != null) enemyAnimator.PlayIdle();

        RefreshUI();
        if (statusText != null) statusText.text = "Собери 5 ходов и нажми Бой";
        RefreshButtonStates();
    }

    private void GenerateEnemyQueue()
    {
        enemyQueue.Clear();
        if (rules == null) return;
        for (int i = 0; i < rules.slotCount; i++)
            enemyQueue.Add((MoveType)Random.Range(0, 4));
    }

    private void RerollEnemy()
    {
        if (isBusy || battleFinished) return;
        PlayOneShot(uiClickSfx);
        GenerateEnemyQueue();
        RefreshSlots();
        RefreshButtonStates();
        if (statusText != null) statusText.text = "Ходы врага перемешаны";
    }

    private void AddPlayerMove(MoveType move)
    {
        if (isBusy || battleFinished || rules == null) return;
        if (playerQueue.Count >= rules.slotCount) return;
        PlayOneShot(uiClickSfx);
        playerQueue.Add(move);
        RefreshSlots();
        RefreshButtonStates();
        if (statusText != null) statusText.text = $"Выбран ход {playerQueue.Count}/{rules.slotCount}: {ToRu(move)}";
    }

    private void ClearPlayerQueue()
    {
        if (isBusy || battleFinished) return;
        PlayOneShot(uiClickSfx);
        playerQueue.Clear();
        RefreshSlots();
        RefreshButtonStates();
        if (statusText != null) statusText.text = "Цепочка очищена";
    }

    private void TryStartBattle()
    {
        if (isBusy || battleFinished || rules == null || playerStats == null || enemyStats == null) return;
        if (playerQueue.Count < rules.slotCount)
        {
            if (statusText != null) statusText.text = "Нужно собрать 5 ходов";
            return;
        }
        PlayOneShot(uiConfirmSfx != null ? uiConfirmSfx : uiClickSfx);
        StartCoroutine(BattleRoutine());
    }

    private IEnumerator BattleRoutine()
    {
        isBusy = true;
        RefreshButtonStates();

        if (playerMover != null && combatStage != null && combatStage.playerStart != null) playerMover.SnapTo(combatStage.playerStart);
        if (enemyMover != null && combatStage != null && combatStage.enemyStart != null) enemyMover.SnapTo(combatStage.enemyStart);
        FaceEachOther();
        if (playerAnimator != null) playerAnimator.PlayIdle();
        if (enemyAnimator != null) enemyAnimator.PlayIdle();

        BattleResolution resolution = BattleResolver.Resolve(playerQueue, enemyQueue, rules, playerStats, enemyStats);

        for (int i = 0; i < resolution.slots.Count; i++)
        {
            SlotResolution slot = resolution.slots[i];
            if (statusText != null) statusText.text = BuildSlotStatus(slot);
            yield return PlaySlot(slot);
            if (playerStats.CurrentHP <= 0 || enemyStats.CurrentHP <= 0)
                break;
        }

        bool hasWinner = playerStats.CurrentHP <= 0 || enemyStats.CurrentHP <= 0;
        if (hasWinner)
        {
            battleFinished = true;
            if (playerStats.CurrentHP <= 0 && enemyStats.CurrentHP <= 0)
            {
                if (statusText != null) statusText.text = "Оба выбыли";
                if (playerAnimator != null) playerAnimator.PlayDefeat();
                if (enemyAnimator != null) enemyAnimator.PlayDefeat();
                PlayOneShot(defeatSfx);
                ShowResultPanel("Ничья");
            }
            else if (enemyStats.CurrentHP <= 0)
            {
                if (statusText != null) statusText.text = "Победа игрока";
                if (playerAnimator != null) playerAnimator.PlayVictory();
                if (enemyAnimator != null) enemyAnimator.PlayDefeat();
                PlayOneShot(victorySfx);
                ShowResultPanel("Вы победили");
            }
            else
            {
                if (statusText != null) statusText.text = "Победа врага";
                if (playerAnimator != null) playerAnimator.PlayDefeat();
                if (enemyAnimator != null) enemyAnimator.PlayVictory();
                PlayOneShot(defeatSfx);
                ShowResultPanel("Вы проиграли");
            }

            isBusy = false;
            RefreshButtonStates();
            yield break;
        }

        playerQueue.Clear();
        GenerateEnemyQueue();
        RefreshSlots();
        isBusy = false;
        RefreshButtonStates();
        if (statusText != null) statusText.text = "Следующий раунд: собери 5 новых ходов";
    }

    private IEnumerator PlaySlot(SlotResolution slot)
    {
        currentSlot = slot;
        currentSlotActive = true;
        currentImpactResolved = false;
        currentBlockCenterSpawned = false;
        playerPendingDamageFx = PendingDamageFxKind.None;
        enemyPendingDamageFx = PendingDamageFxKind.None;

        if (slot.playerAttackKind == AttackKind.Technique)
        {
            ShowQiSpendFeedback(true, slot.playerQiBefore - slot.playerQiAfter);
            FlashTechniqueSlot(playerSlots, slot.slotIndex);
        }
        if (slot.enemyAttackKind == AttackKind.Technique)
        {
            ShowQiSpendFeedback(false, slot.enemyQiBefore - slot.enemyQiAfter);
            FlashTechniqueSlot(enemySlots, slot.slotIndex);
        }

        FaceEachOther();
        if (playerAnimator != null) playerAnimator.PlayStepForward();
        if (enemyAnimator != null) enemyAnimator.PlayStepForward();

        if (rules.stepLeadTime > 0f)
            yield return new WaitForSeconds(rules.stepLeadTime);

        if (combatStage != null)
            yield return MoveBothTo(GetPlayerImpactPosition(), GetEnemyImpactPosition(), rules.rushSpeed);
        FaceEachOther();

        if (playerAnimator != null) playerAnimator.PlayAttack(slot.playerMove, slot.playerAttackKind, slot.playerTechniqueType);
        if (enemyAnimator != null) enemyAnimator.PlayAttack(slot.enemyMove, slot.enemyAttackKind, slot.enemyTechniqueType);

        float impactWait = 0f;
        while (!currentImpactResolved && impactWait < rules.impactEventTimeout)
        {
            impactWait += Time.deltaTime;
            yield return null;
        }

        if (!currentImpactResolved)
            ResolveImpactFromFallback();

        switch (slot.exchangeType)
        {
            case ExchangeType.FullHit:
            {
                FighterAnimator winnerAnimator = slot.winner == WinnerSide.Player ? playerAnimator : enemyAnimator;
                FighterAnimator loserAnimator = slot.winner == WinnerSide.Player ? enemyAnimator : playerAnimator;
                FighterMover winnerMover = slot.winner == WinnerSide.Player ? playerMover : enemyMover;
                FighterMover loserMover = slot.winner == WinnerSide.Player ? enemyMover : playerMover;
                Vector3 winnerStart = slot.winner == WinnerSide.Player ? GetPlayerStartPosition() : GetEnemyStartPosition();
                Vector3 loserStart = slot.winner == WinnerSide.Player ? GetEnemyStartPosition() : GetPlayerStartPosition();

                Coroutine loserRoutine = null;
                if (loserMover != null)
                    loserRoutine = StartCoroutine(MoveSingleToAndPlayIdle(loserMover, loserStart, rules.returnSpeed, loserAnimator));
                else if (loserAnimator != null)
                    loserAnimator.PlayIdle();

                yield return WaitForAnimatorAttackFinish(winnerAnimator, rules.finishEventTimeout);
                if (slot.winner == WinnerSide.Player)
                {
                    if (playerAnimator != null) playerAnimator.PlayStepBack();
                }
                else
                {
                    if (enemyAnimator != null) enemyAnimator.PlayStepBack();
                }

                if (winnerMover != null)
                    yield return MoveSingleTo(winnerMover, winnerStart, rules.returnSpeed);

                if (loserRoutine != null)
                    yield return loserRoutine;
                break;
            }

            case ExchangeType.PartialHit:
            case ExchangeType.FullBlock:
                yield return WaitForBothAttackFinish(rules.finishEventTimeout);
                if (playerAnimator != null) playerAnimator.PlayStepBack();
                if (enemyAnimator != null) enemyAnimator.PlayStepBack();
                if (combatStage != null)
                    yield return MoveBothTo(GetPlayerStartPosition(), GetEnemyStartPosition(), rules.returnSpeed);
                break;
        }

        if (rules.resultPause > 0f)
            yield return new WaitForSeconds(rules.resultPause);

        currentSlotActive = false;
        playerPendingDamageFx = PendingDamageFxKind.None;
        enemyPendingDamageFx = PendingDamageFxKind.None;

        if (playerStats.CurrentHP > 0 && enemyStats.CurrentHP > 0)
        {
            if (playerAnimator != null) playerAnimator.PlayIdle();
            if (enemyAnimator != null) enemyAnimator.PlayIdle();
            FaceEachOther();
        }
    }

    private IEnumerator WaitForAnimatorAttackFinish(FighterAnimator fighter, float timeout)
    {
        if (fighter == null) yield break;
        float timer = 0f;
        while (!fighter.AttackFinished && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (!fighter.AttackFinished)
            fighter.ForceAttackFinished();
    }

    private IEnumerator WaitForBothAttackFinish(float timeout)
    {
        float timer = 0f;
        while (timer < timeout)
        {
            bool playerDone = playerAnimator == null || playerAnimator.AttackFinished;
            bool enemyDone = enemyAnimator == null || enemyAnimator.AttackFinished;
            if (playerDone && enemyDone) break;
            timer += Time.deltaTime;
            yield return null;
        }
        if (playerAnimator != null && !playerAnimator.AttackFinished) playerAnimator.ForceAttackFinished();
        if (enemyAnimator != null && !enemyAnimator.AttackFinished) enemyAnimator.ForceAttackFinished();
    }

    public void OnTechniqueCastFx(FighterAnimator source)
    {
        if (!currentSlotActive || source == null || !IsTechniqueFor(source)) return;
        TechniqueType type = GetTechniqueTypeFor(source);
        SpawnFxAt(GetTechniqueCastPrefab(type), source.AttackFxPoint);
        PlayOneShot(GetTechniqueReleaseSfx(type));
    }

    public void OnAttackSourceFx(FighterAnimator source)
    {
        if (!currentSlotActive || source == null) return;

        bool isPlayer = source == playerAnimator;
        bool sourceIsTechnique = IsTechniqueFor(source);
        bool anyTechnique = IsTechniqueFor(playerAnimator) || IsTechniqueFor(enemyAnimator);
        bool sourceWonCleanHit =
            (currentSlot.exchangeType == ExchangeType.FullHit) &&
            ((currentSlot.winner == WinnerSide.Player && isPlayer) ||
             (currentSlot.winner == WinnerSide.Enemy && !isPlayer));

        bool shouldSpawn = false;

        switch (currentSlot.exchangeType)
        {
            case ExchangeType.FullHit:
                shouldSpawn = sourceWonCleanHit;
                break;

            case ExchangeType.PartialHit:
                // Partial = both attacks still connect, so both may spawn.
                shouldSpawn = true;
                break;

            case ExchangeType.FullBlock:
                if (anyTechnique)
                {
                    // Technique full block:
                    // FX only on the attacking technique side, no normal clash/source on defender.
                    shouldSpawn = sourceIsTechnique;
                }
                else
                {
                    // Normal full block:
                    // both sides still visibly attacked, so spawn both normal source FX.
                    shouldSpawn = true;
                }
                break;
        }

        if (!shouldSpawn)
            return;

        if (sourceIsTechnique)
        {
            SpawnFxAt(GetTechniqueHitPrefab(GetTechniqueTypeFor(source)), source.AttackFxPoint);
        }
        else
        {
            SpawnFxAt(GetNormalSourceFxPrefab(source.CurrentMove), source.AttackFxPoint);
        }
    }

    public void OnBlockContactFx(FighterAnimator source)
    {
        if (!currentSlotActive || source == null) return;

        // Clash VFX is only for normal-vs-normal exchanges.
        // Techniques use their own VFX and should not spawn this effect.
        bool anyTechnique = currentSlot.playerAttackKind == AttackKind.Technique || currentSlot.enemyAttackKind == AttackKind.Technique;
        if (anyTechnique) return;

        // No clash effect on clean full hits where one side simply gets through.
        if (currentSlot.exchangeType == ExchangeType.FullHit) return;

        // Event can exist on both fighters' clips, but the clash should spawn only once.
        if (currentBlockCenterSpawned) return;
        currentBlockCenterSpawned = true;

        Transform clashPoint = (combatStage != null && combatStage.impactFxPoint != null)
            ? combatStage.impactFxPoint
            : source.AttackFxPoint;

        SpawnFxAt(blockContactFxPrefab, clashPoint);
        SpawnCenterBlockFx();

        if (currentSlot.exchangeType == ExchangeType.FullBlock)
            PlayOneShot(GetNormalBlockSfx());
        else if (currentSlot.exchangeType == ExchangeType.PartialHit)
            PlayOneShot(partialHitImpactSfx != null ? partialHitImpactSfx : GetNormalHitSfx(source.CurrentMove));
    }

    public void OnDamageFx(FighterAnimator target)
    {
        if (!currentSlotActive || target == null) return;
        bool isPlayer = target == playerAnimator;
        PendingDamageFxKind kind = isPlayer ? playerPendingDamageFx : enemyPendingDamageFx;
        if (kind == PendingDamageFxKind.None) return;

        switch (kind)
        {
            case PendingDamageFxKind.Normal:
                SpawnFxAt(hitTargetFxPrefab, target.DamageFxPoint);
                PlayOneShot(damageReactSfx);
                break;
            case PendingDamageFxKind.TechniqueDamage:
                SpawnFxAt(techniqueDamageFxPrefab, target.DamageFxPoint);
                PlayOneShot(techniqueDamageReactSfx);
                break;
        }

        if (isPlayer) playerPendingDamageFx = PendingDamageFxKind.None;
        else enemyPendingDamageFx = PendingDamageFxKind.None;
    }

    public void OnAttackImpact(FighterAnimator source)
    {
        if (!currentSlotActive || currentImpactResolved || source == null) return;

        if (currentSlot.exchangeType == ExchangeType.FullHit)
        {
            bool isWinnerEvent = (currentSlot.winner == WinnerSide.Player && source == playerAnimator) ||
                                 (currentSlot.winner == WinnerSide.Enemy && source == enemyAnimator);
            if (!isWinnerEvent) return;
        }

        ResolveImpactCore();
    }

    private void ResolveImpactFromFallback()
    {
        if (!currentSlotActive || currentImpactResolved) return;
        ResolveImpactCore();
    }

    private void ResolveImpactCore()
    {
        currentImpactResolved = true;
        bool anyTechnique = currentSlot.playerAttackKind == AttackKind.Technique || currentSlot.enemyAttackKind == AttackKind.Technique;

        switch (currentSlot.exchangeType)
        {
            case ExchangeType.FullHit:
                TriggerImpactFeedback(anyTechnique ? rules.techniqueHitStop : rules.fullHitStop,
                    anyTechnique ? rules.techniqueHitShakeDuration : rules.fullHitShakeDuration,
                    anyTechnique ? rules.techniqueHitShakeMagnitude : rules.fullHitShakeMagnitude);

                PlayOneShot(anyTechnique
                    ? GetTechniqueResultSfx(ExchangeType.FullHit)
                    : GetNormalHitSfx(currentSlot.winner == WinnerSide.Player ? currentSlot.playerMove : currentSlot.enemyMove));
                if (currentSlot.winner == WinnerSide.Player)
                {
                    bool playerUsedTechnique = currentSlot.playerAttackKind == AttackKind.Technique;
                    enemyPendingDamageFx = playerUsedTechnique ? PendingDamageFxKind.TechniqueDamage : PendingDamageFxKind.Normal;
                    if (enemyAnimator != null)
                    {
                        if (playerUsedTechnique) enemyAnimator.InterruptToTechniqueReaction(ExchangeType.FullHit);
                        else enemyAnimator.InterruptToHitReact();
                    }
                }
                else if (currentSlot.winner == WinnerSide.Enemy)
                {
                    bool enemyUsedTechnique = currentSlot.enemyAttackKind == AttackKind.Technique;
                    playerPendingDamageFx = enemyUsedTechnique ? PendingDamageFxKind.TechniqueDamage : PendingDamageFxKind.Normal;
                    if (playerAnimator != null)
                    {
                        if (enemyUsedTechnique) playerAnimator.InterruptToTechniqueReaction(ExchangeType.FullHit);
                        else playerAnimator.InterruptToHitReact();
                    }
                }
                break;

            case ExchangeType.PartialHit:
                TriggerImpactFeedback(rules.partialHitStop, rules.partialHitShakeDuration, rules.partialHitShakeMagnitude);
                PlayOneShot(anyTechnique
                    ? GetTechniqueResultSfx(ExchangeType.PartialHit)
                    : (partialHitImpactSfx != null ? partialHitImpactSfx : GetNormalHitSfx(currentSlot.playerMove == MoveType.Sword || currentSlot.enemyMove == MoveType.Sword ? MoveType.Sword : MoveType.Hand)));
                playerPendingDamageFx = currentSlot.enemyAttackKind == AttackKind.Technique ? PendingDamageFxKind.TechniqueDamage : PendingDamageFxKind.Normal;
                enemyPendingDamageFx = currentSlot.playerAttackKind == AttackKind.Technique ? PendingDamageFxKind.TechniqueDamage : PendingDamageFxKind.Normal;

                // По текущему ТЗ техника на partial должна вести себя через TechHitLight.
                if (currentSlot.enemyAttackKind == AttackKind.Technique && playerAnimator != null)
                    playerAnimator.InterruptToTechniqueReaction(ExchangeType.PartialHit);

                if (currentSlot.playerAttackKind == AttackKind.Technique && enemyAnimator != null)
                    enemyAnimator.InterruptToTechniqueReaction(ExchangeType.PartialHit);
                break;

            case ExchangeType.FullBlock:
                TriggerImpactFeedback(rules.blockStop, rules.blockShakeDuration, rules.blockShakeMagnitude);
                if (anyTechnique)
                {
                    PlayOneShot(GetTechniqueResultSfx(ExchangeType.FullBlock));

                    if (currentSlot.enemyAttackKind == AttackKind.Technique && playerAnimator != null)
                        playerAnimator.InterruptToTechniqueReaction(ExchangeType.FullBlock);

                    if (currentSlot.playerAttackKind == AttackKind.Technique && enemyAnimator != null)
                        enemyAnimator.InterruptToTechniqueReaction(ExchangeType.FullBlock);
                }
                break;
        }

        int playerDamageTaken = playerStats != null ? Mathf.Max(0, playerStats.CurrentHP - currentSlot.playerHpAfter) : 0;
        int enemyDamageTaken = enemyStats != null ? Mathf.Max(0, enemyStats.CurrentHP - currentSlot.enemyHpAfter) : 0;

        if (playerStats != null) playerStats.ApplyHp(currentSlot.playerHpAfter);
        if (enemyStats != null) enemyStats.ApplyHp(currentSlot.enemyHpAfter);
        if (playerStats != null) playerStats.ApplyQi(currentSlot.playerQiAfter);
        if (enemyStats != null) enemyStats.ApplyQi(currentSlot.enemyQiAfter);
        RefreshStatsUi();

        if (playerDamageTaken > 0)
            ShowFloatingDamage(playerDamageAnchor != null ? playerDamageAnchor : (playerAnimator != null ? playerAnimator.transform : null), playerDamageTaken);

        if (enemyDamageTaken > 0)
            ShowFloatingDamage(enemyDamageAnchor != null ? enemyDamageAnchor : (enemyAnimator != null ? enemyAnimator.transform : null), enemyDamageTaken);

        bool equalExchangeWithoutDamage = currentSlot.exchangeType == ExchangeType.FullBlock
            && playerDamageTaken <= 0
            && enemyDamageTaken <= 0;

        if (equalExchangeWithoutDamage)
            ShowFloatingExchangeMessage();
    }

    private void ShowFloatingDamage(Transform worldAnchor, int amount)
    {
        if (amount <= 0)
            return;

        if (!TryGetUiLocalPoint(worldAnchor, out Vector2 localPoint))
            return;

        FloatingCombatText instance = Instantiate(floatingDamagePrefab, floatingDamageRoot);
        instance.Play(localPoint + floatingDamageScreenOffset, amount);
    }

    private void ShowFloatingExchangeMessage()
    {
        if (string.IsNullOrWhiteSpace(equalExchangeMessage))
            return;

        Transform messageAnchor = GetExchangeMessageAnchor();
        if (!TryGetUiLocalPoint(messageAnchor, out Vector2 localPoint))
            return;

        FloatingCombatText instance = Instantiate(floatingDamagePrefab, floatingDamageRoot);
        instance.PlayInfo(localPoint + floatingExchangeMessageScreenOffset, equalExchangeMessage);
    }

    private Transform GetExchangeMessageAnchor()
    {
        if (combatStage != null && combatStage.impactFxPoint != null)
            return combatStage.impactFxPoint;

        if (playerDamageAnchor != null)
            return playerDamageAnchor;

        if (enemyDamageAnchor != null)
            return enemyDamageAnchor;

        if (playerAnimator != null)
            return playerAnimator.transform;

        if (enemyAnimator != null)
            return enemyAnimator.transform;

        return null;
    }

    private bool TryGetUiLocalPoint(Transform worldAnchor, out Vector2 localPoint)
    {
        localPoint = Vector2.zero;

        if (floatingDamageRoot == null || floatingDamagePrefab == null || worldAnchor == null)
            return false;

        Camera cam = Camera.main;
        if (cam == null)
            return false;

        Vector3 screenPoint = cam.WorldToScreenPoint(worldAnchor.position);
        if (screenPoint.z < 0f)
            return false;

        Canvas rootCanvas = floatingDamageRoot.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = rootCanvas.worldCamera != null ? rootCanvas.worldCamera : cam;

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(floatingDamageRoot, screenPoint, uiCamera, out localPoint);
    }

    private void TriggerImpactFeedback(float stopDuration, float shakeDuration, float shakeMagnitude)
    {
        if (stopDuration > 0f)
        {
            if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
            hitStopRoutine = StartCoroutine(HitStopRoutine(stopDuration));
        }

        if (cameraShakeTarget != null && shakeDuration > 0f && shakeMagnitude > 0f)
        {
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            cameraShakeTarget.localPosition = cameraBaseLocalPos;
            shakeRoutine = StartCoroutine(ShakeRoutine(shakeDuration, shakeMagnitude));
        }
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        float previous = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = previous <= 0f ? 1f : previous;
        hitStopRoutine = null;
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            Vector2 offset = Random.insideUnitCircle * magnitude;
            cameraShakeTarget.localPosition = cameraBaseLocalPos + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }
        cameraShakeTarget.localPosition = cameraBaseLocalPos;
        shakeRoutine = null;
    }

    private bool IsTechniqueFor(FighterAnimator fighter)
    {
        if (fighter == playerAnimator) return currentSlot.playerAttackKind == AttackKind.Technique;
        if (fighter == enemyAnimator) return currentSlot.enemyAttackKind == AttackKind.Technique;
        return false;
    }

    private TechniqueType GetTechniqueTypeFor(FighterAnimator fighter)
    {
        if (fighter == playerAnimator) return currentSlot.playerTechniqueType;
        if (fighter == enemyAnimator) return currentSlot.enemyTechniqueType;
        return TechniqueType.None;
    }

    private Vector3 GetPlayerStartPosition()
    {
        if (combatStage != null && combatStage.playerStart != null)
            return combatStage.playerStart.position;
        return playerMover != null ? playerMover.transform.position : Vector3.zero;
    }

    private Vector3 GetEnemyStartPosition()
    {
        if (combatStage != null && combatStage.enemyStart != null)
            return combatStage.enemyStart.position;
        return enemyMover != null ? enemyMover.transform.position : Vector3.zero;
    }

    private Vector3 GetPlayerImpactPosition()
    {
        if (combatStage != null)
        {
            bool useSwordImpact = currentSlot.playerMove == MoveType.Sword && combatStage.playerSwordImpact != null;
            if (useSwordImpact)
                return combatStage.playerSwordImpact.position;

            if (combatStage.playerImpact != null)
                return combatStage.playerImpact.position;
        }

        return playerMover != null ? playerMover.transform.position : Vector3.zero;
    }

    private Vector3 GetEnemyImpactPosition()
    {
        if (combatStage != null)
        {
            bool useSwordImpact = currentSlot.enemyMove == MoveType.Sword && combatStage.enemySwordImpact != null;
            if (useSwordImpact)
                return combatStage.enemySwordImpact.position;

            if (combatStage.enemyImpact != null)
                return combatStage.enemyImpact.position;
        }

        return enemyMover != null ? enemyMover.transform.position : Vector3.zero;
    }

    private IEnumerator MoveSingleTo(FighterMover mover, Vector3 target, float speed)
    {
        if (mover == null)
            yield break;

        Vector3 start = mover.transform.position;
        float distance = Vector3.Distance(start, target);
        if (distance <= 0.001f)
        {
            mover.SetPosition(target);
            yield break;
        }

        float duration = distance / Mathf.Max(0.01f, speed);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            mover.SetPosition(Vector3.Lerp(start, target, k));
            yield return null;
        }

        mover.SetPosition(target);
    }

    private IEnumerator MoveSingleToAndPlayIdle(FighterMover mover, Vector3 target, float speed, FighterAnimator animatorToIdle)
    {
        yield return MoveSingleTo(mover, target, speed);
        if (animatorToIdle != null)
            animatorToIdle.PlayIdle();
    }

    private IEnumerator MoveBothTo(Vector3 playerTarget, Vector3 enemyTarget, float speed)
    {
        if (playerMover == null || enemyMover == null)
            yield break;

        Vector3 pStart = playerMover.transform.position;
        Vector3 eStart = enemyMover.transform.position;
        float pDistance = Vector3.Distance(pStart, playerTarget);
        float eDistance = Vector3.Distance(eStart, enemyTarget);
        float maxDistance = Mathf.Max(pDistance, eDistance);

        if (maxDistance <= 0.001f)
        {
            playerMover.SetPosition(playerTarget);
            enemyMover.SetPosition(enemyTarget);
            FaceEachOther();
            yield break;
        }

        float duration = maxDistance / Mathf.Max(0.01f, speed);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            playerMover.SetPosition(Vector3.Lerp(pStart, playerTarget, k));
            enemyMover.SetPosition(Vector3.Lerp(eStart, enemyTarget, k));
            FaceEachOther();
            yield return null;
        }

        playerMover.SetPosition(playerTarget);
        enemyMover.SetPosition(enemyTarget);
        FaceEachOther();
    }

    private void FaceEachOther()
    {
        if (playerMover == null || enemyMover == null) return;
        Transform enemyFace = enemyLookTarget != null ? enemyLookTarget : enemyMover.transform;
        Transform playerFace = playerLookTarget != null ? playerLookTarget : playerMover.transform;
        playerMover.FaceTarget(enemyFace);
        enemyMover.FaceTarget(playerFace);
    }


    public bool CanPlayAttackTrail(FighterAnimator source)
    {
        if (!currentSlotActive || source == null)
            return true;

        bool isPlayer = source == playerAnimator;
        bool sourceIsTechnique = IsTechniqueFor(source);
        bool playerUsesTechnique = currentSlot.playerAttackKind == AttackKind.Technique;
        bool enemyUsesTechnique = currentSlot.enemyAttackKind == AttackKind.Technique;
        bool anyTechnique = playerUsesTechnique || enemyUsesTechnique;

        if (!anyTechnique)
        {
            switch (currentSlot.exchangeType)
            {
                case ExchangeType.FullHit:
                    return (currentSlot.winner == WinnerSide.Player && isPlayer) ||
                           (currentSlot.winner == WinnerSide.Enemy && !isPlayer);
                case ExchangeType.PartialHit:
                case ExchangeType.FullBlock:
                    return true;
                default:
                    return true;
            }
        }

        bool sourceIsTechniqueAttacker =
            (isPlayer && playerUsesTechnique) ||
            (!isPlayer && enemyUsesTechnique);

        switch (currentSlot.exchangeType)
        {
            case ExchangeType.FullBlock:
            case ExchangeType.PartialHit:
            case ExchangeType.FullHit:
                return sourceIsTechniqueAttacker || (playerUsesTechnique && enemyUsesTechnique && sourceIsTechnique);
            default:
                return !sourceIsTechnique ? false : sourceIsTechniqueAttacker;
        }
    }

    public void OnMoveSwingSfx(FighterAnimator source)
    {
        if (source == null) return;
        PlayOneShot(GetMoveSwingSfx(source.CurrentMove));
    }

    public void OnStepForwardSfx() => PlayOneShot(stepForwardSfx);
    public void OnStepBackSfx() => PlayOneShot(stepBackSfx);
    public void OnDashInSfx() => PlayOneShot(dashInSfx);
    public void OnLandingBackSfx() => PlayOneShot(landingBackSfx);


    private AudioClip GetTechniqueReleaseSfx(TechniqueType type)
    {
        switch (type)
        {
            case TechniqueType.DragonFist: return dragonFistReleaseSfx != null ? dragonFistReleaseSfx : techniqueCastSfx;
            case TechniqueType.CraneKick: return craneKickReleaseSfx != null ? craneKickReleaseSfx : techniqueCastSfx;
            case TechniqueType.MoonSlash: return moonSlashReleaseSfx != null ? moonSlashReleaseSfx : techniqueCastSfx;
            case TechniqueType.VoidPalm: return voidPalmReleaseSfx != null ? voidPalmReleaseSfx : techniqueCastSfx;
            default: return techniqueCastSfx;
        }
    }

    private AudioClip GetNormalHitSfx(MoveType moveType)
    {
        switch (moveType)
        {
            case MoveType.Sword:
                return swordHitSfx != null ? swordHitSfx : (bodyHitSfx != null ? bodyHitSfx : fullHitImpactSfx);
            case MoveType.Hand:
            case MoveType.Leg:
            case MoveType.Palm:
                return bodyHitSfx != null ? bodyHitSfx : fullHitImpactSfx;
            default:
                return fullHitImpactSfx;
        }
    }

    private AudioClip GetTechniqueResultSfx(ExchangeType exchangeType)
    {
        switch (exchangeType)
        {
            case ExchangeType.FullBlock:
                return techniqueBlockSfx;
            case ExchangeType.PartialHit:
                return techniqueHitLightSfx != null ? techniqueHitLightSfx : techniqueHitSfx;
            case ExchangeType.FullHit:
                return techniqueHitHeavySfx != null ? techniqueHitHeavySfx : techniqueHitSfx;
            default:
                return techniqueHitSfx;
        }
    }

    private AudioClip GetNormalBlockSfx()
    {
        if (normalClashSfx != null)
            return normalClashSfx;
        if (normalBlockSfx != null)
            return normalBlockSfx;
        return blockImpactSfx;
    }

    private AudioClip GetMoveSwingSfx(MoveType moveType)
    {
        switch (moveType)
        {
            case MoveType.Hand: return handSwingSfx;
            case MoveType.Leg: return legSwingSfx;
            case MoveType.Sword: return swordSwingSfx;
            case MoveType.Palm: return palmSwingSfx;
            default: return null;
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private void SpawnCenterBlockFx()
    {
        if (combatStage == null || combatStage.impactFxPoint == null || blockCenterBurstFxPrefab == null) return;
        SpawnFxAt(blockCenterBurstFxPrefab, combatStage.impactFxPoint);
    }

    private void SpawnFxAt(GameObject prefab, Transform point)
    {
        if (prefab == null || point == null) return;

        GameObject fx = Instantiate(prefab, point.position, point.rotation);

        ParticleSystem[] systems = fx.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < systems.Length; i++)
        {
            systems[i].Clear(true);
            systems[i].Play(true);
        }
    }

    private GameObject GetTechniqueCastPrefab(TechniqueType type)
    {
        switch (type)
        {
            case TechniqueType.DragonFist: return dragonFistCastFxPrefab;
            case TechniqueType.CraneKick: return craneKickCastFxPrefab;
            case TechniqueType.MoonSlash: return moonSlashCastFxPrefab;
            case TechniqueType.VoidPalm: return voidPalmCastFxPrefab;
            default: return null;
        }
    }

    private GameObject GetTechniqueHitPrefab(TechniqueType type)
    {
        switch (type)
        {
            case TechniqueType.DragonFist: return dragonFistHitFxPrefab;
            case TechniqueType.CraneKick: return craneKickHitFxPrefab;
            case TechniqueType.MoonSlash: return moonSlashHitFxPrefab;
            case TechniqueType.VoidPalm: return voidPalmHitFxPrefab;
            default: return null;
        }
    }

    private GameObject GetNormalSourceFxPrefab(MoveType moveType)
    {
        switch (moveType)
        {
            case MoveType.Hand:
                return handSourceFxPrefab != null ? handSourceFxPrefab : hitSourceFxPrefab;
            case MoveType.Leg:
                return legSourceFxPrefab != null ? legSourceFxPrefab : hitSourceFxPrefab;
            case MoveType.Sword:
                return swordSourceFxPrefab != null ? swordSourceFxPrefab : hitSourceFxPrefab;
            case MoveType.Palm:
                return palmSourceFxPrefab != null ? palmSourceFxPrefab : hitSourceFxPrefab;
            default:
                return hitSourceFxPrefab;
        }
    }

    private void RefreshUI()
    {
        RefreshStatsUi();
        RefreshSlots();
    }

    private void RefreshStatsUi()
    {
        if (playerStats != null)
        {
            SetBar(playerHpFillImage, playerStats.CurrentHP, playerStats.MaxHP);
            SetBar(playerQiFillImage, playerStats.CurrentQi, playerStats.MaxQi);
            SetText(playerHpText, $"HP Игрока: {playerStats.CurrentHP}/{playerStats.MaxHP}");
            SetText(playerQiText, $"Qi Игрока: {playerStats.CurrentQi}/{playerStats.MaxQi}");
            SetText(playerHpBarText, $"{playerStats.CurrentHP}/{playerStats.MaxHP}");
            SetText(playerQiBarText, $"{playerStats.CurrentQi}/{playerStats.MaxQi}");
        }

        if (enemyStats != null)
        {
            SetBar(enemyHpFillImage, enemyStats.CurrentHP, enemyStats.MaxHP);
            SetBar(enemyQiFillImage, enemyStats.CurrentQi, enemyStats.MaxQi);
            SetText(enemyHpText, $"HP Врага: {enemyStats.CurrentHP}/{enemyStats.MaxHP}");
            SetText(enemyQiText, $"Qi Врага: {enemyStats.CurrentQi}/{enemyStats.MaxQi}");
            SetText(enemyHpBarText, $"{enemyStats.CurrentHP}/{enemyStats.MaxHP}");
            SetText(enemyQiBarText, $"{enemyStats.CurrentQi}/{enemyStats.MaxQi}");
        }
    }

    private void RefreshSlots()
    {
        List<AttackPlan> playerPreview = BattleResolver.BuildPreviewPlans(playerQueue, playerStats != null ? playerStats.CurrentQi : 0, playerStats, rules);
        List<AttackPlan> enemyPreview = BattleResolver.BuildPreviewPlans(enemyQueue, enemyStats != null ? enemyStats.CurrentQi : 0, enemyStats, rules);

        if (playerSlots != null)
        {
            for (int i = 0; i < playerSlots.Length; i++)
            {
                if (playerSlots[i] == null) continue;
                if (i < playerQueue.Count && i < playerPreview.Count)
                {
                    playerSlots[i].SetMove(playerQueue[i], playerPreview[i].attackKind, playerPreview[i].techniqueType);
                }
                else
                {
                    playerSlots[i].SetMove(null);
                }
            }
        }

        if (enemySlots != null)
        {
            for (int i = 0; i < enemySlots.Length; i++)
            {
                if (enemySlots[i] == null) continue;
                if (i < enemyQueue.Count && i < enemyPreview.Count)
                {
                    enemySlots[i].SetMove(enemyQueue[i], enemyPreview[i].attackKind, enemyPreview[i].techniqueType);
                }
                else
                {
                    enemySlots[i].SetMove(null);
                }
            }
        }
    }

    private void FlashTechniqueSlot(MoveSlotView[] slots, int index)
    {
        if (slots == null || index < 0 || index >= slots.Length || slots[index] == null)
            return;
        slots[index].PlayTechniqueFlash();
    }

    private void SetBar(Image fillImage, int current, int max)
    {
        if (fillImage == null)
            return;

        float normalized = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        fillImage.fillAmount = normalized;
    }

    private void SetText(TMP_Text textField, string value)
    {
        if (textField != null)
            textField.text = value;
    }

    private static string ToRu(MoveType move)
    {
        switch (move)
        {
            case MoveType.Hand: return "Кулак";
            case MoveType.Leg: return "Нога";
            case MoveType.Sword: return "Меч";
            case MoveType.Palm: return "Ладонь";
            default: return "?";
        }
    }

    private void ShowQiSpendFeedback(bool forPlayer, int amount)
    {
        if (amount <= 0) return;

        if (forPlayer)
        {
            if (playerQiFlashRoutine != null) StopCoroutine(playerQiFlashRoutine);
            playerQiFlashRoutine = StartCoroutine(FlashQiText(playerQiText, playerQiBaseColor));
            if (playerQiPopupRoutine != null) StopCoroutine(playerQiPopupRoutine);
            playerQiPopupRoutine = StartCoroutine(ShowQiPopup(playerQiSpendPopupText, playerQiPopupBasePos, amount));
        }
        else
        {
            if (enemyQiFlashRoutine != null) StopCoroutine(enemyQiFlashRoutine);
            enemyQiFlashRoutine = StartCoroutine(FlashQiText(enemyQiText, enemyQiBaseColor));
            if (enemyQiPopupRoutine != null) StopCoroutine(enemyQiPopupRoutine);
            enemyQiPopupRoutine = StartCoroutine(ShowQiPopup(enemyQiSpendPopupText, enemyQiPopupBasePos, amount));
        }
    }

    private IEnumerator FlashQiText(TMP_Text text, Color baseColor)
    {
        if (text == null) yield break;
        float t = 0f;
        while (t < qiFlashDuration)
        {
            t += Time.unscaledDeltaTime;
            float ping = Mathf.PingPong(t * 8f, 1f);
            text.color = Color.Lerp(baseColor, qiFlashColor, ping);
            yield return null;
        }
        text.color = baseColor;
    }

    private IEnumerator ShowQiPopup(TMP_Text popup, Vector2 basePos, int amount)
    {
        if (popup == null) yield break;
        popup.gameObject.SetActive(true);
        popup.text = $"-{amount} Qi";
        Color baseColor = popup.color;
        RectTransform rt = popup.rectTransform;

        float t = 0f;
        while (t < qiPopupDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / qiPopupDuration);
            rt.anchoredPosition = basePos + Vector2.up * qiPopupRise * k;
            Color c = baseColor;
            c.a = 1f - k;
            popup.color = c;
            yield return null;
        }

        rt.anchoredPosition = basePos;
        popup.color = baseColor;
        popup.gameObject.SetActive(false);
    }

    private void SetButton(Button button, bool value)
    {
        if (button != null) button.interactable = value;
    }

    private void RefreshButtonStates()
    {
        bool inputAllowed = !isBusy && !battleFinished && rules != null;
        bool canAddMoves = inputAllowed && playerQueue.Count < rules.slotCount;
        bool hasAnyMoves = playerQueue.Count > 0;
        bool queueReady = inputAllowed && playerQueue.Count >= rules.slotCount;

        SetButton(handButton, canAddMoves);
        SetButton(legButton, canAddMoves);
        SetButton(swordButton, canAddMoves);
        SetButton(palmButton, canAddMoves);
        SetButton(clearButton, inputAllowed && hasAnyMoves);
        SetButton(fightButton, queueReady);
        SetButton(rerollEnemyButton, inputAllowed);
    }

    private void ShowResultPanel(string title)
    {
        if (resultTitleText != null) resultTitleText.text = title;
        if (resultInfoText != null && playerStats != null)
            resultInfoText.text = $"У игрока осталось HP: {playerStats.CurrentHP}/{playerStats.MaxHP}\nУ игрока осталось Qi: {playerStats.CurrentQi}/{playerStats.MaxQi}";
        if (resultPanel != null) resultPanel.SetActive(true);
        RefreshButtonStates();
    }

    private string BuildSlotStatus(SlotResolution slot)
    {
        string playerLabel = ToRu(slot.playerMove);
        string enemyLabel = ToRu(slot.enemyMove);
        if (slot.playerAttackKind == AttackKind.Technique)
            playerLabel += $" ({slot.playerTechniqueType})";
        if (slot.enemyAttackKind == AttackKind.Technique)
            enemyLabel += $" ({slot.enemyTechniqueType})";
        return $"Слот {slot.slotIndex + 1}: {playerLabel} vs {enemyLabel}";
    }

}
