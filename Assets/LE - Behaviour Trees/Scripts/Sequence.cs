using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public class Sequence : Node
    {
        public Sequence() { }

        public Sequence(string name) : base(name) { }

        public override Status Process()
        {
            Status childStatus = children[currentChild].Process();
            if (childStatus != Status.Success)
            {
                return childStatus;
            }

            currentChild++;
            if (currentChild >= children.Count)
            {
                currentChild = 0;
                return Status.Success;
            }

            return Status.Running;
        }
    }
}
