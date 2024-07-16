using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMoveDoor : MonoBehaviour, IInteractable
{
    public Vector3 position;

    public event Action<InGamePlayer> OnMove;

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
        var player = unit as InGamePlayer;
        if(player)
        {
            player.transform.position = position;
            OnMove?.Invoke(player);
        }
    }
}
