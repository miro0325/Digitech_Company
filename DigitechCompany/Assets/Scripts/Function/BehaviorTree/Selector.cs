using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public sealed class Selector : Node
    {
        public Selector() : base() { }
        public Selector(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            //Debug.Log("Check Select");
            foreach (var node in children)
            {
                var nodeState = node.Evaluate();
                switch(nodeState)
                {
                    case NodeState.Failure:
                        continue;
                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;
                    case NodeState.Succes:
                        state = NodeState.Succes;
                        return state;
                    default:
                        continue;
                }
            }
            state = NodeState.Failure;
            return state;
        }
    }
}

