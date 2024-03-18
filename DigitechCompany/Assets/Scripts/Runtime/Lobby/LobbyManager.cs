using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Game.Lobby
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        private static LobbyManager instance;
        public static LobbyManager Instance => instance ??= FindObjectOfType<LobbyManager>();

        private bool isConnectedMaster;
        private bool isConnectedLobby;
        private List<RoomInfo> rooms = new();

        public bool IsConnectedMaster => isConnectedMaster;
        public bool IsConnectedLobby => isConnectedLobby;
        public List<RoomInfo> Rooms => rooms;

        private void Awake()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            isConnectedMaster = true;
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            isConnectedLobby = true;
        }
    }
}