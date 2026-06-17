using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class InputSetupConfig : ScriptableObject
{
    public abstract IInputProvider CreateInput(Camera camera, IPlaygroundBounds bounds);
    public abstract UniTask<InputVisualizerBase> SetupVisuals(IInputProvider provider, IVehicle targetCar, bool hasVisualizerInstance);
}