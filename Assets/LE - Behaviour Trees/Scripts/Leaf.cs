using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public class Leaf : Node
    {
        public delegate Status Tick();
        public Tick ProcessMethod;

        public Leaf() { }

        public Leaf(string name, Tick processMethod)
        {
            this.name = name;
            ProcessMethod = processMethod;
        }

        public override Status Process()
        {
            if (ProcessMethod != null)
            {
                return ProcessMethod();
            }
            else
            {
                return Status.Failure;
            }
        }
    }
}
