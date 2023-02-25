using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTrees
{
    public class Node
    {
        public enum Status
        {
            Success,
            Running,
            Failure
        }

        public string name;
        public Status status;
        public List<Node> children = new List<Node>();
        public int currentChild = 0;

        public Node() { }

        public Node(string name)
        {
            this.name = name;
        }

        public void AddChild(Node node) 
        { 
            children.Add(node);
        }

        public virtual Status Process()
        {
            return children[currentChild].Process();
        }
    }
}
