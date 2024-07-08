using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Photon.Pun;
using System;

public class Laptop : ItemBase
{
    private readonly int closeAnim = Animator.StringToHash("LaptopClose");
    private readonly int openAnim = Animator.StringToHash("LaptopOpen");

    private Animator animator;
    
    private bool isOpen = false;
    public bool isPlaying = false;

    //function
    public override void OnCreate()
    {
        base.OnCreate();
        
        animator = GetComponent<Animator>();
    }

    public override void OnInteract(UnitBase unit)
    {
        base.OnInteract(unit);
        animator.Play("Idle");
        isOpen = false;
        isPlaying = false;
    }

    public override bool IsUsable(InteractID id)
    {
        if(id == InteractID.ID2) return true;
        return false;
    }

    public override void OnUsePressed(InteractID id)
    {
        if (isPlaying) return;
        base.OnUsePressed(id);
        isOpen = !isOpen;
        isPlaying = true;
        var anim = (isOpen ? openAnim : closeAnim);
        animator.Play(anim);
    }

    [PunRPC]
    protected override void OnUsePressedRpc(int id)
    {
        isOpen = !isOpen;
        isPlaying = true;
        var anim = (isOpen ? openAnim : closeAnim);
        animator.Play(anim);
    }

    public void EndAnimation()
    {
        isPlaying = false;
    }

    // protected override void OnSendData(List<Func<object>> send)
    // {
    //     base.OnSendData(send);
    //     send.Add(() => isPlaying);
    // }

    // protected override void OnReceiveData(List<Action<object>> receive)
    // {
    //     base.OnReceiveData(receive);
    //     receive.Add(obj => isPlaying = (bool)obj);
    // }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);

        if (stream.IsWriting)
        {
            stream.SendNext(isPlaying);
        }
        else
        {
            isPlaying = (bool)stream.ReceiveNext();
        }
    }
}
