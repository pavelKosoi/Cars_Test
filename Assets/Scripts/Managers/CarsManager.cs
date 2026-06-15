using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarsManager
{
    public readonly CarsConfig CarsConfig;
    public VehicleStrategyFactory VehicleStrategyFactory {  get; private set; }

    Dictionary<CarsConfig.CarType, CarPool> pools;
   
    public CarsManager(CarsConfig carsConfig, Playground playground)
    {
        CarsConfig = carsConfig;      

        pools = new Dictionary<CarsConfig.CarType, CarPool>();
        var groupedCars = carsConfig.Cars.GroupBy(c => c.CarType);
        foreach (var group in groupedCars)
        {
            pools[group.Key] = new CarPool(group.ToArray());
        }
    }
    
    public void InitVehicleStrategyFactory(VehicleStrategyFactory vehicleStrategyFactory) =>
          VehicleStrategyFactory = vehicleStrategyFactory;


    public CarsConfig.CarSetings GetCarSettings(CarsConfig.CarType type)
    {
        if (pools != null && pools.TryGetValue(type, out var pool))
        {
            return pool.GetNext();
        }

        return null;
    }

    class CarPool
    {
        readonly CarsConfig.CarSetings[] items;
        int remaining;

        public CarPool(CarsConfig.CarSetings[] items)
        {
            this.items = items;
            remaining = items.Length;
        }

        public CarsConfig.CarSetings GetNext()
        {
            if (items.Length == 0) return null;

            if (remaining == 0) remaining = items.Length;
         

            int randomIndex = Random.Range(0, remaining);
            var selected = items[randomIndex];
            
            items[randomIndex] = items[remaining - 1];
            items[remaining - 1] = selected;

            remaining--;

            return selected;
        }
    }
}