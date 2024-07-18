using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Drill : AttackableItem
{

    public override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void SubscribeEvent()
    {
        isUsePressed
            .Subscribe(b =>
            {
                if (b)
                {
                    animator.SetTrigger(Animator_AttackHash);
                }
                animator.SetBool(Animator_AttackPressedHash, b);

            });
    }

    public override void OnUsePressed(InteractID id)
    {
        if (id != InteractID.ID2) return;
        delayTime = 10f;
        //Debug.Log("Ready");
        animator.enabled = true;
        isUsePressed.Value = true;
        isUsing = true;
    }

    public override void OnAttack()
    {
        //if (pressedTime < 2.5f) return;
        curBattery -= requireBattery;
        //shootFX.Play();
        RaycastHit[] hits = Physics.BoxCastAll(attackPoint.position, new Vector3(0.6f, 0.6f, 0.6f), attackPoint.forward, attackPoint.rotation, attackRadius, LayerMask.GetMask("Player", "Monster", "Damagable"));
        foreach (var hit in hits)
        {
            var entity = hit.collider.GetComponent<IDamagable>();
            if (entity == null) continue;
            if (OwnUnit.gameObject == entity.OwnObject) continue;
            if (entity.IsInvulnerable) continue;
            if (entity is InGamePlayer)
            {
                hit.collider.GetComponent<InGamePlayer>().Damage(atkDamage, OwnUnit);
            }
            else
            {
                entity.Damage(atkDamage, OwnUnit);
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        if (isUsing)
        {
        }
        else
        {
        }
        //if(isUsePressed.Value && isUsing)
        //{
        //    pressedTime += Time.deltaTime;
        //}
    }


    public void OnAttackAnimationStart()
    {
    }

    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();
    }
}
