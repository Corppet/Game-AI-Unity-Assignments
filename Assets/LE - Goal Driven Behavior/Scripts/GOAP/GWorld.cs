using System.Collections;
using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEngine;

public sealed class GWorld
{
    private static readonly GWorld instance = new();
    private static WorldStates world;
    private static Queue<GameObject> patients;
    private static Queue<GameObject> cubicles;


    static GWorld()
    {
        world = new();
        patients = new();
        cubicles = new();

        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cubicle");
        foreach (GameObject c in cubes)
        {
            cubicles.Enqueue(c);
        }

        if (cubes.Length > 0)
        {
            world.ModifyState("FreeCubicle", cubes.Length);
        }

        Time.timeScale = 5;
    }

    private GWorld() { }

    public void AddPatient(GameObject patient)
    {
        patients.Enqueue(patient);
    }

    public GameObject RemovePatient()
    {
        if (patients.Count == 0)
            return null;
        else
            return patients.Dequeue();
    }

    public void AddCubicle(GameObject cubicle)
    {
        cubicles.Enqueue(cubicle);
    }

    public GameObject RemoveCubicle()
    {
        if (cubicles.Count == 0)
            return null;
        else
            return cubicles.Dequeue();
    }

    public static GWorld Instance
    {
        get { return instance; }
    }

    public WorldStates GetWorld()
    {
        return world;
    }
}
