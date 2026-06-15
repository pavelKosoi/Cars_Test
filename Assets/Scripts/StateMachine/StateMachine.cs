using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    IState currentState;
    Dictionary<Type, IState> states = new Dictionary<Type, IState>();

    public IState CurrentState => currentState;
    public event Action<IState> StateChanged;

    public void RegisterState<T>(T state) where T : IState
    {
        states[typeof(T)] = state;
    }

    public T GetState<T>() where T : class, IState
    {
        return states[typeof(T)] as T;
    }


    public void ChangeState<T>() where T : IState
    {
        if (states.TryGetValue(typeof(T), out var newState))
        {
            if (newState == currentState) return;
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
            StateChanged?.Invoke(currentState);            
        }
        else
        {
            Debug.LogError($"State {typeof(T).Name} not registered!");
        }
    }

    public void Update()
    {        
        currentState?.Tick();
    }

    public void Dispose()
    {
        foreach (var item in states.Values)
        {
            item.Dispose();
        }
    }
}
