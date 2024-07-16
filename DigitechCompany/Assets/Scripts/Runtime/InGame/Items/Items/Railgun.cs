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
                    shootFX.Play();
                    chargeFX.Stop();
                }
                animator.SetBool(Animator_AttackPressedHash, b);
                chargeFX.Play();
            });
    }

    public override void OnAttack()
    {
        RaycastHit[] hits = Physics.BoxCastAll(attackPoint.position, new Vector3(0.5f, 0.5f, 0.5f), attackPoint.forward, attackPoint.rotation, attackRadius, LayerMask.GetMask("Player", "Monster", "Damagable"));
        foreach (var hit in hits)
        {
            var entity = hit.collider.GetComponent<IDamagable>();
            if (OwnUnit.gameObject == entity.OwnObject) continue;
            if (entity.IsInvulnerable) continue;
            Debug.Log(entity.OwnObject.name);
            if (entity is InGamePlayer)
            {
                hit.collider.GetComponent<InGamePlayer>().Damage(atkDamage, OwnUnit);
            }
            else
            {
                entity.Damage(atkDamage, OwnUnit);
            }
            break;
        }
    }

    protected override void Update()
    {
        base.Update();
    }
}
