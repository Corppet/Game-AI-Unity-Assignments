using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Pathfinding
{
    public enum HeuristicType
    {
        Distance,
        ManhattanDistance
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerController : MonoBehaviour
    {
        [HideInInspector] public HeuristicType heuristic;

        [Range(0f, 1f)]
        public float heuristicWeight = .5f;

        [SerializeField] private GameObject cursorTargetPrefab;
        [SerializeField] private GameObject targetPrefab;
        [SerializeField] private Color reachableColor;
        [SerializeField] private Color unreachableColor;

        [Space(5)]

        [SerializeField] private LayerMask graphMask;
        [SerializeField] private LayerMask obstacleMask;

        private NavMeshAgent agent;

        private GameObject cursorTarget;
        private MeshRenderer cursorRenderer;
        private bool isReachable;

        private GameObject target;
        private List<Vector3> targetPath;
        private int pathIndex;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();

            heuristic = HeuristicType.Distance;
        }

        private void Update()
        {
            RaycastHit hit;
            bool isHit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
            if (isHit)
            {
                if (hit.collider.CompareTag("Platform"))
                {
                    isReachable = true;
                }
                else
                {
                    isReachable = false;
                }

                if (cursorTarget != null)
                {
                    cursorTarget.transform.position = hit.point;
                    if (isReachable)
                    {
                        cursorRenderer.material.color = reachableColor;
                    }
                    else
                    {
                        cursorRenderer.material.color = unreachableColor;
                    }
                }
                else
                {
                    cursorTarget = Instantiate(cursorTargetPrefab, hit.point, Quaternion.identity);
                    cursorRenderer = cursorTarget.GetComponent<MeshRenderer>();
                }
            }
            else if (cursorTarget != null)
            {
                Destroy(cursorTarget);
                cursorRenderer = null;
                cursorTarget = null;
            }

            if (Input.GetMouseButtonDown(0) && isHit)
            {
                if (target != null)
                {
                    Destroy(target);
                    target = null;
                }

                target = Instantiate(targetPrefab, hit.point, Quaternion.identity);

                if (isReachable)
                {
                    target.GetComponent<MeshRenderer>().material.color = reachableColor;
                    FindPath(hit.point);
                    MapManager.instance.DrawPath(targetPath);
                }
                else
                {
                    target.GetComponent<MeshRenderer>().material.color = unreachableColor;
                }
            }

            if (targetPath != null && pathIndex < targetPath.Count)
            {
                FollowPath();
            }
        }

        private float GetPathCost(List<Node> path)
        {
            float cost = 0;
            for (int i = 1;  i < path.Count; i++)
            {
                Node node = path[i];
                Node prev = path[0];

                cost += Vector3.Distance(node.transform.position, prev.transform.position);
            }
            return cost;
        }

        private float PriorityFunction(List<Node> path, Node goal)
        {
            Node last = path[path.Count - 1];
            Vector3 lastPos = last.transform.position;
            Vector3 goalPos = goal.transform.position;

            switch (heuristic)
            {
                case HeuristicType.Distance:
                    return (1f - heuristicWeight) * GetPathCost(path) + heuristicWeight * Vector3.Distance(lastPos, goalPos);
                case HeuristicType.ManhattanDistance:
                    Vector3 manDist = goalPos - lastPos;
                    return (1f - heuristicWeight) * GetPathCost(path) + heuristicWeight * (manDist.x + manDist.y + manDist.z);
            }

            return 0f;
        }

        private void FindPath(Vector3 destination)
        {
            int maxRange = Mathf.Max(MapManager.instance.mapData.width, MapManager.instance.mapData.height);

            // find the nearest starting point in the corner graph
            Node start = null;
            float minDist = Mathf.Infinity;
            for (int i = 10; i < maxRange + 10; i += 10)
            {
                Collider[] nodes = Physics.OverlapSphere(transform.position, i, graphMask);
                if (nodes.Length > 0)
                {
                    foreach (Collider node in nodes)
                    {
                        // ignore if node isn't in line of sight
                        Vector3 direction = node.transform.position - transform.position;
                        if (Physics.Raycast(transform.position, direction.normalized, direction.magnitude, obstacleMask))
                            continue;

                        if (direction.magnitude < minDist)
                        {
                            start = node.GetComponent<Node>();
                            minDist = direction.magnitude;
                        }
                    }

                    Debug.Log("Found start node");
                    break;
                }
            }

            // find the nearest ending point in the corner graph
            Node end = null;
            minDist = Mathf.Infinity;
            for (int i = 10; i < maxRange + 10; i += 10)
            {
                Collider[] nodes = Physics.OverlapSphere(destination, i, graphMask);
                if (nodes.Length > 0)
                {
                    foreach (Collider node in nodes)
                    {
                        // ignore if node isn't in line of sight
                        Vector3 direction = node.transform.position - destination;
                        if (Physics.Raycast(destination, direction.normalized, direction.magnitude, obstacleMask))
                            continue;

                        if (direction.magnitude < minDist)
                        {
                            end = node.GetComponent<Node>();
                            minDist = direction.magnitude;
                        }
                    }

                    Debug.Log("Found end node");
                    break;
                }
            }

            // use A* to find the shortest path
            List<Node> minPath = new List<Node>();
            HashSet<Node> visited = new HashSet<Node>();
            PriorityQueue<List<Node>, float> fringe = new PriorityQueue<List<Node>, float>();

            List<Node> startPath = new List<Node>(new Node[] { start });
            fringe.Enqueue(startPath, PriorityFunction(startPath, end));
            while (fringe.Count > 0)
            {
                List<Node> path = fringe.Dequeue();
                Node node = path[path.Count - 1];

                // current node is the goal
                if (node == end)
                {
                    minPath = path;
                    break;
                }

                if (visited.Contains(node))
                    continue;

                foreach (Node next in node.neighbors)
                {
                    List<Node> newPath = new List<Node>(path);
                    newPath.Add(next);
                    fringe.Enqueue(newPath, PriorityFunction(newPath, end));
                }

                visited.Add(node);
            }

            // parse the Node-path into a Vector3-path
            List<Vector3> points = new List<Vector3>();
            points.Add(transform.position);
            foreach (Node node in minPath)
            {
                points.Add(node.transform.position);
            }
            points.Add(destination);

            targetPath = points;
            pathIndex = 0;
        }

        private void FollowPath()
        {
            Debug.Log(Vector3.Distance(agent.destination, transform.position));
            agent.SetDestination(targetPath[pathIndex]);
            if (Vector3.Distance(agent.destination, transform.position) < 1.5f)
            {
                pathIndex++;
                Debug.Log(pathIndex);
            }
        }
    }
}
