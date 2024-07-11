using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using UnityEngine.AI;

public class Patrol : Node
{
    private MonsterBase monster;
    
    private Transform transform;
    private Vector3[] waypoints;

    private int curIndex = 0;
    private int cornerIndex = 0;

    private float delayTime = 1;
    private float curTime = 0;

    private bool isDelay = false;
    private bool isCalculatePath = false;
    private bool isRandomPoint = false;

    private NavMeshPath path = new NavMeshPath();


    public Patrol(MonsterBase monster, Vector3[] waypoints, List<Node> children, bool isRandomPoint = false, float delayTime = 1 ) : base(children)
    {
        this.transform = monster.transform;
        this.monster = monster;
        this.waypoints = waypoints;
        this.isRandomPoint = isRandomPoint;
        this.delayTime = delayTime;
    }

    public Patrol(MonsterBase monster, Vector3[] waypoints, bool isRandomPoint = false, float delayTime = 1) : base()
    {
        this.transform = monster.transform;
        this.monster = monster;
        this.waypoints = waypoints;
        this.isRandomPoint = isRandomPoint;
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
            Vector3 curPoint = waypoints[curIndex];
            if (Vector3.Distance(transform.position, curPoint) < 0.7f)
            {
                transform.position = curPoint;
                isDelay = true;
                if(!isRandomPoint)
                {
                    curIndex = (curIndex + 1) % waypoints.Length;
                }
                else
                {
                    int newIndex = Random.Range(0, waypoints.Length);   
                    while(curIndex == newIndex)
                    {
                        newIndex = Random.Range(0, waypoints.Length);
                    }
                    curIndex = newIndex;
                }
                isCalculatePath = false;
            }
            else
            {
                if(!isCalculatePath)
                {
                    path = new NavMeshPath();
                    monster.Agent.CalculatePath(curPoint, path);
                    cornerIndex = 0;
                    isCalculatePath = true;
                }
                if (path.corners.Length > 0 && cornerIndex < path.corners.Length)
                {
                    monster.Move(path.corners[cornerIndex]);
                    if (Vector3.Distance(transform.position, path.corners[cornerIndex]) < 0.01f)
                    {
                        if(cornerIndex < path.corners.Length - 1) //endpoint
                        {
                            cornerIndex++;
                        }
                    }
                }

            }
        }
        
        state = NodeState.Running;
        return state;
    }
}
