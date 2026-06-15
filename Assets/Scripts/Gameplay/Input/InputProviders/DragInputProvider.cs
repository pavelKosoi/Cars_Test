using UnityEngine;

public class DragInputProvider : IInputProvider
{
    Camera camera;
    Plane raycastPlane;
    IPlaygroundBounds bounds; 

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

                worldPoint = bounds.ConstrainPoint(rawHitPoint);
                return true;
            }
        }

        worldPoint = Vector3.zero;
        return false;
    }
}