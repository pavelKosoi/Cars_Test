using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "VisualsConfig", menuName = "Scriptable Objects/VisualsConfig")]
public class VisualsConfig : ScriptableObject
{
    [Header("Effects Prefabs")]
    [SerializeField] AssetReferenceGameObject collisionSparksRef;
    [SerializeField] AssetReferenceGameObject skidmarkLineRef;
    [SerializeField] AssetReferenceGameObject floatingTextRef;
    [SerializeField] AssetReferenceGameObject winnerConfettiRef;

    [Header("Pool Settings")]
    [SerializeField] int maxSimultaneousSparks = 10;
    [SerializeField] int maxSimultaneousSkidmarks = 5;
    [SerializeField] int maxSimultaneousFloatingTexts = 15;

    public AssetReferenceGameObject CollisionSparksRef => collisionSparksRef;
    public AssetReferenceGameObject SkidmarkLineRef => skidmarkLineRef;
    public AssetReferenceGameObject FloatingTextRef => floatingTextRef;
    public AssetReferenceGameObject WinnerConfettiRef => winnerConfettiRef;

    public int MaxSimultaneousFloatingTexts => maxSimultaneousFloatingTexts;
    public int MaxSimultaneousSparks => maxSimultaneousSparks;
    public int MaxSimultaneousSkidmarks => maxSimultaneousSkidmarks;
}
