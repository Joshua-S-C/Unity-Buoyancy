using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WaveController))]
public class WaveCollider : PhysicsCollider
{
    public float density;
    public bool AlwaysUseSurfaceNormal = true, SetPosition = false;

    WaveController controller;

    public Vector3 Origin => transform.position;

    public Vector3 Up => transform.up;
    public float Offset => Vector3.Dot(Origin, transform.up);
    public override Shape shape => Shape.Wave;

    private void Awake()
    {
        controller = GetComponent<WaveController>();
    }

    public float getHeight(Vector2 pos)
    {
        return controller.getHeight(pos) + Origin.y;
    }

    public Vector3 getNormal(Vector2 pos)
    {
        return controller.getNormalAtPos(pos);
    }
}
