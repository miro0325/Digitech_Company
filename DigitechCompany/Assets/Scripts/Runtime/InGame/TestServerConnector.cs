using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Game.InGame
{
    public class TestServerConnector : MonoBehaviourPunCallbacks
    {
        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            PhotonNetwork.CreateRoom("Test");
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.LoadLevel("test-gamescene");
        }
    }
}