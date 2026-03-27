using UnityEngine;

public class FighterMover : MonoBehaviour
{
    public void SnapTo(Transform point)
    {
        if (point == null) return;
        transform.position = point.position;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void FaceTarget(Transform target)
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}
