using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Hospital
{
    public class SubGoal
    {
        public Dictionary<string, int> sgoals;
        public bool remove;

        public SubGoal(string s, int i, bool r)
        {
            sgoals = new()
            {
                { s, i }
            };
            remove = r;
        }
    }

    public class GAgent : MonoBehaviour
    {
        public List<GAction> actions = new();
        public Dictionary<SubGoal, int> goals = new();
        public GAction currentAction;

        private GPlanner planner;
        private Queue<GAction> actionQueue;
        SubGoal currentGoal;

        protected virtual void Start()
        {
            GAction[] acts = GetComponents<GAction>();
            foreach (GAction a in acts)
            {
                actions.Add(a);
            }
        }

        private bool invoked = false;
        private void CompleteAction()
        {
            currentAction.running = false;
            currentAction.PostPerform();
            invoked = false;
        }

        private void LateUpdate()
        {
            if (currentAction is not null && currentAction.running)
            {
                float distanceToTarget = Vector3.Distance(transform.position, currentAction.target.transform.position);
                if (currentAction.agent.hasPath && distanceToTarget < 2f)
                {
                    if (!invoked)
                    {
                        Invoke("CompleteAction", currentAction.duration);
                        invoked = true;
                    }
                }

                return;
            }

            if (planner is null || actionQueue is null)
            {
                planner = new();

                var sortedGoals = from entry in goals orderby entry.Value descending select entry;

                foreach(KeyValuePair<SubGoal, int> pair in sortedGoals)
                {
                    actionQueue = planner.Plan(actions, pair.Key.sgoals, null);
                    if (actionQueue is not null)
                    {
                        currentGoal = pair.Key;
                        break;
                    }
                }
            }

            if (actionQueue is not null && actionQueue.Count == 0)
            {
                if (currentGoal.remove)
                {
                    goals.Remove(currentGoal);
                }

                planner = null;
            }
            else if (actionQueue is not null && actionQueue.Count > 0)
            {
                currentAction = actionQueue.Dequeue();
                if (currentAction.PrePerform())
                {
                    if (currentAction.target is null && currentAction.targetTag != string.Empty)
                    {
                        currentAction.target = GameObject.FindWithTag(currentAction.targetTag);
                    }

                    if (currentAction.targetTag is not null)
                    {
                        currentAction.running = true;
                        currentAction.agent.SetDestination(currentAction.target.transform.position);
                    }
                }
                else
                {
                    actionQueue = null;
                }
            }
        }
    }
}
