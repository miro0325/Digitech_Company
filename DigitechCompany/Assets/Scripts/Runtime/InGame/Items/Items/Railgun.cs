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
        RaycastHit[] hits = Physics.BoxCastAll(attackPoint.position, new Vector3(0.5f, 0.5f, 0.5f), attackPoint.forward, attackPoint.rotation, attackRadius, LayerMask.GetMask("Player", "Monster", "Damagable"));
        foreach (var hit in hits)
        {
            var entity = hit.collider.GetComponent<IDamagable>();
            if(entity == null) continue;
            if (OwnUnit.gameObject == entity.OwnObject) continue;
            if (entity.IsInvulnerable) continue;
            if (entity is InGamePlayer)
            {
                hit.collider.GetComponent<InGamePlayer>().Damage(atkDamage, OwnUnit);
            }
            else
            {
                Debug.Log(entity.OwnObject.name);
                entity.Damage(atkDamage, OwnUnit);
            }
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
