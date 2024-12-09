using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WaveController))]

public class WaveCollider : PhysicsCollider
{
    public float density;

    WaveController controller;

    public Vector3 Origin => transform.position;
    public Vector3 Normal => transform.up;
    public float Offset => Vector3.Dot(Origin, Normal);
    public override Shape shape => Shape.Wave;

    private void Awake()
    {
        controller = GetComponent<WaveController>();
    }

    public float getHeight(Vector2 pos)
    {
        return controller.getHeight(pos);
    }

    public Vector3 getNormal(Vector2 pos)
    {
        return controller.getNormalAtPos(pos);
    }
}
