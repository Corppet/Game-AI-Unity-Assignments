using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public class Selector : Node
    {
        public Selector() { }

        public Selector(string name) : base(name) { }

        public override Status Process()
        {
            Status childStatus = children[currentChild].Process();
            switch (childStatus)
            {
                case Status.Success:
                    currentChild = 0;
                    return Status.Success;
                case Status.Failure:
                    currentChild++;
                    if (currentChild >= children.Count)
                    {
                        currentChild = 0;
                        return Status.Failure;
                    }
                    break;
            }

            return Status.Running;
        }
    }
}
