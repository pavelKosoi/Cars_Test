using UnityEngine;
using static Utilities;

public class NpcControlStrategy : VehicleControlStrategyBase
{
    IPlaygroundBounds bounds;
  
    const int PathResolution = 30;
    Vector3[] waypoints = new Vector3[PathResolution];
    int currentWaypointIndex = 0;

    public override bool IsConstrainedByBounds => false;
    public NpcControlStrategy(CarController controller, IPlaygroundBounds bounds) : base(controller)
    {
        this.bounds = bounds;
    }

    public override void Reset()
    {
        currentWaypointIndex = 0;

        Vector3 startPoint = carController.transform.position;
        Vector3 startDirection = carController.transform.forward;
        Vector3 centerPoint = bounds.GetCenter();

        Vector3 targetPoint = bounds.GetPointOnOppositeSide(startPoint, startDirection);
        float interpolationStep = 1f / (PathResolution - 1);

        for (int i = 0; i < PathResolution; i++)
        {
            float t = i * interpolationStep;
            waypoints[i] = EvaluateQuadraticBezier(startPoint, centerPoint, targetPoint, t);
        }
    }

    public override Vector3 GetMoveDirection()
    {
#if UNITY_EDITOR
        foreach (var item in waypoints)
        {
            DrawPoint(item, 0.1f, Color.red);
        }
#endif 

        if (currentWaypointIndex >= PathResolution)
        {
            carController.ReturnToPool();
            return Vector3.zero;
        }

        Vector3 targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = targetWaypoint - carController.transform.position;

        if (direction.sqrMagnitude < 0.15f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex < PathResolution)
            {
                direction = waypoints[currentWaypointIndex] - carController.transform.position;
            }
        }

        return direction.normalized;
    }


}