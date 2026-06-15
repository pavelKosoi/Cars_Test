using TMPro;
using UnityEngine;

public class CountdownScreen : ScreenBase
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TweenSettings timerTweenSettings;

    public ITimerVisual TimerVisual { get; private set; }

    private void Awake()
    {
        TimerVisual = new TextTimerVisual(timerText, timerTweenSettings, Vector2.one * 3f, Vector2.one * 0.3f);
    } 
}
