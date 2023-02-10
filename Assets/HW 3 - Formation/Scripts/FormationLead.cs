using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void FollowPath()
    {
        if (pathIndex >= pathRenderer.positionCount)
        {
            pathIndex = 0;
        }

        Vector3 targetPosition = pathRenderer.GetPosition(pathIndex);
        Vector3 direction = targetPosition - transform.position;
        direction.Normalize();

        // change the lead's rotation to face the next point in the path
        //transform.rotation = Quaternion.LookRotation(direction);
        transform.LookAt(targetPosition);

        // set the lead's velocity to the average velocity of the agents
        float averageVelocity = 0f;
        foreach (FormationAgent agent in FormationManager.instance.agents)
        {
            // get the velocity in the lead's direction
            float agentVelocity = agent.agent.velocity.magnitude;
            if (agentVelocity == 0f)
            {
                agentVelocity = agent.agent.speed;
            }
            averageVelocity += agentVelocity;
        }
        averageVelocity /= FormationManager.instance.agents.Count;
        leadRigidbody.velocity = direction * 3f;

        Debug.DrawLine(transform.position, targetPosition, Color.red);

        if (Vector3.Distance(transform.position, targetPosition) < .2f)
        {
            pathIndex++;
        }
    }

    private void ScalableFormation()
    {
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

        FollowPath();
    }

    private void TwoLevelFormation()
    {
        FollowPath();
    }

    /// <summary>
    /// Raycasts in front of the object to detect any obstacles in its way. If an obstacle 
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
