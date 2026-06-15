using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "CarsConfig", menuName = "Scriptable Objects/CarsConfig")]
public class CarsConfig : ScriptableObject
{
    public enum CarType
    {
        Player,
        NPC
    }

    [Serializable]
    public class CarSetings
    {
        public CarType CarType;
        public AssetReferenceGameObject prefabRef;
        public Color color;
    }

    [SerializeField] CarSetings[] carsSetings;

    public CarSetings[] Cars => carsSetings;
    
}
