using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class CompetitorSpawner
{
    IPlaygroundBounds playgroundBounds;
    CarsManager carsManager;
    MatchScoreModel matchScoreModel;

    GamePropertiesConfig gamePropertiesConfig => GeneralGameManager.Instance.GamePropertiesConfig;
    public List<CarController> AllActiveCars { get; } = new();
    public List<CompetitorProfile> ActiveProfiles { get; } = new();

    public CompetitorSpawner(IPlaygroundBounds playgroundBounds, CarsManager carsManager, MatchScoreModel matchScoreModel)
    {
        this.playgroundBounds = playgroundBounds;
        this.carsManager = carsManager;
        this.matchScoreModel = matchScoreModel;
    }

    public void SpawnCompetitors()
    {
        ActiveProfiles.Clear();
        SpawnPlayer(gamePropertiesConfig.PlayerSpawnQuadrant);
        SpawnBots(gamePropertiesConfig.PlayerSpawnQuadrant);
    }

    void SpawnPlayer(SpawnQuadrant playerQuadrant)
    {
        Vector3 spawnPoint = playgroundBounds.GetQuadrantSpawnPoint(playerQuadrant);
        var playerCarController = SpawnRacer(CarsConfig.CarType.Player, spawnPoint);

        playerCarController.SetCollisionProfile(CollisionLayer.Player,
            CollisionLayer.Note | CollisionLayer.Npc);

        playerCarController.InjectControlStrategy<PlayerControlStrategy>();
        playerCarController.ActiveStrategy.Reset();
    }


    void SpawnBots(SpawnQuadrant excludeQuadrant)
    {
        SpawnQuadrant[] availableQuadrants = Enum.GetValues(typeof(SpawnQuadrant))
            .Cast<SpawnQuadrant>().Where(q => q != excludeQuadrant).ToArray();

        for (int i = 0; i < gamePropertiesConfig.BotCount; i++)
        {
            SpawnQuadrant currentQuadrant = availableQuadrants[i];
            Vector3 spawnPoint = playgroundBounds.GetQuadrantSpawnPoint(currentQuadrant);
            var botCarController = SpawnRacer(CarsConfig.CarType.Player, spawnPoint);

            botCarController.SetCollisionProfile(CollisionLayer.Bot,
                CollisionLayer.Note | CollisionLayer.Npc);

            botCarController.InjectControlStrategy<BotControlStrategy>();
            botCarController.ActiveStrategy.Reset();

        }
    }

    CarController SpawnRacer(CarsConfig.CarType type, Vector3 spawnPoint)
    {
        var carSettings = carsManager.GetCarSettings(type);
        var carInstance = ObjectsPool.GetInstance(carSettings.prefabRef, spawnPoint, true);
        carInstance.transform.LookAt(playgroundBounds.GetCenter());
        
        var carController = carInstance.GetComponent<CarController>();

        matchScoreModel.RegisterCollector(carController);
        carController.OnMoneyCollected += (collector, amount) =>
        {
            matchScoreModel.AddScore(collector, amount);
        };

        AllActiveCars.Add(carController);

        var profile = new CompetitorProfile(carController, carSettings);
        ActiveProfiles.Add(profile);

        return carController;
    }


    public void Clear()
    {
        foreach (var item in AllActiveCars)
        {
            item.ReturnToPool();
        }

        AllActiveCars.Clear();
        ActiveProfiles.Clear();
    }
}