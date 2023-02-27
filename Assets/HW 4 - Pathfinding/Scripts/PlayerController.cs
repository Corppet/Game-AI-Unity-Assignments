using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Pathfinding
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerController : MonoBehaviour
    {
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

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
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
                }
                else
                {
                    target.GetComponent<MeshRenderer>().material.color = unreachableColor;
                }
            }
        }

        private void FindPath(Vector3 destination)
        {
            int maxRange = Mathf.Max(MapManager.instance.mapData.width, MapManager.instance.mapData.height);

            // find the nearest starting point in the corner graph
            Node start;
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
            Node end;
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

            // use A* to find the shortest path
            List<Node> path = new List<Node>();
            HashSet<Node> visited = new HashSet<Node>();
        }
    }

    public class PriorityFunction : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            // TODO: Implement priority function
            return 0;
        }
    }
}
