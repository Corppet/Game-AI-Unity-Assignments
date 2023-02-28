using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace Pathfinding
{
    public enum HeuristicType
    {
        Distance,
        ManhattanDistance
    }

    public class PlayerController : MonoBehaviour
    {
        [HideInInspector] public float speed;

        [Header("Cursor Settings")]
        [SerializeField] private GameObject cursorTargetPrefab;
        [SerializeField] private GameObject targetPrefab;
        [SerializeField] private Color reachableColor;
        [SerializeField] private Color unreachableColor;

        [Space(5)]

        [Header("Pathfinding Settings")]
        [SerializeField] private LayerMask graphMask;
        [SerializeField] private LayerMask obstacleMask;
        [HideInInspector] public HeuristicType heuristic;
        [HideInInspector] public float heuristicWeight;
        [HideInInspector] public float searchDelay;


        private GameObject cursorTarget;
        private MeshRenderer cursorRenderer;
        private bool isReachable;

        private bool isSearching;
        private Coroutine searchCoroutine;
        private GameObject target;
        private List<Vector3> targetPath;
        private int pathIndex;

        private void Awake()
        {
            isSearching = false;
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
                    if (isSearching)
                    {
                        StopCoroutine(searchCoroutine);
                        isSearching = false;
                    }
                    searchCoroutine = StartCoroutine(FindPath(hit.point));
                }
                else
                {
                    target.GetComponent<MeshRenderer>().material.color = unreachableColor;
                }
            }

            if (!isSearching && targetPath != null && pathIndex < targetPath.Count)
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

        private IEnumerator FindPath(Vector3 destination)
        {
            isSearching = true;

            MapManager map = MapManager.instance;
            int maxRange = Mathf.Max(map.mapData.width, map.mapData.height);

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

                    break;
                }
            }

            // clear all the search edges
            foreach (Transform child in map.graphSettings.searchParent)
            {
                Destroy(child.gameObject);
            }

            // use A* to find the shortest path
            List<Node> minPath = new List<Node>();
            HashSet<Node> visited = new HashSet<Node>();
            PriorityQueue<List<Node>, float> fringe = new PriorityQueue<List<Node>, float>();

            List<Node> startPath = new List<Node>(new Node[] { start });
            fringe.Enqueue(startPath, PriorityFunction(startPath, end));
            while (fringe.Count > 0)
            {
                yield return new WaitForSeconds(searchDelay);

                List<Node> path = fringe.Dequeue();
                Node node = path[path.Count - 1];

                // if there is an edge in the path, render the last one
                if (path.Count > 1)
                {
                    GraphSettings gs = map.graphSettings;
                    LineRenderer edge = Instantiate(gs.searchEdgePrefab, gs.searchParent)
                        .GetComponent<LineRenderer>();
                    edge.positionCount = 2;
                    edge.SetPosition(0, path[path.Count - 2].transform.position);
                    edge.SetPosition(1, node.transform.position);
                }

                // Special Case: Current node can see the goal
                //               or the goal is the last node in the path
                Vector3 direction = destination - node.transform.position;
                RaycastHit hit;
                if (!Physics.Raycast(node.transform.position, direction.normalized, out hit, direction.magnitude, 
                    obstacleMask | graphMask) || node == end)
                {
                    minPath = path;
                    break;
                }

                if (visited.Contains(node))
                    continue;

                foreach (Node next in node.neighbors)
                {
                    List<Node> newPath = new List<Node>(path)
                    {
                        next
                    };
                    fringe.Enqueue(newPath, PriorityFunction(newPath, end));
                }

                visited.Add(node);
            }

            // parse the Node-path into a Vector3-path
            List<Vector3> points = new List<Vector3>
            {
                transform.position
            };
            foreach (Node node in minPath)
            {
                points.Add(node.transform.position);
            }
            points.Add(destination);

            // flatten the path to the ground + pathOffset
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Vector3(
                    points[i].x,
                    map.mapTiles.cellSize.y + map.mapTiles.groundMap.tileAnchor.y
                    + map.graphSettings.pathOffset,
                    points[i].z);
            }

            targetPath = points;
            pathIndex = 0;

            DrawPath();

            isSearching = false;
        }

        private void FollowPath()
        {
            Vector3 destination = new Vector3(targetPath[pathIndex].x,
                transform.position.y, 
                targetPath[pathIndex].z);
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            if (Vector3.Distance(destination, transform.position) < .01f)
            {
                pathIndex++;
            }
        }

        private void DrawPath()
        {
            LineRenderer renderer = MapManager.instance.graphSettings.pathRenderer;
            renderer.positionCount = targetPath.Count;
            for (int i = 0; i < targetPath.Count; i++)
            {
                renderer.SetPosition(i, targetPath[i]);
            }
        }
    }
}
