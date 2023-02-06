using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView), typeof(NavMeshAgent))]
public class FormationAgent : MonoBehaviour
{
    [HideInInspector] public int formationID;
    [HideInInspector] public Vector3 offset;

    [SerializeField] private LayerMask obstacleMask;

    private FieldOfView fov;
    private NavMeshAgent agent;
    private bool isOnCooldown;
    private float originalSpeed;

    private void OnEnable()
    {
        fov = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed;
        isOnCooldown = false;
    }

    private void FixedUpdate()
    {
        
    }

    /// <summary>
    /// Follows the formation that moves according to the formation lead.
    /// </summary>
    private void FollowFormation()
    {
        Vector3 leadPos = FormationManager.instance.formationLead.position;

        // rotate the offset around the current leadPos
        Vector3 offsetRotated = Quaternion.Euler(0, FormationManager.instance.formationLead.eulerAngles.y, 0) * offset;

        // set the agent's destination to the rotated offset
        agent.SetDestination(leadPos + offsetRotated);
    }

    /// <summary>
    /// Follows the player's invisible breadcrumb trail instead of the formation path.
    /// </summary>
    private void FollowTrail(bool isFirstCall = false)
    {
        if (isFirstCall)
        {
            
        }
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
}
