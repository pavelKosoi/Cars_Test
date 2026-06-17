using UnityEngine;

public class CompetitorProfile
{
    public readonly CarController Car;
    public readonly CarsConfig.CarSetings CarSetings;
    public bool IsHuman;

    public CompetitorProfile(CarController carController, CarsConfig.CarSetings carSetings, bool isHuman)
    {
        Car = carController;
        CarSetings = carSetings;
        IsHuman = isHuman;
    }
}