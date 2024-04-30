using Photon.Pun;
using UnityEngine;

public class ItemBase : MonoBehaviourPun, IInteractable
{
    protected UnitBase ownUnit;

    public bool InHand => ownUnit != null;

    public virtual void OnInteract(UnitBase unit) { }
    public virtual void OnUse() { }
    public virtual void OnThrow() { }
}