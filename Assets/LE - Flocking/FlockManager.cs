using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [HideInInspector] public static FlockManager instance { get; private set; }

    [Header("Flock Settings")]
    [Range(1, 100)]
    public int numFish = 20;
    public Vector3 swimLimits = new Vector3(5, 5, 5);

    [Space(5)]

    [Header("Fish Settings")]
    [Range(0.0f, 5.0f)]
    public float minSpeed = 0.5f;
    [Range(0.0f, 5.0f)]
    public float maxSpeed = 2.0f;
    [Range(1.0f, 10.0f)]
    public float neighbourDistance = 3.0f;
    [Range(1.0f, 20.0f)]
    public float rotationSpeed = 5.0f;

    [SerializeField] private GameObject[] fishPrefabs;
    [SerializeField] private Transform goal;

    [HideInInspector] public GameObject[] fishes;
    [HideInInspector] public Vector3 goalPos;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Cursor.visible = false;
    }
    
    private void Start()
    {
        // setup flock of fishes
        fishes = new GameObject[numFish];
        for (int i = 0; i < numFish; i++)
        {
            Vector3 pos = transform.position + new Vector3(
                Random.Range(-swimLimits.x, swimLimits.x),
                Random.Range(-swimLimits.y, swimLimits.y),
                Random.Range(-swimLimits.z, swimLimits.z));

            fishes[i] = Instantiate(fishPrefabs[Random.Range(0, fishPrefabs.Length)], pos, Quaternion.identity);
        }

        goalPos = transform.position;
    }

    private void Update()
    {
        /*
        if (Random.Range(0, 100) < 10)
        {
            goalPos = transform.position + new Vector3(
                Random.Range(-swimLimits.x, swimLimits.x),
                Random.Range(-swimLimits.y, swimLimits.y),
                Random.Range(-swimLimits.z, swimLimits.z));
        }
        */

        // move the goal to the mouse position
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        goalPos = Camera.main.ScreenToWorldPoint(mousePosition);
        goalPos.y = 0f;
        goal.position = goalPos;
    }
}
