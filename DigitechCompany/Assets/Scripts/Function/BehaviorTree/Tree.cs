using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class Tree
    {
        private Node root;

        public Tree(Node _root)
        {
            root = _root;
        }

        public void Update()
        {
            root?.Evaluate();
        }
    }
}

