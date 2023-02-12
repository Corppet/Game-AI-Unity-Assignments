using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider), typeof(FieldOfView), typeof(NavMeshAgent))]
public class FormationAgent : MonoBehaviour
{
    [Header("Obstacle Avoidance")]
    [Range(0f, 5f)]
    public float avoidDistance = 1f;
    [Range(0f, 5f)]
    public float avoidTime = 1f;

    [HideInInspector] public int formationID;
    [HideInInspector] public Vector3 formationDestination;
    [HideInInspector] public NavMeshAgent agent;

    private FieldOfView fov;
    private float cooldownTime;

    public bool CanReachDestination()
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(formationDestination, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }

    private void OnEnable()
    {
        fov = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = FormationManager.instance.maxSpeed;
    }

    private void Update()
    {
        if (cooldownTime > 0f) 
        { 
            cooldownTime -= Time.deltaTime;
        }

        switch (FormationManager.instance.currentFormation)
        {
            case FormationMode.Scalable:
                ScalableFormation();
                break;
            case FormationMode.TwoLevel:
                TwoLevelFormation();
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ref List<FormationAgent> agents = ref FormationManager.instance.agents;

            // remove this agent from the manager agents and update ids
            agents.RemoveAt(formationID);
            for (int i = formationID; i < agents.Count; i++)
            {
                agents[i].formationID = i;
            }

            // destroy this agent
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Follows the formation that moves according to the formation lead.
    /// </summary>
    private void FollowFormation()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(formationDestination, out hit, agent.height * 2, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.Log("Agent " + formationID + " could not find valid destination.");
            agent.SetDestination(FormationManager.instance.formationLead.transform.position);
        }
    }

    /// <summary>
    /// Shoots raycasts in the agent's fov to find new destinations to avoid incoming 
    /// obstacles.
    /// </summary>
    private void AvoidObstacles()
    {
        float range = fov.viewRadius * 1.5f;
        ref float angle = ref fov.viewAngle;
        LayerMask obstacleMask = FormationManager.instance.obstacleMask;
        Vector3 pos = transform.position;
        Vector3 front = transform.forward;
        Vector3 up = transform.up;

        // raycast in front to avoid obstacles
        RaycastHit hitFront;
        bool wallInFront = Physics.Raycast(pos, front, out hitFront, range, 
            obstacleMask);

        // left raycast
        RaycastHit hitLeft;
        Vector3 left = Quaternion.AngleAxis(-angle / 2f, up) * front;
        bool wallOnLeft = Physics.Raycast(pos, left.normalized, out hitLeft, range, 
            obstacleMask);

        // right raycast
        RaycastHit hitRight;
        Vector3 right = Quaternion.AngleAxis(angle / 2f, up) * front;
        bool wallOnRight = Physics.Raycast(pos, right.normalized, out hitRight, range, 
            obstacleMask);

        Transform leadTransform = FormationManager.instance.formationLead.transform;

        if (wallInFront)
        {
            if (wallOnLeft && wallOnRight)
            {
                // Last Resort: Agent follows lead if walls are on both sides
                agent.SetDestination(leadTransform.position);
            }
            else
            {
                Vector3 reflectVec = Vector3.Reflect(hitFront.point - pos, hitFront.normal)
                    .normalized * avoidDistance;
                agent.SetDestination(reflectVec);

                // Draw lines to show the incoming "beam" and the reflection.
                Debug.DrawLine(transform.position, hitFront.point, Color.yellow);
                Debug.DrawRay(hitFront.point, reflectVec, Color.yellow);

            }
        }
        else if (wallOnLeft || wallOnRight)
        {
            // if a wall is on either side, go straight
            agent.SetDestination(transform.position + transform.forward * fov.viewRadius);
        }
    }

    /// <summary>
    /// Cone checks in front of the object to detect any obstacles in its way. If an obstacle 
    /// is detected, the object will follow the player's trail instead of following the 
    /// formation path.
    /// </summary>
    private bool CheckForObstacles()
    {
        ref float range = ref fov.viewRadius;

        // find all obstacles within the agent's range
        Collider[] obstaclesInRange = Physics.OverlapSphere(transform.position, range,
            FormationManager.instance.obstacleMask);

        foreach (Collider collider in obstaclesInRange)
        {
            Transform obstacle = collider.transform;

            // return true if obstacle is within the agent's field of view
            Vector3 direction = (obstacle.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, direction) <= fov.viewAngle / 2)
            {
                cooldownTime = avoidTime;
                return true;
            }
        }

        // no obstacle in agent's fielf of view
        return false;
    }

    private void ScalableFormation()
    {
        FollowFormation();
    }

    private void TwoLevelFormation()
    {
        if (CheckForObstacles() || cooldownTime > 0f)
        {
            AvoidObstacles();
        }
        else
        {
            FollowFormation();
        }
    }
}
