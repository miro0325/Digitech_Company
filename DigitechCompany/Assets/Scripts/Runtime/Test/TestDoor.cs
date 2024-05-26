using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 position;

    public string GetInteractionExplain(UnitBase unit)
    {
        return "¡¯¿‘";
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        return 1;
    }

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        return InteractID.ID1;
    }

    public bool IsInteractable(UnitBase unit)
    {
        return true;
    }

    public void OnInteract(UnitBase unit)
    {
        var player = unit as Player;
        if(player) player.SetPosition(position);
    }
}
