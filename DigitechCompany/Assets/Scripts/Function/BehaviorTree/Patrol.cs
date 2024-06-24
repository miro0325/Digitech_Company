using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using UnityEngine.AI;

public class Patrol : Node
{
    private MonsterBase monster;
    
    private Transform transform;
    private Transform[] waypoints;

    private int curIndex = 0;
    private int cornerIndex = 0;

    private float delayTime = 1;
    private float curTime = 0;

    private bool isDelay = false;
    private bool isCalculatePath = false;

    private NavMeshPath path = new NavMeshPath();


    public Patrol(MonsterBase monster, Transform[] waypoints, List<Node> children,float delayTime = 1) : base(children)
    {
        this.transform = monster.transform;
        this.monster = monster;
        this.waypoints = waypoints;
        this.delayTime = delayTime;
    }

    public Patrol(MonsterBase monster, Transform[] waypoints, float delayTime = 1) : base()
    {
        this.transform = monster.transform;
        this.monster = monster;
        this.waypoints = waypoints;
        this.delayTime = delayTime;
    }

    public override NodeState Evaluate()
    {
        if(isDelay)
        {
            curTime += Time.deltaTime;
            if(curTime < delayTime)
            {
                return NodeState.Running;
            }
            isDelay = false;
            curTime = 0;
            
        }
        else
        {
            Transform curPoint = waypoints[curIndex];
            if (Vector3.Distance(transform.position, curPoint.position) < 0.01f)
            {
                transform.position = curPoint.position;
                isDelay = true;
                curIndex = (curIndex + 1) % waypoints.Length;
                cornerIndex = 0;
            }
            else
            {
                if(!isCalculatePath)
                {
                    path = new NavMeshPath();
                    monster.Agent.CalculatePath(curPoint.position, path);
                    isCalculatePath = true;
                }
                //Debug.Log(path.corners.Length + " " + curPoint.position);
                if (path.corners.Length > 0 && cornerIndex < path.corners.Length)
                {
                    Debug.Log("test");
                    monster.Agent.SetDestination(path.corners[cornerIndex]);
                    if (Vector3.Distance(transform.position, path.corners[cornerIndex]) < 0.01f)
                    {
                        if(cornerIndex+1>=path.corners.Length)
                        {
                            isCalculatePath=false;
                            cornerIndex = 0;
                        }
                        else
                        {
                            cornerIndex++;
                        }
                    }
                }

            }
        }
        
        state = NodeState.Running;
        return base.Evaluate();
    }
}
