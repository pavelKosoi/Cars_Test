using UnityEngine;

public interface IInputProvider
{ 
    bool TryGetTargetPoint(out Vector3 worldPoint);
}