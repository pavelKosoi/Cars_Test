using UnityEngine;

public class DragLineVisualizer : InputVisualizerBase
{
    [Header("Settings")]
    [SerializeField] float yOffset = 0.15f;
    [SerializeField] int resolution = 20;

    [SerializeField] float maxLength = 8f;

    [SerializeField, Range(0.1f, 1f)] float curveFactor = 0.5f;

    [Header("References")]
    [SerializeField] Transform pointer;
    [SerializeField] GameObject visual;

    LineRenderer line;
  

    private void Awake()
    {
        line = GetComponentInChildren<LineRenderer>();

        OnReturnedToPool += () => SetVisualsActive(false);
        SetVisualsActive(false);
    }
  

    private void Update()
    {
        if (dragInput == null || carTransform == null || !dragInput.IsDragging)
        {
            SetVisualsActive(false);
            return;
        }

        SetVisualsActive(true);
        DrawIntentCurve();
    }

    void DrawIntentCurve()
    {
        Vector3 startPos = carTransform.position + Vector3.up * yOffset;
        Vector3 targetPos = dragInput.CurrentTarget + Vector3.up * yOffset;

        Vector3 directionToTarget = targetPos - startPos;
        float distance = directionToTarget.magnitude;

        if (distance > maxLength)
        {
            targetPos = startPos + (directionToTarget.normalized * maxLength);
            distance = maxLength;
        }

        Vector3 controlPoint = startPos + carTransform.forward * (distance * curveFactor);

        line.positionCount = resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);

            Vector3 point = Utilities.EvaluateQuadraticBezier(startPos, controlPoint, targetPos, t);
            line.SetPosition(i, point);
        }

        if (pointer != null) pointer.position = targetPos;
    }

    void SetVisualsActive(bool active)
    {
        if (visual != null && visual.activeSelf != active)
        {
            visual.SetActive(active);
        }
    }
}