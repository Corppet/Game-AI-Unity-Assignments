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
        transform.rotation = Quaternion.LookRotation(direction);

        // set the lead's velocity to the average velocity of the agents
        float averageVelocity = 0f;
        foreach (FormationAgent agent in FormationManager.instance.agents)
        {
            averageVelocity += agent.agent.velocity.magnitude;
        }
        averageVelocity /= FormationManager.instance.agents.Count;
        leadRigidbody.velocity = direction * averageVelocity;

        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            pathIndex++;
        }
    }

    private void ScalableFormation()
    {
        FollowPath();
    }

    private void TwoLevelFormation()
    {
        FollowPath();
    }
}
