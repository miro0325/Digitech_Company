using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public GameObject OwnObject
    {
        get;
    }

    public bool IsInvulnerable
    {
        get;
    }
    
    public void Damage(float damage,UnitBase attacker);
}
