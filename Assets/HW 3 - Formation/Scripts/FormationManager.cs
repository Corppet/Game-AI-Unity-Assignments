using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public enum FormationMode
{
    Scalable,
    TwoLevel
}

public class FormationManager : MonoBehaviour
{
    [HideInInspector] public static FormationManager instance { get; private set; }

    [HideInInspector] public List<FormationAgent> agents;

    public FormationLead formationLead;
    public FormationMode currentFormation;
    public LayerMask obstacleMask;

    [Space(5)]

    [Header("Formation Settings")]
    [Range(12, 20)]
    public int numAgents = 12;

    [Space(5)]

    [Header("Scalable Formation Settings")]
    [Range(0f, 10f)]
    public float maxRadius = 5f;
    [HideInInspector] public float radius;

    [Space(5)]

    [Header("Two-Level Formation Settings")]
    [Range(0f, 10f)]
    [Tooltip("The orthogonal distance between each agent in the grid formation.")]
    public float gapDistance = 5f;
    [Range(1, 5)]
    [Tooltip("The number of agents in a single row of the grid formation (number of columns).")]
    public int rowWidth = 4;

    [Space(10)]

    [Header("References")]
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private Transform formationParent;
    [SerializeField] private TMP_Dropdown formationDropdown;
    [SerializeField] private Slider agentCountSlider;
    [SerializeField] private TMP_Text agentCountText;

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot,
        Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    /// <summary>
    /// Function for changing formations when a new dropdown value is set.
    /// </summary>
    public void SetFormation()
    {
        ChangeFormation((FormationMode)formationDropdown.value);
    }

    public void ChangeFormation(FormationMode newMode)
    {
        currentFormation = newMode;
        SetupAgents();
    }

    /// <summary>
    /// Function for changing the number of agents when a new slider value is set.
    /// </summary>
    public void SetNumAgents()
    {
        numAgents = (int)agentCountSlider.value;
        agentCountText.text = "Agents: " + numAgents;
        SetupAgents();
    }

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

        radius = maxRadius;
        agents = new List<FormationAgent>();
    }

    private void Start()
    {
        SetupDropdown();

        agentCountSlider.value = numAgents;
        agentCountText.text = "Agents: " + numAgents;
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

    private void SetupDropdown()
    {
        formationDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (FormationMode mode in System.Enum.GetValues(typeof(FormationMode)))
        {
            options.Add(mode.ToString());
        }
        formationDropdown.AddOptions(options);
        formationDropdown.value = (int)currentFormation;
    }

    private void SetupAgents()
    {
        foreach (FormationAgent agent in agents)
        {
            Destroy(agent.gameObject);
        }
        agents.Clear();

        // create agents
        for (int i = 0; i < numAgents; i++)
        {
            GameObject newAgent = Instantiate(agentPrefab, formationLead.transform.position, 
                Quaternion.identity, formationParent);
            newAgent.name = "Agent " + i;
            newAgent.GetComponent<FormationAgent>().formationID = i;
            agents.Add(newAgent.GetComponent<FormationAgent>());
        }
    }

    private void ScalableFormation()
    {
        // move agents to form a circle around the formation lead
        for (int i = 0; i < agents.Count; i++)
        {
            float angle = i * Mathf.PI * 2f / agents.Count;
            Vector3 newPos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            newPos += formationLead.transform.position;
            
            agents[i].formationDestination = newPos;
        }
    }

    private void TwoLevelFormation()
    {
        Transform leadTransform = formationLead.transform;
        int numRows = Mathf.CeilToInt((float)agents.Count / rowWidth);

        // move agents to form a grid on the formation lead
        for (int i = 0; i < agents.Count; i++)
        {
            int row = i / rowWidth;
            int col = i % rowWidth;

            Vector3 newPos = leadTransform.position;

            // first half of rows are in front of the lead
            if (row < numRows / 2)
            {
                newPos += Vector3.forward * gapDistance * (numRows / 2 - row);
            }
            // second half of rows are behind the lead
            else
            {
                newPos += Vector3.back * gapDistance * (row - numRows / 2);
            }

            // if the number of rows is even, offset the rows by half a gap
            if (numRows % 2 == 0)
            {
                newPos += Vector3.back * gapDistance / 2;
            }

            // first half of columns are to the left of the lead
            if (col < rowWidth / 2)
            {
                newPos += Vector3.left * gapDistance * (rowWidth / 2 - col);
            }
            // second half of columns are to the right of the lead
            else
            {
                newPos += Vector3.right * gapDistance * (col - rowWidth / 2);
            }

            // if the number of columns is even, offset the rows by half a gap
            if (rowWidth % 2 == 0)
            {
                newPos += Vector3.right * gapDistance / 2;
            }

            agents[i].formationDestination = newPos;
        }
    }
}
