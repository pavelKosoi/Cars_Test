using UnityEngine;

public interface IPlaygroundBounds
{
    Vector3 GetCenter();
    Vector3 ConstrainPoint(Vector3 desiredPoint, float objectRadius = 0);
    Vector3 GetPointOnOppositeSide(Vector3 startPoint, Vector3 forwardDirection);
    Vector3 GetRandomPointOnEdge();
    Vector3 GetQuadrantSpawnPoint(SpawnQuadrant quadrant);
    Vector3 GetRandomPointInside(float scaleMultiplier = 1);
}

public enum SpawnQuadrant
{
    North,
    East,
    South,
    West
}