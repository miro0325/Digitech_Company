using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum DoorState
{
    Open,Close,Lock
}

public class Door : MonoBehaviourPun,IInteractable,IPunObservable
{
    [SerializeField] private Transform door;
    [SerializeField] private DoorState doorState = DoorState.Close;
    [SerializeField] private float interactDelay;
    [SerializeField] private float rotSpeed;
    [SerializeField] private float openAngle;

    private bool isOpening = false;

    private Quaternion openRot;
    private Quaternion closeRot = Quaternion.Euler(0,0,0);

    void Start()
    {
        
    }

    void Update()
    {
        Debug.DrawRay(transform.parent.position,transform.parent.forward, Color.yellow);
        if (doorState == DoorState.Lock) return;
        if(isOpening)
        {
            door.transform.rotation = Quaternion.Slerp(door.transform.rotation, openRot, Time.deltaTime * rotSpeed);
        } else
        {
            door.transform.rotation = Quaternion.Slerp(door.transform.rotation,closeRot, Time.deltaTime * rotSpeed);
        }
    }

    private void OpenDoor(UnitBase unit)
    {
        var dirToUnit = unit.transform.position - transform.parent.position;
        float dot = Vector3.Dot(transform.parent.right, dirToUnit.normalized);
        Debug.Log(dot);
        if(dot > 0)
        {
            openRot = Quaternion.Euler(0, -openAngle, 0);
        } else
        {
            openRot = Quaternion.Euler(0, openAngle, 0);
        }
        doorState = DoorState.Open;
        isOpening = true;
    }

    private void CloseDoor()
    {
        doorState = DoorState.Close;
        isOpening = false;
    }

    public string GetInteractionExplain(UnitBase unit)
    {
        switch(doorState)
        {
            case DoorState.Open:
                return "¥›±‚";
            case DoorState.Close:
                return "ø≠±‚";
            case DoorState.Lock:
                if(CheckKeyItem(unit))
                {
                    return "¿·±› «ÿ¡¶";
                }
                return "¿·±Ë";
            default:
                return "Error";
        }
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        return interactDelay;
    }

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        if(doorState == DoorState.Lock)
        {
            return InteractID.ID2;
        } else
        {
            return InteractID.ID1;
        }
    }

    public bool IsInteractable(UnitBase unit)
    {
        if(doorState == DoorState.Lock)
        {
            if(CheckKeyItem(unit))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    public void OnInteract(UnitBase unit)
    {
        switch(doorState)
        {
            case DoorState.Lock:
                if(CheckKeyItem(unit))
                {
                    doorState = DoorState.Close;
                    NetworkObject.Destory(unit.ItemContainer.GetCurrentSlotItem().guid);
                }
                break;
            case DoorState.Close:
                OpenDoor(unit);
                break;
            case DoorState.Open:
                CloseDoor();
                break;
        }
    }

    private bool CheckKeyItem(UnitBase unit)
    {
        if (unit is Player)
        {
            var curItem = unit.ItemContainer.GetCurrentSlotItem();
            if (curItem != null && curItem.Key.Equals("Key"))
            {
                return true;
            }
            return false;
        } else
        {
            return false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(isOpening);
            stream.SendNext((int)doorState);
        }
        else
        {
            isOpening = (bool)stream.ReceiveNext();
            doorState = (DoorState)stream.ReceiveNext();
        }
    }
}
