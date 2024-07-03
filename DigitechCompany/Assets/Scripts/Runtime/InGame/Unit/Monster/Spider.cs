using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class Spider : MonsterBase
{
    protected override void Attack()
    {
    }

    protected override void Death()
    {
    }

    public override void Move(Vector3 targetPos)
    {
        base.Move(targetPos);
    }

    protected override void Spawn()
    {

        tree = new BehaviorTree.Tree(new Selector(new List<Node>
        {
            new Sequence(new List<Node>
            {
                new CheckEnemyInFOV(fov),
                new TraceTarget(this)
            }),
            new Patrol(this,waypoints)
        }));
        //tree = new BehaviorTree.Tree(
        //    new Patrol(this,waypoints)
        //);
    }

    

    protected override void Start()
    {
        base.Start();
        Spawn();
    }

    protected override void Update()
    {
        base.Update();
    }
}
