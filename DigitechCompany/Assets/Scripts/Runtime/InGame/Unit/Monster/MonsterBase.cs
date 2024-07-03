using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public abstract class MonsterBase : UnitBase
{
    [SerializeField]
    protected FieldOfView fov;
    [SerializeField]
    protected Transform[] waypoints;
    [SerializeField]
    protected float tempSpeed;
    [SerializeField]
    protected Animator animator;

    protected Stats testBaseStat;
    protected NavMeshAgent agent;
    protected NavMeshPath path;
    protected BehaviorTree.Tree tree;
    protected Vector3 destination;

    public override Stats BaseStats => testBaseStat;
    public NavMeshAgent Agent => agent;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = tempSpeed;
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
        Spawn();
    }

    protected virtual void Update()
    {
        tree.Update();
    }

    protected bool RotateToDir(Vector3 targetPos, float betweenAngle = 10f)
    {
        Vector2 forward = new Vector2(transform.position.z, transform.position.x);
        Vector2 steeringTarget = new Vector2(targetPos.z, targetPos.x);

        Vector2 dir = steeringTarget - forward;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        //transform.eulerAngles = Vector3.up * angle;
        if (Quaternion.Angle(Quaternion.Euler(Vector3.up * angle), transform.rotation) < betweenAngle)
        {
            agent.speed = tempSpeed;
            Debug.Log("Rotate Complete");
            return true;
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(Quaternion.Euler(transform.eulerAngles), Quaternion.Euler(Vector3.up * angle), Time.deltaTime * agent.angularSpeed);
            agent.speed = 0;
            return false;
        }
    }

    protected abstract void Spawn();

    public virtual void Move(Vector3 targetPos)
    {
        if(RotateToDir(targetPos))
            agent.SetDestination(targetPos);
    }

    protected abstract void Attack();

    protected abstract void Death();

    public Vector3 GetNavMeshPosition(Vector3 pos, NavMeshHit navMeshHit = default(NavMeshHit), float sampleRadius = 5f, int areaMask = -1)
    {
        if (NavMesh.SamplePosition(pos, out navMeshHit, sampleRadius, areaMask))
        {
            return navMeshHit.position;
        }
        return pos;
    }

    public bool SetDestinationToPosition(Vector3 position, bool checkForPath = false)
    {
        NavMeshHit hit = default(NavMeshHit);
        if (checkForPath)
        {
            position = GetNavMeshPosition(position, hit, 1.75f);
            path = new NavMeshPath();
            if (!agent.CalculatePath(position, path))
            {
                return false;
            }
            if (Vector3.Distance(path.corners[path.corners.Length - 1], GetNavMeshPosition(position, hit, 2.7f)) > 1.55f)
            {
                return false;
            }
        }
        
        destination = GetNavMeshPosition(position, hit, -1f);
        return true;
    }

}
