using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private TMP_Text playerRankText;
    [SerializeField] private TMP_Text enemyRankText;

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

    [Header("Legacy Buttons")]
    [SerializeField] private Button handButton;
    [SerializeField] private Button legButton;
    [SerializeField] private Button swordButton;
    [SerializeField] private Button palmButton;

    [Header("Round Choice Buttons")]
    [SerializeField] private Button choiceLeftButton;
    [SerializeField] private Button choiceRightButton;
    [SerializeField] private MoveSlotView choiceLeftView;
    [SerializeField] private MoveSlotView choiceRightView;

    [Header("Round Controls")]
    [SerializeField] private Button clearButton;
    [SerializeField] private Button fightButton;
    [SerializeField] private Button rerollEnemyButton;

    [Header("Enemy Round Behavior")]
    [SerializeField] private EnemyArchetype enemyArchetype = EnemyArchetype.Aggressor;
    [SerializeField] private MoveType enemyPreferredStyle = MoveType.Sword;

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

    [Header("Technique Cast VFX - Player")]
    [SerializeField] private GameObject dragonFistCastFxPrefab;
    [SerializeField] private GameObject craneKickCastFxPrefab;
    [SerializeField] private GameObject moonSlashCastFxPrefab;
    [SerializeField] private GameObject voidPalmCastFxPrefab;

    [Header("Technique Cast VFX - Enemy (optional)")]
    [SerializeField] private GameObject enemyDragonFistCastFxPrefab;
    [SerializeField] private GameObject enemyCraneKickCastFxPrefab;
    [SerializeField] private GameObject enemyMoonSlashCastFxPrefab;
    [SerializeField] private GameObject enemyVoidPalmCastFxPrefab;

    [Header("Technique Hit VFX - Player")]
    [SerializeField] private GameObject dragonFistHitFxPrefab;
    [SerializeField] private GameObject craneKickHitFxPrefab;
    [SerializeField] private GameObject moonSlashHitFxPrefab;
    [SerializeField] private GameObject voidPalmHitFxPrefab;

    [Header("Technique Hit VFX - Enemy (optional)")]
    [SerializeField] private GameObject enemyDragonFistHitFxPrefab;
    [SerializeField] private GameObject enemyCraneKickHitFxPrefab;
    [SerializeField] private GameObject enemyMoonSlashHitFxPrefab;
    [SerializeField] private GameObject enemyVoidPalmHitFxPrefab;

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
    private readonly List<MoveType> playerRoundPool = new List<MoveType>();
    private readonly List<MoveType> originalPlayerRoundPool = new List<MoveType>();
    private readonly List<MoveType> currentOffer = new List<MoveType>();

    private bool clearUsedThisRound;
    private int offersShownThisRound;
    private int meaningfulOffersShownInProtectedWindow;

    private bool isBusy;
    private bool battleFinished;
    private bool revealEnemyFullQueue;

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


    private Button GetChoiceLeftButton() => choiceLeftButton != null ? choiceLeftButton : handButton;
    private Button GetChoiceRightButton() => choiceRightButton != null ? choiceRightButton : legButton;

    private void BindButtons()
    {
        Button left = GetChoiceLeftButton();
        Button right = GetChoiceRightButton();

        if (left != null) left.onClick.AddListener(() => PickCurrentOffer(0));
        if (right != null) right.onClick.AddListener(() => PickCurrentOffer(1));

        if (clearButton != null) clearButton.onClick.AddListener(ClearPlayerQueue);
        if (fightButton != null) fightButton.onClick.AddListener(TryStartBattle);
        if (restartButton != null) restartButton.onClick.AddListener(ResetBattle);
    }

    private void UnbindButtons()
    {
        Button left = GetChoiceLeftButton();
        Button right = GetChoiceRightButton();

        if (left != null) left.onClick.RemoveAllListeners();
        if (right != null) right.onClick.RemoveAllListeners();

        if (clearButton != null) clearButton.onClick.RemoveAllListeners();
        if (fightButton != null) fightButton.onClick.RemoveAllListeners();
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
        enemyQueue.Clear();
        playerRoundPool.Clear();
        currentOffer.Clear();
        revealEnemyFullQueue = false;
        clearUsedThisRound = false;
        offersShownThisRound = 0;
        meaningfulOffersShownInProtectedWindow = 0;

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

        PrepareNewRoundState();
        RefreshUI();
        if (statusText != null) statusText.text = "Выбери 1 из 2 и собери цепочку из 5 ходов";
        RefreshButtonStates();
    }

    private void PrepareNewRoundState()
    {
        playerQueue.Clear();
        enemyQueue.Clear();
        playerRoundPool.Clear();
        currentOffer.Clear();
        revealEnemyFullQueue = false;
        clearUsedThisRound = false;
        offersShownThisRound = 0;
        meaningfulOffersShownInProtectedWindow = 0;

        GeneratePlayerRoundPool();
        GenerateEnemyQueue();
        BuildNextPlayerOffer(true);
    }

    private void GeneratePlayerRoundPool()
    {
        playerRoundPool.Clear();
        originalPlayerRoundPool.Clear();

        int[] shape = PickPoolShape(EnemyArchetype.Chaotic, false);
        MoveType[] types = ShuffleMoves(new[] { MoveType.Hand, MoveType.Leg, MoveType.Sword, MoveType.Palm });

        for (int i = 0; i < shape.Length && i < types.Length; i++)
        {
            for (int k = 0; k < shape[i]; k++)
                playerRoundPool.Add(types[i]);
        }

        originalPlayerRoundPool.AddRange(playerRoundPool);
    }

    private void GenerateEnemyQueue()
    {
        enemyQueue.Clear();

        List<MoveType> enemyPool = GenerateEnemyRoundPool();
        if (enemyPool.Count == 0 || rules == null)
            return;

        MoveType hotStyle = enemyPreferredStyle;
        if (enemyArchetype != EnemyArchetype.Master)
        {
            hotStyle = enemyPool.GroupBy(m => m).OrderByDescending(g => g.Count()).First().Key;
        }

        MoveType supportStyle = GetMasterCoverStyle(enemyPreferredStyle);

        while (enemyQueue.Count < rules.slotCount && enemyPool.Count > 0)
        {
            List<MoveType> options = enemyPool.Distinct().ToList();
            List<int> weights = new List<int>();

            for (int i = 0; i < options.Count; i++)
            {
                MoveType move = options[i];
                int weight = 1;

                switch (enemyArchetype)
                {
                    case EnemyArchetype.Aggressor:
                        if (move == hotStyle) weight += 4;
                        if (enemyQueue.Count > 0 && enemyQueue[enemyQueue.Count - 1] == move) weight += 4;
                        if (enemyPool.Count(x => x == move) >= 2) weight += 2;
                        break;

                    case EnemyArchetype.Chaotic:
                        if (enemyQueue.Count > 0 && enemyQueue[enemyQueue.Count - 1] != move) weight += 4;
                        if (enemyQueue.Count == 0 || enemyQueue.Count(x => x == move) < 2) weight += 2;
                        if (enemyQueue.Count > 0 && enemyQueue[enemyQueue.Count - 1] == move) weight = Mathf.Max(1, weight - 2);
                        break;

                    case EnemyArchetype.Master:
                        if (move == enemyPreferredStyle) weight += 5;
                        if (move == supportStyle) weight += 3;
                        if (enemyQueue.Count > 0 && enemyQueue[enemyQueue.Count - 1] == move) weight += 2;
                        break;
                }

                weights.Add(Mathf.Max(1, weight));
            }

            MoveType picked = PickWeightedMove(options, weights);
            enemyQueue.Add(picked);
            RemoveFirstOccurrence(enemyPool, picked);
        }
    }

    private List<MoveType> GenerateEnemyRoundPool()
    {
        List<MoveType> result = new List<MoveType>();
        int[] shape = PickPoolShape(enemyArchetype, true);

        MoveType[] orderedTypes;
        switch (enemyArchetype)
        {
            case EnemyArchetype.Aggressor:
            {
                MoveType hotStyle = (MoveType)Random.Range(0, 4);
                List<MoveType> rest = new List<MoveType> { MoveType.Hand, MoveType.Leg, MoveType.Sword, MoveType.Palm };
                rest.Remove(hotStyle);
                rest = rest.OrderBy(_ => Random.value).ToList();
                orderedTypes = new[] { hotStyle, rest[0], rest[1], rest[2] };
                break;
            }

            case EnemyArchetype.Master:
            {
                MoveType coverStyle = GetMasterCoverStyle(enemyPreferredStyle);
                List<MoveType> rest = new List<MoveType> { MoveType.Hand, MoveType.Leg, MoveType.Sword, MoveType.Palm };
                rest.Remove(enemyPreferredStyle);
                rest.Remove(coverStyle);
                rest = rest.OrderBy(_ => Random.value).ToList();
                orderedTypes = new[] { enemyPreferredStyle, coverStyle, rest[0], rest[1] };
                break;
            }

            default:
                orderedTypes = ShuffleMoves(new[] { MoveType.Hand, MoveType.Leg, MoveType.Sword, MoveType.Palm });
                break;
        }

        for (int i = 0; i < shape.Length && i < orderedTypes.Length; i++)
        {
            for (int k = 0; k < shape[i]; k++)
                result.Add(orderedTypes[i]);
        }

        return result;
    }

    private int[] PickPoolShape(EnemyArchetype archetype, bool enemy)
    {
        int w4222 = Mathf.Max(0, rules.shape4222Weight);
        int w3322 = Mathf.Max(0, rules.shape3322Weight);
        int w3331 = Mathf.Max(0, rules.shape3331Weight);

        if (enemy)
        {
            switch (archetype)
            {
                case EnemyArchetype.Aggressor:
                    w4222 += 25;
                    w3322 += 10;
                    break;
                case EnemyArchetype.Chaotic:
                    w3331 += 25;
                    break;
                case EnemyArchetype.Master:
                    w3322 += 20;
                    w4222 += 10;
                    break;
            }
        }

        int total = Mathf.Max(1, w4222 + w3322 + w3331);
        int roll = Random.Range(0, total);

        if (roll < w4222) return new[] { 4, 2, 2, 2 };
        if (roll < w4222 + w3322) return new[] { 3, 3, 2, 2 };
        return new[] { 3, 3, 3, 1 };
    }

    private void PickCurrentOffer(int offerIndex)
    {
        if (isBusy || battleFinished || rules == null) return;
        if (offerIndex < 0 || offerIndex >= currentOffer.Count) return;
        if (playerQueue.Count >= rules.slotCount) return;

        PlayOneShot(uiClickSfx);

        MoveType selected = currentOffer[offerIndex];

        // New round rule:
        // after choosing 1 of 2, both offered cards leave the round pool.
        // only the chosen card is added to the player's final queue.
        for (int i = 0; i < currentOffer.Count; i++)
            RemoveFirstOccurrence(playerRoundPool, currentOffer[i]);

        playerQueue.Add(selected);
        currentOffer.Clear();

        if (playerQueue.Count < rules.slotCount)
            BuildNextPlayerOffer(false);

        RefreshUI();
        RefreshButtonStates();

        if (statusText != null)
        {
            if (playerQueue.Count < rules.slotCount)
                statusText.text = $"Выбран ход {playerQueue.Count}/{rules.slotCount}: {ToRu(selected)}";
            else
                statusText.text = "Цепочка готова. Нажми Бой";
        }
    }

    private void BuildNextPlayerOffer(bool forceMeaningfulAfterReset)
    {
        currentOffer.Clear();

        if (playerQueue.Count >= rules.slotCount || playerRoundPool.Count <= 0 || rules == null)
            return;

        bool mustGuaranteeMeaningful = forceMeaningfulAfterReset;

        if (!mustGuaranteeMeaningful && offersShownThisRound < rules.protectedOfferCount)
        {
            if (offersShownThisRound < rules.minimumMeaningfulOffersInProtectedWindow)
                mustGuaranteeMeaningful = true;
            else if (offersShownThisRound == rules.protectedOfferCount - 1 && meaningfulOffersShownInProtectedWindow < rules.minimumMeaningfulOffersInProtectedWindow)
                mustGuaranteeMeaningful = true;
        }

        int firstIndex = Random.Range(0, playerRoundPool.Count);
        MoveType first = playerRoundPool[firstIndex];

        List<MoveType> candidateMoves = new List<MoveType>();
        List<int> candidateWeights = new List<int>();
        bool hasDifferentTypeAlternative = false;

        for (int i = 0; i < playerRoundPool.Count; i++)
        {
            if (i == firstIndex)
                continue;

            MoveType candidate = playerRoundPool[i];
            if (candidate != first)
                hasDifferentTypeAlternative = true;
        }

        for (int i = 0; i < playerRoundPool.Count; i++)
        {
            if (i == firstIndex)
                continue;

            MoveType candidate = playerRoundPool[i];

            // Better UX rule:
            // avoid identical pairs like Hand/Hand unless there is literally no other type left.
            if (hasDifferentTypeAlternative && candidate == first)
                continue;

            if (mustGuaranteeMeaningful && !IsMeaningfulChoice(candidate) && !IsMeaningfulChoice(first))
                continue;

            candidateMoves.Add(candidate);
            candidateWeights.Add(GetOfferWeight(candidate));
        }

        MoveType second;
        if (candidateMoves.Count == 0)
        {
            second = FindFallbackSecond(firstIndex, mustGuaranteeMeaningful, first, hasDifferentTypeAlternative);
        }
        else
        {
            bool randomSecond = !mustGuaranteeMeaningful && Random.value < rules.pairSecondRandomChance;
            second = randomSecond
                ? candidateMoves[Random.Range(0, candidateMoves.Count)]
                : PickWeightedMove(candidateMoves, candidateWeights);
        }

        currentOffer.Add(first);
        currentOffer.Add(second);

        offersShownThisRound++;
        if (IsMeaningfulChoice(first) || IsMeaningfulChoice(second))
            meaningfulOffersShownInProtectedWindow++;
    }

    private MoveType FindFallbackSecond(int excludedIndex, bool mustGuaranteeMeaningful, MoveType firstMove, bool hasDifferentTypeAlternative)
    {
        List<MoveType> fallback = new List<MoveType>();
        for (int i = 0; i < playerRoundPool.Count; i++)
        {
            if (i == excludedIndex)
                continue;

            MoveType move = playerRoundPool[i];

            if (hasDifferentTypeAlternative && move == firstMove)
                continue;

            if (!mustGuaranteeMeaningful || IsMeaningfulChoice(move))
                fallback.Add(move);
        }

        if (fallback.Count > 0)
            return fallback[Random.Range(0, fallback.Count)];

        for (int i = 0; i < playerRoundPool.Count; i++)
        {
            if (i == excludedIndex)
                continue;

            MoveType move = playerRoundPool[i];
            if (!hasDifferentTypeAlternative || move != firstMove)
                return move;
        }

        for (int i = 0; i < playerRoundPool.Count; i++)
        {
            if (i != excludedIndex)
                return playerRoundPool[i];
        }

        return playerRoundPool[Mathf.Clamp(excludedIndex, 0, playerRoundPool.Count - 1)];
    }

    private int GetOfferWeight(MoveType move)
    {
        int weight = Mathf.Max(0, rules.pairNeutralWeight);

        if (ContinuesCurrentSeries(move))
            weight += Mathf.Max(0, rules.pairSeriesWeight);

        if (CountersVisibleEnemy(move))
            weight += Mathf.Max(0, rules.pairCounterEnemyWeight);

        if (MatchesPlayerDominantStyle(move))
            weight += Mathf.Max(0, rules.pairStyleWeight);

        return Mathf.Max(1, weight);
    }

    private bool IsMeaningfulChoice(MoveType move)
    {
        return ContinuesCurrentSeries(move)
            || CountersVisibleEnemy(move)
            || HelpsPotentialTriple(move);
    }

    private bool ContinuesCurrentSeries(MoveType move)
    {
        if (playerQueue.Count == 0)
            return false;

        int streak = GetCurrentSeriesLength(playerQueue);
        return streak > 0 && playerQueue[playerQueue.Count - 1] == move;
    }

    private bool CountersVisibleEnemy(MoveType move)
    {
        int visibleCount = Mathf.Min(enemyQueue.Count, rules != null ? rules.enemyVisibleMoves : 3);
        for (int i = 0; i < visibleCount; i++)
        {
            if (BattleResolver.Beats(move, enemyQueue[i]))
                return true;
        }
        return false;
    }

    private bool MatchesPlayerDominantStyle(MoveType move)
    {
        if (playerQueue.Count == 0)
            return false;

        MoveType dominant = playerQueue.GroupBy(x => x).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).First().Key;
        return dominant == move;
    }

    private bool HelpsPotentialTriple(MoveType move)
    {
        int alreadyChosen = playerQueue.Count(x => x == move);
        int remaining = playerRoundPool.Count(x => x == move);
        return alreadyChosen > 0 && (alreadyChosen + remaining) >= 3;
    }

    private int GetCurrentSeriesLength(List<MoveType> queue)
    {
        if (queue == null || queue.Count == 0)
            return 0;

        MoveType tail = queue[queue.Count - 1];
        int length = 1;
        for (int i = queue.Count - 2; i >= 0; i--)
        {
            if (queue[i] != tail)
                break;
            length++;
        }
        return length;
    }

    private void ClearPlayerQueue()
    {
        if (isBusy || battleFinished || clearUsedThisRound) return;
        if (playerQueue.Count <= 0) return;

        PlayOneShot(uiClickSfx);

        // New clear rule:
        // restore the same original 10-card round pool snapshot,
        // do NOT generate a new round, and do NOT lose cards that were shown earlier.
        playerRoundPool.Clear();
        playerRoundPool.AddRange(originalPlayerRoundPool);

        playerQueue.Clear();
        currentOffer.Clear();

        clearUsedThisRound = true;
        offersShownThisRound = 0;
        meaningfulOffersShownInProtectedWindow = 0;

        BuildNextPlayerOffer(true);
        RefreshUI();
        RefreshButtonStates();

        if (statusText != null) statusText.text = "Цепочка очищена. Выбери новую пару";
    }

    private void RerollEnemy()
    {
        // Legacy button: new round system no longer rerolls enemy manually.
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
        revealEnemyFullQueue = true;
        RefreshSlots();
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

        PrepareNewRoundState();
        RefreshUI();
        isBusy = false;
        RefreshButtonStates();
        if (statusText != null) statusText.text = "Следующий раунд: выбери 1 из 2 и собери цепочку";
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
        SpawnFxAt(GetTechniqueCastPrefab(type, source), source.AttackFxPoint);
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
            SpawnFxAt(GetTechniqueHitPrefab(GetTechniqueTypeFor(source), source), source.AttackFxPoint);
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

    private GameObject GetTechniqueCastPrefab(TechniqueType type, FighterAnimator source)
    {
        bool isEnemy = source != null && source == enemyAnimator;

        switch (type)
        {
            case TechniqueType.DragonFist:
                return isEnemy && enemyDragonFistCastFxPrefab != null ? enemyDragonFistCastFxPrefab : dragonFistCastFxPrefab;
            case TechniqueType.CraneKick:
                return isEnemy && enemyCraneKickCastFxPrefab != null ? enemyCraneKickCastFxPrefab : craneKickCastFxPrefab;
            case TechniqueType.MoonSlash:
                return isEnemy && enemyMoonSlashCastFxPrefab != null ? enemyMoonSlashCastFxPrefab : moonSlashCastFxPrefab;
            case TechniqueType.VoidPalm:
                return isEnemy && enemyVoidPalmCastFxPrefab != null ? enemyVoidPalmCastFxPrefab : voidPalmCastFxPrefab;
            default:
                return null;
        }
    }

    private GameObject GetTechniqueHitPrefab(TechniqueType type, FighterAnimator source)
    {
        bool isEnemy = source != null && source == enemyAnimator;

        switch (type)
        {
            case TechniqueType.DragonFist:
                return isEnemy && enemyDragonFistHitFxPrefab != null ? enemyDragonFistHitFxPrefab : dragonFistHitFxPrefab;
            case TechniqueType.CraneKick:
                return isEnemy && enemyCraneKickHitFxPrefab != null ? enemyCraneKickHitFxPrefab : craneKickHitFxPrefab;
            case TechniqueType.MoonSlash:
                return isEnemy && enemyMoonSlashHitFxPrefab != null ? enemyMoonSlashHitFxPrefab : moonSlashHitFxPrefab;
            case TechniqueType.VoidPalm:
                return isEnemy && enemyVoidPalmHitFxPrefab != null ? enemyVoidPalmHitFxPrefab : voidPalmHitFxPrefab;
            default:
                return null;
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
            SetText(playerRankText, $"Ранг: {playerStats.GetRankLabelRu()}");
        }

        if (enemyStats != null)
        {
            SetBar(enemyHpFillImage, enemyStats.CurrentHP, enemyStats.MaxHP);
            SetBar(enemyQiFillImage, enemyStats.CurrentQi, enemyStats.MaxQi);
            SetText(enemyHpText, $"HP Врага: {enemyStats.CurrentHP}/{enemyStats.MaxHP}");
            SetText(enemyQiText, $"Qi Врага: {enemyStats.CurrentQi}/{enemyStats.MaxQi}");
            SetText(enemyHpBarText, $"{enemyStats.CurrentHP}/{enemyStats.MaxHP}");
            SetText(enemyQiBarText, $"{enemyStats.CurrentQi}/{enemyStats.MaxQi}");
            SetText(enemyRankText, $"Ранг: {enemyStats.GetRankLabelRu()}");
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
                    playerSlots[i].SetMove(playerQueue[i], playerPreview[i].attackKind, playerPreview[i].techniqueType);
                else
                    playerSlots[i].SetMove(null);
            }
        }

        int visibleEnemy = revealEnemyFullQueue
            ? enemyQueue.Count
            : (rules != null ? Mathf.Clamp(rules.enemyVisibleMoves, 0, rules.slotCount) : 3);

        if (enemySlots != null)
        {
            for (int i = 0; i < enemySlots.Length; i++)
            {
                if (enemySlots[i] == null) continue;

                if (i < enemyQueue.Count && i < enemyPreview.Count)
                {
                    if (i < visibleEnemy)
                        enemySlots[i].SetMove(enemyQueue[i], enemyPreview[i].attackKind, enemyPreview[i].techniqueType);
                    else
                        enemySlots[i].SetHidden();
                }
                else
                {
                    enemySlots[i].SetMove(null);
                }
            }
        }

        RefreshOfferViews();
    }

    private void RefreshOfferViews()
    {
        MoveType? left = currentOffer.Count > 0 ? currentOffer[0] : (MoveType?)null;
        MoveType? right = currentOffer.Count > 1 ? currentOffer[1] : (MoveType?)null;

        if (choiceLeftView != null)
            choiceLeftView.SetMove(left);
        else
            SetChoiceButtonLabel(GetChoiceLeftButton(), left);

        if (choiceRightView != null)
            choiceRightView.SetMove(right);
        else
            SetChoiceButtonLabel(GetChoiceRightButton(), right);
    }

    private void SetChoiceButtonLabel(Button button, MoveType? move)
    {
        if (button == null)
            return;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = move.HasValue ? ToRu(move.Value) : "-";
    }

    private static void RemoveFirstOccurrence(List<MoveType> list, MoveType move)
    {
        if (list == null)
            return;

        int index = list.IndexOf(move);
        if (index >= 0)
            list.RemoveAt(index);
    }

    private static MoveType[] ShuffleMoves(MoveType[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int j = Random.Range(i, array.Length);
            MoveType tmp = array[i];
            array[i] = array[j];
            array[j] = tmp;
        }
        return array;
    }

    private static MoveType PickWeightedMove(List<MoveType> moves, List<int> weights)
    {
        if (moves == null || moves.Count == 0)
            return MoveType.Hand;

        int total = 0;
        for (int i = 0; i < weights.Count; i++)
            total += Mathf.Max(1, weights[i]);

        int roll = Random.Range(0, Mathf.Max(1, total));
        int cumulative = 0;
        for (int i = 0; i < moves.Count; i++)
        {
            cumulative += Mathf.Max(1, weights[i]);
            if (roll < cumulative)
                return moves[i];
        }

        return moves[moves.Count - 1];
    }

    private MoveType GetMasterCoverStyle(MoveType preferred)
    {
        switch (preferred)
        {
            case MoveType.Hand: return MoveType.Sword;
            case MoveType.Leg: return MoveType.Palm;
            case MoveType.Sword: return MoveType.Hand;
            case MoveType.Palm: return MoveType.Leg;
            default: return MoveType.Hand;
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

    private void SetButtonVisible(Button button, bool visible)
    {
        if (button != null && button.gameObject.activeSelf != visible)
            button.gameObject.SetActive(visible);
    }

    private void RefreshButtonStates()
    {
        bool inputAllowed = !isBusy && !battleFinished && rules != null;
        bool canPick = inputAllowed && playerQueue.Count < rules.slotCount && currentOffer.Count > 0;
        bool queueReady = inputAllowed && playerQueue.Count >= rules.slotCount;
        bool canClear = inputAllowed && playerQueue.Count > 0 && !clearUsedThisRound;

        Button leftChoice = GetChoiceLeftButton();
        Button rightChoice = GetChoiceRightButton();

        SetButtonVisible(leftChoice, canPick && currentOffer.Count > 0);
        SetButtonVisible(rightChoice, canPick && currentOffer.Count > 1);

        SetButton(leftChoice, canPick && currentOffer.Count > 0);
        SetButton(rightChoice, canPick && currentOffer.Count > 1);

        SetButtonVisible(handButton != leftChoice && handButton != rightChoice ? handButton : null, false);
        SetButtonVisible(legButton != leftChoice && legButton != rightChoice ? legButton : null, false);
        SetButtonVisible(swordButton, false);
        SetButtonVisible(palmButton, false);

        SetButton(clearButton, canClear);
        SetButton(fightButton, queueReady);
        SetButton(rerollEnemyButton, false);
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


    public void SetupExternalBattle(BattleLaunchData data)
    {
        if (data == null)
            return;

        if (playerStats != null)
            playerStats.ApplyExternalConfig(data.player);

        if (enemyStats != null)
            enemyStats.ApplyExternalConfig(data.enemy);

        ResetBattle();
    }

    public void StopBattleForHub()
    {
        StopAllCoroutines();

        isBusy = false;
        battleFinished = false;
        currentSlotActive = false;
        currentImpactResolved = false;
        currentBlockCenterSpawned = false;
        revealEnemyFullQueue = false;

        playerQueue.Clear();
        enemyQueue.Clear();
        playerRoundPool.Clear();
        originalPlayerRoundPool.Clear();
        currentOffer.Clear();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        RefreshUI();
        RefreshButtonStates();
    }

}
