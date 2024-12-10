using Codice.Client.BaseCommands;
using Codice.CM.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using static UnityEditor.PlayerSettings;

// All the Unity auto includes that were accidentally added

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
    public IWave currentWave { get { return waves[(int)waveType]; } private set { } }

    public List<Material> mats = new List<Material>();
    public List<IWave> waves = new List<IWave>();

    public FlatWave flat;
    public SineWave sine;

    public GerstnerWaves gerstner;
    public List<GerstnerWave> gerstnerWaves;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        waves.Add(flat);
        waves.Add(sine);
        waves.Add(gerstner);

        changeMaterial();
        updateMaterial(meshRenderer.sharedMaterial);

        gerstner.loadWaves(gerstnerWaves);
    }

    private void FixedUpdate()
    {
        gerstner.loadWaves(gerstnerWaves);
        updateMaterial(meshRenderer.sharedMaterial);
    }

    private void LateUpdate()
    {
        if (waveType != prevWaveType)
            changeMaterial();

        prevWaveType = waveType;
    }
    
    private void updateMaterial(Material mat)
    {
        currentWave.updateShader(mat);
    }

    private void updateFromMaterial(Material mat)
    {
        currentWave.updateFromShader(mat);
    }

    public void changeMaterial()
    {
        meshRenderer.material = mats[(int)waveType];
    }

    public float getHeight(Vector2 pos)
    {
        return currentWave.getHeightAtPos(pos);
    }

    public Vector3 getNormalAtPos(Vector2 pos)
    {
        return currentWave.getNormalAtPos(pos);
    }
}

public interface IWave
{
    /// <returns>sets relevant shader params</returns>
    public void updateShader(Material mat);

    /// <summary>
    /// Updates params from shader
    /// </summary>
    /// <param name="mat"></param>
    public void updateFromShader(Material mat);

    /// <returns>height relative to the Object</returns>
    public float getHeightAtPos(Vector2 pos);

    /// <returns>vertex point relative to the Object</returns>
    public Vector3 getUndulate(Vector2 pos);

    /// <returns>normal relative to the Object</returns>
    public Vector3 getNormalAtPos(Vector2 pos);
}

[Serializable]
public class FlatWave : IWave
{
    public void updateShader(Material mat)
    {
        // Nothing to update lol
        return;
    }

    public void updateFromShader(Material mat)
    {

    }

    public float getHeightAtPos(Vector2 pos)
    {
        return 0;
    }

    public Vector3 getUndulate(Vector2 pos)
    {
        return Vector3.zero;
    }

    public Vector3 getNormalAtPos(Vector2 pos)
    {
        return Vector3.up;
    }
}

[Serializable]
public class SineWave : IWave
{
    [SerializeField, Range(0,30)]
    public float amplitude = 5;
    [SerializeField, Range(0, 50)]
    public float wavelength = 50;
    [SerializeField, Range(0, 50)]
    public float speed = 50;

    private float waveNumber { get { return 2 * Mathf.PI / wavelength; } set { } }

    public void updateShader(Material mat)
    {
        mat.SetFloat("_Amplitude", amplitude);
        mat.SetFloat("_Wavelength", wavelength);
        mat.SetFloat("_Speed", speed);
    }

    public void updateFromShader(Material mat)
    {
        Debug.Log("Updating from Shader");
        amplitude = mat.GetFloat("_Amplitude");
        wavelength = mat.GetFloat("_Wavelength");
        speed = mat.GetFloat("_Speed");
    }

    public float getHeightAtPos(Vector2 pos)
    {
        float f = waveNumber * (pos.x - speed * Time.time);
        float height = amplitude * Mathf.Sin(f);
        return height;
    }

    public Vector3 getUndulate(Vector2 pos)
    {
        float f = waveNumber * (pos.x - speed * Time.time);

        float height = amplitude * Mathf.Sin(f);

        return new Vector3(pos.x, height, pos.y);
    }

    public Vector3 getNormalAtPos(Vector2 pos)
    {
        float f = waveNumber * (pos.x - speed * Time.time);
        float height = amplitude * Mathf.Sin(f);

        Vector3 tangent = Vector3.Normalize(new Vector3(1, waveNumber * amplitude * Mathf.Cos(f), 0));
        Vector3 normal = new Vector3(-tangent.y, tangent.x, 0);
        return normal;
    }

}

[Serializable]
public class GerstnerWaves : IWave
{
    List<GerstnerWave> waves = new List<GerstnerWave>();
    int maxWavesIndex = 10;
    List<Vector4> wavesAsVectors = new List<Vector4>(10);

    public GerstnerWaves()
    {
        for (int i = 0; i < maxWavesIndex; i++)
            waves.Add(new GerstnerWave());
        wavesToVector4();
    }

    public void loadWaves(List<GerstnerWave> waves)
    {
        this.waves = waves;
        wavesToVector4();
    }

    public void wavesToVector4()
    {
        wavesAsVectors.Clear();

        for (int i = 0; i < wavesAsVectors.Capacity && i < waves.Count; i++)
        {
            GerstnerWave wave = waves[i];

            if (i >= wavesAsVectors.Count)
            {
                wavesAsVectors.Add(wave.toVector4());
                continue;
            }

            wavesAsVectors[i] = wave.toVector4();
        }

        //Debug.Log(wavesAsVectors[0]);
    }

    public void updateShader(Material mat)
    {
        wavesToVector4();
        mat.SetInteger("_NumWaves", wavesAsVectors.Count);
        mat.SetVectorArray("_Waves", wavesAsVectors.ToArray());
    }

    public void updateFromShader(Material mat)
    {
        throw new NotImplementedException();
    }

    public float getHeightAtPos(Vector2 pos)
    {
        float height = 0;

        foreach (GerstnerWave wave in waves)
            height += wave.getHeight(pos);

        return height;
    }

    public Vector3 getUndulate(Vector2 pos)
    {
        throw new NotImplementedException();
    }

    public Vector3 getNormalAtPos(Vector2 pos)
    {
        Vector3 normal = Vector3.zero;

        foreach (GerstnerWave wave in waves)
            normal += wave.getNormalAtPos(pos);
        
        return normal.normalized;
    }
}

[Serializable]
public class GerstnerWave
{
    [SerializeField, Range(0,10)] int waveHeightResolution = 1;

    public Vector2 dir = new Vector2(1,1);
    [SerializeField, Range(0, 30)]
    public float wavelength = 0f;
    [SerializeField, Range(0, 1)]
    float steepness = .5f; 

    private float k { get { return 2 * Mathf.PI / wavelength; } set { } }
    private float c { get { return Mathf.Sqrt(9.8f / k); } set { } }
    private float a { get { return steepness / k; } set { } }
    private Vector2 d { get { return dir.normalized; } set { } }


    public GerstnerWave() { }

    GerstnerWave(float dirX = 1, float dirY = 0, float steepness = .5f, float wavelength = 10f)
    {
        this.dir.x = dirX;
        this.dir.y = dirY;
        this.steepness = steepness;
        this.wavelength = wavelength;
    }

    /// <summary>
    /// Passing into GPU
    /// </summary>
    public Vector4 toVector4()
    {
        Vector4 info = new Vector4();

        info.x = dir.x;
        info.y = dir.y;
        info.z = steepness;
        info.w = wavelength;

        return info;
    }

    public float getHeight(Vector2 pos)
    {
        // omg it works
       
        float oldF = k * (Vector2.Dot(d, pos) - (c * Time.time));

        // The position we need to look at to get the height at our real position
        Vector2 newPos = new Vector2(
            k * pos.x - a * Mathf.Cos(oldF),
            k * pos.y - a * Mathf.Cos(oldF)
        );

        for (int i = 0; i < waveHeightResolution; i++)
        {
            newPos.x = k * pos.x - a * Mathf.Cos(newPos.x);
            newPos.y = k * pos.y - a * Mathf.Cos(newPos.y);
        }

        float f = k * (Vector2.Dot(d, newPos) - (c * Time.time));

        return a * Mathf.Sin(f);
    }

    public Vector3 getUndulate(Vector2 pos)
    {
        float f = k * (Vector3.Dot(d, pos) - c * Time.time);

        return new Vector3(
            d.x * (a * Mathf.Cos(f)),
            a * Mathf.Sin(f),
            d.y * (a * (Mathf.Cos(f)))
        );
    }

    public Vector3 getNormalAtPos(Vector2 pos) {
        Vector3 tangent = Vector3.zero, binormal = Vector3.zero;

        float f = k * (Vector2.Dot(d, pos) - c * Time.time);

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


