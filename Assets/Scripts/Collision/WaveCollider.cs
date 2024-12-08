using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using static UnityEngine.GraphicsBuffer;

/*public class GerstnerWave
{
    public Vector2 dir = new Vector2(1,1);
    public float steepness = .5f, wavelength = 10f;

    public GerstnerWave() { }

    GerstnerWave(float dirX = 1, float dirY = 0, float steepness = .5f, float wavelength = 10f)
    {
        this.dir.x = dirX;
        this.dir.y = dirY;
        this.steepness = steepness;
        this.wavelength = wavelength;
    }

    public Vector3 getUndulate(Vector2 pos)
    {
        // Used in Calcs --------------------------------------------------------/
        float k = 2 * Mathf.PI / wavelength;
        float c = Mathf.Sqrt(9.8f / k);
        Vector2 d = dir.normalized;
        float f = k * (Vector3.Dot(d, pos) - c * Time.time);
        float a = steepness / k;

        return new Vector3(
            d.x * (a * Mathf.Cos(f)),
            a * Mathf.Sin(f),
            d.y * (a * (Mathf.Cos(f)))
        );
    }

    public Vector3 getNormalAtPos(Vector2 pos) {
        Vector3 tangent = Vector3.zero, binormal = Vector3.zero;

        float k = 2 * Mathf.PI / wavelength;
        float c = Mathf.Sqrt(9.8f / k);
        Vector2 d = dir.normalized;
        float f = k * (Vector2.Dot(d, pos) - c * Time.time);
        float a = steepness / k;

        tangent += new Vector3(
            -d.x * d.x * (steepness * Mathf.Sin(f)),
            d.x * (steepness * Mathf.Cos(f)),
            -d.x * d.y * (steepness * Mathf.Sin(f))
        );
        binormal += new Vector3(
            -d.x * d.y * (steepness * Mathf.Sin(f)),
            d.y * (steepness * Mathf.Cos(f)),
            -d.y * d.y * (steepness * Mathf.Sin(f))
        );
        return Vector3.Normalize(Vector3.Cross(binormal, tangent));
    }
}*/

public class GerstnerWave
{
    float amplitude = 5, wavelength = 50, speed = 50;

    public Vector3 getUndulate(Vector2 pos)
    {
        float k = 2 * Mathf.PI / wavelength;
        float f = k * (pos.x - speed * Time.time);

        float height = amplitude * Mathf.Sin(f);

        return new Vector3(pos.x, height, pos.y);
    }

    public Vector3 getNormalAtPos(Vector2 pos)
    {
        float k = 2 * Mathf.PI / wavelength;
        float f = k * (pos.x - speed * Time.time);
        float height = amplitude * Mathf.Sin(f);

        Vector3 tangent = Vector3.Normalize(new Vector3(1, k * amplitude * Mathf.Cos(f), 0));
        Vector3 normal = new Vector3(-tangent.y, tangent.x, 0);
        return normal;
    }
}

public class WaveCollider : PhysicsCollider
{
    public float density;
    public GerstnerWave wave = new GerstnerWave();

    public Vector3 Origin => transform.position;
    public Vector3 Normal => transform.up;
    public float Offset => Vector3.Dot(Origin, Normal);
    public override Shape shape => Shape.Wave;

    public float getHeight(Vector2 pos)
    {
        return wave.getUndulate(pos).y;
    }

    public Vector3 getNormalAtPos(Vector2 pos)
    {
        return wave.getNormalAtPos(pos);
    }
}
