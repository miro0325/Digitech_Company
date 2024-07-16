using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BehaviorTree;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;
using Photon.Pun;



public class Spider : MonsterBase
{
    public enum SpiderState
    {
        Idle, Attack
    }
    
    [Header("Spider Setting")]
    [SerializeField] private ProceduralWalk[] legs;
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private SpiderState state = SpiderState.Idle;

    private InGamePlayer targetPlayer;
    private Vector3 nestPosition;
    
    protected override void Spawn()
    {
        nestPosition = transform.position;
        testBaseStat.SetStat(Stats.Key.Hp, x => 100);
        maxStats.ChangeFrom(testBaseStat);
        tree = new BehaviorTree.Tree(new Sequence(new List<Node>
        {
            new Action(() => CheckDeath()),
            new Action(() => DetectDoor()),
            new Selector(new List<Node>
            {
                new Sequence(new List<Node>
                {
                    new Action(() => IsCalled()),
                    new Action(() => GoToCalledPos())
                }),
                new Sequence(new List<Node>
                {
                    new Action(() => CheckPlayerInFOV()),
                    new Action(() => CheckState(SpiderState.Attack)),
                    new Action(() => FollowTarget()),
                    new Action(() => AttackTarget())
                }),
                new Sequence(new List<Node>
                {
                    new Action(() => CheckState(SpiderState.Idle)),
                    new Action(() => ReturnToNest()),
                })
            })
        }));
    }

    protected override void Start()
    {
        base.Start();
        
        Debug.Log(NavMesh.SamplePosition(transform.position, out var hit, 5, -1));
        agent.enabled = true;
        agent.SetDestination(hit.position);
        Spawn();
    }

    protected override void Update()
    {
        if (photonView.IsMine)
        {
            base.Update();
        } else
        {
            FixTransform();
        }
    }

    protected override NodeState GoToCalledPos()
    {
        Vector3 finalPos;
        while (true)
        {
            if (SetRandomPosition(targetPos, out finalPos, 3))
            {
                break;
            }
        }
        if (Vector3.Distance(transform.position, finalPos) > 0.6f)
        {
            if (SetDestinationToPosition(finalPos))
            {
                Move(destination);
            }
            return NodeState.Running;
        }
        
        while(curDelayTime < delayTime)
        {
            curDelayTime += Time.deltaTime;
        }
        state = SpiderState.Idle;
        return NodeState.Succes;
    }

    private NodeState CheckState(SpiderState _state, bool reverse = false)
    {
        if (state != _state) return reverse ? NodeState.Succes : NodeState.Failure;
        return reverse ? NodeState.Failure : NodeState.Succes;
    }

    private NodeState ReturnToNest()
    {
        if (Vector3.Distance(transform.position, destination) > 0.6f)
        {
            SetDestinationToPosition(nestPosition);
            Move(destination);
            return NodeState.Running;
        }
        return NodeState.Succes;
    }

    private NodeState CheckPlayerInFOV()
    {
        if (targetPlayer != null)
        {
            if (targetPlayer.IsDie)
            {
                state = SpiderState.Idle;
                targetPlayer = null;
            }
            else
            {
                state = SpiderState.Attack;
                return NodeState.Succes;
            }
        }
        var targetUnits = fov.TargetUnits.Where(x => x is InGamePlayer).Select(x => x as InGamePlayer).Where(x => x != x.IsDie).ToList();
        if (targetUnits.Count > 0)
        {
            state = SpiderState.Attack;
            targetPlayer = targetUnits[0];
            return NodeState.Succes;
        }
        return NodeState.Failure;
    }

    private NodeState FollowTarget()
    {
        SetDestinationToPosition(targetPlayer.transform.position);
        Move(destination);
        if (Vector3.Distance(transform.position, destination) < attackRange)
        {
            return NodeState.Succes;
        }
        else
        {
            agent.isStopped = false;
        }
        return NodeState.Running;
    }

    private NodeState AttackTarget()
    {
        if (isAttacking) return NodeState.Running;
        if (Vector3.Distance(transform.position, destination) < attackRange)
        {
            //agent.isStopped = true;
            Attack();
            return NodeState.Succes;
        }
        return NodeState.Running;
    }

    protected override void Attack()
    {
        isAttacking = true;
        animator.SetTrigger(Animator_AttackHash);
        base.Attack();
    }

    [PunRPC]
    protected override void AttackRPC()
    {
        isAttacking = true;
        animator.SetTrigger(Animator_AttackHash);
    }

    public void OnAttack()
    {
        if (!photonView.IsMine) return;
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, LayerMask.GetMask("Player"));
        foreach (Collider hit in hits)
        {
            var player = hit.GetComponent<InGamePlayer>();
            player?.Damage(attackDamage,this);
            break;
        }
    }

    public void OnAttackAnimEnd()
    {
        isAttacking = false;
        agent.isStopped = false;
    }

    protected override void Death()
    {
        rigBuilder.enabled = false;
        foreach (var leg in legs)
        {
            leg.enabled = false;
        }
        base.Death();
    }

    [PunRPC]
    protected override void DeathRPC()
    {
        rigBuilder.enabled = false;
        foreach (var leg in legs)
        {
            leg.enabled = false;
        }
        base.DeathRPC();
    }

    public override void Move(Vector3 targetPos)
    {
        base.Move(targetPos);
    }

}
