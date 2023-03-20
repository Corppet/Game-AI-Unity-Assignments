using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Hospital
{
    public abstract class GAction : MonoBehaviour
    {
        public string actionName = "Action";
        public float cost = 1.0f;
        public GameObject target;
        public string targetTag;
        public float duration = 0f;
        public WorldState[] preconditions;
        public WorldState[] afterEffects;
        public NavMeshAgent agent;

        public Dictionary<string, int> preconditionsDict;
        public Dictionary<string, int> afterEffectsDict;

        public WorldStates agentBeliefs;

        public bool running = false;

        public GAction() 
        {
            preconditionsDict = new();
            afterEffectsDict = new();
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();

            if (preconditions is not null)
            {
                foreach (WorldState state in preconditions)
                {
                    preconditionsDict.Add(state.key, state.value);
                }
            }

            if (afterEffects is not null)
            {
                foreach (WorldState state in afterEffects)
                {
                    afterEffectsDict.Add(state.key, state.value);
                }
            }

        }

        public bool IsAchievable()
        {
            return true;
        }

        public bool IsAchievableGiven(Dictionary<string, int> conditions)
        {
            foreach (KeyValuePair<string, int> kvp in conditions)
            {
                if (!conditions.ContainsKey(kvp.Key))
                {
                    return false;
                }
            }

            return true;
        }

        public abstract bool PrePerform();
        public abstract bool PostPerform();
    }
}
