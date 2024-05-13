using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBase : MonoBehaviour
{
    public Stats maxStats;
    public Stats curStats;
    public Stats.Modifier modifier;

    protected ItemBase ownItem;

    protected virtual void Awake()
    {
        maxStats = new Stats();
        curStats = new Stats();
        modifier = new Stats.Modifier();
    }
    protected virtual void OnSpawn()
    {

    }

    protected virtual void OnDamaged(float value)
    {

    }

    protected virtual void OnDeath()
    {

    }
}
