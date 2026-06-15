using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextTimerVisual : TimerVisualBase
{
    TextMeshProUGUI text;
    TweenSettings tweenSettings;
    Vector3 startScale;
    Vector3 targetScale;
    Tween tween;

    public TextTimerVisual(TextMeshProUGUI text, TweenSettings tweenSettings,
        Vector3 startScale, Vector3 targetScale)
    {
        this.text = text;
        this.tweenSettings = tweenSettings;
        this.startScale = startScale;
        this.targetScale = targetScale;
    }

    public override void Tick(int remainTime)
    {
        if (tween != null) tween.Kill();

        text.text = remainTime == 0 ? "Go!" : remainTime.ToString();

        text.transform.localScale = startScale;
        tween = text.transform.DOScale(targetScale, tweenSettings.duration).SetEase(tweenSettings.ease);
    }
}