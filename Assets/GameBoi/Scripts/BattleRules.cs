using UnityEngine;

[CreateAssetMenu(fileName = "BattleRules", menuName = "Murim AutoBattle/Battle Rules")]
public class BattleRules : ScriptableObject
{
    [Header("Round")]
    [Min(1)] public int slotCount = 5;

    [Header("Technique")]
    [Min(0)] public int techniqueCost = 8;
    [Min(0f)] public float techniqueDamageMultiplier = 2f;

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
}
