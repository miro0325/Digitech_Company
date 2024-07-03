using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using UnityEngine.AI;

public class TraceTarget : Node
{
    private MonsterBase monster;

    private NavMeshPath path = new NavMeshPath();
    private int cornerIndex = 0;
    private bool isCalculatePath = false;
    private float dist;

    public TraceTarget(MonsterBase monster, float dist = 0.01f) : base()
    {
        this.monster = monster;
        this.dist = dist;
    }

    public TraceTarget(MonsterBase monster, List<Node> children, float dist = 0.01f) : base(children)
    {
        this.monster = monster;
        this.dist = dist;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        if(!isCalculatePath)
        {
            path = new NavMeshPath();
            if(monster.Agent.CalculatePath(target.position, path))
            {
                if(path.corners.Length > 1)
                {
                    isCalculatePath = true;
                }
            }
        }
        if(!isCalculatePath)
        {
            if (Vector3.Distance(monster.transform.position, target.position) > 0.5f)
            {
                monster.Move(target.position);
            }
        } 
        else
        {
            monster.Move(target.position);
            if (Vector3.Distance(monster.transform.position, path.corners[cornerIndex]) < 0.01f)
            {
                if (cornerIndex + 1 >= path.corners.Length)
                {
                    isCalculatePath = false;
                    cornerIndex = 0;
                }
                else
                {
                    cornerIndex++;
                }
            }
            if(Vector3.Distance(monster.transform.position, target.position) < dist)
            {
                state = NodeState.Succes;
                return NodeState.Succes;
            }
        }
        state = NodeState.Running;
        return state;   
    }
}
