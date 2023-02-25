using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

namespace BehaviourTrees
{
    public class RobberBehaviour : MonoBehaviour
    {
        public enum ActionState
        {
            Idle,
            Working
        }

        [Header("Settings")]
        [Range(0, 1000)]
        [SerializeField] int diamondValue = 300;
        [Range(0, 1000)]
        [SerializeField] int balance = 800;

        [Space(10)]

        [Header("References")]
        [SerializeField] Transform diamond;
        [SerializeField] Transform van;
        [SerializeField] Transform backdoor;
        [SerializeField] Transform frontdoor;

        [SerializeField] Slider balanceSlider;
        [SerializeField] TMP_Text balanceText;

        BehaviourTree tree;
        NavMeshAgent agent;

        ActionState state;
        Node.Status treeStatus;

        public void UpdateBalance()
        {
            balance = (int)balanceSlider.value;
            balanceText.text = "Balance: " + balance;
        }

        public Node.Status HasMoney()
        {
            if (balance >= 500)
            {
                return Node.Status.Failure;
            }
            else
            {
                return Node.Status.Success;
            }
        }

        public Node.Status GoToDoor(Transform door)
        {
            Node.Status doorStatus = GoToLocation(door.position);
            if (doorStatus == Node.Status.Success)
            {
                if (!door.GetComponent<Lock>().isLocked)
                {
                    door.gameObject.SetActive(false);
                    return Node.Status.Success;
                }
                else
                {
                    return Node.Status.Failure;
                }
            }
            else
            {
                return doorStatus;
            }
        }

        public Node.Status GoToFrontdoor()
        {
            return GoToDoor(frontdoor);
        }

        public Node.Status GoToBackdoor()
        {
            return GoToDoor(backdoor);
        }

        public Node.Status GoToDiamond()
        {
            Node.Status diamondStatus = GoToLocation(diamond.position);
            if (diamondStatus == Node.Status.Success) 
            {
                diamond.parent = transform;
            }

            return diamondStatus;
        }

        public Node.Status GoToVan() 
        {
            Node.Status vanStatus = GoToLocation(van.position);
            if (vanStatus == Node.Status.Success)
            {
                balanceSlider.value += diamondValue;
                diamond.gameObject.SetActive(false);
            }
            return vanStatus;
        }

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            tree = new BehaviourTree();
            state = ActionState.Idle;
            treeStatus = Node.Status.Running;

            // setup slider
            balanceSlider.value = balance;
            balanceText.text = "Balance: " + balance;

            // setup behaviour tree
            Sequence steal = new Sequence("Steal Something");
            Leaf hasMoney = new Leaf("Has Money", HasMoney);
            Leaf goToFrontdoor = new Leaf("Go to Frontdoor", GoToFrontdoor);
            Leaf goToBackdoor = new Leaf("Go to Backdoor", GoToBackdoor);
            Leaf goToDiamond = new Leaf("Go to Diamond", GoToDiamond);
            Leaf goToVan = new Leaf("Go to Van", GoToVan);
            Selector openDoor = new Selector("Open Door");

            openDoor.AddChild(goToFrontdoor);
            openDoor.AddChild(goToBackdoor);

            steal.AddChild(hasMoney);
            steal.AddChild(openDoor);
            steal.AddChild(goToDiamond);
            steal.AddChild(goToVan);
            tree.AddChild(steal);

            tree.PrintTree();
            tree.Process();
        }

        void Update()
        {
            if (treeStatus != Node.Status.Success)
            {
                treeStatus = tree.Process();
            }
        }

        Node.Status GoToLocation(Vector3 destination)
        {
            float distance = Vector3.Distance(destination, transform.position);
            if (state == ActionState.Idle)
            {
                state = ActionState.Working;
                agent.SetDestination(destination);
            }
            else if (Vector3.Distance(agent.pathEndPosition, destination) >= 2)
            {
                state = ActionState.Idle;
                return Node.Status.Failure;
            }
            else if (distance < 2)
            {
                state = ActionState.Idle;
                return Node.Status.Success;
            }

            return Node.Status.Running;
        }
    }
}
