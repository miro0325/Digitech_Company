using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LobbyPunCallbackReceiver : MonoBehaviourPunCallbacks, IService
{
    public event Action onConnectedToMaster;
    public event Action onCreatedRoom;
    public event Action<List<RoomInfo>> onRoomListUpdate;
    public event Action onJoinedRoom;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    public override void OnConnectedToMaster() => onConnectedToMaster?.Invoke();
    public override void OnCreatedRoom() => onCreatedRoom?.Invoke();
    public override void OnRoomListUpdate(List<RoomInfo> roomList) => onRoomListUpdate?.Invoke(roomList);
    public override void OnJoinedRoom() => onJoinedRoom?.Invoke();
}