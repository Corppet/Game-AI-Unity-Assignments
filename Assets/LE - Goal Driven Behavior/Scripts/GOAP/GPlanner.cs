using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hospital
{
    public class Node
    {
        public Node parent;
        public float cost;
        public Dictionary<string, int> state;
        public GAction action;

        public Node(Node parent, float cost, Dictionary<string, int> allStates, GAction action)
        {
            this.parent = parent;
            this.cost = cost;
            state = new(allStates);
            this.action = action;
        }

        public Node(Node parent, float cost, Dictionary<string, int> allStates, 
            Dictionary<string, int> beliefStates, GAction action)
        {
            this.parent = parent;
            this.cost = cost;
            state = new(allStates);
            foreach (KeyValuePair<string, int> b in beliefStates)
            {
                if (!state.ContainsKey(b.Key))
                    state.Add(b.Key, b.Value);
            }
            this.action = action;
        }

    }

    public class GPlanner
    {
        public Queue<GAction> plan(List<GAction> actions, Dictionary<string, int> goal, WorldStates beliefStates)
        {
            List<GAction> usableActions = new();
            foreach (GAction a in actions)
            {
                if (a.IsAchievable())
                    usableActions.Add(a);
            }

            List<Node> leaves = new();
            Node start = new(null, 0, GWorld.Instance.GetWorld().GetStates(), 
                beliefStates.GetStates(), null);

            bool success = BuildGraph(start, leaves, usableActions, goal);

            if (!success)
            {
                Debug.Log("NO PLAN");
                return null;
            }

            Node cheapest = null;
            foreach (Node leaf in leaves)
            {
                if (cheapest == null)
                    cheapest = leaf;
                else
                {
                    if (leaf.cost < cheapest.cost)
                        cheapest = leaf;
                }
            }

            List<GAction> result = new();
            Node n = cheapest;
            while (n != null)
            {
                if (n.action != null)
                {
                    result.Insert(0, n.action);
                }
                n = n.parent;
            }

            Queue<GAction> queue = new();
            foreach (GAction a in result)
            {
                queue.Enqueue(a);
            }

            Debug.Log("The Plan is: ");
            foreach (GAction a in queue)
            {
                Debug.Log("Q: " + a.actionName);
            }

            return queue;
        }

        private bool BuildGraph(Node parent, List<Node> leaves, List<GAction> usuableActions, Dictionary<string, int> goal)
        {
            bool foundPath = false;
            foreach (GAction action in usuableActions)
            {
                if (action.IsAchievableGiven(parent.state))
                {
                    Dictionary<string, int> currentState = new(parent.state);
                    foreach (KeyValuePair<string, int> eff in action.afterEffectsDict)
                    {
                        if (!currentState.ContainsKey(eff.Key))
                            currentState.Add(eff.Key, eff.Value);
                    }

                    Node node = new Node(parent, parent.cost + action.cost, currentState, action);

                    if (GoalAchieved(goal, currentState))
                    {
                        leaves.Add(node);
                        foundPath = true;
                    }
                    else
                    {
                        List<GAction> subset = ActionSubset(usuableActions, action);
                        bool found = BuildGraph(node, leaves, subset, goal);
                        if (found)
                            foundPath = true;
                    }
                }
            }
            return foundPath;
        }

        private bool GoalAchieved(Dictionary<string, int> goal, Dictionary<string, int> state)
        {
            foreach (KeyValuePair<string, int> g in goal)
            {
                if (!state.ContainsKey(g.Key))
                    return false;
            }
            return true;
        }

        private List<GAction> ActionSubset(List<GAction> actions, GAction removeMe)
        {
            List<GAction> subset = new();
            foreach (GAction a in actions)
            {
                if (!a.Equals(removeMe))
                    subset.Add(a);
            }
            return subset;
        }
    }
}
