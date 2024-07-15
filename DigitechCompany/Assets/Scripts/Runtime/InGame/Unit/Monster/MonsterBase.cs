using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using Photon.Pun;

public abstract class MonsterBase : UnitBase, IPunObservable
{
    protected static int Animator_AttackHash = Animator.StringToHash("Attack");
    protected static int Animator_DamagedHash = Animator.StringToHash("Damaged");
    protected static int Animator_DeathHash = Animator.StringToHash("Death");

    public override Stats BaseStats => testBaseStat;
    public NavMeshAgent Agent => agent;

    [Header("Monstar Basic Setting"),Space(20)]
    [SerializeField]
    protected FieldOfView fov;
    [SerializeField]
    protected List<Vector3> waypoints = new();
    [SerializeField]
    protected float tempSpeed;
    [SerializeField]
    protected Animator animator;

    [Header("Monstar Attack Setting"), Space(20)]
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float attackRadius;
    [SerializeField] protected float attackRange;
    [Space(5)][SerializeField] protected float attackDamage;
 
    public int curIndex = -1;

    protected NavMeshAgent agent;
    protected NavMeshPath path;
    protected BehaviorTree.Tree tree;
    protected Stats testBaseStat = new();
    protected Vector3 destination;
    protected Vector3 receivePos;
    protected Vector3 targetPos;
    protected Quaternion receiveRot;
    protected Quaternion lastRotation;
    protected float rotationThreshold = 0.1f;
    protected float delayTime;
    protected float curDelayTime = 0;
    protected bool isAttacking = false;
    protected bool isDeath = false;
    protected bool isCalled = false;

    protected virtual void Start()
    {
        
        agent = GetComponent<NavMeshAgent>();
        if (!photonView.IsMine)
        {
            agent.enabled = false;
        }
        else
        {
            agent.speed = tempSpeed;
            path = new NavMeshPath();
        }
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
        Spawn();
    }

    public virtual void Inititalize(Transform[] waypoints)
    {
        this.waypoints = waypoints.Select(x => x.position).ToList();
        if (!photonView.IsMine) return;
        foreach(var waypoint in waypoints)
        {
            photonView.RPC(nameof(InitRPC), RpcTarget.Others, waypoint.position);
        }
    }

    [PunRPC]
    protected virtual void InitRPC(Vector3 point)
    {
        
        Debug.Log(point + " " + photonView.ViewID);
        if(gameObject != null)
            waypoints.Add(point);
    }

    protected virtual void Update()
    {
        //if (!photonView.IsMine) return;
        tree.Update();
    }

    protected void FixTransform()
    {
        if(!photonView.IsMine)
        {
            Debug.Log(receivePos);
            transform.position = Vector3.Lerp(transform.position, receivePos, 10 * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, receiveRot, 10 * Time.deltaTime);
        }
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
            //Debug.Log("Rotate Complete");
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
        if (SetDestinationToPosition(targetPos))
        {
            agent.SetDestination(destination);
        }
        else
        {
            Debug.LogError($"Can't Go To Destination");
        }
    }

    protected virtual void Attack()
    {
        //photonView.RPC(nameof(AttackRPC), RpcTarget.Others);
    }

    [PunRPC]
    protected abstract void AttackRPC();

    protected override void Death()
    {
        base.Death();
        animator.SetTrigger(Animator_DeathHash);
        agent.isStopped = true;
        isDeath = true;
    }

    [PunRPC]
    protected override void DeathRPC()
    {
        //animator.SetTrigger(Animator_DeathHash);
        agent.isStopped = true;
        isDeath = true;
    }

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
        NavMeshHit hit = default;
        if (checkForPath)
        {
            position = GetNavMeshPosition(position, hit, 1.75f);
            path.ClearCorners();
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

    protected NodeState CheckDeath(bool reverse = false)
    {
        NodeState state;
        if (isDeath) state = reverse ? NodeState.Succes : NodeState.Failure;
        else state = reverse ? NodeState.Failure : NodeState.Succes;
        return state;
    }

    protected NodeState IsCalled()
    {
        return (isCalled) ? NodeState.Succes : NodeState.Failure;
    } 

    protected virtual NodeState GoToCalledPos()
    {
        Vector3 finalPos;
        while(true)
        {
            if(SetRandomPosition(targetPos, out finalPos, 3))
            {
                break;
            }
        }
        if(Vector3.Distance(transform.position,finalPos) > 0.6f)
        {
            if (SetDestinationToPosition(finalPos))
            {
                Move(destination);
            }
            return NodeState.Running;
        }
        return NodeState.Succes;
    }

    //�� �߰� �� ���� ����
    protected virtual NodeState DetectDoor()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f,LayerMask.GetMask("Door"));
        foreach(Collider c in hits)
        {
            if(c.TryGetComponent(out Door door))
            {
                if(!door.IsOpen)
                {
                    door.OnInteract(this);
                    return NodeState.Succes;
                }
            }
        }
        return NodeState.Running;
    }

    public override void Damage(float damage,UnitBase attacker)
    {
        if (curStats.GetStat(Stats.Key.Hp) <= 0) return;
        
        base.Damage(damage, attacker);
        animator.SetTrigger(Animator_DamagedHash);
        
    }

    //���͸� Ư�� ��ġ�� �̵���Ų��
    public virtual void CallMonsterToPos(Vector3 pos, InGamePlayer targetPlayer = null)
    {
        targetPos = pos;
        isCalled = true;
        delayTime = Random.Range(1.5f, 4.5f);
        curDelayTime = 0;
    }

    //���� NavMesh ��ġ�� ���Ѵ�
    protected bool SetRandomPosition(Vector3 pos, out Vector3 finalPosition, float radius, int layerMask = -1)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, layerMask))
        {
            finalPosition = hit.position;
            agent.SetDestination(finalPosition);
            return true;
        }
        finalPosition = transform.position;
        return false;
    }

    //protected bool DetectRotation()
    //{
    //    Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
    //    //  lastRotation = transform.rotation;
    //    float angle;
    //    Vector3 axis;
    //    deltaRotation.ToAngleAxis(out angle, out axis);

    //    float angularSpeed = angle / Time.deltaTime;

    //    if (angularSpeed > rotationThreshold)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            receivePos = (Vector3)stream.ReceiveNext();
            receiveRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
