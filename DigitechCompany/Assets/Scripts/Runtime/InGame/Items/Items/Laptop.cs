using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Photon.Pun;

public class Laptop : ItemBase
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

    public override void OnUse(InteractID id)
    {
        if (isPlaying) return;
        base.OnUse(id);
        isOpen = !isOpen;
        isPlaying = true;
        var anim = (isOpen ? openAnim : closeAnim);
        animator.Play(anim);
    }

    protected override void OnUseRpc(int id)
    {
        isOpen = !isOpen;
        isPlaying = true;
        var anim = (isOpen ? openAnim : closeAnim);
        animator.Play(anim);
    }

    public void EndAnimation()
    {
        isPlaying = false;
        photonView.RPC(nameof(EndAnimationRPC), RpcTarget.Others);
    }

    [PunRPC]
    private void EndAnimationRPC()
    {
        isPlaying = false;
    }
}
