using UnityEngine;

public class CompetitorProfile
{
    public readonly CarController Car;
    public readonly CarsConfig.CarSetings CarSetings;


    public CompetitorProfile(CarController carController, CarsConfig.CarSetings carSetings)
    {
        Car = carController;
        CarSetings = carSetings;
    }
}