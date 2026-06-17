using System;
using UnityEngine;

public interface IVehicle
{
    void MoveTo(Vector3 targetDirection);
    bool Stop { get; set; }
    CarTelemetry Telemetry { get; }

    event Action<Vector3, float> OnCollided;
}