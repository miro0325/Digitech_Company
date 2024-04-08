using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Game.Lobby
{
    public enum ConnectingState { None, TryMaster, TryLobby, InLobby, TryRoom, InRoom }

    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        private ConnectingState connectingState;
        private List<RoomInfo> rooms = new();

        public ConnectingState ConnectingState => connectingState;
        public List<RoomInfo> Rooms => rooms;

        private void Awake()
        {
            ServiceProvider.Register(this);
        }

        public void ConnectToOnlineServer()
        {
            connectingState = ConnectingState.TryMaster;
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public void ConnectToRoom()
        {
            connectingState = ConnectingState.TryRoom;
        }

        public override void OnConnectedToMaster()
        {
            connectingState = ConnectingState.TryLobby;
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            connectingState = ConnectingState.InLobby;
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            connectingState = ConnectingState.None;
        }

        public override void OnJoinedRoom()
        {
            connectingState = ConnectingState.InRoom;
            PhotonNetwork.LoadLevel("test-gamescene");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            base.OnJoinRoomFailed(returnCode, message);
            connectingState = ConnectingState.InLobby;
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            rooms = roomList;
        }
    }
}