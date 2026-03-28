using UnityEngine;

public class FighterAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AutoBattleController controller;
    [SerializeField] private string layerName = "Base Layer";

    [Header("FX Points")]
    [SerializeField] private Transform attackFxPoint; // legacy fallback
    [SerializeField] private Transform handFxPoint;
    [SerializeField] private Transform palmFxPoint;
    [SerializeField] private Transform legFxPoint;
    [SerializeField] private Transform swordFxPoint;
    [SerializeField] private Transform damageFxPoint;

    [Header("Legacy Shared FX Point")]
    [SerializeField, HideInInspector] private Transform handPalmFxPoint;

    [Header("Sword Visuals")]
    [SerializeField] private GameObject swordOnBackObject;
    [SerializeField] private GameObject swordInHandObject;

    [Header("Normal Attack Trails Per Move")]
    [SerializeField] private ParticleSystem handAttackTrail;
    [SerializeField] private ParticleSystem legAttackTrail;
    [SerializeField] private ParticleSystem swordAttackTrail;
    [SerializeField] private ParticleSystem palmAttackTrail;
    [SerializeField] private ParticleSystem normalAttackTrail; // legacy fallback

    [Header("Technique Trails")]
    [SerializeField] private ParticleSystem techniqueAttackTrail;

    [Header("State Names")]
    [SerializeField] private string idleState = "BattleIdle";
    [SerializeField] private string stepForwardState = "StepForward";
    [SerializeField] private string stepBackState = "StepBack";
    [SerializeField] private string handAttackState = "HandAttack";
    [SerializeField] private string legAttackState = "LegAttack";
    [SerializeField] private string swordAttackState = "SwordAttack";
    [SerializeField] private string palmAttackState = "PalmAttack";
    [SerializeField] private string hitReactState = "HitReact";
    [SerializeField] private string victoryState = "victoryState";
    [SerializeField] private string defeatState = "defeatState";
    [SerializeField] private string techBlockReactState = "TechBlockReact";
    [SerializeField] private string techHitLightState = "TechHitLight";
    [SerializeField] private string techHitHeavyState = "TechHitHeavy";

    [Header("Legacy Hidden Fields")]
    [SerializeField, HideInInspector] private string techFullBlockReactState = "TechFullBlockReact";
    [SerializeField, HideInInspector] private string techBlockReactFallbackState = "TechBlockReact";

    [Header("Blend")]
    [SerializeField] private float defaultBlend = 0.05f;

    public bool AttackFinished { get; private set; }
    public bool AttackInterrupted { get; private set; }
    public MoveType CurrentMove { get; private set; }
    public AttackKind CurrentAttackKind { get; private set; } = AttackKind.Normal;
    public TechniqueType CurrentTechniqueType { get; private set; } = TechniqueType.None;
    public Transform AttackFxPoint => GetAttackFxPoint();
    public Transform DamageFxPoint => damageFxPoint != null ? damageFxPoint : transform;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        if (controller == null)
            controller = FindObjectOfType<AutoBattleController>();

        UpgradeLegacyTechniqueStateFields();
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (controller == null)
            controller = FindObjectOfType<AutoBattleController>();

        UpgradeLegacyTechniqueStateFields();
    }

    private void OnValidate()
    {
        UpgradeLegacyTechniqueStateFields();
    }

    public void PlayIdle() => PlayState(idleState, false);
    public void PlayStepForward() => PlayState(stepForwardState, ShouldKeepSwordInHandForCurrentMove());
    public void PlayStepBack() => PlayState(stepBackState, ShouldKeepSwordInHandForCurrentMove());
    public void PlayHitReact() => PlayState(hitReactState, ShouldKeepSwordInHandForCurrentMove());
    public void PlayVictory() => PlayStateOrFallback(victoryState, idleState, false);
    public void PlayDefeat() => PlayStateOrFallback(defeatState, hitReactState, ShouldKeepSwordInHandForCurrentMove());
    public void PlayTechBlockReact() => PlayStateOrFallback(techBlockReactState, stepBackState, ShouldKeepSwordInHandForCurrentMove());
    public void PlayTechHitLight() => PlayStateOrFallback(techHitLightState, stepBackState, ShouldKeepSwordInHandForCurrentMove());
    public void PlayTechHitHeavy() => PlayStateOrFallback(techHitHeavyState, hitReactState, ShouldKeepSwordInHandForCurrentMove());

    public void PlayAttack(MoveType move, AttackKind attackKind, TechniqueType techniqueType)
    {
        CurrentMove = move;
        CurrentAttackKind = attackKind;
        CurrentTechniqueType = techniqueType;
        AttackFinished = false;
        AttackInterrupted = false;

        switch (move)
        {
            case MoveType.Hand: PlayState(handAttackState, false); break;
            case MoveType.Leg: PlayState(legAttackState, false); break;
            case MoveType.Sword: PlayState(swordAttackState, true); break;
            case MoveType.Palm: PlayState(palmAttackState, false); break;
        }
    }

    public void InterruptToHitReact()
    {
        AttackInterrupted = true;
        AttackFinished = true;
        PlayHitReact();
    }

    public void InterruptToTechniqueReaction(ExchangeType exchangeType)
    {
        AttackInterrupted = true;
        AttackFinished = true;

        switch (exchangeType)
        {
            case ExchangeType.FullBlock:
                PlayTechBlockReact();
                break;
            case ExchangeType.PartialHit:
                PlayTechHitLight();
                break;
            case ExchangeType.FullHit:
                PlayTechHitHeavy();
                break;
        }
    }

    public void AE_AttackFinished()
    {
        AttackFinished = true;
    }

    public void AE_AttackImpact()
    {
        if (controller != null)
            controller.OnAttackImpact(this);
    }

    public void AE_SpawnAttackSourceFx()
    {
        if (controller != null)
            controller.OnAttackSourceFx(this);
    }

    public void AE_SpawnDamageFx()
    {
        if (controller != null)
            controller.OnDamageFx(this);
    }

    public void AE_SpawnBlockContactFx()
    {
        if (controller != null)
            controller.OnBlockContactFx(this);
    }

    public void AE_SpawnTechniqueCastFx()
    {
        if (controller != null)
            controller.OnTechniqueCastFx(this);
    }

    public void AE_PlayMoveSwingSfx()
    {
        if (controller != null)
            controller.OnMoveSwingSfx(this);
    }

    public void AE_PlayStepForwardSfx()
    {
        if (controller != null)
            controller.OnStepForwardSfx();
    }

    public void AE_PlayStepBackSfx()
    {
        if (controller != null)
            controller.OnStepBackSfx();
    }

    public void AE_PlayDashInSfx()
    {
        if (controller != null)
            controller.OnDashInSfx();
    }

    public void AE_PlayLandingBackSfx()
    {
        if (controller != null)
            controller.OnLandingBackSfx();
    }

    public void AE_StartAttackTrail()
    {
        if (controller != null && !controller.CanPlayAttackTrail(this))
            return;

        ParticleSystem ps = CurrentAttackKind == AttackKind.Technique
            ? techniqueAttackTrail
            : GetNormalTrailForMove(CurrentMove);

        if (ps == null) return;

        ps.Clear(true);
        ps.Play(true);
    }

    public void AE_StopAttackTrail()
    {
        StopTrail(handAttackTrail);
        StopTrail(legAttackTrail);
        StopTrail(swordAttackTrail);
        StopTrail(palmAttackTrail);
        StopTrail(normalAttackTrail);
        StopTrail(techniqueAttackTrail);
    }

    private ParticleSystem GetNormalTrailForMove(MoveType move)
    {
        switch (move)
        {
            case MoveType.Hand:
                return handAttackTrail != null ? handAttackTrail : normalAttackTrail;
            case MoveType.Leg:
                return legAttackTrail != null ? legAttackTrail : normalAttackTrail;
            case MoveType.Sword:
                return swordAttackTrail != null ? swordAttackTrail : normalAttackTrail;
            case MoveType.Palm:
                return palmAttackTrail != null ? palmAttackTrail : normalAttackTrail;
            default:
                return normalAttackTrail;
        }
    }

    private void StopTrail(ParticleSystem ps)
    {
        if (ps != null)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void ForceAttackFinished()
    {
        AttackFinished = true;
    }

    private void PlayStateOrFallback(string requestedState, string fallbackState, bool swordInHand)
    {
        if (HasState(requestedState)) PlayState(requestedState, swordInHand);
        else PlayState(fallbackState, swordInHand);
    }

    private bool HasState(string stateName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName)) return false;
        return animator.HasState(0, Animator.StringToHash($"{layerName}.{stateName}"));
    }

    private bool ShouldKeepSwordInHandForCurrentMove()
    {
        return CurrentMove == MoveType.Sword;
    }

    private void UpgradeLegacyTechniqueStateFields()
    {
        if (string.IsNullOrWhiteSpace(techBlockReactState) || techBlockReactState == "TechBlockReact")
        {
            if (!string.IsNullOrWhiteSpace(techBlockReactFallbackState))
                techBlockReactState = techBlockReactFallbackState;
        }

        if (string.IsNullOrWhiteSpace(techHitLightState))
            techHitLightState = "TechHitLight";

        if (string.IsNullOrWhiteSpace(techHitHeavyState))
            techHitHeavyState = "TechHitHeavy";
    }

    private Transform GetAttackFxPoint()
    {
        Transform point = null;
        switch (CurrentMove)
        {
            case MoveType.Hand:
                point = handFxPoint != null ? handFxPoint : handPalmFxPoint;
                break;
            case MoveType.Palm:
                point = palmFxPoint != null ? palmFxPoint : handPalmFxPoint;
                break;
            case MoveType.Leg:
                point = legFxPoint;
                break;
            case MoveType.Sword:
                point = swordFxPoint;
                break;
        }

        if (point != null)
            return point;
        if (attackFxPoint != null)
            return attackFxPoint;
        return transform;
    }

    private void SetSwordVisual(bool inHand)
    {
        if (swordOnBackObject != null) swordOnBackObject.SetActive(!inHand);
        if (swordInHandObject != null) swordInHandObject.SetActive(inHand);
    }

    private void PlayState(string stateName, bool swordInHand)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName)) return;
        SetSwordVisual(swordInHand);
        animator.CrossFade($"{layerName}.{stateName}", defaultBlend);
    }
}
