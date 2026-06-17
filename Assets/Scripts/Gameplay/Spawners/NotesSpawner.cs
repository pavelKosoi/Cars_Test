using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using static MoneyConfig;

public class NotesSpawner : ContinuousSpawnerBase<NoteController>
{
    MoneyConfig moneyConfig;
    IPlaygroundBounds playgroundBounds;
    int currentTotalDenomination = 0;


    public NotesSpawner(MoneyConfig moneyConfig, IPlaygroundBounds playgroundBounds)
    {
        this.moneyConfig = moneyConfig;
        this.playgroundBounds = playgroundBounds;

        gameProperties = ServiceLocator.Get<GamePropertiesConfig>();
    }

    protected override bool CanSpawnBatch() =>
        activeInstances.Count < gameProperties.MaxSimultaneousNotes &&
        currentTotalDenomination < gameProperties.MaxNotesDenomination;

    protected override bool CanSpawnInstance() => CanSpawnBatch();

    protected override int GetSpawnCount() => Random.Range(gameProperties.MinNotesPerSpawn, gameProperties.MaxNotesPerSpawn + 1);

    protected override float GetSpawnDelay() => Random.Range(gameProperties.MinNotesSpawnInterval, gameProperties.MaxNotesSpawnInterval);

    protected override bool TrySpawnInstance(out NoteController instance)
    {
        instance = null;
        Note noteToSpawn = GetRandomNoteByWeight();

        if (noteToSpawn == null) return false;

        Vector3 spawnPoint = playgroundBounds.GetRandomPointInside(gameProperties.NotesSpawnAreaMultiplier);
        var noteInstance = ObjectsPool.GetInstance(noteToSpawn.prefabRef, spawnPoint, true);

        instance = noteInstance.GetComponent<NoteController>();
        currentTotalDenomination += noteToSpawn.denomination;

        var capturedInstance = instance;
        int capturedDenomination = noteToSpawn.denomination;

        instance.OnReturnedToPool += () =>
        {
            currentTotalDenomination -= capturedDenomination;
            HandleInstanceReturned(capturedInstance);
        };
        instance.Init(noteToSpawn);

        return true;
    }

    Note GetRandomNoteByWeight()
    {
        int availableCapacity = gameProperties.MaxNotesDenomination - currentTotalDenomination;
        var validNotes = moneyConfig.Notes.Where(n => n.denomination <= availableCapacity).ToList();

        if (validNotes.Count == 0) return null;

        int totalWeight = validNotes.Sum(n => n.spawnWeight);
        int randomVal = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var note in validNotes)
        {
            cumulativeWeight += note.spawnWeight;
            if (randomVal < cumulativeWeight) return note;
        }

        return validNotes.Last();
    }
}