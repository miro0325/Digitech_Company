using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Photon.Pun;

public class Laptop : ItemBase, IPunObservable
{
    private readonly int closeAnim = Animator.StringToHash("LaptopClose");
    private readonly int openAnim = Animator.StringToHash("LaptopOpen");

    private bool isOpen = false;
    public bool isPlaying = false;

    protected override void Update()
    {
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
        //photonView.RPC(nameof(EndAnimationRPC), RpcTarget.Others);
    }

    //[PunRPC]
    //private void EndAnimationRPC()
    //{
    //    isPlaying = false;
    //}

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
