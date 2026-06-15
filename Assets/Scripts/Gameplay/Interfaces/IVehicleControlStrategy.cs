using UnityEngine;

public interface IVehicleControlStrategy 
{
    bool IsConstrainedByBounds { get; }
    public void Reset();
    public Vector3 GetMoveDirection();
}
