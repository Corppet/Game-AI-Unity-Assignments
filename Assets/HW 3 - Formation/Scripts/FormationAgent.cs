using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider), typeof(FieldOfView), typeof(NavMeshAgent))]
public class FormationAgent : MonoBehaviour
{
    [HideInInspector] public int formationID;
    [HideInInspector] public Vector3 formationDestination;
    [HideInInspector] public NavMeshAgent agent;

    [SerializeField] private LayerMask obstacleMask;

    private FieldOfView fov;

    private void OnEnable()
    {
        fov = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void FixedUpdate()
    {
        FollowFormation();
    }

    /// <summary>
    /// Follows the formation that moves according to the formation lead.
    /// </summary>
    private void FollowFormation()
    {
        agent.SetDestination(formationDestination);
    }
    
    /// <summary>
    /// Raycasts in front of the object to detect any obstacles in its way. If an obstacle is detected, the object will
    /// follow the player's trail instead of following the formation path.
    /// </summary>
    private bool CheckForObstacles()
    {
        ref float range = ref fov.viewRadius;

        // find all obstacles within the agent's range
        Collider[] obstaclesInRange = Physics.OverlapSphere(transform.position, range, obstacleMask);

        foreach (Collider collider in obstaclesInRange)
        {
            Transform obstacle = collider.transform;

            // return true if obstacle is within the agent's field of view
            Vector3 direction = (obstacle.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, direction) <= fov.viewAngle / 2)
                return true;
        }

        // no obstacle in agent's fielf of view
        return false;
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
}
