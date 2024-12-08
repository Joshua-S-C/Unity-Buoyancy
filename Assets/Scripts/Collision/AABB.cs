using UnityEngine;

public class AABB : PhysicsCollider
{
    public override Shape shape => Shape.AABB;

    // DONE: YOUR CODE HERE
    public Vector3 Center
    {
        get
        {
            return transform.position;
        }
    }

    public Vector3 HalfWidth
    {
        get
        {
            return transform.localScale / 2;
        }
    }

}
