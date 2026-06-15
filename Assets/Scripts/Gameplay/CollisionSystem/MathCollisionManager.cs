using System;
using System.Collections.Generic;
using UnityEngine;

public class MathCollisionManager
{
    List<MathCollider> colliders = new();

    public void Register(MathCollider c) => colliders.Add(c);
    public void Unregister(MathCollider c) => colliders.Remove(c);

    public void Update()
    {
        for (int i = 0; i < colliders.Count; i++)
        {
            var a = colliders[i];
            if (!a.IsActive) continue;

            for (int j = i + 1; j < colliders.Count; j++)
            {
                var b = colliders[j];
                if (!b.IsActive) continue;

                bool aHearsB = (a.CollidesWith & b.Layer) != 0;
                bool bHearsA = (b.CollidesWith & a.Layer) != 0;

                if (!aHearsB && !bHearsA) continue;

                float distSqr = (a.Position - b.Position).sqrMagnitude;
                float radSum = a.Radius + b.Radius;

                if (distSqr <= radSum * radSum)
                {
                    if (!a.IsTrigger && !b.IsTrigger)
                    {
                        ResolvePenetration(a, b, distSqr, radSum);
                    }

                    if (aHearsB) a.TriggerCollision(b);
                    if (bHearsA && b.IsActive) b.TriggerCollision(a);
                }
            }
        }
    }

    void ResolvePenetration(MathCollider a, MathCollider b, float distSqr, float radSum)
    {
        float dist = Mathf.Sqrt(distSqr);
        if (dist == 0f) return;

        float penetration = radSum - dist;
        Vector3 normal = (a.Position - b.Position).normalized;
        Vector3 correction = normal * (penetration * 0.5f);

        a.ApplyPenetration(b, correction);
        b.ApplyPenetration(a, -correction);
    }
}


[Flags]
public enum CollisionLayer
{
    None = 0,
    Player = 1 << 0,
    Npc = 1 << 1,
    Bot = 1 << 2,
    Note = 1 << 3
}