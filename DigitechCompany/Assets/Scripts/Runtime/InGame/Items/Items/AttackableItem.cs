using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using UniRx;
using System.Collections.Generic;
using System;

public class AttackableItem : ItemBase, IInteractable
{
    //static
    protected static int Animator_AttackHash = Animator.StringToHash("attack");
    protected static int Animator_AttackPressedHash = Animator.StringToHash("attackPressed");

    //field
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float atkDamage;
    [SerializeField] protected float attackRadius;

    protected ReactiveProperty<bool> isUsePressed = new();
    protected float delayTime;
    protected Animator animator;
    protected bool isUsing = false;

    //function
    public override void OnCreate()
    {
        base.OnCreate();
        
        animator = GetComponent<Animator>();
        SubscribeEvent();
    }

    protected virtual void SubscribeEvent()
    {
        isUsePressed
            .Subscribe(b =>
            {
                if (b)
                    animator.SetTrigger(Animator_AttackHash);
                animator.SetBool(Animator_AttackPressedHash, b);
            });
    }

    public override bool IsUsable(InteractID id)
    {
        if(UseBattery)
        {
            if (curBattery < requireBattery) return false;
        }
        if(id == InteractID.ID2) return delayTime <= 0;
        return false;
    }

    public override string GetUseExplain(InteractID id, UnitBase unit)
    {
        if(id == InteractID.ID2) return "АјАн";
        return "";
    }
    
    protected override void Update()
    {
        base.Update();
        if(!isUsePressed.Value && delayTime > 0)
            delayTime -= Time.deltaTime;

        //set camera view if ownUnit is pv controller
        if(!ReferenceEquals(OwnUnit, null))
            animator.SetLayerWeight(1, OwnUnit.photonView.IsMine ? 1 : 0);
    }

    public override void OnUsePressed(InteractID id)
    {
        if(id != InteractID.ID2) return;
        delayTime = 100f;
        animator.enabled = true;
        isUsePressed.Value = true;
        isUsing = true;
    }

    public override void OnUseReleased()
    {
        isUsePressed.Value = false;
    }

    public override void OnInactive()
    {
        base.OnInactive();
        animator.enabled = false;
        isUsePressed.Value = false;
    }

    // protected override void OnSendData(List<Func<object>> send)
    // {
    //     base.OnSendData(send);
    //     send.Add(() => isUsePressed.Value);
    // }

    // protected override void OnReceiveData(List<Action<object>> receive)
    // {
    //     base.OnReceiveData(receive);
    //     receive.Add(obj => isUsePressed.Value = (bool)obj);
    // }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        if(stream.IsWriting)
        {
            stream.SendNext(isUsePressed.Value);
        }
        else
        {
            isUsePressed.Value = (bool)stream.ReceiveNext();
        }
    }

    public virtual void OnAttack()
    {
        RaycastHit[] hits = Physics.BoxCastAll(OwnUnit.EyeLocation.position, new Vector3(0.5f,0.5f,0.5f),OwnUnit.EyeLocation.forward,OwnUnit.EyeLocation.rotation,attackRadius, LayerMask.GetMask("Player","Monster","Damagable"));
        foreach (var hit in hits)
        {
            var entity = hit.collider.GetComponent<IDamagable>();

            if(OwnUnit.gameObject == entity.OwnObject) continue;
            if (entity.IsInvulnerable) continue;
            if(entity is InGamePlayer)
            {
                hit.collider.GetComponent<InGamePlayer>().Damage(atkDamage,OwnUnit);
            }
            else
            {
                Debug.Log(entity.OwnObject.name);
                entity.Damage(atkDamage, OwnUnit);
            }
            break;
        }
    }

    public virtual void OnAttackAnimationEnd()
    {
        delayTime = 0.1f;
        animator.enabled = false;
        isUsing = false;
    }
}