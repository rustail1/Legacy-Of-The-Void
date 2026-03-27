using UnityEngine;

public class FighterCombatStats : MonoBehaviour
{
    [Header("Body")]
    [Min(0)] [SerializeField] private int bodyLevel = 0;

    [Header("HP Scaling")]
    [Min(0)] [SerializeField] private int baseHp = 100;
    [Min(0)] [SerializeField] private int hpPerBodyLevel = 10;

    [Header("Damage Scaling")]
    [Min(0)] [SerializeField] private int baseDamage = 10;
    [Min(0)] [SerializeField] private int damagePerBodyLevel = 5;
    [Min(0f)] [SerializeField] private float partialDamageMultiplier = 0.5f;

    [Header("Qi (set in Inspector)")]
    [Min(0)] [SerializeField] private int maxQi = 20;
    [Min(0)] [SerializeField] private int currentQi = 0;

    [Header("Unlocked Techniques")]
    [SerializeField] private bool unlockedDragonFist = true;
    [SerializeField] private bool unlockedCraneKick = true;
    [SerializeField] private bool unlockedMoonSlash = true;
    [SerializeField] private bool unlockedVoidPalm = true;

    public int BodyLevel => bodyLevel;
    public int BaseHp => baseHp;
    public int HpPerBodyLevel => hpPerBodyLevel;
    public int DamagePerBodyLevel => damagePerBodyLevel;
    public float PartialDamageMultiplier => partialDamageMultiplier;

    public int MaxHP => baseHp + bodyLevel * hpPerBodyLevel;
    public int BaseDamage => baseDamage + bodyLevel * damagePerBodyLevel;
    public int PartialDamage => Mathf.RoundToInt(BaseDamage * partialDamageMultiplier);
    public int MaxQi => maxQi;
    public int StartQi => Mathf.Clamp(currentQi, 0, maxQi);

    public int CurrentHP { get; private set; }
    public int CurrentQi { get; private set; }

    private void Awake()
    {
        ResetForBattle();
    }

    public void ResetForBattle()
    {
        CurrentHP = MaxHP;
        CurrentQi = StartQi;
    }

    public void ApplyHp(int value)
    {
        CurrentHP = Mathf.Clamp(value, 0, MaxHP);
    }

    public void ApplyQi(int value)
    {
        CurrentQi = Mathf.Clamp(value, 0, MaxQi);
    }

    public bool HasEnoughQi(int amount)
    {
        return CurrentQi >= amount;
    }

    public bool IsTechniqueUnlocked(TechniqueType techniqueType)
    {
        switch (techniqueType)
        {
            case TechniqueType.DragonFist: return unlockedDragonFist;
            case TechniqueType.CraneKick: return unlockedCraneKick;
            case TechniqueType.MoonSlash: return unlockedMoonSlash;
            case TechniqueType.VoidPalm: return unlockedVoidPalm;
            default: return false;
        }
    }
}
