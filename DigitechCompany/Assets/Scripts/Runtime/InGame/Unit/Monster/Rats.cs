using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BehaviorTree;
using UnityEngine.AI;


public class Rats : MonsterBase
{
    public enum RatsState
    {
        Idle, Searching, Stolen, Attack, RunAway, Bring
    }

    public static Vector3 NestPosition;
    public static List<ItemBase> itemsInNest = new();
    [SerializeField]
    private GameObject weapon;
    [SerializeField]
    private RatsState state = RatsState.Idle;
    private InGamePlayer targetPlayer;

    [SerializeField]
    private int maxSearchCount = 3;
    private int curSearchCount = 0;
    private int curIndex = -1;

    [SerializeField]
    private float itemDetectRange;

    private bool isArrive = true;
    private bool isAlreadySetRandomPoint = false;
    private Vector3 targetPos = Vector3.zero;
    private ItemBase targetItem = null;

    protected override void Attack()
    {
        throw new System.NotImplementedException();
    }

    protected override void Death()
    {
        throw new System.NotImplementedException();
    }

    protected override void Spawn()
    {
        //Rats[] array = FindObjectsOfType<Rats>();
        //for (int i = 0; i < array.Length; i++)
        //{
        //    if(NestPosition != null)
        //}
        if(NestPosition == Vector3.zero)
        {
            NestPosition = transform.position;
        }
        Debug.Log(NestPosition);
        tree = new BehaviorTree.Tree(new Loop(new List<Node> {
            new Action(() => CheckItemInNest()),
            new Selector(
                new List<Node>
                {
                    
                    new Sequence(new List<Node>
                    {
                        new Action(() => CheckState(RatsState.Idle)),
                        new Action(() => ReturnToNest())
                    }),
                    new Sequence(new List<Node>
                    {
                        new Action(() => CheckState(RatsState.Searching)),
                        new Action(() => SearchItem()),
                        new CheckEnemyInFOV(fov,true),
                        new Action(() => SetState(RatsState.RunAway)),
                    }),
                    new Sequence(new List<Node>
                    {
                        new Action(() => CheckState(RatsState.Bring)),
                        new Action(() => GoToItem()),
                    }),
                    new Sequence(new List<Node>
                    {
                        new Action(() => CheckState(RatsState.RunAway)),
                        new Action(() => GoToRandomPos(transform.position,3,5))
                    }),
                    new Sequence(new List<Node>
                    {
                        new Action(() => CheckState(RatsState.Stolen)),
                    }),
                    new Sequence(new List<Node>
                    {
                        new Action(() => CheckState(RatsState.Attack)),
                        new Action(() => CheckTarget()),
                        new TraceTarget(this,0.1f),
                        new Action(() => AttackTarget()),
                    }),
                }
            )}
        )) ;
    }

    protected override void Start()
    {
        base.Start();
        
    }

    protected override void Update()
    {
        base.Update();
        animator.SetBool("IsRun", !agent.isStopped);
        var itemManager = ServiceLocator.For(this).Get<ItemManager>();
        itemManager?.SpawnItem(transform.position + Vector3.one, "Protein");
    }

    private NodeState CheckState(RatsState _state)
    {
        if (state != _state) return NodeState.Failure;
        Debug.Log(_state);
        return NodeState.Succes;
    }

    private NodeState SetState(RatsState _state)
    {
        state = _state;
        return NodeState.Succes;
    }

    private NodeState CheckItemInNest()
    {
        if(targetPlayer != null && !targetPlayer.IsDie) return NodeState.Succes;
        foreach(var item in itemsInNest)
        {
            if(item.CurUnit is InGamePlayer)
            {
                targetPlayer = item.CurUnit as InGamePlayer;
                state = RatsState.Attack;
                return NodeState.Succes;
            }
        }
        return NodeState.Failure;
    }

    protected NodeState ReturnToNest()
    {
        SetDestinationToPosition(NestPosition);
        Debug.Log(destination);
        Move(destination);
        if(Vector3.Distance(transform.position, destination) < 0.5f)
        {
            if(targetItem != null && targetItem.CurUnit.gameObject.Equals(gameObject))
            {
                targetItem.OnDiscard();
                weapon.SetActive(true);
            }
            state = RatsState.Searching;
            curSearchCount = 0;
            return NodeState.Succes;
        }
        return NodeState.Running;
    }

    private NodeState SearchItem()
    {
        if (curSearchCount == maxSearchCount)
        {
            state = RatsState.Idle;
            return NodeState.Succes;
        }
        if(isArrive)
        {
            isArrive = false;
            curIndex = Random.Range(0, waypoints.Length);
        }
        if(!SetDestinationToPosition(waypoints[curIndex].position, true))
        {
            return NodeState.Failure;
        }
        Move(destination);
        Collider[] hits = Physics.OverlapBox(transform.position, Vector3.one * itemDetectRange, Quaternion.identity, LayerMask.GetMask("Item"));
        if (hits.Length > 0)
        {
            if(targetItem == null)
            {
                targetItem = hits[0].GetComponent<ItemBase>();
                curSearchCount = 0;
                isArrive = true;
                state = RatsState.Bring;
                targetPos = GetNavMeshPosition(targetItem.transform.position, default(NavMeshHit));
                return NodeState.Succes;
            }
        }
        if (Vector3.Distance(transform.position, destination) < 1f) {
            curSearchCount++;
            
            isArrive = true;
        }
        return NodeState.Running;
    }

    private NodeState GoToItem()
    {
        Move(targetPos);
        if(Vector3.Distance(transform.position,targetPos) < 0.7f)
        {
            targetItem.OnInteract(this);
            weapon.SetActive(false);
            state = RatsState.Idle;
            return NodeState.Succes;
        }
        return NodeState.Running;
    }

    private NodeState GoToRandomPos(Vector3 pos, float minRange, float maxRange, int count = 100)
    {
        NodeState nodeState = NodeState.Running;
        if(!isAlreadySetRandomPoint)
        {
            int i = 0;
            NavMeshHit navMeshHit = default(NavMeshHit);
            Vector3 randomPos = transform.position;
            while (i < count)
            {
                Vector3 randomDir = Random.Range(minRange, maxRange) * Random.insideUnitCircle;
                randomDir.y = pos.y;
                if (NavMesh.SamplePosition(pos, out navMeshHit, 20, LayerMask.GetMask("Ground")))
                {
                    randomPos = navMeshHit.position;
                    break;
                }
                i++;
            }
            targetPos = randomPos;
            isAlreadySetRandomPoint = true;
        }
        Move(targetPos);
        if(Vector3.Distance(transform.position, targetPos) < 1f)
        {
            nodeState = NodeState.Succes;
            state = RatsState.Searching;
            isAlreadySetRandomPoint = false;
        }
        return nodeState;
    }

    private NodeState CheckTarget()
    {
        if(targetPlayer == null || (targetPlayer != null && targetPlayer.IsDie))
        {
            state = RatsState.Idle;
            return NodeState.Failure;
        } else
        {
            return NodeState.Succes;
        }
    }

    private NodeState AttackTarget()
    {
        Attack();
        return NodeState.Succes;
    }

    //private NodeState FindTarget()
    //{
    //    foreach(var )
    //}
}
