using UnityEngine;

public class MenuState : GameStateBase
{

    public override void Enter()
    {
        base.Enter();
        UiScreensManager.Instance.Show<MenuScreen>();
    }

    public override void Tick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IsFinished = true;
            GeneralGameManager.Instance.SetState<CountdownState>();
        }
    }
}
