using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Hospital
{
    public class Node
    {
        public Node parent;
        public float cost;
        public Dictionary<string, int> states;
        public GAction action;

        public Node(Node parent, float cost, Dictionary<string, int> states, GAction action)
        {
            this.parent = parent;
            this.cost = cost;
            this.states = new(states);
            this.action = action;
        }
    }

    public class GPlanner
    {
        public Queue<GAction> Plan(List<GAction> actions, Dictionary<string, int> goals, WorldStates states)
        {
            List<GAction> usableActions = new();
            foreach (GAction action in actions)
            {
                if (action.IsAchievable())
                {
                    usableActions.Add(action);
                }
            }

            List<Node> leaves = new();
            Node start = new(null, 0, GWorld.Instance.GetWorld().states, null);

            bool success = BuildGraph(start, leaves, usableActions, goals);

            if (!success)
            {
                Debug.Log("No plan");
                return null;
            }

            Node cheapest = null;
            foreach (Node leaf in leaves)
            {
                if (cheapest is null)
                {
                    cheapest = leaf;
                }
                else
                {
                    if (leaf.cost < cheapest.cost)
                    {
                        cheapest = leaf;
                    }
                }
            }

            List<GAction> result = new();
            Node node = cheapest;
            while (node is not null)
            {
                if (node.action is not null)
                {
                    result.Insert(0, node.action);
                }

                node = node.parent;
            }

            Queue<GAction> queue = new();
            foreach (GAction action in result)
            {
                queue.Enqueue(action);
            }

            Debug.Log("The plan is: ");
            foreach (GAction action in queue)
            {
                Debug.Log("Q: " + action.actionName);
            }

            return queue;
        }

        private bool BuildGraph(Node parent, List<Node> leaves, List<GAction> usableActions, Dictionary<string, int> goals)
        {
            bool foundPath = false;
            foreach (GAction action in usableActions)
            {
                if (action.IsAchievableGiven(parent.states))
                {
                    Dictionary<string, int> currentState = new(parent.states);
                    foreach (KeyValuePair<string, int> effect in action.afterEffectsDict)
                    {
                        if (!currentState.ContainsKey(effect.Key))
                        {
                            currentState.Add(effect.Key, effect.Value);
                        }

                        Node node = new(parent, parent.cost + action.cost, currentState, action);

                        if (GoalAchieved(goals, currentState))
                        {
                            leaves.Add(node);
                            foundPath = true;
                        }
                        else
                        {
                            List<GAction> subset = ActionSubset(usableActions, action);
                            bool found = BuildGraph(node, leaves, subset, goals);
                            if (found)
                            {
                                foundPath = true;   
                            }
                        }
                    }
                }
            }

            return foundPath = true;
        }

        private bool GoalAchieved(Dictionary<string, int> goal, Dictionary<string, int> state)
        {
            foreach (KeyValuePair <string, int> effect in goal)
            {
                if (!state.ContainsKey(effect.Key))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }

        private List<GAction> ActionSubset(List<GAction> actions, GAction removeMe)
        {
            List<GAction> subset = new();
            foreach (GAction action in actions)
            {
                if (!action.Equals(removeMe))
                {
                    subset.Add(action);
                }
            }

            return subset;
        }
    }
}
