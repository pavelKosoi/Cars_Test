using System;

public class VehicleStrategyFactory
{
    IPlaygroundBounds playgroundBounds;
    IInputProvider inputProvider;
    IBotPerception botPerception;

    public VehicleStrategyFactory(IPlaygroundBounds playgroundBounds, IInputProvider inputProvider, IBotPerception botPerception)
    {
        this.playgroundBounds = playgroundBounds;
        this.inputProvider = inputProvider;
        this.botPerception = botPerception;
    }

    public IVehicleControlStrategy Create<T>(CarController controller) where T : IVehicleControlStrategy
    {
        Type type = typeof(T);

        return type switch
        {
            _ when type == typeof(NpcControlStrategy) =>
                new NpcControlStrategy(controller, playgroundBounds),

            _ when type == typeof(PlayerControlStrategy) =>
                new PlayerControlStrategy(controller, inputProvider),

            _ when type == typeof(BotControlStrategy) =>
           new BotControlStrategy(controller, botPerception, playgroundBounds),

            _ => throw new ArgumentException($"Strategy of type {type} is not supported by Factory.")
        };
    }
}