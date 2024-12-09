using Codice.Client.BaseCommands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
public enum WaveType
{
    None = 0,
    Sine = 1,
    Gerstner = 2
}

// TODO Make this abstact and have children for each wave type

/// <summary>
/// Handles CPU side Wave simulation, and updating the shader
/// </summary>
public class WaveController : MonoBehaviour
{
    private Renderer meshRenderer;

    public WaveType waveType = WaveType.None;
    private WaveType prevWaveType;

    public List<Material> mats = new List<Material>();
    public List<Wave> waves = new List<Wave>();

    public FlatWave flat;
    public SineWave sine;
    public GerstnerWaves gerstner;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        waves.Add(flat);
        waves.Add(sine);
        waves.Add(gerstner);
    }

    private void Update()
    {
        updateMaterial(meshRenderer.material);
    }

    private void LateUpdate()
    {
        if (waveType != prevWaveType)
            changeMaterial();

        prevWaveType = waveType;
    }

    private void updateMaterial(Material mat)
    {
        waves[(int)waveType].updateShader(mat);
    }

    public void changeMaterial()
    {
        meshRenderer.material = mats[(int)waveType];
    }

    public float getHeight(Vector2 pos)
    {
        return waves[(int)waveType].getUndulate(pos).y;
    }

    public Vector3 getNormalAtPos(Vector2 pos)
    {
        return waves[(int)waveType].getNormalAtPos(pos);
    }
}

public interface Wave
{
    public void updateShader(Material mat);
    public Vector3 getUndulate(Vector2 pos);
    public Vector3 getNormalAtPos(Vector2 pos);
}

[ExecuteInEditMode]
[Serializable]
public class FlatWave : Wave
{
    public Vector3 getNormalAtPos(Vector2 pos)
    {
        throw new NotImplementedException();
    }

    public Vector3 getUndulate(Vector2 pos)
    {
        throw new NotImplementedException();
    }

    public void updateShader(Material mat)
    {
        // Nothing to update lol
        return;
    }
}

[Serializable]
public class SineWave : Wave
{
    public float amplitude = 5, wavelength = 50, speed = 50;

    public void updateShader(Material mat)
    {
        Debug.Log("Updated Shader");
        mat.SetFloat("Amplitude", amplitude);
        mat.SetFloat("Wavelength", wavelength);
        mat.SetFloat("Speed", speed);
    }

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

[Serializable]
public class GerstnerWaves : Wave
{
    List<GerstnerWave> waves = new List<GerstnerWave>();
    public void updateShader(Material mat)
    {
        throw new NotImplementedException();
    }

    public Vector3 getUndulate(Vector2 pos)
    {
        return waves[0].getUndulate(pos);
        throw new NotImplementedException();

    }

    public Vector3 getNormalAtPos(Vector2 pos)
    {
        return waves[0].getNormalAtPos(pos);
        throw new NotImplementedException();

    }

}

[Serializable]
public class GerstnerWave
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
}


