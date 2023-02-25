using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public class BehaviourTree : Node
    {
        public BehaviourTree() 
        {
            name = "Tree";
        }

        public BehaviourTree(string name)
        {
            this.name = name;
        }

        public override Status Process()
        {
            return children[currentChild].Process();
        }

        struct NodeLevel
        {
            public Node node;
            public int level;

            public NodeLevel(Node node, int level)
            {
                this.node = node;
                this.level = level;
            }
        }

        public void PrintTree()
        {
            string output = string.Empty;
            Stack<NodeLevel> stack = new Stack<NodeLevel>();
            stack.Push(new NodeLevel(this, 0));

            while (stack.Count > 0)
            {
                NodeLevel current = stack.Pop();
                Node curNode = current.node;
                output += new string('-', current.level) + curNode.name + "\n";

                for (int i = curNode.children.Count - 1; i >= 0; i--)
                {
                    stack.Push(new NodeLevel(curNode.children[i], current.level + 1));
                }
            }

            Debug.Log(output);
        }
    }
}
