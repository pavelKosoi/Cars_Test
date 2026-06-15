using UnityEngine;

public class CircularPlayground : Playground
{
    [SerializeField] float radius;
    [SerializeField, Range(0f, 1f)] float spawnRadiusMultiplier = 0.8f;

    public override Vector3 ConstrainPoint(Vector3 desiredPoint, float objectRadius = 0f)
    {
        Vector3 offset = desiredPoint - transform.position;

        float effectiveRadius = Mathf.Max(0, radius - objectRadius);

        if (offset.sqrMagnitude > effectiveRadius * effectiveRadius)
        {
            return transform.position + offset.normalized * effectiveRadius;
        }
        return desiredPoint;
    }

    public override Vector3 GetCenter() => transform.position;

    public override Vector3 GetPointOnOppositeSide(Vector3 startPoint, Vector3 forwardDirection) =>
        Utilities.GetPointOnRimWithExclusion(transform.position, radius, forwardDirection);

    public override Vector3 GetRandomPointOnEdge() =>
        Utilities.GetPointOnCircleRim(transform.position, radius);

    public override Vector3 GetQuadrantSpawnPoint(SpawnQuadrant quadrant)
    {
        Vector3 direction = quadrant switch
        {
            SpawnQuadrant.North => Vector3.forward,
            SpawnQuadrant.East => Vector3.right,
            SpawnQuadrant.South => Vector3.back,
            SpawnQuadrant.West => Vector3.left,
            _ => Vector3.back
        };

        return GetCenter() + direction * (radius * spawnRadiusMultiplier);
    }

    public override Vector3 GetRandomPointInside(float scaleMultiplier = 1f)
    {
        float clampedScale = Mathf.Clamp01(scaleMultiplier);

        Vector2 randomCircle = Random.insideUnitCircle * (radius * clampedScale);
        return transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}