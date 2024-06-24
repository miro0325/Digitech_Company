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
    protected Stats testBaseStat;
    protected NavMeshAgent agent;
    protected BehaviorTree.Tree tree;

    public override Stats BaseStats => testBaseStat;
    public NavMeshAgent Agent => agent;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        Spawn();
    }

    private void Update()
    {
        
    }

    protected abstract void Spawn();

    protected abstract void Move();

    protected abstract void Attack();

    protected abstract void Death();
}
