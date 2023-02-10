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
    [Range(1, 100)]
    public int numAgents = 12;

    [Space(5)]

    [Header("Scalable Formation Settings")]
    [Range(0f, 100f)]
    public float maxRadius = 5f;
    [HideInInspector] public float radius;

    [Space(5)]

    [Header("Two-Level Formation Settings")]
    [Range(0f, 180f)]
    public float vFormationAngle = 30f;
    [Range(0f, 180f)]
    public float vFormationDistance = 2f;

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
    }

    private void Start()
    {
        SetupDropdown();

        agentCountSlider.value = numAgents;
        agentCountText.text = "Agents: " + numAgents;
        SetupAgents(true);
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

    private void SetupAgents(bool isFirstCall = false)
    {
        if (isFirstCall)
        {
            agents = new List<FormationAgent>();
        }
        else
        {
            foreach (FormationAgent agent in agents)
            {
                Destroy(agent.gameObject);
            }
            agents.Clear();
        }

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

        // move agents to form a v-shape behind the formation lead
        for (int i = 0; i < agents.Count; i++)
        {
            Vector3 pos = new Vector3(Mathf.Cos(vFormationAngle * Mathf.Deg2Rad) * vFormationDistance
                * (i / 2 + 1) * Mathf.Pow(-1, i), 
                leadTransform.position.y, 
                -Mathf.Sin(vFormationAngle * Mathf.Deg2Rad)) * vFormationDistance * (i / 2 + 1);
            pos += leadTransform.position;
            pos = RotatePointAroundPivot(pos, leadTransform.position, 
                leadTransform.eulerAngles);
            
            agents[i].formationDestination = pos;
        }
    }
}
