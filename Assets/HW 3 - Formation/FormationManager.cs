using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum FormationMode
{
    Scalable,
    TwoLevel
}

public class FormationManager : MonoBehaviour
{
    [HideInInspector] public static FormationManager instance { get; private set; }

    [HideInInspector] public List<FormationAgent> agents;

    public Transform formationLead;
    public FormationMode currentFormation;

    [Space(5)]

    [Header("Formation Settings")]
    [Range(1, 100)]
    public int numAgents = 12;

    [Space(5)]

    [Header("Scalable Formation Settings")]
    [Range(0f, 100f)]
    public float standardRadius = 10f;
    [HideInInspector] public float currentRadius;

    [Space(5)]

    [Header("Two-Level Formation Settings")]
    [Range(0f, 180f)]
    public float vFormationAngle = 30f;
    [Range(0f, 180f)]
    public float vFormationDistance = 1f;

    [Space(10)]

    [Header("References")]
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private Transform formationParent;

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

        currentRadius = standardRadius;
    }

    private void Start()
    {
        // create agents
        for (int i = 0; i < numAgents; i++)
        {
            GameObject newAgent = Instantiate(agentPrefab, formationParent);
            newAgent.name = "Agent " + i;
            newAgent.GetComponent<FormationAgent>().formationID = i;
            agents.Add(newAgent.GetComponent<FormationAgent>());
        }
    }

    private void Update()
    {
        switch (currentFormation)
        {
            case FormationMode.Scalable:
                ScalableFormation();
                break;
            case FormationMode.TwoLevel:
                TwoLevelFormation();
                break;
        }
    }

    private void ScalableFormation()
    {
        // move agents to form a circle around the formation lead
        for (int i = 0; i < agents.Count; i++)
        {
            float angle = i * Mathf.PI * 2f / agents.Count;
            Vector3 newPos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * currentRadius;
            agents[i].formationDestination = newPos;
        }
    }

    private void TwoLevelFormation()
    {
        // move agents to form a v-shape behind the formation lead
        for (int i = 0; i < agents.Count; i++)
        {

        }
    }
}
