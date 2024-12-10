using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : PhysicsCollider
{
    [SerializeField] bool overrideVolume = false;

    public override Shape shape => Shape.Sphere;
    public Vector3 Center => transform.position;

    public float Radius => transform.localScale.x / 2;
    public float Volume => 
        overrideVolume ? 
        volume : 
        (4f / 3f * Mathf.PI * (Radius * Radius * Radius));

    [SerializeField] float volume;
    [SerializeField] float ViewDensity;

    private void Update()
    {
        ViewDensity = 1 / invMass / Volume;
        if (!overrideVolume)
            volume = Volume;
    }
}
