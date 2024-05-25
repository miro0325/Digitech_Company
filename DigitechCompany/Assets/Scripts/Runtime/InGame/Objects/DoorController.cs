using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour,IInteractable
{
    [SerializeField] private Basement basement;
    
    public string GetInteractionExplain(UnitBase unit)
    {
        if(basement.IsOpenDoor)
        {
            return "´Ý±â";
        } else
        {
            return "¿­±â";
        }
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        return 0;
    }

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        throw new System.NotImplementedException();
    }

    public bool IsInteractable(UnitBase unit)
    {
        if(!basement.IsMovingDoor)
            return true;
        else 
            return false;
    }

    public void OnInteract(UnitBase unit)
    {
        basement.InteractDoor();
    }

}
