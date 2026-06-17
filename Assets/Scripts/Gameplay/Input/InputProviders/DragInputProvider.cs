using System;
using UnityEngine;

public class DragInputProvider : IInputProvider
{
    Camera camera;
    Plane raycastPlane;
    IPlaygroundBounds bounds;

    public bool IsDragging { get; private set; }
    public Vector3 CurrentTarget { get; private set; }

    public DragInputProvider(Camera camera, IPlaygroundBounds bounds)
    {
        this.camera = camera;
        this.bounds = bounds;

        raycastPlane = new Plane(Vector3.up, bounds.GetCenter());
    }

    public bool TryGetTargetPoint(out Vector3 worldPoint)
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (raycastPlane.Raycast(ray, out float enter))
            {
                Vector3 rawHitPoint = ray.GetPoint(enter);

                CurrentTarget = bounds.ConstrainPoint(rawHitPoint);
                IsDragging = true;

                worldPoint = CurrentTarget;
                return true;
            }
        }

        IsDragging = false;
        worldPoint = Vector3.zero;
        return false;
    }

    public void Dispose()
    {
        IsDragging = false;
        CurrentTarget = Vector3.zero;
    }
}