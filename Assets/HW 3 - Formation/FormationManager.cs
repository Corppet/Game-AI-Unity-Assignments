using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FormationManager : MonoBehaviour
{
    [HideInInspector] public static FormationManager instance { get; private set; }

    [HideInInspector] public List<GameObject> agents;

    [Header("Formation Settings")]
    [Range(1, 100)]
    public int numAgents = 12;

    [Space(5)]

    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private Transform formationParent;

    private NavMeshAgent agent;
    private bool isOnCooldown;
    private float originalSpeed;

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
    }

    private void Start()
    {
        // setup agents in a V-shaped formation
        agents = new List<GameObject>();
        for (int i = 0; i < numAgents; i++)
        {
            Vector3 pos = transform.position + new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f));

            agents.Add(Instantiate(agentPrefab, pos, Quaternion.identity, formationParent));
        }
    }
}
