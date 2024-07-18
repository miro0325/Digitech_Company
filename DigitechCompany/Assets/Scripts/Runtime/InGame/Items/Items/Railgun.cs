using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Railgun : AttackableItem
{
    [SerializeField]
    private ParticleSystem chargeFX;
    [SerializeField]
    private ParticleSystem shootFX;
    private float pressedTime = 0;

    public override void OnCreate()
    {
        base.OnCreate();
        chargeFX.Stop();
    }

    public override void OnUsePressed(InteractID id)
    {
        if (id != InteractID.ID2) return;
        delayTime = 10f;
        //Debug.Log("Ready");
        animator.enabled = true;
        isUsePressed.Value = true;
        isUsing = true;
        pressedTime = 0;

        base.OnUsePressed(id);
    }

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

    protected override void Update()
    {
        base.Update();
        if(isUsing)
        {
            chargeFX.Play();
        }
        else
        {
            chargeFX.Stop();
        }
    }


    public void OnAttackAnimationStart()
    {
        shootFX.Play();
    }

    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();
    }
}
