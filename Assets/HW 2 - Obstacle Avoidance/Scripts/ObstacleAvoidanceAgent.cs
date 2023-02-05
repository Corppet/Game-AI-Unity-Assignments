using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

[RequireComponent(typeof(LineRenderer), typeof(NavMeshAgent))]
public class ObstacleAvoidanceAgent : MonoBehaviour
{
    [Header("Pursue Movement")]
    [SerializeField] protected float targetRange = 10f;

    [Space(5)]

    [Header("Wander Movement")]
    [SerializeField] protected float wanderRadius = 10f;
    [SerializeField] protected float wanderDistance = 20f;
    protected Vector3 wanderTarget;

    [Space(5)]

    [Header("Mixed Movement")]
    [SerializeField] protected float cooldownDuration = 5f;
    [SerializeField] protected float fleeDistance = 5f;

    [Space(5)]

    [Header("Line Renderer")]
    [Range(10, 1000)]
    [SerializeField] protected int circleSegments = 36;

    [Space(5)]

    [Header("Collision Detection")]
    [SerializeField] protected float avoidDistance = 3f;
    [SerializeField] protected LayerMask obstacleMask;
    protected bool obstacleDetected;
    protected Vector3 avoidanceTarget;

    [Space(10)]

    [SerializeField] protected Transform target;
    protected PlayerController targetController;
    protected LineRenderer targetRenderer;

    protected NavMeshAgent agent;
    protected bool isOnCooldown;
    protected float originalSpeed;

    protected LineRenderer wanderRenderer;
    [SerializeField] protected LineRenderer destinationRenderer;

    protected FieldOfView fov;

    [SerializeField] protected TMP_Text statusText;

    protected void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderRenderer = GetComponent<LineRenderer>();
        targetController = target.GetComponent<PlayerController>();
        targetRenderer = target.GetComponent<LineRenderer>();
        fov = GetComponent<FieldOfView>();
    }

    protected void Start()
    {
        wanderTarget = Vector3.zero;
        destinationRenderer.positionCount = 2;
        originalSpeed = agent.speed;
    }

    protected void FixedUpdate()
    {
        obstacleDetected = false;
        AvoidWalls();
        ConeCheck();

        if (obstacleDetected)
        {
            StartCoroutine(StartCooldown(cooldownDuration));
        }

        if (isOnCooldown)
        {
            return;
        }

        Seek(target.position);
        statusText.text = "Seeking Target";

        //Debug.Log("Destination: " + agent.destination);
        //Debug.Log("Distance: " + Vector3.Distance(transform.position, agent.destination));
    }

    protected void DrawCircle(Vector3 center, float radius)
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

    protected void DrawDestinationLine(Vector3 location)
    {
        Vector3 selfFlat = new Vector3(transform.position.x, 1.5f, transform.position.z);
        destinationRenderer.SetPosition(0, selfFlat);
        Vector3 locFlat = new Vector3(location.x, 1.5f, location.z);
        destinationRenderer.SetPosition(1, locFlat);
    }

    protected void DrawTargetRange(float radius)
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

    protected void Seek(Vector3 location)
    {
        agent.SetDestination(location);
        DrawDestinationLine(location);
    }

    protected void Flee(Vector3 location)
    {
        Vector3 destination = (transform.position - location) * fleeDistance;

        agent.SetDestination(destination);
        DrawDestinationLine(destination);
    }

    protected void Pursue()
    {
        // find the direction the target is looking at and relative angles
        // between the target and the bot
        Vector3 targetDirection = target.position - transform.position;
        float relativeHeading = Vector3.Angle(transform.forward,
            transform.TransformVector(target.forward));
        float toTargetAngle = Vector3.Angle(transform.forward,
            transform.TransformVector(targetDirection));

        ref float targetSpeed = ref targetController.currentSpeed;
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

    protected void Evade()
    {
        // find the direction the target is looking at and relative angles
        // between the target and the bot
        Vector3 targetDirection = target.position - transform.position;
        float relativeHeading = Vector3.Angle(transform.forward,
            transform.TransformVector(target.forward));
        float toTargetAngle = Vector3.Angle(transform.forward,
            transform.TransformVector(targetDirection));

        ref float targetSpeed = ref targetController.currentSpeed;
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

    protected void Wander()
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

    protected void Hide()
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

    protected void CleverHide()
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

    protected void Mix()
    {
        if (isOnCooldown)
            return;

        if (!TargetInRange())
            Wander();
        else if (CanSeeTarget() && CanSeeMe())
        {
            CleverHide();
            StartCoroutine(StartCooldown(cooldownDuration));
        }
        else
            Pursue();
    }

    /// <summary>
    /// Raycasts forward and checks if a wall is in front.
    /// </summary>
    /// <returns>
    /// True if a wall is found in front, and internally sets the new destination for the 
    /// NavMeshAgent. False otherwise (destination is unchanged).
    /// </returns>
    protected void AvoidWalls()
    {
        ref float range = ref fov.viewRadius;

        // shoot raycasts to the sides and check for any walls
        RaycastHit hit;
        Vector3 left = Quaternion.AngleAxis(-fov.viewAngle / 2f, transform.up) * transform.forward;
        Vector3 right = Quaternion.AngleAxis(fov.viewAngle / 2f, transform.up) * transform.forward;
        
        // check for walls on left
        bool wallOnLeft = false;
        Debug.DrawRay(transform.position, left.normalized * range, Color.blue);
        if (Physics.Raycast(transform.position, left.normalized, out hit, range))
        {
            if (hit.collider.CompareTag("Still Obstacle"))
            {
                // if a wall is found, find a new destination
                wallOnLeft = true;
                obstacleDetected = true;

                // toggle detected on the obstacle
                hit.collider.GetComponent<OAObstacle>().Detected();
            }
        }

        // check for walls on right
        bool wallOnRight = false;
        Debug.DrawRay(transform.position, right.normalized * range, Color.blue);
        if (Physics.Raycast(transform.position, right.normalized, out hit, range))
        {
            if (hit.collider.CompareTag("Still Obstacle"))
            {
                // if a wall is found, find a new destination
                wallOnRight = true;
                obstacleDetected = true;

                // toggle detected on the obstacle
                hit.collider.GetComponent<OAObstacle>().Detected();
            }
        }

        // shoot a raycast forward and check if it hits a wall
        bool wallInFront = false;
        if (Physics.Raycast(transform.position, transform.forward, out hit, range, obstacleMask))
        {
            if (hit.collider.CompareTag("Still Obstacle"))
            {
                // if a wall is found, find a new destination
                wallInFront = true;
                obstacleDetected = true;

                // toggle detected on the obstacle
                hit.collider.GetComponent<OAObstacle>().Detected();
            }
        }

        // adjust the destination based on the raycasts
        if (wallInFront)
        {
            if (wallOnLeft && wallOnRight)
            {
                // if there are walls on both sides, turn around
                Vector3 newDestination = transform.position - transform.forward * range;
                if (NavMesh.SamplePosition(newDestination, out NavMeshHit navHit, range, 1))
                {
                    Seek(navHit.position);
                }

                StartCoroutine(StartCooldown(2f));
            }
            else if (wallOnLeft)
            {
                // if there is a wall on the left, turn right
                Vector3 newDestination = transform.position + transform.right * range;
                if (NavMesh.SamplePosition(newDestination, out NavMeshHit navHit, range, 1))
                {
                    Seek(navHit.position);
                }
            }
            else if (wallOnRight)
            {
                // if there is a wall on the right, turn left
                Vector3 newDestination = transform.position - transform.right * range;
                if (NavMesh.SamplePosition(newDestination, out NavMeshHit navHit, range, 1))
                {
                    Seek(navHit.position);
                }
            }

            statusText.text = "Avoiding Walls";
        }
        else if (wallOnLeft)
        {
            // if there is a wall on the left, turn right
            Vector3 newDestination = transform.position + transform.right * range;
            if (NavMesh.SamplePosition(newDestination, out NavMeshHit navHit, range, 1))
            {
                Seek(navHit.position);
            }

            statusText.text = "Avoiding Walls";
        }
        else if (wallOnRight)
        {
            // if there is a wall on the right, turn left
            Vector3 newDestination = transform.position - transform.right * range;
            if (NavMesh.SamplePosition(newDestination, out NavMeshHit navHit, range, 1))
            {
                Seek(navHit.position);
            }

            statusText.text = "Avoiding Walls";
        }
    }

    /// <summary>
    /// Checks for any moving obstacles in the field of view and avoids them
    /// via collision prediction.
    /// </summary>
    protected void ConeCheck()
    {
        // check for any obstacles within the agent's fov
        Collider[] obstaclesInRange = Physics.OverlapSphere(transform.position, 
            fov.viewRadius, obstacleMask);

        // find the closest moving obstacle
        Collider closest = null;
        float closestDistance = -1f;
        foreach (Collider obstacle in obstaclesInRange)
        {
            Transform obstacleTransform = obstacle.transform;
            Vector3 direction = (obstacleTransform.position - transform.position).normalized;

            // ignore obstacle if out of FOV
            if (Vector3.Angle(transform.forward, direction) > fov.viewAngle / 2f)
                continue;

            // ignore obstacle if it is a non-moving obstacle
            // (handled by AvoidWalls())
            if (obstacle.CompareTag("Still Obstacle"))
                continue;

            float nextDistance = Vector3.Distance(transform.position, obstacleTransform.position);
            if (!closest || nextDistance < closestDistance)
            {
                closest = obstacle;
                closestDistance = nextDistance;
            }
        }

        if (!closest)
            return;

        obstacleDetected = true;

        // toggle detected on the obstacle
        closest.GetComponent<OAObstacle>().Detected();

        // use collision prediction to avoid the closest moving obstacle
        Vector3 closestVelocity = closest.GetComponent<Rigidbody>().velocity;
        Vector3 dp = closest.transform.position - transform.position;
        Vector3 dv = closestVelocity - agent.velocity;
        float t = -Vector3.Dot(dp, dv) / Mathf.Pow(dv.magnitude, 2);

        // find the predicted positions of the target and the agent
        Vector3 targetNextPos = closest.transform.position + closestVelocity * t;
        Vector3 agentNextPos = transform.position + agent.velocity * t;
        float distance = Vector3.Distance(targetNextPos, agentNextPos);

        // adjust avoidance target accordingly
        if (distance > 1f)
            return;

        // evade from the future position
        Vector3 targetDirection = closest.transform.position - transform.position;
        float targetSpeed = closestVelocity.magnitude;
        float lookAhead = targetDirection.magnitude / (agent.speed + targetSpeed);
        Flee(closest.transform.position + closest.transform.forward * lookAhead);
        StartCoroutine(StartCooldown(3f));

        statusText.text = "Avoiding Moving Obstacles";
    }

    protected IEnumerator StartCooldown(float duration)
    {
        isOnCooldown = true;

        yield return new WaitForSeconds(duration);

        isOnCooldown = false;
    }

    protected bool CanSeeTarget()
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

    protected bool CanSeeMe()
    {
        Vector3 targetDir = transform.position - target.position;
        float lookAngle = Vector3.Angle(target.forward, targetDir);
        if (lookAngle < 60f)
            return true;
        else
            return false;
    }

    protected bool TargetInRange()
    {
        if (Vector3.Distance(transform.position, target.position) <= targetRange)
            return true;
        else
            return false;
    }
}
