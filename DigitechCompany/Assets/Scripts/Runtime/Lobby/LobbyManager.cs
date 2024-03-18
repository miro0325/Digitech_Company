using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Game.Lobby
{
    public enum ConnectingState { None, TryMaster, TryLobby, Done }

    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        public class ConnectionCompletionDelegateHolder
        {
            public Action onComplete;
        }
        
        private static LobbyManager instance;
        public static LobbyManager Instance => instance ??= FindObjectOfType<LobbyManager>();

        private ConnectingState connectingState;
        private ConnectionCompletionDelegateHolder connectionCompletionDelegateHolder;
        private List<RoomInfo> rooms = new();

        public ConnectingState ConnectingState => connectingState;
        public List<RoomInfo> Rooms => rooms;

        public ConnectionCompletionDelegateHolder ConnectToOnlineServer()
        {
            connectingState = ConnectingState.TryMaster;
            PhotonNetwork.ConnectUsingSettings();
            connectionCompletionDelegateHolder = new();
            return connectionCompletionDelegateHolder;
        }

        public override void OnConnectedToMaster()
        {
            connectingState = ConnectingState.TryLobby;
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            connectingState = ConnectingState.Done;
            connectionCompletionDelegateHolder.onComplete?.Invoke();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            rooms = roomList;
        }
    }
}