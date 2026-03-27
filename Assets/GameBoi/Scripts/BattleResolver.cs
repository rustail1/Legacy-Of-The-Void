using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackPlan
{
    public MoveType moveType;
    public AttackKind attackKind;
    public TechniqueType techniqueType;
}

[System.Serializable]
public class SlotResolution
{
    public int slotIndex;
    public MoveType playerMove;
    public MoveType enemyMove;
    public ExchangeType exchangeType;
    public WinnerSide winner;

    public AttackKind playerAttackKind;
    public AttackKind enemyAttackKind;
    public TechniqueType playerTechniqueType;
    public TechniqueType enemyTechniqueType;

    public int playerDamageTaken;
    public int enemyDamageTaken;
    public int playerHpAfter;
    public int enemyHpAfter;
    public int playerQiBefore;
    public int playerQiAfter;
    public int enemyQiBefore;
    public int enemyQiAfter;
}

[System.Serializable]
public class BattleResolution
{
    public List<SlotResolution> slots = new List<SlotResolution>();
    public bool playerDefeated;
    public bool enemyDefeated;
}

public static class BattleResolver
{
    public static BattleResolution Resolve(
        List<MoveType> playerQueue,
        List<MoveType> enemyQueue,
        BattleRules rules,
        FighterCombatStats playerStats,
        FighterCombatStats enemyStats)
    {
        BattleResolution resolution = new BattleResolution();

        int playerHp = playerStats.CurrentHP;
        int enemyHp = enemyStats.CurrentHP;
        int playerQi = playerStats.CurrentQi;
        int enemyQi = enemyStats.CurrentQi;

        int count = Mathf.Min(rules.slotCount, Mathf.Min(playerQueue.Count, enemyQueue.Count));
        for (int i = 0; i < count; i++)
        {
            int playerQiBefore = playerQi;
            int enemyQiBefore = enemyQi;

            AttackPlan playerAttack = BuildAttackPlan(playerQueue, i, playerQi, playerStats, rules);
            if (playerAttack.attackKind == AttackKind.Technique)
                playerQi = Mathf.Max(0, playerQi - rules.techniqueCost);

            AttackPlan enemyAttack = BuildAttackPlan(enemyQueue, i, enemyQi, enemyStats, rules);
            if (enemyAttack.attackKind == AttackKind.Technique)
                enemyQi = Mathf.Max(0, enemyQi - rules.techniqueCost);

            ExchangeType exchangeType;
            WinnerSide winner;
            int playerDamage = 0;
            int enemyDamage = 0;

            if (playerAttack.moveType == enemyAttack.moveType)
            {
                exchangeType = ExchangeType.FullBlock;
                winner = WinnerSide.None;
            }
            else if (Beats(playerAttack.moveType, enemyAttack.moveType))
            {
                exchangeType = ExchangeType.FullHit;
                winner = WinnerSide.Player;
                enemyDamage = GetDamageForAttack(playerAttack, exchangeType, playerStats, rules);
            }
            else if (Beats(enemyAttack.moveType, playerAttack.moveType))
            {
                exchangeType = ExchangeType.FullHit;
                winner = WinnerSide.Enemy;
                playerDamage = GetDamageForAttack(enemyAttack, exchangeType, enemyStats, rules);
            }
            else
            {
                exchangeType = ExchangeType.PartialHit;
                winner = WinnerSide.None;
                playerDamage = GetDamageForAttack(enemyAttack, exchangeType, enemyStats, rules);
                enemyDamage = GetDamageForAttack(playerAttack, exchangeType, playerStats, rules);
            }

            playerHp = Mathf.Max(0, playerHp - playerDamage);
            enemyHp = Mathf.Max(0, enemyHp - enemyDamage);

            resolution.slots.Add(new SlotResolution
            {
                slotIndex = i,
                playerMove = playerAttack.moveType,
                enemyMove = enemyAttack.moveType,
                exchangeType = exchangeType,
                winner = winner,
                playerAttackKind = playerAttack.attackKind,
                enemyAttackKind = enemyAttack.attackKind,
                playerTechniqueType = playerAttack.techniqueType,
                enemyTechniqueType = enemyAttack.techniqueType,
                playerDamageTaken = playerDamage,
                enemyDamageTaken = enemyDamage,
                playerHpAfter = playerHp,
                enemyHpAfter = enemyHp,
                playerQiBefore = playerQiBefore,
                playerQiAfter = playerQi,
                enemyQiBefore = enemyQiBefore,
                enemyQiAfter = enemyQi
            });

            if (playerHp <= 0 || enemyHp <= 0)
                break;
        }

        resolution.playerDefeated = playerHp <= 0;
        resolution.enemyDefeated = enemyHp <= 0;
        return resolution;
    }

    public static List<AttackPlan> BuildPreviewPlans(List<MoveType> queue, int availableQi, FighterCombatStats stats, BattleRules rules)
    {
        List<AttackPlan> plans = new List<AttackPlan>();
        if (queue == null || stats == null || rules == null)
            return plans;

        int qi = availableQi;
        for (int i = 0; i < queue.Count; i++)
        {
            AttackPlan plan = BuildAttackPlan(queue, i, qi, stats, rules);
            plans.Add(plan);
            if (plan.attackKind == AttackKind.Technique)
                qi = Mathf.Max(0, qi - rules.techniqueCost);
        }

        return plans;
    }

    private static AttackPlan BuildAttackPlan(List<MoveType> queue, int index, int currentQi, FighterCombatStats stats, BattleRules rules)
    {
        MoveType moveType = queue[index];
        TechniqueType techniqueType = MapTechnique(moveType);

        bool hasCombo = index >= 2 &&
                        queue[index] == queue[index - 1] &&
                        queue[index] == queue[index - 2];

        if (hasCombo && currentQi >= rules.techniqueCost && stats.IsTechniqueUnlocked(techniqueType))
        {
            return new AttackPlan
            {
                moveType = moveType,
                attackKind = AttackKind.Technique,
                techniqueType = techniqueType
            };
        }

        return new AttackPlan
        {
            moveType = moveType,
            attackKind = AttackKind.Normal,
            techniqueType = TechniqueType.None
        };
    }

    private static int GetDamageForAttack(AttackPlan attack, ExchangeType exchangeType, FighterCombatStats stats, BattleRules rules)
    {
        if (exchangeType == ExchangeType.FullBlock)
            return 0;

        int normalDamage = exchangeType == ExchangeType.FullHit
            ? stats.BaseDamage
            : stats.PartialDamage;

        if (attack.attackKind == AttackKind.Technique)
            return Mathf.RoundToInt(normalDamage * Mathf.Max(0f, rules.techniqueDamageMultiplier));

        return normalDamage;
    }

    private static TechniqueType MapTechnique(MoveType moveType)
    {
        switch (moveType)
        {
            case MoveType.Hand: return TechniqueType.DragonFist;
            case MoveType.Leg: return TechniqueType.CraneKick;
            case MoveType.Sword: return TechniqueType.MoonSlash;
            case MoveType.Palm: return TechniqueType.VoidPalm;
            default: return TechniqueType.None;
        }
    }

    public static bool Beats(MoveType attacker, MoveType defender)
    {
        return (attacker == MoveType.Leg && defender == MoveType.Hand) ||
               (attacker == MoveType.Hand && defender == MoveType.Palm) ||
               (attacker == MoveType.Palm && defender == MoveType.Sword) ||
               (attacker == MoveType.Sword && defender == MoveType.Leg);
    }
}
