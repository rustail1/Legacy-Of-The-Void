using UnityEngine;

public class FighterCombatStats : MonoBehaviour
{
    [Header("Rank")]
    [SerializeField] private MasterRank rank = MasterRank.ThirdRate;
    [SerializeField] private bool useManualStats = false;

    [Header("Technique Tier Limit")]
    [SerializeField] private bool useManualTechniqueTierRange = false;
    [SerializeField] private TechniqueTier manualMinTechniqueTier = TechniqueTier.T1;
    [SerializeField] private TechniqueTier manualMaxTechniqueTier = TechniqueTier.T4;

    [Header("Body / Qi")]
    [Min(0)] [SerializeField] private int bodyLevel = 0;
    [Min(0)] [SerializeField] private int qiLevel = 0;

    [Header("HP Scaling")]
    [Min(0)] [SerializeField] private int baseHp = 50;
    [Min(0)] [SerializeField] private int hpPerBodyLevel = 20;

    [Header("Damage Scaling")]
    [Min(0)] [SerializeField] private int baseDamage = 10;
    [Min(0)] [SerializeField] private int damagePerBodyLevel = 2;
    [Min(0f)] [SerializeField] private float partialDamageMultiplier = 0.6f;

    [Header("Qi Scaling")]
    [Min(0)] [SerializeField] private int baseMaxQi = 20;
    [Min(0)] [SerializeField] private int maxQiPerQiLevel = 5;
    [Min(0)] [SerializeField] private int startQiPerQiLevel = 2;

    [Header("Unlocked Techniques")]
    [SerializeField] private bool unlockedDragonFist = true;
    [SerializeField] private bool unlockedCraneKick = true;
    [SerializeField] private bool unlockedMoonSlash = true;
    [SerializeField] private bool unlockedVoidPalm = true;

    public MasterRank Rank => rank;
    public bool UseManualStats => useManualStats;
    public bool UseManualTechniqueTierRange => useManualTechniqueTierRange;
    public int BodyLevel => bodyLevel;
    public int QiLevel => qiLevel;

    public int RankBattleQiBonus => GetRankBattleQiBonus(rank);

    public int MaxHP => baseHp + bodyLevel * hpPerBodyLevel;
    public int BaseDamage => baseDamage + bodyLevel * damagePerBodyLevel;
    public int PartialDamage => Mathf.RoundToInt(BaseDamage * partialDamageMultiplier);
    public int MaxQi => baseMaxQi + qiLevel * maxQiPerQiLevel;
    public int StartQi => Mathf.Clamp(qiLevel * startQiPerQiLevel, 0, MaxQi);
    public int BattleQi => Mathf.Clamp(StartQi + RankBattleQiBonus, 0, MaxQi);

    public int CurrentHP { get; private set; }
    public int CurrentQi { get; private set; }

    private void Awake()
    {
        ApplyRankPresetIfNeeded();
        ResetForBattle();
    }

    private void OnValidate()
    {
        ApplyRankPresetIfNeeded();
        if ((int)manualMinTechniqueTier > (int)manualMaxTechniqueTier)
            manualMaxTechniqueTier = manualMinTechniqueTier;
    }

    public void ResetForBattle()
    {
        ApplyRankPresetIfNeeded();
        CurrentHP = MaxHP;
        CurrentQi = MaxQi;
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

    public bool IsTechniqueAllowedByTier(TechniqueType techniqueType)
    {
        TechniqueTier currentTier = GetTechniqueTier(techniqueType);
        GetAllowedTechniqueTierRange(out TechniqueTier minTier, out TechniqueTier maxTier);
        return (int)currentTier >= (int)minTier && (int)currentTier <= (int)maxTier;
    }

    public void GetAllowedTechniqueTierRange(out TechniqueTier minTier, out TechniqueTier maxTier)
    {
        if (useManualTechniqueTierRange)
        {
            minTier = manualMinTechniqueTier;
            maxTier = manualMaxTechniqueTier;
            if ((int)minTier > (int)maxTier)
                maxTier = minTier;
            return;
        }

        switch (rank)
        {
            case MasterRank.ThirdRate:
                minTier = TechniqueTier.T1;
                maxTier = TechniqueTier.T1;
                return;
            case MasterRank.SecondRate:
                minTier = TechniqueTier.T1;
                maxTier = TechniqueTier.T1;
                return;
            case MasterRank.FirstRate:
                minTier = TechniqueTier.T1;
                maxTier = TechniqueTier.T2;
                return;
            case MasterRank.PeakMaster:
                minTier = TechniqueTier.T2;
                maxTier = TechniqueTier.T3;
                return;
            case MasterRank.OneFlower:
                minTier = TechniqueTier.T3;
                maxTier = TechniqueTier.T4;
                return;
            default:
                minTier = TechniqueTier.T1;
                maxTier = TechniqueTier.T1;
                return;
        }
    }

    public string GetTechniqueTierRangeLabelRu()
    {
        GetAllowedTechniqueTierRange(out TechniqueTier minTier, out TechniqueTier maxTier);
        if (minTier == maxTier)
            return minTier.ToString();
        return $"{minTier}-{maxTier}";
    }

    public string GetRankLabelRu()
    {
        switch (rank)
        {
            case MasterRank.ThirdRate: return "Третьесортный";
            case MasterRank.SecondRate: return "Второсортный";
            case MasterRank.FirstRate: return "Первосортный";
            case MasterRank.PeakMaster: return "Мастер пика";
            case MasterRank.OneFlower: return "Один цветок";
            default: return "?";
        }
    }

    private void ApplyRankPresetIfNeeded()
    {
        if (useManualStats)
            return;

        GetRankBodyRange(rank, out int bodyMin, out int bodyMax);
        GetRankQiRange(rank, out int qiMin, out int qiMax);

        bodyLevel = Mathf.RoundToInt((bodyMin + bodyMax) * 0.5f);
        qiLevel = Mathf.RoundToInt((qiMin + qiMax) * 0.5f);
    }

    private static TechniqueTier GetTechniqueTier(TechniqueType techniqueType)
    {
        switch (techniqueType)
        {
            case TechniqueType.DragonFist: return TechniqueTier.T1;
            case TechniqueType.CraneKick: return TechniqueTier.T2;
            case TechniqueType.MoonSlash: return TechniqueTier.T3;
            case TechniqueType.VoidPalm: return TechniqueTier.T4;
            default: return TechniqueTier.T1;
        }
    }

    private static int GetRankBattleQiBonus(MasterRank rankValue)
    {
        switch (rankValue)
        {
            case MasterRank.ThirdRate: return 4;
            case MasterRank.SecondRate: return 6;
            case MasterRank.FirstRate: return 8;
            case MasterRank.PeakMaster: return 10;
            case MasterRank.OneFlower: return 12;
            default: return 0;
        }
    }

    private static void GetRankBodyRange(MasterRank rankValue, out int min, out int max)
    {
        switch (rankValue)
        {
            case MasterRank.ThirdRate:
                min = 0; max = 5; return;
            case MasterRank.SecondRate:
                min = 5; max = 10; return;
            case MasterRank.FirstRate:
                min = 10; max = 20; return;
            case MasterRank.PeakMaster:
                min = 20; max = 35; return;
            case MasterRank.OneFlower:
                min = 35; max = 50; return;
            default:
                min = 0; max = 0; return;
        }
    }

    private static void GetRankQiRange(MasterRank rankValue, out int min, out int max)
    {
        switch (rankValue)
        {
            case MasterRank.ThirdRate:
                min = 0; max = 5; return;
            case MasterRank.SecondRate:
                min = 5; max = 10; return;
            case MasterRank.FirstRate:
                min = 10; max = 20; return;
            case MasterRank.PeakMaster:
                min = 20; max = 30; return;
            case MasterRank.OneFlower:
                min = 30; max = 40; return;
            default:
                min = 0; max = 0; return;
        }
    }
}
