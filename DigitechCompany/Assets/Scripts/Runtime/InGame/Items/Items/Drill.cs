using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Drill : AttackableItem
{
    public override void OnAttack()
    {
        //if (pressedTime < 2.5f) return;
        curBattery -= requireBattery;
        //shootFX.Play();
        var hits = Physics.OverlapBox(attackPoint.position, new Vector3(0.6f, 0.6f, 0.6f), attackPoint.rotation, LayerMask.GetMask("Player", "Monster", "Damagable"));
        foreach (var hit in hits)
        {
            if(!hit.TryGetComponent<IDamagable>(out var damagable)) continue;
            if (OwnUnit.gameObject == damagable.OwnObject) continue;
            if (damagable.IsInvulnerable) continue;

            damagable.Damage(atkDamage, OwnUnit);
        }
    }

    public void OnAttackAnimationStart()
    {

    }

    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();
    }
}
