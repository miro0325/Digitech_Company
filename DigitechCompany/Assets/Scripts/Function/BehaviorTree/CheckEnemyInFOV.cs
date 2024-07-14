using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class CheckEnemyInFOV : Node
{
    private FieldOfView fov;
    private bool isInit = false; 

    public CheckEnemyInFOV(FieldOfView fov, bool isInit = false) : base()
    {
        this.fov = fov;
        this.isInit = isInit;
    }

    public CheckEnemyInFOV(FieldOfView fov, List<Node> children) : base(children)
    {
        this.fov = fov;
    }

    public override NodeState Evaluate()
    {
        if(!isInit)
        {
            object t = GetData("target");
            if (t == null)
            {
                if (fov.TargetUnits.Count > 0)
                {
                    parentNode.parentNode.SetData("target", fov.TargetUnits[0].transform);

                    state = NodeState.Succes;
                    return state;
                }
                state = NodeState.Failure;
                return state;
            }
            state = NodeState.Succes;
            return state;
        }
        else
        {
            if (fov.TargetUnits.Count > 0)
            {
                parentNode.parentNode.SetData("target", fov.TargetUnits[0].transform);

                state = NodeState.Succes;
                return state;
            }
            state = NodeState.Failure;
            return state;
        }
        
    }
}
