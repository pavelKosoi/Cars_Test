using UnityEngine;

public readonly struct CarTelemetry
{
    public readonly float Speed;
    public readonly float AngularVelocity;
    public readonly Vector3 Velocity;
    public readonly float SteeringAlignment;
    public readonly float TurnDirection; 

    public CarTelemetry(float speed, float angularVelocity, Vector3 velocity, float steeringAlignment, float turnDirection)
    {
        Speed = speed;
        AngularVelocity = angularVelocity;
        Velocity = velocity;
        SteeringAlignment = steeringAlignment;
        TurnDirection = turnDirection;
    }
}