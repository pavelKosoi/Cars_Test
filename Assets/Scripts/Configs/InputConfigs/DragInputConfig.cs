using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "DragInputConfig", menuName = "Scriptable Objects/Input/Drag Input Config")]
public class DragInputConfig : InputSetupConfig
{
    [SerializeField] AssetReferenceGameObject dragLineVisualizerRef;

    public override IInputProvider CreateInput(Camera camera, IPlaygroundBounds bounds)
    {
        return new DragInputProvider(camera, bounds);
    }

    public override async UniTask<InputVisualizerBase> SetupVisuals(IInputProvider provider, IVehicle targetCar, bool hasVisualizerInstance)
    {
        if (!hasVisualizerInstance)
        {
            await ObjectsPool.RegisterEntryAsync(new ObjectsPool.PoolEntry(dragLineVisualizerRef, 1, null));
        }

        var visualObj = ObjectsPool.GetInstance(dragLineVisualizerRef, Vector3.zero, true);

        if (visualObj.TryGetComponent(out InputVisualizerBase visualizer))
        {
            visualizer.Init(provider as DragInputProvider, (targetCar as Component).transform);
            return visualizer;
        }

        return null;
    }
}