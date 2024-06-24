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
            foreach(var node in children)
            {
                switch(node.Evaluate())
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

