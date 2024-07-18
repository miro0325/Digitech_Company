using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using UniRx;
using System.Collections.Generic;
using System;

public class AttackableItem : ItemBase, IInteractable
{
    private SoundManager soundManager => ServiceLocator.GetEveryWhere<SoundManager>();

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

        photonView.RPC(nameof(OnUsePressedRpc), RpcTarget.All, (int)id);
    }

    [PunRPC]
    protected override void OnUsePressedRpc(int id)
    {
        animator.enabled = true;
        isUsePressed.Value = true;
    }

    public override void OnUseReleased()
    {
        isUsePressed.Value = false;
        photonView.RPC(nameof(OnUseReleasedRpc), RpcTarget.All);
    }

    [PunRPC]
    protected override void OnUseReleasedRpc()
    {
        isUsePressed.Value = false;
    }

    public override void OnDiscard()
    {
        animator.enabled = false;
        base.OnDiscard();
    }

    [PunRPC]
    protected override void OnDiscardRpc()
    {
        animator.enabled = false;
        base.OnDiscardRpc();
    }

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
        soundManager.PlaySound(Sound.Shovel_Attack, transform.position, 1f);
        var hits = Physics.OverlapBox(OwnUnit.EyeLocation.position, new Vector3(0.5f,0.5f,0.5f),OwnUnit.EyeLocation.rotation, LayerMask.GetMask("Player","Monster","Damagable"));
        foreach (var hit in hits)
        {
            if(!hit.TryGetComponent<IDamagable>(out var damagable)) continue;
            if (OwnUnit.gameObject == damagable.OwnObject) continue;
            if (damagable.IsInvulnerable) continue;

            damagable.Damage(atkDamage, OwnUnit);
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