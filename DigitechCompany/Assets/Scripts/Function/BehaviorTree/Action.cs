using System.Collections.Generic;
using System;

namespace BehaviorTree
{
    public sealed class Action : Node
    {
        Func<NodeState> onUpdate = null;

        public Action(Func<NodeState> onUpdate) : base()
        {
            this.onUpdate = onUpdate;
        }

        public Action(Func<NodeState> onUpdate, List<Node> children) : base(children)
        {
            this.onUpdate = onUpdate;
        }

        public override NodeState Evaluate() => onUpdate?.Invoke() ?? NodeState.Failure;
    }
}

