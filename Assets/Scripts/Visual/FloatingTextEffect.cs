using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class FloatingTextEffect : MonoBehaviour, IPoolable
{
    [SerializeField] float floatHeight = 2f;
    [SerializeField] TweenSettings moveTweenSettings;
    [SerializeField] TweenSettings fadeTweenSettings;

    TextMeshPro textMesh;
    Tween moveTween;
    Tween fadeTween;

    public event Action OnReturnedToPool;

    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshPro>();
    }

    public void Init(int amount)
    {
        KillTweens();

        if (amount > 0)
        {
            textMesh.text = $"+{amount}K";
            textMesh.color = Color.white;
        }
        else
        {
            textMesh.text = $"{amount}K"; 
            textMesh.color = Color.red;
        }

        transform.forward = Camera.main.transform.forward;

     
        moveTween = transform.DOMoveY(transform.position.y + floatHeight, moveTweenSettings.duration)
            .SetEase(moveTweenSettings.ease);

        fadeTween = textMesh.DOFade(0f, fadeTweenSettings.duration)
            .SetEase(fadeTweenSettings.ease).OnComplete(ReturnToPool);
    }

    public void ReturnToPool()
    {
        KillTweens();
        ObjectsPool.ReturnToPool(gameObject);
    }

    void KillTweens()
    {
        moveTween?.Kill();
        fadeTween?.Kill();

        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = 1f;
            textMesh.color = c;
        }
    }

    void OnDestroy()
    {
        KillTweens();
    }
}