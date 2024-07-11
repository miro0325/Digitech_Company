using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BehaviorTree;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;



public class Spider : MonsterBase
{
    [Header("Spider Setting")]
    [SerializeField] private ProceduralWalk[] legs;
    [SerializeField] private RigBuilder rigBuilder;
    private InGamePlayer targetPlayer;
    
    protected override void Spawn()
    {
        testBaseStat.SetStat(Stats.Key.Hp, x => 100);
        maxStats.ChangeFrom(testBaseStat);
        tree = new BehaviorTree.Tree(new Sequence(new List<Node>
        {
            new Action(() => CheckDeath()),
            new Selector(new List<Node>
            {
                new Sequence(new List<Node>
                {
                    new Action(() => CheckPlayerInFOV()),
                    new Action(() => FollowTarget()),
                    new Action(() => AttackTarget())
                }),
                new Patrol(this,waypoints.ToArray())
            })
        }));
    }

    protected override void Start()
    {
        base.Start();
        
        Debug.Log(NavMesh.SamplePosition(transform.position, out var hit, 5, -1));
        agent.enabled = true;
        // agent.Warp(hit.position);
        agent.SetDestination(hit.position);
        Spawn();
    }

    protected override void Update()
    {
        base.Update();
        Debug.LogError(transform.position);
        Debug.LogError(agent.enabled);
    }

    private NodeState CheckPlayerInFOV()
    {
        if (targetPlayer != null)
        {
            if (targetPlayer.IsDie)
            {
                targetPlayer = null;
            }
            else
            {
                return NodeState.Succes;
            }
        }
        var targetUnits = fov.TargetUnits.Where(x => x is InGamePlayer).Select(x => x as InGamePlayer).Where(x => x != x.IsDie).ToList();
        if (targetUnits.Count > 0)
        {
            targetPlayer = targetUnits[0];
            return NodeState.Succes;
        }
        Debug.Log(targetPlayer != null);
        return NodeState.Failure;
    }

    private NodeState FollowTarget()
    {
        //if (targetPlayer == null) return NodeState.Failure;
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
    }

    public void OnAttack()
    {
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, LayerMask.GetMask("Player"));
        foreach (Collider hit in hits)
        {
            var player = hit.GetComponent<InGamePlayer>();
            player.Damage(attackDamage,this);
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

    public override void Move(Vector3 targetPos)
    {
        base.Move(targetPos);
    }

}
