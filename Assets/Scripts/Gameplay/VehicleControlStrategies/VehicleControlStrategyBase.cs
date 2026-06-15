using UnityEngine;

public abstract class VehicleControlStrategyBase : IVehicleControlStrategy
{
    protected CarController carController;
    public VehicleControlStrategyBase(CarController carController)
    {
        this.carController = carController;
    }

    public abstract Vector3 GetMoveDirection();
    public virtual void Reset() { }
    public virtual bool IsConstrainedByBounds => true;
}
