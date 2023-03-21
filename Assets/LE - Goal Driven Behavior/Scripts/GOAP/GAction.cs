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
        public float duration = 0;
        public WorldState[] preconditions;
        public WorldState[] afterEffects;
        public NavMeshAgent agent;

        public Dictionary<string, int> preconditionsDict;
        public Dictionary<string, int> afterEffectsDict;

        public WorldStates agentBeliefs;

        public GInventory inventory;
        public WorldStates beliefs;

        public bool running = false;

        public GAction()
        {
            preconditionsDict = new();
            afterEffectsDict = new();
        }

        public void Awake()
        {
            agent = GetComponent<NavMeshAgent>();

            if (preconditions != null)
                foreach (WorldState w in preconditions)
                {
                    preconditionsDict.Add(w.key, w.value);
                }

            if (afterEffects != null)
                foreach (WorldState w in afterEffects)
                {
                    afterEffectsDict.Add(w.key, w.value);
                }

            inventory = GetComponent<GAgent>().inventory;
            beliefs = GetComponent<GAgent>().beliefs;
        }

        public bool IsAchievable()
        {
            return true;
        }

        public bool IsAchievableGiven(Dictionary<string, int> conditions)
        {
            foreach (KeyValuePair<string, int> p in preconditionsDict)
            {
                if (!conditions.ContainsKey(p.Key))
                    return false;
            }
            return true;
        }

        public abstract bool PrePerform();
        public abstract bool PostPerform();
    }
}
