using UnityEngine;
using TMPro;
using DG.Tweening;

public class MinutesTextTimerVisual : TimerVisualBase
{
    TextMeshProUGUI text;
    TweenSettings tweenSettings;
    Vector3 startScale;
    Vector3 targetScale;
    Tween tween;

    Color defaultColor;

    public MinutesTextTimerVisual(TextMeshProUGUI text, TweenSettings tweenSettings,
        Vector3 startScale, Vector3 targetScale)
    {
        this.text = text;
        this.tweenSettings = tweenSettings;
        this.startScale = startScale;
        this.targetScale = targetScale;

        this.defaultColor = text.color;
    }

 
    public override void Tick(int remainTime)
    {
        if (tween != null) tween.Kill();

        int minutes = remainTime / 60;
        int seconds = remainTime % 60;

        text.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (remainTime < 5)
        {
            text.color = Color.red;

            text.transform.localScale = targetScale;

            tween = text.transform.DOScale(startScale, tweenSettings.duration)
                .SetEase(tweenSettings.ease).SetLoops(2, LoopType.Yoyo); 


        }
        else
        {
            text.color = defaultColor;
            text.transform.localScale = startScale;
        }
    }
}