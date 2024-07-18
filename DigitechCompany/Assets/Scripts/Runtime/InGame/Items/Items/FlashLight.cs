using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FlashLight : ItemBase
{
    [SerializeField] private bool isOnFlashlight = false;
    [SerializeField] private GameObject lightObj;
    
    public override string GetInteractionExplain(UnitBase unit)
    {
        return base.GetInteractionExplain(unit);
    }

    public override float GetInteractRequireTime(UnitBase unit)
    {
        return base.GetInteractRequireTime(unit);
    }

    public override string GetUseExplain(InteractID id, UnitBase unit)
    {
        return base.GetUseExplain(id, unit);
    }

    public override InteractID GetTargetInteractID(UnitBase unit)
    {
        return base.GetTargetInteractID(unit);
    }

    public override bool IsInteractable(UnitBase unit)
    {
        return base.IsInteractable(unit);
    }

    public override bool IsUsable(InteractID id)
    {
        if (id != InteractID.ID2) return false;
        if(UseBattery)
        {
            if(curBattery <= requireBattery)
            {
                return false;
            }
        }
        return true;
    }

    protected override void Update()
    {
        base.Update();
        if (isOnFlashlight)
        {
            curBattery -= requireBattery * Time.deltaTime;
            if(curBattery <= 0)
            {
                curBattery = 0;
                isOnFlashlight = false;
                lightObj.SetActive(false);
            }
        }
    }

    public override void OnInteract(UnitBase unit)
    {
        base.OnInteract(unit);
    }

    public override void OnUsePressed(InteractID id)
    {
        Debug.Log("OnUsePressed");
        if (id != InteractID.ID2) return;
        isOnFlashlight = !isOnFlashlight;
        lightObj.SetActive(isOnFlashlight);
        base.OnUsePressed(id);
    }

    [PunRPC]
    protected override void OnUsePressedRpc(int id)
    {
        isOnFlashlight = !isOnFlashlight;
        lightObj.SetActive(isOnFlashlight);
        base.OnUsePressedRpc(id);
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        if(stream.IsWriting)
        {
            stream.SendNext(isOnFlashlight);
        } else
        {
            isOnFlashlight = (bool)stream.ReceiveNext();
        }
    }

}
