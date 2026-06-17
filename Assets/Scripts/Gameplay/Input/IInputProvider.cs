using System;
using UnityEngine;

public interface IInputProvider : IDisposable
{ 
    bool TryGetTargetPoint(out Vector3 worldPoint);    
}