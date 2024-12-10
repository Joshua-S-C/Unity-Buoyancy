using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PhysicsCollider;

public static class CollisionDetection
{
    public static int CollisionChecks;

    public struct VectorDeltas
    {
        public Vector3 s1;
        public Vector3 s2;
        public static VectorDeltas zero
        {
            get
            {
                return new VectorDeltas { s1 = Vector3.zero, s2 = Vector3.zero };
            }
        }

        public void ApplyToPosition(PhysicsCollider s1, PhysicsCollider s2)
        {
            s1.position += this.s1;
            s2.position += this.s2;
        }

        public void ApplyToVelocity(PhysicsCollider s1, PhysicsCollider s2)
        {
            s1.velocity += this.s1;
            s2.velocity += this.s2;
        }
    };

    public class CollisionInfo
    {
        public Vector3 normal = Vector3.zero;
        public float penetration = 0;
        public float pctToMoveS1 = 0;
        public float pctToMoveS2 = 0;
        public float separatingVelocity = 0;
        public bool IsColliding => penetration > 0;
        public bool HasInfiniteMass => pctToMoveS1 + pctToMoveS2 == 0;
    }

    public delegate void NormalAndPenCalculation(PhysicsCollider s1, PhysicsCollider s2, out Vector3 normal, out float penetration);

    public static NormalAndPenCalculation[,] collisionFns = new NormalAndPenCalculation[(int)Shape.Count, (int)Shape.Count];

    static CollisionDetection()
    {
        collisionFns = new NormalAndPenCalculation[(int)Shape.Count, (int)Shape.Count];
        for (int i = 0; i < (int)Shape.Count; i++)
        {
            for (int j = 0; j < (int)Shape.Count; j++)
            {
                collisionFns[i, j] = (PhysicsCollider _, PhysicsCollider _, out Vector3 _, out float _) => throw new NotImplementedException();
            }
        }

        collisionFns[(int)Shape.Sphere, (int)Shape.Sphere] = TestSphereSphere;
        AddCollisionFns(Shape.Sphere, Shape.Plane, TestSpherePlane);
        AddCollisionFns(Shape.Sphere, Shape.AABB, TestSphereAABB);
        AddCollisionFns(Shape.Sphere, Shape.OBB, TestSphereOBB);
        AddCollisionFns(Shape.Sphere, Shape.Wave, TestSphereWave);

        // Static colliders do nothing
        NormalAndPenCalculation nop = (PhysicsCollider _, PhysicsCollider _, out Vector3 n, out float p) => { n = Vector3.zero; p = -1; };
        AddCollisionFns(Shape.OBB, Shape.Plane, nop);
        AddCollisionFns(Shape.AABB, Shape.Plane, nop);
        AddCollisionFns(Shape.AABB, Shape.OBB, nop);
        AddCollisionFns(Shape.Plane, Shape.Plane, nop);
        AddCollisionFns(Shape.OBB, Shape.OBB, nop);
        AddCollisionFns(Shape.AABB, Shape.AABB, nop);
    }

    static void AddCollisionFns(Shape s1, Shape s2, NormalAndPenCalculation fn)
    {
        NormalAndPenCalculation backwardsFn =
            (PhysicsCollider a, PhysicsCollider b, out Vector3 c, out float d) =>
            {
                fn(b, a, out c, out d);
                c = -c;
            };

        collisionFns[(int)s1, (int)s2] = fn;
        collisionFns[(int)s2, (int)s1] = backwardsFn;
    }

    public static void TestSphereSphere(PhysicsCollider shape1, PhysicsCollider shape2, out Vector3 normal, out float penetration)
    {
        Sphere s1 = shape1 as Sphere;
        Sphere s2 = shape2 as Sphere;

        Vector3 s2ToS1 = s1.Center - s2.Center;
        float dist = s2ToS1.magnitude;
        float sumOfRadii = (s1.Radius + s2.Radius);
        penetration = sumOfRadii - dist;
        normal = dist == 0 ? Vector3.zero : (s2ToS1 / dist);
    }

    public static void TestSpherePlane(PhysicsCollider s1, PhysicsCollider s2, out Vector3 normal, out float penetration)
    {
        Sphere s = s1 as Sphere;
        PlaneCollider p = s2 as PlaneCollider;

        float offset = Vector3.Dot(s.Center, p.Normal) - p.Offset;
        float dist = Mathf.Abs(offset);
        penetration = s.Radius - dist;
        normal = offset >= 0 ? p.Normal : -p.Normal;
    }

    private static void TestSphereAABB(PhysicsCollider s1, PhysicsCollider aabb1, out Vector3 normal, out float penetration)
    {
        Sphere s = s1 as Sphere;
        AABB aabb = aabb1 as AABB;

        Vector3 closestPoint = Vector3.Max(aabb.Center - aabb.HalfWidth, Vector3.Min(s.Center, aabb.Center + aabb.HalfWidth));

        float distance = (closestPoint - s.Center).magnitude;
        
        penetration = s.Radius - distance;
        normal = (s.Center - closestPoint).normalized;
    }

    private static void TestSphereOBB(PhysicsCollider s1, PhysicsCollider obb1, out Vector3 normal, out float penetration)
    {
        Sphere s = s1 as Sphere;
        OBB obb = obb1 as OBB;

        Vector3 closestPoint = obb.Center;

    // look theres a heart v
        for (int i = 0; i <3; i++)
        {
            float projection = Vector3.Dot(s.Center - obb.Center, obb.Axes[i]);

            if (projection > obb.HalfWidth[i])
                projection = obb.HalfWidth[i];
            if (projection < -obb.HalfWidth[i])
                projection = -obb.HalfWidth[i];

            closestPoint += projection * obb.Axes[i];
        }

        float distance = (closestPoint - s.Center).magnitude;   // Did not work using Square Magnitude

        penetration = s.Radius - distance;
        normal = (s.Center - closestPoint).normalized;
    }

    public static void TestSphereWave(PhysicsCollider s1, PhysicsCollider f1, out Vector3 normal, out float penetration)
    {
        Sphere s = s1 as Sphere;
        WaveCollider f = f1 as WaveCollider;

        Vector2 pos = new Vector2(s.Center.x, s.Center.z);

        // Correct
        float centerDistance = s.Center.y - f.getHeight(pos);

        // 
        normal = (f.getNormal(pos) + Vector3.up).normalized;

        // Correct
        penetration = -centerDistance;
        penetration += s.Radius;

        if (centerDistance * centerDistance > s.Radius * s.Radius)
            normal = f.Up;

/*
        s1.position = new Vector3(
            s1.position.x, 
            f.getHeight(pos),
            s1.position.z
            );
*/
        Debug.Log($"Normal: {normal} Pen: {penetration} Height: {f.getHeight(pos)} CentrDist: {centerDistance}");
    }

    public static CollisionInfo GetCollisionInfo(PhysicsCollider s1, PhysicsCollider s2)
    {
        CollisionInfo info = new CollisionInfo();
        NormalAndPenCalculation calc = collisionFns[(int)s1.shape, (int)s2.shape];

        try
        {
            calc(s1, s2, out info.normal, out info.penetration);
        }
        catch (NotImplementedException e)
        {
            Debug.Log($"Tried to test collision between {s1.shape} and {s2.shape}, but no collision detection function was found.");
            throw e;
        }

        {
            float sumOfInvMasses = s1.invMass + s2.invMass;
            if (sumOfInvMasses == 0) return info; // Both masses infinite, avoid divide-by-zero error
            info.pctToMoveS1 = s1.invMass / sumOfInvMasses;
            info.pctToMoveS2 = s2.invMass / sumOfInvMasses;

            info.separatingVelocity = Vector3.Dot(s1.velocity - s2.velocity, info.normal);
        }

        return info;
    }

    public static void ApplyCollisionResolution(PhysicsCollider c1, PhysicsCollider c2)
    {
        CollisionChecks++;
        CollisionInfo info = GetCollisionInfo(c1, c2);

        VectorDeltas delPos = ResolvePosition(info);
        VectorDeltas delVel = ResolveVelocity(info);

        delPos.ApplyToPosition(c1, c2);
        delVel.ApplyToVelocity(c1, c2);
    }

    public static void ApplyCollisionResolution(PhysicsCollider c1, WaveCollider c2)
    {
        CollisionChecks++;
        CollisionInfo info = GetCollisionInfo(c1, c2);

        // Not Overlapping
        if (info.penetration <= 0) return;

        // 
        Vector3 force = info.normal;
        //force *= info.penetration * c2.density;
        if (info.penetration > 1) info.penetration = 1;

        //Fb	=	-ρgV
        
        // Approximating displaced
        //Vector3 force = Vector3.up;
        float volume = info.penetration * c1.GetComponent<Sphere>().Volume;
        force *= c2.density * c1.GetComponent<Particle3D>().gravity.magnitude * volume; 

        c1.TryGetComponent(out Particle3D component);
        component.AddForce(force);
    }

    public static VectorDeltas ResolvePosition(CollisionInfo info)
    {
        if (!info.IsColliding) return VectorDeltas.zero;
        if (info.HasInfiniteMass) return VectorDeltas.zero;

        return new VectorDeltas
        {
            s1 = info.pctToMoveS1 * info.normal * info.penetration,
            s2 = info.pctToMoveS2 * -info.normal * info.penetration
        };
    }

    public static VectorDeltas ResolveVelocity(CollisionInfo info)
    {
        if (!info.IsColliding) return VectorDeltas.zero;
        if (info.HasInfiniteMass) return VectorDeltas.zero;
        float restitution = 1;

        float separatingVelocity = info.separatingVelocity;
        if (separatingVelocity >= 0) return VectorDeltas.zero;
        float newSeparatingVelocity = -separatingVelocity * restitution;
        float deltaVelocity = newSeparatingVelocity - separatingVelocity;

        return new VectorDeltas
        {
            s1 = deltaVelocity * info.pctToMoveS1 * info.normal,
            s2 = deltaVelocity * info.pctToMoveS2 * -info.normal
        };
    }
}
