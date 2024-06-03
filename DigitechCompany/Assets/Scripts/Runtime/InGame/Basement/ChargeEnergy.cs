using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeEnergy : MonoBehaviour, IInteractable
{
    private bool isCharging = false;
    
    public string GetInteractionExplain(UnitBase unit)
    {
        return "ÃæÀü";
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
        return true;
    }

    public void OnInteract(UnitBase unit)
    {
        
    }

}
