using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CompanySellArea : MonoBehaviour, IInteractable
{
    [SerializeField] private MeshRenderer area;

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        return InteractID.ID1;
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        return 0.5f;
    }

    public bool IsInteractable(UnitBase unit)
    {
        return unit.Inventory.GetCurrentSlotItem() != null;
    }

    public string GetInteractionExplain(UnitBase unit)
    {
        return IsInteractable(unit) ? "ÆÇ¸Å" : "";
    }

    public void OnInteract(UnitBase unit)
    {
        var player = unit as InGamePlayer;
        if(!player) return;

        var item = player.DiscardCurrentItem();
        var layPosition =
            // area.transform.position +
            new Vector3(
                Random.Range(area.bounds.min.x, area.bounds.max.x),
                Random.Range(area.bounds.min.y, area.bounds.max.y),
                Random.Range(area.bounds.min.z, area.bounds.max.z)
            );

        item.transform.DOMove(layPosition, 0.2f).SetEase(Ease.Unset);
    }
}
