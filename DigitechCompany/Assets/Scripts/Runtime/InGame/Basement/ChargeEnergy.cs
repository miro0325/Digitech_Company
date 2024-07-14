using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChargeEnergy : MonoBehaviour, IInteractable
{
    private UserInput input => UserInput.input;

    [SerializeField] private ParticleSystem chargingEffect;
    [SerializeField] private float chargingTime = 1;
    [SerializeField] private float interactableRange;
    private bool isCharging = false;
    private WaitForSeconds wait;

    private void Awake()
    {
        wait = new WaitForSeconds(chargingTime);
    }

    public string GetInteractionExplain(UnitBase unit)
    {
        if (isCharging || Vector3.Distance(transform.position, unit.transform.position) > interactableRange) return "";
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
            //Debug.Log(Vector3.Distance(transform.position, player.transform.position));
            if (Vector3.Distance(transform.position, player.transform.position) > interactableRange) return false;
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
        var item = player.Inventory.GetCurrentSlotItem();
        var originPos = player.ItemHolder.transform.position;
        var itemOriginPos = item.transform.position;
        item.transform.DOMove(transform.position, 0.5f);
        yield return player.ItemHolder.transform.DOMove(transform.position, 0.5f).WaitForCompletion();
        chargingEffect.Play();
        yield return wait;
        item.transform.DOMove(itemOriginPos, 0.5f);
        player.ItemHolder.transform.DOMove(originPos, 0.5f).OnComplete(() => {
            isCharging = false;
            input.Player.Enable();
        });
    }
}
