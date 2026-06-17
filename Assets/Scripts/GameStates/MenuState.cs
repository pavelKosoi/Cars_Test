using UnityEngine;

public class MenuState : GameStateBase
{

    public override void Enter()
    {
        base.Enter();
        ServiceLocator.Get<UiScreensManager>().Show<MenuScreen>();
    }

    public override void Tick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IsFinished = true;
            ServiceLocator.Get<IStateSwitcher<GameStateBase>>().SetState<GameplayState>();
        }
    }
}
