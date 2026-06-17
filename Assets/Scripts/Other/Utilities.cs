using DG.Tweening;
using System;
using UnityEngine;
using Random = UnityEngine.Random;


public static class Utilities
{
    public static Vector3 GetPointOnCircleRim(Vector3 circleCenter, float circleRadius)
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 dir = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
        return circleCenter + dir * circleRadius;
    }

    public static Vector3 GetPointOnRimWithExclusion(Vector3 circleCenter, float circleRadius, Vector3 baseDirection, float outerAngle = 180f, float exclusionAngle = 60f)
    {
        float halfOuter = outerAngle / 2f;
        float halfExclusion = exclusionAngle / 2f;

        float sideSign = Random.value > 0.5f ? 1f : -1f;

        float randomAngleOffset = Random.Range(halfExclusion, halfOuter) * sideSign;

        Vector3 finalDirection = Quaternion.Euler(0, randomAngleOffset, 0) * baseDirection.normalized;

        return circleCenter + finalDirection * circleRadius;
    }

    public static void DrawPoint(Vector3 p, float size, Color color, float duration = 0f)
    {
        Vector3 r = Vector3.right * size;
        Vector3 u = Vector3.up * size;
        Vector3 f = Vector3.forward * size;

        Debug.DrawLine(p - r, p + r, color, duration);
        Debug.DrawLine(p - u, p + u, color, duration);
        Debug.DrawLine(p - f, p + f, color, duration);
    }

    public static Vector3 EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;

        Vector3 term1 = (u * u) * p0;
        Vector3 term2 = (2f * u * t) * p1;
        Vector3 term3 = (t * t) * p2;

        return term1 + term2 + term3;     
    }

    public static void SetAnchor(this RectTransform rect, AnchorPreset preset)
    {
        switch (preset)
        {
            case AnchorPreset.MiddleCenter:
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                break;

            case AnchorPreset.TopCenter:
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                break;

            case AnchorPreset.TopLeft:
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                break;

            case AnchorPreset.StretchAll:
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                break;
        }
    }
}
[Serializable]
public class TweenSettings
{
    public Ease ease;
    public float duration;
}


public enum AnchorPreset
{
    MiddleCenter,
    TopCenter,
    TopLeft,
    TopRight,
    BottomCenter,
    StretchAll
}