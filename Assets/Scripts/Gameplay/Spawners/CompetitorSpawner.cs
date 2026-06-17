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

    GamePropertiesConfig gameProperties;
    public List<CarController> AllActiveCars { get; } = new();
    public List<CompetitorProfile> ActiveProfiles { get; } = new();
    public CarController PlayerCar { get; private set; }

    public CompetitorSpawner(IPlaygroundBounds playgroundBounds, CarsManager carsManager, MatchScoreModel matchScoreModel)
    {
        this.playgroundBounds = playgroundBounds;
        this.carsManager = carsManager;
        this.matchScoreModel = matchScoreModel;

        gameProperties = ServiceLocator.Get<GamePropertiesConfig>();
    }

    public void SpawnCompetitors()
    {
        ActiveProfiles.Clear();
        SpawnPlayer(gameProperties.PlayerSpawnQuadrant);
        SpawnBots(gameProperties.PlayerSpawnQuadrant);
    }

    void SpawnPlayer(SpawnQuadrant playerQuadrant)
    {
        Vector3 spawnPoint = playgroundBounds.GetQuadrantSpawnPoint(playerQuadrant);
        var playerCarController = SpawnRacer(CarsConfig.CarType.Player, spawnPoint, true);

        playerCarController.SetCollisionProfile(CollisionLayer.Player,
            CollisionLayer.Note | CollisionLayer.Npc);

        playerCarController.InjectControlStrategy<PlayerControlStrategy>();
        playerCarController.ActiveStrategy.Reset();

        PlayerCar = playerCarController;

        ServiceLocator.Get<PlayerFeedbackVisualizer>().Init(PlayerCar, PlayerCar.transform);
    }


    void SpawnBots(SpawnQuadrant excludeQuadrant)
    {
        SpawnQuadrant[] availableQuadrants = Enum.GetValues(typeof(SpawnQuadrant))
            .Cast<SpawnQuadrant>().Where(q => q != excludeQuadrant).ToArray();

        for (int i = 0; i < gameProperties.BotCount; i++)
        {
            SpawnQuadrant currentQuadrant = availableQuadrants[i];
            Vector3 spawnPoint = playgroundBounds.GetQuadrantSpawnPoint(currentQuadrant);
            var botCarController = SpawnRacer(CarsConfig.CarType.Player, spawnPoint, false);

            botCarController.SetCollisionProfile(CollisionLayer.Bot,
                CollisionLayer.Note | CollisionLayer.Npc);

            botCarController.InjectControlStrategy<BotControlStrategy>();
            botCarController.ActiveStrategy.Reset();

        }
    }

    CarController SpawnRacer(CarsConfig.CarType type, Vector3 spawnPoint, bool isHuman)
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

        var profile = new CompetitorProfile(carController, carSettings, isHuman);
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