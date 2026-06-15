using UnityEngine;

public interface IVehicle
{
    public void MoveTo(Vector3 targetDirection);
    public void InjectControlStrategy<T>() where T : IVehicleControlStrategy;
}
