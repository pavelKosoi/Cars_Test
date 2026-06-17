using System;
using UnityEngine;

public class MathCollider : MonoBehaviour
{
    [Header("Settings")]
    public float Radius = 1f;
    public bool IsTrigger = false;

    [Header("Layers")]
    public CollisionLayer Layer;
    public CollisionLayer CollidesWith;

    public event Action<MathCollider> OnCollisionEnter;

    public event Action<MathCollider, MathCollider, Vector3> OnResolvePenetration;

    public bool IsActive => gameObject.activeInHierarchy;
    public Vector3 Position => transform.position;

    private void OnEnable()
    {
        ServiceLocator.Get<MathCollisionManager>().Register(this);
    }

    private void OnDisable()
    {
        ServiceLocator.Get<MathCollisionManager>().Unregister(this);
    }

    public void TriggerCollision(MathCollider other)
    {
        OnCollisionEnter?.Invoke(other);
    }

    public void ApplyPenetration(MathCollider other, Vector3 correction)
    {
        OnResolvePenetration?.Invoke(this, other, correction);
    }

    public Vector3 GetContactPoint(MathCollider other)
    {
        Vector3 directionToOther = other.Position - Position;
        float radiusProportion = Radius / (Radius + other.Radius);

        return Position + (directionToOther * radiusProportion);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsTrigger ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}