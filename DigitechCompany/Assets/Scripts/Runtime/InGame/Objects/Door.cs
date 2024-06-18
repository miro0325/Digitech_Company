using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;

public enum DoorState
{
    Open, Close, Lock
}

public class Door : MonoBehaviourPun, IInteractable, IPunObservable
{
    [SerializeField] private Transform door;
    [SerializeField] private DoorState doorState = DoorState.Close;
    [SerializeField] private float openDelay;
    [SerializeField] private float closeDelay;
    [SerializeField] private float unlockDelay;
    [SerializeField] private float rotSpeed;
    [SerializeField] private float openAngle;

    private Quaternion openRot;
    private Quaternion closeRot = Quaternion.Euler(0, 0, 0);

    void Update()
    {
        Debug.DrawRay(transform.parent.position, transform.parent.forward, Color.yellow);
        if (doorState == DoorState.Lock) return;
        if (doorState == DoorState.Open)
        {
            door.transform.localRotation = Quaternion.Slerp(door.transform.localRotation, openRot, Time.deltaTime * rotSpeed);
        }
        else
        {
            door.transform.localRotation = Quaternion.Slerp(door.transform.localRotation, closeRot, Time.deltaTime * rotSpeed);
        }
    }

    public string GetInteractionExplain(UnitBase unit)
    {
        switch (doorState)
        {
            case DoorState.Open:
                return "´Ý±â";
            case DoorState.Close:
                return "¿­±â";
            case DoorState.Lock:
                if (CheckKeyItem(unit))
                {
                    return "Àá±Ý ÇØÁ¦";
                }
                return "Àá±è";
            default:
                return "Error";
        }
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        switch (doorState)
        {
            case DoorState.Open:
                return closeDelay;
            case DoorState.Close:
                return openDelay;
            case DoorState.Lock:
                if (CheckKeyItem(unit))
                    return unlockDelay;
                return 0;

            default:
                return 0;
        }
    }

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        if (doorState == DoorState.Lock)
        {
            return InteractID.ID2;
        }
        else
        {
            return InteractID.ID1;
        }
    }

    public bool IsInteractable(UnitBase unit)
    {
        if (doorState == DoorState.Lock)
        {
            if (CheckKeyItem(unit))
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
        switch (doorState)
        {
            case DoorState.Lock:
                if (CheckKeyItem(unit))
                {
                    doorState = DoorState.Close;
                    var key = unit.Inventory.GetCurrentSlotItem();
                    var player = unit as InGamePlayer;
                    player.DiscardCurrentItem();
                    key.DestroyItem();
                }
                break;
            case DoorState.Close:
                //calculate unit relative position
                var dirToUnit = unit.transform.position - transform.parent.position;
                float dot = Vector3.Dot(transform.parent.right, dirToUnit.normalized);
                Debug.Log(dot);
                
                openRot = Quaternion.Euler(0, dot > 0 ? -openAngle : openAngle, 0);
                doorState = DoorState.Open;
                break;
            case DoorState.Open:
                doorState = DoorState.Close;
                break;
        }

        photonView.RPC(nameof(OnInteractRPC), RpcTarget.Others, (int)doorState, openRot);
    }

    [PunRPC]
    private void OnInteractRPC(int doorState, Quaternion openRot)
    {
        switch ((DoorState)doorState)
        {
            case DoorState.Open:
                this.openRot = openRot;
                this.doorState = DoorState.Open;
                break;
            case DoorState.Close:
                this.doorState = DoorState.Close;
                break;
        }
    }

    private bool CheckKeyItem(UnitBase unit)
    {
        if (unit is InGamePlayer)
        {
            var curItem = unit.Inventory.GetCurrentSlotItem();
            if (curItem != null && curItem.Key.Equals("Key"))
            {
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)doorState);
            stream.SendNext(openRot);
        }
        else
        {
            doorState = (DoorState)(int)stream.ReceiveNext();
            openRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
