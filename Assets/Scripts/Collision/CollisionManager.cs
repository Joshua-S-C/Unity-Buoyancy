using System.Collections;
using System.Collections.Generic;
using static CollisionDetection;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollisionManager : MonoBehaviour
{
    [SerializeField] Sphere[] spheres;
    [SerializeField] PlaneCollider[] planes;
    [SerializeField] WaveCollider[] waves;

    private void StandardCollisionResolution()
    {
        spheres = FindObjectsOfType<Sphere>();
        planes = FindObjectsOfType<PlaneCollider>();
        waves = FindObjectsOfType<WaveCollider>();
        
        for (int i = 0; i < spheres.Length; i++)
        {
            Sphere s1 = spheres[i];
            for (int j = i + 1; j < spheres.Length; j++)
            {
                Sphere s2 = spheres[j];
                ApplyCollisionResolution(s1, s2);
            }
            foreach (PlaneCollider plane in planes)
            {
                ApplyCollisionResolution(s1, plane);
            }

            foreach (WaveCollider wave in waves)
            {
                ApplyCollisionResolution(s1, wave);
            }
        }
    }

    private void FixedUpdate()
    {
        CollisionChecks = 0;

        StandardCollisionResolution();
    }
}
