using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView), typeof(Rigidbody))]
public class FormationLead : MonoBehaviour
{
    [SerializeField] private LineRenderer pathRenderer;
    private int pathIndex;

    private Rigidbody leadRigidbody;
    private FieldOfView fov;

    private void Awake()
    {
        leadRigidbody= GetComponent<Rigidbody>();
        fov = GetComponent<FieldOfView>();

        FlattenPath();
        pathIndex = 1;
    }

    private void Update()
    {
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

    private void FlattenPath()
    {
        for (int i = 0; i < pathRenderer.positionCount; i++)
        {
            Vector3 position = pathRenderer.GetPosition(i);
            position.y = transform.position.y;
            pathRenderer.SetPosition(i, position);
        }
    }

    private void FollowPath(FormationMode followMode)
    {
        if (pathIndex >= pathRenderer.positionCount)
        {
            pathIndex = 0;
        }

        Vector3 targetPosition = pathRenderer.GetPosition(pathIndex);
        Vector3 direction = targetPosition - transform.position;
        direction.Normalize();

        // change the lead's rotation to face the next point in the path
        transform.LookAt(targetPosition);

        if (followMode == FormationMode.Scalable)
        {
            leadRigidbody.velocity = direction * 3f;
        }
        else if (followMode == FormationMode.TwoLevel)
        {
            ref float maxSpeed = ref FormationManager.instance.maxSpeed;

            // get the average velocity and destination distance of all the agents
            float averageSpeed = 0;
            foreach (FormationAgent agent in FormationManager.instance.agents)
            {
                // if the agent's destination is not viable, ignore
                if (!agent.CanReachDestination())
                {
                    averageSpeed += maxSpeed;
                    continue;
                }

                float distance = Vector3.Distance(agent.transform.position,
                       agent.formationDestination);
                float currentSpeed = agent.agent.velocity.magnitude;

                if (distance < 1.5f)
                {
                    averageSpeed += maxSpeed;
                }
                else
                {
                    //averageSpeed += Mathf.Clamp(currentSpeed, 0f, maxSpeed * .5f);
                    //averageSpeed -= currentSpeed * distance;
                    averageSpeed += currentSpeed / distance;
                }
            }
            averageSpeed /= FormationManager.instance.agents.Count;
            averageSpeed = Mathf.Clamp(averageSpeed, 0f, maxSpeed * .75f);
            leadRigidbody.velocity = direction * averageSpeed;
        }

        if (Vector3.Distance(transform.position, targetPosition) < .2f)
        {
            pathIndex++;
        }
    }

    private void ScalableFormation()
    {
        fov.SetActive(true);

        // shrink the formation radius if an obstacle is found
        fov.viewRadius = FormationManager.instance.radius * 1.5f;
        if (CheckForObstacles())
        {
            FormationManager.instance.radius = Mathf.Max(
                FormationManager.instance.radius - .1f, .1f);
        }
        else
        // expand the formation radius if no obstacle is found
        {
            FormationManager.instance.radius = Mathf.Min(
                FormationManager.instance.radius + .1f, 
                FormationManager.instance.maxRadius * 1.5f);
        }
        
        FollowPath(FormationMode.Scalable);
    }

    private void TwoLevelFormation()
    {
        fov.SetActive(false);
        FollowPath(FormationMode.TwoLevel);
    }

    /// <summary>
    /// Cone-checks in front of the object to detect any obstacles in its way. If an obstacle 
    /// is detected, the object will follow the player's trail instead of following the 
    /// formation path.
    /// </summary>
    private bool CheckForObstacles()
    {
        ref float range = ref fov.viewRadius;

        // find all obstacles within the lead's range
        Collider[] obstaclesInRange = Physics.OverlapSphere(transform.position, range, 
            FormationManager.instance.obstacleMask);

        foreach (Collider collider in obstaclesInRange)
        {
            Transform obstacle = collider.transform;

            // return true if obstacle is within the lead's field of view
            Vector3 direction = (obstacle.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, direction) <= fov.viewAngle / 2)
                return true;
        }

        // no obstacle in lead's field of view
        return false;
    }
}
