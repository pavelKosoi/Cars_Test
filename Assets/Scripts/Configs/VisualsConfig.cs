using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "VisualsConfig", menuName = "Scriptable Objects/VisualsConfig")]
public class VisualsConfig : ScriptableObject
{
    [Header("Effects Prefabs")]
    public AssetReferenceGameObject CollisionSparksRef;
    public AssetReferenceGameObject SkidmarkLineRef;

    [Header("Pool Settings")]
    public int MaxSimultaneousSparks = 10;
    public int MaxSimultaneousSkidmarks = 5;
}
