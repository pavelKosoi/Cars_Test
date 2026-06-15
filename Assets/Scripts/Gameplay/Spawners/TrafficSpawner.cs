using UnityEngine;

public class TrafficSpawner : ContinuousSpawnerBase<CarController>
{
    IPlaygroundBounds playgroundBounds;
    CarsManager carsManager;

    GamePropertiesConfig gameProperties => GeneralGameManager.Instance.GamePropertiesConfig;
    float minDistanceSqr => gameProperties.MinDistanceBetweenNpcs * gameProperties.MinDistanceBetweenNpcs;

    public TrafficSpawner(IPlaygroundBounds playgroundBounds, CarsManager carsManager)
    {
        this.playgroundBounds = playgroundBounds;
        this.carsManager = carsManager;
    }

    protected override bool CanSpawnBatch() => activeInstances.Count < gameProperties.MaxSimultaneousNpcs;

    protected override int GetSpawnCount() => Random.Range(gameProperties.MinNpcsPerSpawn, gameProperties.MaxNpcsPerSpawn + 1);

    protected override float GetSpawnDelay() => Random.Range(gameProperties.MinSpawnInterval, gameProperties.MaxSpawnInterval);

    protected override bool TrySpawnInstance(out CarController instance)
    {
        instance = null;

        if (!TryGetSafeSpawnPoint(out Vector3 spawnPoint))
            return false;

        var carSettings = carsManager.GetCarSettings(CarsConfig.CarType.NPC);
        var carInstance = ObjectsPool.GetInstance(carSettings.prefabRef, spawnPoint, true);

        carInstance.transform.LookAt(playgroundBounds.GetCenter());

        instance = carInstance.GetComponent<CarController>();
        var capturedInstance = instance;

        instance.SetCollisionProfile(CollisionLayer.Npc, CollisionLayer.None);
        instance.OnReturnedToPool += () => HandleInstanceReturned(capturedInstance);

        instance.InjectControlStrategy<NpcControlStrategy>();
        instance.ActiveStrategy.Reset();

        return true;
    }

    bool TryGetSafeSpawnPoint(out Vector3 spawnPoint)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var potentialPoint = playgroundBounds.GetRandomPointOnEdge();
            bool isValid = true;

            foreach (var car in activeInstances)
            {
                if ((car.transform.position - potentialPoint).sqrMagnitude < minDistanceSqr)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                spawnPoint = potentialPoint;
                return true;
            }
        }

        spawnPoint = Vector3.zero;
        return false;
    }
}