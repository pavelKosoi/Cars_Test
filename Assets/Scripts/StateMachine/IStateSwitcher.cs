public interface IStateSwitcher<TBaseState>
{
    void SetState<T>() where T : TBaseState;
}