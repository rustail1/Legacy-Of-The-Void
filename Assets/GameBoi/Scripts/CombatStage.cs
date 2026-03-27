using UnityEngine;

public class CombatStage : MonoBehaviour
{
    [Header("Start Points")]
    public Transform playerStart;
    public Transform enemyStart;

    [Header("Impact Points")]
    public Transform playerImpact;
    public Transform enemyImpact;

    [Header("Sword Impact Points (optional)")]
    public Transform playerSwordImpact;
    public Transform enemySwordImpact;

    [Header("Center FX Point")]
    public Transform impactFxPoint;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawPoint(playerStart, Color.green);
        DrawPoint(enemyStart, Color.green);
        DrawPoint(playerImpact, Color.yellow);
        DrawPoint(enemyImpact, Color.yellow);
        DrawPoint(playerSwordImpact, new Color(1f, 0.6f, 0f));
        DrawPoint(enemySwordImpact, new Color(1f, 0.6f, 0f));
        DrawPoint(impactFxPoint, Color.magenta);
    }

    private static void DrawPoint(Transform point, Color color)
    {
        if (point == null) return;
        Gizmos.color = color;
        Gizmos.DrawSphere(point.position, 0.08f);
    }
#endif
}
