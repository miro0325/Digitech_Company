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

    protected override void Move()
    {
    }

    protected override void Spawn()
    {
        
        tree = new BehaviorTree.Tree(new Patrol(this,waypoints));
    }

    protected override void Start()
    {
        base.Start();
        Spawn();
    }

    private void Update()
    {
        tree.Update();
    }
}
