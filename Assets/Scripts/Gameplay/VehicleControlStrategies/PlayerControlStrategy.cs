using UnityEngine;

public class PlayerControlStrategy : VehicleControlStrategyBase
{
    private IInputProvider inputProvider;

    public PlayerControlStrategy(CarController carController, IInputProvider inputProvider) : base(carController)
    {
        this.inputProvider = inputProvider;
    }
 

    public override Vector3 GetMoveDirection()
    {
        if (inputProvider.TryGetTargetPoint(out Vector3 worldPoint))
        {
            Vector3 direction = worldPoint - carController.transform.position;
            if (direction.sqrMagnitude < 0.1f) return Vector3.zero;

            return direction.normalized;
        }
      
        return Vector3.zero;
    }
}