using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SkidmarkEffect : MonoBehaviour, IPoolable
{
    [SerializeField] float fadeDuration = 3f;

    LineRenderer lineRenderer;

    Color initialStartColor;
    Color initialEndColor;

    CancellationTokenSource fadeCts;

    public event Action OnReturnedToPool;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        initialStartColor = lineRenderer.startColor;
        initialEndColor = lineRenderer.endColor;
    }

    public void Init()
    {
        fadeCts?.Cancel();
        fadeCts = new CancellationTokenSource();

        lineRenderer.positionCount = 0;

        SetLineAlphaMultiplier(1f);
    }

    public void AddPoint(Vector3 point)
    {
        int nextIndex = lineRenderer.positionCount;
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(nextIndex, point);
    }

    public void FadeAndRelease()
    {
        FadeRoutineAsync(fadeCts.Token).Forget();
    }

    async UniTaskVoid FadeRoutineAsync(CancellationToken token)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            bool isCancelled = await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled) return;

            timer += Time.deltaTime;

            float alphaMultiplier = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            SetLineAlphaMultiplier(alphaMultiplier);
        }

        ReturnToPool();
    }

    void SetLineAlphaMultiplier(float multiplier)
    {
        Color start = initialStartColor;     
        start.a = initialStartColor.a * multiplier;

        Color end = initialEndColor;
        end.a = initialEndColor.a * multiplier;

        lineRenderer.startColor = start;
        lineRenderer.endColor = end;
    }

    public void ReturnToPool()
    {
        fadeCts?.Cancel();
        if (ObjectsPool.ReturnToPool(gameObject))
        {
            lineRenderer.positionCount = 0;
        }
    }

    void OnDestroy()
    {
        fadeCts?.Cancel();
        fadeCts?.Dispose();
    }
}