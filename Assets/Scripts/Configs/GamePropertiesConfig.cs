using UnityEngine;

[CreateAssetMenu(fileName = "GamePropertiesConfig", menuName = "Scriptable Objects/GamePropertiesConfig")]
public class GamePropertiesConfig : ScriptableObject
{
    [Header("Match Settings")]
    [SerializeField] int countdownDuration;
    [SerializeField] int playtimeDuration;
    [SerializeField, Range(0,3)] int botCount;
    [SerializeField] SpawnQuadrant playerSpawnQuadrant;
    [SerializeField] int npcCollisionFine;

    [Header("Bot AI Settings")]
    [SerializeField, Range(1f, 10f)] float botTrafficAvoidanceRadius;
    [SerializeField, Range(0f, 3f)] float botTrafficEvasionStrength;
    
    [SerializeField, Range(1f, 5f)] float botSeparationRadius;
    [SerializeField, Range(0f, 2f)] float botSeparationStrength;
   
    [SerializeField, Range(0f, 0.5f)] float botTargetRandomness;

    [Header("Traffic Settings")]
    [SerializeField] int minNpcsPerSpawn;
    [SerializeField] int maxNpcsPerSpawn;
    [SerializeField] int maxSimultaneousNpcs;
    [SerializeField] float minTrafficSpawnInterval;
    [SerializeField] float maxTrafficSpawnInterval;
    [SerializeField] float minDistanceBetweenNpcs;

    [Header("Notes Settings")]
    [SerializeField] int minNotesPerSpawn;
    [SerializeField] int maxNotesPerSpawn;
    [SerializeField] int maxSimultaneousNotes;
    [SerializeField] int maxNotesDenomination;
    [SerializeField] float minNotesSpawnInterval;
    [SerializeField] float maxNotesSpawnInterval;
    [SerializeField, Range(0.1f, 1f)] float notesSpawnAreaMultiplier = 0.8f;

    public int CountdownDuration => countdownDuration;
    public int PlaytimeDuration => playtimeDuration;
    public int BotCount => botCount;
    public int MinNpcsPerSpawn => minNpcsPerSpawn;
    public int MaxNpcsPerSpawn => maxNpcsPerSpawn;
    public int MaxSimultaneousNpcs => maxSimultaneousNpcs;
    public float MinSpawnInterval => minTrafficSpawnInterval;
    public float MaxSpawnInterval => maxTrafficSpawnInterval;
    public float MinDistanceBetweenNpcs => minDistanceBetweenNpcs;

    public int MinNotesPerSpawn => minNotesPerSpawn;
    public int MaxNotesPerSpawn => maxNotesPerSpawn;
    public int MaxSimultaneousNotes => maxSimultaneousNotes;
    public int MaxNotesDenomination => maxNotesDenomination;

    public float MinNotesSpawnInterval => minNotesSpawnInterval;
    public float MaxNotesSpawnInterval => maxNotesSpawnInterval;
    public float NotesSpawnAreaMultiplier => notesSpawnAreaMultiplier;
    public SpawnQuadrant PlayerSpawnQuadrant => playerSpawnQuadrant;

    public float BotTrafficAvoidanceRadius => botTrafficAvoidanceRadius;
    public float BotTrafficEvasionStrength => botTrafficEvasionStrength;
    public float BotSeparationRadius => botSeparationRadius;
    public float BotSeparationStrength => botSeparationStrength;
    public float BotTargetRandomness => botTargetRandomness;
    public int NpcCollisionFine => npcCollisionFine;
}