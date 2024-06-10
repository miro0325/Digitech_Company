using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChargeEnergy : MonoBehaviour, IInteractable
{
    private bool isCharging = false;
    
    public string GetInteractionExplain(UnitBase unit)
    {
        if (isCharging) return "";
        else return "ÃæÀü";
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        return 0.2f;
    }

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        return InteractID.ID1;
    }

    public bool IsInteractable(UnitBase unit)
    {
        if (isCharging) return false;
        
        var player = unit as InGamePlayer;
        if(player)
        {
            if (player.Inventory.GetCurrentSlotItem() == null) return false;
        } else
        {
            return false;
        }
        return true;
    }

    public void OnInteract(UnitBase unit)
    {
        var player = unit as InGamePlayer; 
        if(player)
        {
            isCharging = true;
            var originPos = player.ItemHolder.transform.position;
            player.ItemHolder.transform.DOMove(transform.position,0.5f).OnComplete(() => EndCharge(player,originPos));
        }
    }

    private void EndCharge(InGamePlayer player, Vector3 originPos)
    {
        player.ItemHolder.transform.DOMove(originPos, 0.5f).OnComplete(() => isCharging = false);
    }

}
