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

    [Space(5)]

    [Header("Formation Settings")]
    [Range(1, 100)]
    public int numAgents = 12;

    [Space(5)]

    [Header("Scalable Formation Settings")]
    [Range(0f, 100f)]
    public float radius = 10f;

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
    }

    private void Start()
    {
        // setup agents in a V-shaped formation
        agents = new List<FormationAgent>
        {
            // formation lead
            Instantiate(agentPrefab, formationLead.position, Quaternion.identity, formationParent)
                .GetComponent<FormationAgent>()
        };
        formationLead = agents[0].transform;
        
        // other agents
        for (int i = 1; i < numAgents; i++)
        {
            Vector3 pos = transform.position + new Vector3(
                Mathf.Sin(Mathf.Deg2Rad * vFormationAngle) * (i / 2) * vFormationDistance * Mathf.Pow(-1, i),
                formationLead.position.y,
                Mathf.Cos(Mathf.Deg2Rad * vFormationAngle) * (i / 2) * vFormationDistance);

            GameObject agent = Instantiate(agentPrefab, pos, Quaternion.identity, formationParent);
            FormationAgent agentScript = agent.GetComponent<FormationAgent>();
            agentScript.formationID = i;
            agentScript.offset = pos;
            agents.Add(agentScript);
        }
    }

    private void Update()
    {
        
    }
}
