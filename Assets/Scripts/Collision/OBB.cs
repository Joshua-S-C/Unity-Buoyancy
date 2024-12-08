using UnityEngine;

public class OBB : PhysicsCollider
{
    public override Shape shape => Shape.OBB;

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

    public Vector3[] Axes
    {
        get
        {
            return new Vector3[3]
            {
                transform.right, transform.up, transform.forward
            };
        }
    }

}
