using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "MoneyConfig", menuName = "Scriptable Objects/MoneyConfig")]
public class MoneyConfig : ScriptableObject
{
    [Serializable]
    public class Note
    {
        public AssetReferenceGameObject prefabRef;
        public int denomination;
        public int spawnWeight;
    }

    [SerializeField] Note[] notes;

    public Note[] Notes => notes;
}
