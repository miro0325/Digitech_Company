using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public sealed class Sequence : Node
    {
        public Sequence() : base()
        {

        }

        public Sequence(List<Node> children) : base(children)
        {

        }

        public override NodeState Evaluate()
        {
            bool isRunningInChild = false;
            foreach (var node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.Failure:
                        continue;
                    case NodeState.Running:
                        isRunningInChild = true;
                        return state;
                    case NodeState.Succes:
                        state = NodeState.Succes;
                        return state;
                    default:
                        continue;
                }
            }
            state = isRunningInChild ? NodeState.Running : NodeState.Succes;
            return state;
        }
    }
}

