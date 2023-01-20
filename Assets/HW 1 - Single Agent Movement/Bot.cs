using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BotBehavior
{
    Pursue,
    Evade,
    Wander,
    PathFollow,
    Mix
}

[RequireComponent(typeof(LineRenderer), typeof(NavMeshAgent))]
public class Bot : MonoBehaviour
{
    [Header("Wander Movement")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderDistance = 20f;
    private Vector3 wanderTarget;

    [Space(5)]

    [SerializeField] private float cooldownDuration = 5f;
    [SerializeField] private float targetRange = 10f;

    [Space(5)]

    [Header("Line Renderer")]
    [Range(10, 1000)]
    [SerializeField] private int circleSegments = 100;

    [Space(10)]
    
    [SerializeField] private Transform target;
    private Drive targetDrive;
    private LineRenderer targetRenderer;

    private NavMeshAgent agent;
    private bool isOnCooldown;
    private float originalSpeed;

    private LineRenderer wanderRenderer;
    [SerializeField] private LineRenderer destinationRenderer;

    private BotBehavior behavior;

    public void SetBehavior(BotBehavior newBehavior)
    {
        behavior = newBehavior;
        originalSpeed = agent.speed;
        wanderRenderer.positionCount = 0;
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        wanderRenderer.positionCount = circleSegments + 1;
        wanderRenderer.startWidth = 0.1f;
        wanderRenderer.endWidth = 0.1f;

        float deltaTheta = (2f * Mathf.PI) / circleSegments;
        float theta = 0f;

        for (int i = 0; i < circleSegments + 1; i++)
        {
            Vector3 pos = center + radius * new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta));
            pos.y = 0f;
            wanderRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

    private void DrawDestinationLine(Vector3 location)
    {
        Vector3 selfFlat = new Vector3(transform.position.x, 0f, transform.position.z);
        destinationRenderer.SetPosition(0, selfFlat);
        Vector3 locFlat = new Vector3(location.x, 0f, location.z);
        destinationRenderer.SetPosition(1, locFlat);
    }

    private void DrawTargetRange(float radius)
    {
        targetRenderer.positionCount = circleSegments + 1;
        targetRenderer.startWidth = 0.1f;
        targetRenderer.endWidth = 0.1f;

        float deltaTheta = (2f * Mathf.PI) / circleSegments;
        float theta = 0f;

        for (int i = 0; i < circleSegments + 1; i++)
        {
            Vector3 pos = radius * new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta));
            pos.y = 0f;
            targetRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

    private void Seek(Vector3 location)
    {
        agent.SetDestination(location);
        DrawDestinationLine(location);
    }

    private void Flee(Vector3 location)
    {
        agent.SetDestination(transform.position * 2 - location);
        DrawDestinationLine(location);
    }

    private void Pursue()
    {
        // find the direction the target is looking at and relative angles
        // between the target and the bot
        Vector3 targetDirection = target.position - transform.position;
        float relativeHeading = Vector3.Angle(transform.forward, 
            transform.TransformVector(target.forward));
        float toTargetAngle = Vector3.Angle(transform.forward, 
            transform.TransformVector(targetDirection));

        ref float targetSpeed = ref targetDrive.currentSpeed;
        // target is behind the bot or is not moving
        if ((toTargetAngle > 90f && relativeHeading < 20f) || targetSpeed < 0.01f)
            Seek(target.position);
        // target is in front of the bot and is moving
        else
        {
            float lookAhead = targetDirection.magnitude / (agent.speed + targetSpeed);
            Seek(target.position + target.forward * lookAhead);
        }

        // slow down if the agent is close to the target
        if (targetDirection.magnitude < targetRange)
            agent.speed = originalSpeed * targetDirection.magnitude / targetRange;
        else
            agent.speed = originalSpeed;
    }

    private void Evade()
    {
        // find the direction the target is looking at and relative angles
        // between the target and the bot
        Vector3 targetDirection = target.position - transform.position;
        float relativeHeading = Vector3.Angle(transform.forward,
            transform.TransformVector(target.forward));
        float toTargetAngle = Vector3.Angle(transform.forward,
            transform.TransformVector(targetDirection));

        ref float targetSpeed = ref targetDrive.currentSpeed;
        // target is behind the bot or is not moving
        if ((toTargetAngle > 90f && relativeHeading < 20f) || targetSpeed < 0.01f)
            Flee(target.position);
        // target is in front of the bot and is moving
        else
        {
            float lookAhead = targetDirection.magnitude / (agent.speed + targetSpeed);
            Flee(target.position + target.forward * lookAhead);
        }
    }

    private void Wander()
    {
        Debug.Log(Vector3.Distance(transform.position, wanderTarget));
        
        // if the wander target is not set, set it
        if (wanderTarget == Vector3.zero)
        {
            // find a random point around a circle
            float randomAngle = Random.Range(0f, 2f * Mathf.PI);
            Vector3 randomPoint = new Vector3(Mathf.Cos(randomAngle), 0f, Mathf.Sin(randomAngle)) * wanderRadius;

            // offset the circle by the wander distance
            Vector3 forward = transform.position + transform.forward * wanderDistance;
            wanderTarget = randomPoint + forward;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(wanderTarget, out hit, wanderDistance, 1))
            {
                wanderTarget = hit.position;
                DrawCircle(forward, wanderRadius);
            }
            else
            {
                wanderTarget = Vector3.zero;
                wanderRenderer.positionCount = 0;
            }
        }
        // if the bot is close to the wander target, reset it
        else if (Vector3.Distance(transform.position, wanderTarget) < 2.5f)
            wanderTarget = Vector3.zero;
        // if the wander target is set, seek it
        else
            Seek(wanderTarget);
    }

    private void Hide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hidingSpot = World.Instance.GetHidingSpots()[i].transform.position;
            Vector3 hideDir = hidingSpot - target.position;
            Vector3 hidePos = hidingSpot + hideDir.normalized * 5f;

            if (Vector3.Distance(transform.position, hidePos) < dist)
            {
                dist = Vector3.Distance(transform.position, hidePos);
                chosenSpot = hidePos;
            }
        }

        Seek(chosenSpot);
    }

    private void CleverHide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenObject = World.Instance.GetHidingSpots()[0];

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hidingSpot = World.Instance.GetHidingSpots()[i].transform.position;
            Vector3 hideDir = hidingSpot - target.position;
            Vector3 hidePos = hidingSpot + hideDir.normalized * 5f;

            if (Vector3.Distance(transform.position, hidePos) < dist)
            {
                dist = Vector3.Distance(transform.position, hidePos);
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenObject = World.Instance.GetHidingSpots()[i];
                dist = Vector3.Distance(transform.position, hidePos);
            }
        }

        Collider hideCol = chosenObject.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit info;
        float distance = 100f;
        hideCol.Raycast(backRay, out info, distance);


        Seek(info.point + chosenDir.normalized * 5f);
    }

    private void PathFollow()
    {
        
    }

    private void Mix()
    {
        if (isOnCooldown)
            return;

        if (!TargetInRange())
            Wander();
        else if (CanSeeTarget() && CanSeeMe())
        {
            CleverHide();
            StartCoroutine(StartCooldown());
        }
        else
            Pursue();
    }
    
    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;

        yield return new WaitForSeconds(cooldownDuration);

        isOnCooldown = false;
    }

    private bool CanSeeTarget()
    {
        RaycastHit raycastInfo;
        Vector3 targetDir = target.position - transform.position;

        // check if the agent has line of sight of the target
        float lookAngle = Vector3.Angle(transform.forward, targetDir);
        if (Physics.Raycast(transform.position, targetDir, out raycastInfo)
            && raycastInfo.collider.gameObject == target.gameObject && lookAngle < 60f)
            return true;
        else
            return false;
    }

    private bool CanSeeMe()
    {
        Vector3 targetDir = transform.position - target.position;
        float lookAngle = Vector3.Angle(target.forward, targetDir);
        if (lookAngle < 60f)
            return true;
        else
            return false;
    }

    private bool TargetInRange()
    {
        if (Vector3.Distance(transform.position, target.position) <= targetRange)
            return true;
        else
            return false;
    }
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderRenderer = GetComponent<LineRenderer>();
        targetDrive = target.GetComponent<Drive>();
        targetRenderer = target.GetComponent<LineRenderer>();

        wanderTarget = Vector3.zero;
        destinationRenderer.positionCount = 2;
        originalSpeed = agent.speed;

        DrawTargetRange(targetRange);
    }

    private void Update()
    {
        switch (behavior)
        {
            case BotBehavior.Pursue:
                Pursue();
                break;
            case BotBehavior.Evade:
                Evade();
                break;
            case BotBehavior.Wander:
                Wander();
                break;
            case BotBehavior.PathFollow:
                PathFollow();
                break;
            case BotBehavior.Mix:
                Mix();
                break;
        }
    }
}
