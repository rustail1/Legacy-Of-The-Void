using UnityEngine;

[CreateAssetMenu(fileName = "BattleRules", menuName = "Murim AutoBattle/Battle Rules")]
public class BattleRules : ScriptableObject
{
    [Header("Round Core")]
    [Min(1)] public int slotCount = 5;
    [Min(2)] public int roundPoolSize = 10;
    [Range(1, 4)] public int enemyVisibleMoves = 3;

    [Header("Pool Shape Weights")]
    [Min(0)] public int shape4222Weight = 15;
    [Min(0)] public int shape3322Weight = 50;
    [Min(0)] public int shape3331Weight = 35;

    [Header("Smart Pair Weights")]
    [Min(0)] public int pairSeriesWeight = 3;
    [Min(0)] public int pairCounterEnemyWeight = 2;
    [Min(0)] public int pairStyleWeight = 1;
    [Min(0)] public int pairNeutralWeight = 1;
    [Range(0f, 1f)] public float pairSecondRandomChance = 0.25f;

    [Header("Anti-Deadlock")]
    [Min(0)] public int protectedOfferCount = 3;
    [Min(0)] public int minimumMeaningfulOffersInProtectedWindow = 2;

    [Header("Technique Qi Cost")]
    [Min(0)] public int dragonFistQiCost = 6;
    [Min(0)] public int craneKickQiCost = 10;
    [Min(0)] public int moonSlashQiCost = 14;
    [Min(0)] public int voidPalmQiCost = 20;

    [Header("Technique Damage Multiplier")]
    [Min(0f)] public float dragonFistDamageMultiplier = 2f;
    [Min(0f)] public float craneKickDamageMultiplier = 2.5f;
    [Min(0f)] public float moonSlashDamageMultiplier = 3f;
    [Min(0f)] public float voidPalmDamageMultiplier = 3.2f;

    [Header("Movement Speed")]
    [Min(0.1f)] public float rushSpeed = 14f;
    [Min(0.1f)] public float returnSpeed = 10f;

    [Header("Presentation")]
    [Min(0f)] public float stepLeadTime = 0.06f;
    [Min(0f)] public float resultPause = 0.10f;
    [Min(0f)] public float crossFadeDuration = 0.05f;
    [Min(0.1f)] public float impactEventTimeout = 1.20f;
    [Min(0.1f)] public float finishEventTimeout = 1.20f;

    [Header("Hit Stop")]
    [Min(0f)] public float fullHitStop = 0.05f;
    [Min(0f)] public float techniqueHitStop = 0.065f;
    [Min(0f)] public float partialHitStop = 0.03f;
    [Min(0f)] public float blockStop = 0.025f;

    [Header("Camera Shake")]
    [Min(0f)] public float fullHitShakeDuration = 0.10f;
    [Min(0f)] public float fullHitShakeMagnitude = 0.08f;
    [Min(0f)] public float techniqueHitShakeDuration = 0.12f;
    [Min(0f)] public float techniqueHitShakeMagnitude = 0.12f;
    [Min(0f)] public float partialHitShakeDuration = 0.06f;
    [Min(0f)] public float partialHitShakeMagnitude = 0.04f;
    [Min(0f)] public float blockShakeDuration = 0.05f;
    [Min(0f)] public float blockShakeMagnitude = 0.03f;

    public int GetTechniqueQiCost(TechniqueType techniqueType)
    {
        switch (techniqueType)
        {
            case TechniqueType.DragonFist: return dragonFistQiCost;
            case TechniqueType.CraneKick: return craneKickQiCost;
            case TechniqueType.MoonSlash: return moonSlashQiCost;
            case TechniqueType.VoidPalm: return voidPalmQiCost;
            default: return 0;
        }
    }

    public float GetTechniqueDamageMultiplier(TechniqueType techniqueType)
    {
        switch (techniqueType)
        {
            case TechniqueType.DragonFist: return dragonFistDamageMultiplier;
            case TechniqueType.CraneKick: return craneKickDamageMultiplier;
            case TechniqueType.MoonSlash: return moonSlashDamageMultiplier;
            case TechniqueType.VoidPalm: return voidPalmDamageMultiplier;
            default: return 1f;
        }
    }
}
