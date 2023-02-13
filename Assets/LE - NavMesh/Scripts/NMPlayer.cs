using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NMPlayer : MonoBehaviour
{
    [SerializeField] private GameObject cursorTargetPrefab;
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Color reachableColor;
    [SerializeField] private Color unreachableColor;

    private NavMeshAgent agent;
    private GameObject cursorTarget;
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
            if (cursorTarget != null)
            {
                cursorTarget.transform.position = hit.point;
            }
            else
            {
                cursorTarget = Instantiate(cursorTargetPrefab, hit.point, Quaternion.identity);
            }
        }
        else if (cursorTarget != null)
        {
            Destroy(cursorTarget);
            cursorTarget = null;
        }

        if (Input.GetMouseButtonDown(0) && isHit)
        {
            agent.SetDestination(hit.point);

            if (target != null)
            {
                Destroy(target);
                target = null;
            }

            target = Instantiate(targetPrefab, hit.point, Quaternion.identity);

            if (CanReachDestination())
            {
                target.GetComponent<MeshRenderer>().material.color = reachableColor;
            }
            else
            {
                target.GetComponent<MeshRenderer>().material.color = unreachableColor;
            }
        }
    }

    private bool CanReachDestination()
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(agent.destination, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }
}
