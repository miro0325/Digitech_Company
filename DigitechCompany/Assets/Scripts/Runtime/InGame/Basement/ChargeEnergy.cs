using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChargeEnergy : MonoBehaviour, IInteractable
{
    private UserInput input => UserInput.input;

    [SerializeField] private float chargingTime = 1;
    private bool isCharging = false;
    private WaitForSeconds wait;

    private void Awake()
    {
        wait = new WaitForSeconds(chargingTime);
    }

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
        if(player && !isCharging)
        {
           if (player.Inventory.GetCurrentSlotItem() == null) return;
           StartCoroutine(ChargingItem(player));
        }
    }

    IEnumerator ChargingItem(InGamePlayer player)
    {
        isCharging = true;
        
        input.Player.Disable();
        var originPos = player.ItemHolder.transform.position;
        yield return player.ItemHolder.transform.DOMove(transform.position, 0.5f).WaitForCompletion();
        yield return wait;
        player.ItemHolder.transform.DOMove(originPos, 0.5f).OnComplete(() => {
            isCharging = false;
            input.Player.Enable();
        });
    }
}
