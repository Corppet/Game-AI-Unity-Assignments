using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Node : MonoBehaviour
    {
        [HideInInspector] public HashSet<Node> neighbors;
        [HideInInspector] public bool visited;

        public LayerMask obstacleMask;

        [SerializeField] private GameObject edgePrefab;

        public void FindNeighbors()
        {
            if (visited)
                return;

            visited = true;

            foreach (Node node in MapManager.instance.nodes)
            {
                if (node == this)
                    continue;

                // if the two nodes have line of sight, create an edge
                Vector3 direction = node.transform.position - transform.position;
                RaycastHit hit;
                if (Physics.Raycast(transform.position, direction.normalized, out hit, direction.magnitude, obstacleMask))
                {
                    if (hit.collider.gameObject == node.gameObject)
                    {
                        neighbors.Add(node);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                if (!node.neighbors.Contains(this))
                {
                    LineRenderer edge = Instantiate(edgePrefab, transform).GetComponent<LineRenderer>();
                    edge.positionCount = 2;
                    edge.SetPosition(0, transform.position);
                    edge.SetPosition(1, node.transform.position);
                }
            }
        }

        public void Clear()
        {
            neighbors.Clear();
            visited = false;
        }

        private void Awake()
        {
            neighbors = new HashSet<Node>();
            visited = false;
        }
    }
}
