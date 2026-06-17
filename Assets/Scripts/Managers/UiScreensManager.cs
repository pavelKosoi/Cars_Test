using System;
using System.Collections.Generic;
using UnityEngine;

public class UiScreensManager : MonoBehaviour
{
    [SerializeField] ScreenBase[] screens;

    Dictionary<Type, ScreenBase> screensMap = new();
    ScreenBase currentScreen;

    private void Awake()
    {
        foreach (var item in screens)
        {
            screensMap[item.GetType()] = item;     
            item.Hide();
        }
    }

    public TScreen Get<TScreen>() where TScreen : ScreenBase
    {
        if (screensMap.TryGetValue(typeof(TScreen), out var screen))
        {
            return screen as TScreen;
        }
        else
        {
            Debug.LogWarning($"There's no such screen: {typeof(TScreen).Name}");
            return null;
        }
    }

    public TScreen Show<TScreen>() where TScreen : ScreenBase
    {
        var nextScreen = Get<TScreen>();
       
        if (currentScreen == nextScreen) return currentScreen as TScreen;

        if (currentScreen != null)
        {
            currentScreen.Hide();
        }

        nextScreen.Show();
        currentScreen = nextScreen;
        return currentScreen as TScreen;
    }
}