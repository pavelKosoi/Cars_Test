using System.Collections.Generic;
using UnityEngine;

public interface ITrajectoryProvider
{
    bool HasTrajectory { get; }
    Vector3 TargetPoint { get; }
    void GetTrajectoryPoints(List<Vector3> buffer, int resolution, float yOffset);
}