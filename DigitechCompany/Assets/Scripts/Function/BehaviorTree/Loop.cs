using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

namespace BehaviorTree
{
    public class Loop : Node
    {
        public Loop(List<Node> children) : base(children)
        {

        }

        public override NodeState Evaluate()
        {
            Debug.Log("Check Loop");
            bool isRunningInChild = false;
            foreach (var node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.Running:
                        isRunningInChild = true;
                        continue;
                    default:
                        state = NodeState.Succes;
                        continue;
                }
            }
            state = isRunningInChild ? NodeState.Running : NodeState.Succes;
            return state;
        }
    }
}

