using UnityEngine;

public abstract class Playground : MonoBehaviour, IPlaygroundBounds
{
    public abstract Vector3 ConstrainPoint(Vector3 desiredPoint, float objectRadius = 0);

    public abstract Vector3 GetCenter();

    public abstract Vector3 GetPointOnOppositeSide(Vector3 startPoint, Vector3 forwardDirection);

    public abstract Vector3 GetQuadrantSpawnPoint(SpawnQuadrant quadrant);

    public abstract Vector3 GetRandomPointInside(float scaleMultiplier = 1);

    public abstract Vector3 GetRandomPointOnEdge(); 
}
